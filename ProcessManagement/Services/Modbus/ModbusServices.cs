using EasyModbus;
using ProcessManagement.Commons;
using ProcessManagement.Models;
using ProcessManagement.Pages.KehoachSX;
using ProcessManagement.Services.SQLServer;
using System.Net;
using System.Text;

namespace ProcessManagement.Services.Modbus
{
    public class ModbusServices
    {
        private readonly IPAddress address = IPAddress.Parse("192.168.0.17");
        public bool IsServerRunning { get; set; } = false;
        public int NumofConectedClient = 0;
        public ModbusServer? ModbusServer;

        public event EventHandler<object>? RequiredRenderEvent;
        public bool Is_RequiredRenderEvent_Registered()
        {
            return RequiredRenderEvent != null;
        }

        private SQLServerServices SQLServerServices = new();


        public void StartServer()
        {
            if (!IsServerRunning)
            {
                IsServerRunning = true;
                ModbusServer = new();
                ModbusServer.Listen();

                // Modbus Event register
                ModbusServer.CoilsChanged += ModbusServer_CoilsChanged;
                ModbusServer.HoldingRegistersChanged += ModbusServer_HoldingRegistersChanged;
                ModbusServer.NumberOfConnectedClientsChanged += ModbusServer_NumberOfConnectedClientsChanged;
                ModbusServer.LogDataChanged += ModbusServer_LogDataChanged;
            }
            else
            {
                IsServerRunning = false;
                ModbusServer?.StopListening();
                if (ModbusServer != null)
                {
                    // Modbus Event unregister
                    ModbusServer.CoilsChanged -= ModbusServer_CoilsChanged;
                    ModbusServer.HoldingRegistersChanged -= ModbusServer_HoldingRegistersChanged;
                    ModbusServer.NumberOfConnectedClientsChanged -= ModbusServer_NumberOfConnectedClientsChanged;
                    ModbusServer.LogDataChanged -= ModbusServer_LogDataChanged;
                }
                ModbusServer = null;
            }
        }

        // Modbus Event - Loging data  
        private void ModbusServer_LogDataChanged()
        {
            if (ModbusServer != null && IsServerRunning)
            {
                // Update datetime to all clients
                string datetimenow = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                WriteRegData(Regs.Server.ServerDatetime, Regs.RegTypes.RegWriteString, datetimenow); // update datetime now ( 4x58 )
            }
        }

        // Modbus Event - Number of client connected changed
        private void ModbusServer_NumberOfConnectedClientsChanged()
        {
            if (ModbusServer != null)
            {
                int numofConnected = ModbusServer.NumberOfConnections;
                if (numofConnected != NumofConectedClient)
                {
                    NumofConectedClient = numofConnected;
                    RequiredRenderEvent?.Invoke(null, NumofConectedClient);
                }
            }
        }

        // Modbus Event - Holding register changed
        private void ModbusServer_HoldingRegistersChanged(int register, int numberOfRegisters)
        {
            RequiredRenderEvent?.Invoke(null, numberOfRegisters);

            if (ModbusServer != null && IsServerRunning)
            {   
                // Required load lsx // 4x 53
                if (register == Regs.Device01.NgCongRequiredLoadLSX01)  // Device 01 - Nguyen cong Tien phi load LSX // 4x 53
                {
                    if (ModbusServer.holdingRegisters[Regs.Device01.NgCongRequiredLoadLSX01] == 1) // Is required load LSX from device 
                    {
                        int nguyencongID = ModbusServer.holdingRegisters[Regs.Device01.NgCongRequiredLoadLSX01];

                        // Send LSX to device 01
                        WriteLSXdetailstoDevice01(nguyencongID);

                        // Return lsx is load done - chong nhan hai lan khi chua load xong// 0x 02 
                        ModbusServer.coils[Regs.Device01.LSXIsLoading01] = false;

                        // Reset cho lan raising tiep theo
                        ModbusServer.holdingRegisters[Regs.Device01.NgCongRequiredLoadLSX01] = 0;
                    }
                }
            }
        }

        // Modbus Event - Coils changed
        private void ModbusServer_CoilsChanged(int coil, int numberOfCoils)
        {
            if (ModbusServer != null && IsServerRunning)
            {
                if (coil == 4) // SubmitBit (0x3) // Device 01 submit ket qua gia cong
                {
                    ModbusServer.coils[4] = false;

                    // Read submit data from device 01
                    ReadDataSubmitedFromDevice01andUpdate();
                }
            }
        }


        // Load LSX thong tin chi tiet
        private void WriteLSXdetailstoDevice01(int nguyencongID)
        {
            if (Common.CurrentKHSX?.KHSXID == null)
            {
                SendAlarmLogToDevice01(Regs.AlarmCode.LSXNotexist); // send alarmlog 6: chua co LSX
                return;
            }

            // Get nguyen cong
            var tagertNgCong = SQLServerServices.GetNguyenCong(Regs.NguyenCongID.IDs[nguyencongID]);

            var nguyencong = Common.CurrentKHSX?.DSachCongDoans.FirstOrDefault(x => x.TenCongDoan.Value?.ToString()?.Trim() == tagertNgCong.TenNguyenCong.Value?.ToString()?.Trim()) ?? null;

            if (nguyencong != null)
            {
                int slLoihientai = nguyencong.CalculateTongSLNGnguyenCong();
                int slloichophep = int.TryParse(nguyencong.SoluongNG.Value?.ToString(), out int sllcp) ? sllcp : 0;

                var tongOK = 0; var tongNG = 0; var sllotdone = 0;
                foreach (var lot in nguyencong.DSachNVLCongDoans)
                {
                    var OK = (int.TryParse(lot?.CaNgay.OK.Value?.ToString(), out int lotOKngay) ? lotOKngay : 0) + (int.TryParse(lot?.CaDem.OK.Value?.ToString(), out int lotOKdem) ? lotOKdem : 0);

                    var NG = (int.TryParse(lot?.CaNgay.NG.Value?.ToString(), out int lotNGngay) ? lotNGngay : 0) + (int.TryParse(lot?.CaDem.NG.Value?.ToString(), out int lotNGdem) ? lotNGdem : 0);

                    tongOK += OK; tongNG += NG;

                    if (OK > 0 || NG > 0)
                    {
                        sllotdone++;
                    }
                }
                var sldasanxuat = tongOK + tongNG;

                // SLLoichophep to device 01 // 4x48
                WriteRegData(Regs.Device01.SLLoichophep01, Regs.RegTypes.RegHoldingRegisters, slloichophep.ToString());

                // SLLoihientai to device 01 // 4x55
                WriteRegData(Regs.Device01.SLLoihientai01, Regs.RegTypes.RegHoldingRegisters, slLoihientai.ToString());

                // SLDaSanXuat to device 01 // 4x47
                WriteRegData(Regs.Server.SLDaSanXuat, Regs.RegTypes.RegHoldingRegisters, sldasanxuat.ToString());

                // SLLotDone to device 01 // 4x49
                WriteRegData(Regs.Server.SLLotDone, Regs.RegTypes.RegHoldingRegisters, sllotdone.ToString());

                // MaLSX to device01 // 4x16
                string maLSX = Common.CurrentKHSX?.MaLSX.Value?.ToString() ?? string.Empty;
                WriteRegData(Regs.Server.LSXCode, Regs.RegTypes.RegWriteString, maLSX);

                // Masanpham to device01 // 4x22
                string maSP = Common.CurrentKHSX?.SanPham?.MaSP.Value?.ToString() ?? string.Empty;
                WriteRegData(Regs.Server.MaSanPham, Regs.RegTypes.RegWriteString, maSP);

                // LoaiNVL to device01 // 4x28
                string loaiNVL = Common.CurrentKHSX?.LoaiNL.Value?.ToString() ?? string.Empty;
                WriteRegData(Regs.Server.LoaiNVL01, Regs.RegTypes.RegWriteString, loaiNVL);

                // SLSanxuat to device01 // 4x46
                // Maximum num is 32767 for short
                string slSX = Common.CurrentKHSX?.SLSanXuat.Value?.ToString() ?? "0";
                WriteRegData(Regs.Server.SLSanXuat, Regs.RegTypes.RegHoldingRegisters, slSX);

                // SLLotNVL to device 01 // 4x51
                string slLotNVL = Common.CurrentKHSX?.SLLot.Value?.ToString() ?? "0";
                WriteRegData(Regs.Server.SLLot, Regs.RegTypes.RegHoldingRegisters, slLotNVL);
            }
            else
            {
                ResetDataLSXDetailstoDevice01();
                SendAlarmLogToDevice01(Regs.AlarmCode.NCNotexist); // send alarmlog 3 : nguyen cong khong ton tai trong LSX
            }
        }

        // Reset LSX details for client
        private void ResetDataLSXDetailstoDevice01()
        {
            // SLLoichophep to device 01 // 4x48
            WriteRegData(Regs.Device01.SLLoichophep01, Regs.RegTypes.RegHoldingRegisters, "0");

            // SLLoihientai to device 01 // 4x55
            WriteRegData(Regs.Device01.SLLoihientai01, Regs.RegTypes.RegHoldingRegisters, "0");

            // SLDaSanXuat to device 01 // 4x47
            WriteRegData(Regs.Server.SLDaSanXuat, Regs.RegTypes.RegHoldingRegisters, "0");

            // SLLotDone to device 01 // 4x49
            WriteRegData(Regs.Server.SLLotDone, Regs.RegTypes.RegHoldingRegisters, "0");

            // MaLSX to device01 // 4x16
            WriteRegData(Regs.Server.LSXCode, Regs.RegTypes.RegWriteString, "0");

            // Masanpham to device01 // 4x22
            WriteRegData(Regs.Server.MaSanPham, Regs.RegTypes.RegWriteString, "0");

            // LoaiNVL to device01 // 4x28
            WriteRegData(Regs.Server.LoaiNVL01, Regs.RegTypes.RegWriteString, "0");

            // SLSanxuat to device01 // 4x46
            // Maximum num is 32767 for short
            WriteRegData(Regs.Server.SLSanXuat, Regs.RegTypes.RegHoldingRegisters, "0");

            // SLLotNVL to device 01 // 4x51
            WriteRegData(Regs.Server.SLLot, Regs.RegTypes.RegHoldingRegisters, "0");
        }

        // Read submit data from device 01
        private void ReadDataSubmitedFromDevice01andUpdate()
        {
            if (ModbusServer != null && IsServerRunning)
            {
                // Read Maquanlylot (QRCode) // 4x0 - lenght 31
                string maquanlylot = ReadStringData(0, 31);

                // Read Manhanvien // 4x34 - lenght 11
                string manhanvien = ReadStringData(34, 11);

                // Read Calamviec // 0x1 
                bool calamviec = ModbusServer.coils[2];

                // Read SLOK // 4x50
                int slOK = ModbusServer.holdingRegisters[51];

                // Read SLNG // 4x52
                int slNG = ModbusServer.holdingRegisters[53];

                ModbusServer.coils[1] = false; // disable device 01 // 0x0

                // calling update method
                Regs.AlarmCode result = UpdateCalamviec(calamviec, maquanlylot, manhanvien, slOK, slNG);

                // feedback update result to client
                SendAlarmLogToDevice01(result);
            }
        }

        private Regs.AlarmCode UpdateCalamviec(bool ca, string maquanlylot, string manhanvien, int slOK, int slNG)
        {
            Regs.AlarmCode result = Regs.AlarmCode.UpdateSuccess; string errorMess = string.Empty;

            if (string.IsNullOrEmpty(maquanlylot) || string.IsNullOrEmpty(manhanvien))
            {
                return Regs.AlarmCode.NotEnoughInfor;
            }

            if ((slOK == 0 && slNG == 0))
            {
                return Regs.AlarmCode.ErrorSLOKNG;
            }

            // Get target NVLmoiNguyencong
            var targetItems = Common.CurrentKHSX?.DSachCongDoans.SelectMany(item => item.DSachNVLCongDoans).FirstOrDefault(mql => mql.MaQuanLy.Value?.ToString() == maquanlylot) ?? null;

            // Checking maquanlylot
            if (targetItems == null) return Regs.AlarmCode.MQLNotexist; // ma quan ly khong ton tai

            // Checking manhanvien

            // Checking maquanlylot cua nguyen cong da update hay chua
            var isupdated = (targetItems.IsUpdated.Value?.ToString() == "1");
            if (isupdated) return Regs.AlarmCode.IsUpdated;


            // so sanh slOK/slNG tuong ung voi sl con lai cua qua trinh sx

            return result;
        }

        // Method send alarm log to client 
        private void SendAlarmLogToDevice01(Regs.AlarmCode alarmCode)
        {
            if (ModbusServer != null && IsServerRunning)
            {
                ModbusServer.holdingRegisters[58] = (short)alarmCode; // AlarmLog register // 4x57 //
            }
        }

        // Convert string to register type
        private int[] ConvertStringToRegisters(string stringToConvert)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(stringToConvert);
            int[] array = new int[stringToConvert.Length / 2 + stringToConvert.Length % 2];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = bytes[i * 2];
                if (i * 2 + 1 < bytes.Length)
                {
                    array[i] |= bytes[i * 2 + 1] << 8;
                }
            }

            return array;
        }

        // Convert regsiter type to string
        private static string ConvertRegistersToString(int[] registers, int offset, int stringLength)
        {
            byte[] array = new byte[stringLength];
            byte[] array2 = new byte[2];
            for (int i = 0; i < stringLength / 2; i++)
            {
                array2 = BitConverter.GetBytes(registers[offset + i]);
                array[i * 2] = array2[0];
                array[i * 2 + 1] = array2[1];
            }

            return Encoding.Default.GetString(array);
        }

        // Read string data
        public string ReadStringData(int regStartPosition, int stringLengh)
        {
            if (IsServerRunning && ModbusServer != null)
            {
                int regEndPosition = regStartPosition + (stringLengh / 2 + stringLengh % 2);

                ArraySegment<short> segment = new(ModbusServer.holdingRegisters.localArray, regStartPosition, regEndPosition - regStartPosition);

                short[] shortRegsVal = segment.ToArray();

                int[] intRegsVal = Array.ConvertAll(shortRegsVal, x => (int)x);

                string convertResult = ConvertRegistersToString(intRegsVal, 0, stringLengh + 1);

                convertResult = convertResult.Replace("\0", " ").Trim();

                return convertResult;
            }
            else { return string.Empty; }
        }

        // Write string data
        public void WriteRegData(int address, string regType, string? value)
        {
            if (IsServerRunning && value != null)
            {
                if (regType == Regs.RegTypes.RegWriteString)
                {
                    int[] cv = ConvertStringToRegisters(value);

                    ModbusServer.HoldingRegisters? regs = ModbusServer?.holdingRegisters;
                    if (regs != null)
                    {
                        for (int i = 0; i < cv.Length; i++)
                        {
                            short shortvl = (short)cv[i];
                            regs[address + i] = shortvl;
                        }
                    }
                }
                else if (regType == Regs.RegTypes.RegHoldingRegisters)
                {
                    short ival = short.Parse(value); // Maximum num is 32767 for short

                    ModbusServer.HoldingRegisters? regs = ModbusServer?.holdingRegisters;
                    if (regs != null)
                    {
                        regs[address] = ival;
                    }
                }
                else if (regType == Regs.RegTypes.RegAnalogueInputs)
                {
                    short ival = short.Parse(value);
                    ModbusServer.InputRegisters? regs = ModbusServer?.inputRegisters;
                    if (regs != null)
                    {
                        regs[address] = ival;
                    }
                }
                else if (regType == Regs.RegTypes.RegDigitalInputs)
                {
                    bool ival = false;

                    if (value == "1" || value == "True") { ival = true; }

                    ModbusServer.DiscreteInputs? regs = ModbusServer?.discreteInputs;
                    if (regs != null)
                    {
                        regs[address] = ival;
                    }
                }
                else if (regType == Regs.RegTypes.RegCoilOutputs)
                {
                    bool ival = false;

                    if (value == "1" || value == "True") { ival = true; }

                    ModbusServer.Coils? regs = ModbusServer?.coils;
                    if (regs != null)
                    {
                        regs[address] = ival;
                    }
                }
            }
        }

        // Reset all holding regsiter value
        public void ResetAllHoldingRegisters()
        {
            if (IsServerRunning && ModbusServer != null)
            {
                ModbusServer.holdingRegisters = new ModbusServer.HoldingRegisters(ModbusServer);
            }
        }

        // // // // MODBUS CLIENT // // // // NOT USE
        #region Modbus client
        private ModbusClient? modbusClient;
        private string? receiveData = null;
        private string? sendData = null;
        public bool IsConnectedServer = false;
        private string ipServerAddress = "192.168.0.17";
        private string ipServerAddressLocal = "127.0.0.1";
        private int serverPort = 502;
        private int clientID = 1;

        public void InitialModbusClient()
        {
            modbusClient = new();
            // Data event initial
            modbusClient.ReceiveDataChanged += new ModbusClient.ReceiveDataChangedHandler(UpdateReceiveData);
            modbusClient.SendDataChanged += new ModbusClient.SendDataChangedHandler(UpdateSendData);
            modbusClient.ConnectedChanged += new ModbusClient.ConnectedChangedHandler(UpdateConnectedChanged);
        }
        private void UpdateReceiveData(object sender)
        {
            if (modbusClient != null)
            {
                receiveData = "Rx: " + BitConverter.ToString(modbusClient.receiveData).Replace("-", " ") + Environment.NewLine;
            }
        }
        private void UpdateSendData(object sender)
        {
            if (modbusClient != null)
            {
                sendData = "Tx: " + BitConverter.ToString(modbusClient.sendData).Replace("-", " ") + System.Environment.NewLine;
            }
        }
        private void UpdateConnectedChanged(object sender)
        {
            if (modbusClient != null && modbusClient.Connected)
            {
                IsConnectedServer = true;
            }
            else
            {
                IsConnectedServer = false;
            }
        }

        // Connect to server
        public void StartConnectToServer()
        {
            if (modbusClient != null)
            {
                try
                {
                    if (modbusClient.Connected)
                        modbusClient.Disconnect();

                    modbusClient.IPAddress = ipServerAddress;
                    modbusClient.Port = serverPort;
                    modbusClient.SerialPort = string.Empty;

                    modbusClient.Connect();
                }
                catch (Exception)
                {
                    throw new Exception("Can't connect to server");
                }
            }
            else { throw new Exception("Modbus client is null"); }
        }
        public void DisconnectToServer()
        {
            if (modbusClient != null)
            {
                try
                {
                    if (modbusClient.Connected)
                        modbusClient.Disconnect();
                }
                catch (Exception)
                {
                    throw new Exception("Can't disconnect to server");
                }
            }
            else { throw new Exception("Modbus client is null"); }
        }

        // Read from Server
        public bool[] ReadCoils(int startAddressInput, int numOfvalueInput)
        {
            try
            {
                bool[] serverResponse = new bool[numOfvalueInput];

                if (modbusClient != null && modbusClient.Connected)
                {
                    serverResponse = modbusClient.ReadCoils(startAddressInput - 1, numOfvalueInput);
                }
                return serverResponse;
            }
            catch (Exception)
            {
                throw new Exception("Exception Reading values from Server");
            }
        }
        public bool[] ReadDiscreteInputs(int startAddressInput, int numOfvalueInput)
        {
            try
            {
                bool[] serverResponse = new bool[numOfvalueInput];

                if (modbusClient != null && modbusClient.Connected)
                {
                    serverResponse = modbusClient.ReadDiscreteInputs(startAddressInput - 1, numOfvalueInput);
                }
                return serverResponse;
            }
            catch (Exception)
            {
                throw new Exception("Exception Reading values from Server");
            }
        }
        public int[] HoldingRegisters(int startAddressInput, int numOfvalueInput)
        {
            try
            {
                int[] serverResponse = new int[numOfvalueInput];

                if (modbusClient != null && modbusClient.Connected)
                {
                    serverResponse = modbusClient.ReadHoldingRegisters(startAddressInput - 1, numOfvalueInput);
                }
                return serverResponse;
            }
            catch (Exception)
            {
                throw new Exception("Exception Reading values from Server");
            }
        }
        public int[] ReadInputRegisters(int startAddressInput, int numOfvalueInput)
        {
            try
            {
                int[] serverResponse = new int[numOfvalueInput];

                if (modbusClient != null && modbusClient.Connected)
                {
                    serverResponse = modbusClient.ReadInputRegisters(startAddressInput - 1, numOfvalueInput);
                }
                return serverResponse;
            }
            catch (Exception)
            {
                throw new Exception("Exception Reading values from Server");
            }
        }

        // Write to Server
        public void WriteSingleCoil(int startAddressOutput, bool coilsToSend)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteSingleCoil(startAddressOutput - 1, coilsToSend);
                }
            }
            catch (Exception)
            {
                throw new Exception("Exception Writing values to Server");
            }
        }

        public void WriteSingleRegister(int startAddressOutput, int registerToSend)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteSingleRegister(startAddressOutput - 1, registerToSend);
                }
            }
            catch (Exception)
            {
                throw new Exception("Exception Writing values to Server");
            }
        }

        public void MultipleCoils(int startAddressOutput, bool[] coilsToSend)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteMultipleCoils(startAddressOutput - 1, coilsToSend);
                }
            }
            catch (Exception)
            {
                throw new Exception("Exception Writing values to Server");
            }
        }

        public void MultipleRegisters(int startAddressOutput, int[] registersToSend)
        {
            try
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    modbusClient.WriteMultipleRegisters(startAddressOutput - 1, registersToSend);
                }
            }
            catch (Exception)
            {
                throw new Exception("Exception Writing values to Server");
            }
        }
        #endregion
    }
}
