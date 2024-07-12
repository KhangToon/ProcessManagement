using EasyModbus;
using ProcessManagement.Commons;
using ProcessManagement.Models;
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

        public class RegTypes
        {
            public const string RegCoilOutputs = "Coil Outputs"; // 0x
            public const string RegDigitalInputs = "Digital Inputs"; // 2x
            public const string RegAnalogueInputs = "Analogue Inputs"; // 3x
            public const string RegHoldingRegisters = "Holding Registers"; // 4x
            public const string RegWriteString = "Write String";
        }

        public class NguyenCongRegs
        {
            public static Dictionary<int, int> NguyenCongs = new()
            {
                {1, 2}, // Tiện phi
                {2, 3}, // Tiện ren
                {3, 4}, // Khoan lỗ
                {4, 13}, // Bavia + Rữa
                {5, 14}, // Kiểm Pin,M,Ren
                {6, 15}, // Ngoại quan + đóng thùng 
            };
        }

        public void StartServer()
        {
            if (!IsServerRunning)
            {
                IsServerRunning = true;
                ModbusServer = new();
                ModbusServer.Listen();

                // Event register
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
                    ModbusServer.CoilsChanged -= ModbusServer_CoilsChanged;
                    ModbusServer.HoldingRegistersChanged -= ModbusServer_HoldingRegistersChanged;
                    ModbusServer.NumberOfConnectedClientsChanged -= ModbusServer_NumberOfConnectedClientsChanged;
                    ModbusServer.LogDataChanged -= ModbusServer_LogDataChanged;
                }
                ModbusServer = null;
            }
        }

        private void ModbusServer_LogDataChanged()
        {

        }

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

        private void ModbusServer_HoldingRegistersChanged(int register, int numberOfRegisters)
        {
            RequiredRenderEvent?.Invoke(null, numberOfRegisters);

            if (ModbusServer != null && IsServerRunning)
            {
                if (register == 54)  // Device 01 - Nguyen cong Tien phi load LSX // 4x53
                {
                    if (ModbusServer.holdingRegisters[54] == 1)
                    {
                        int nguyencongID = ModbusServer.holdingRegisters[54];

                        // Reset 
                        ModbusServer.holdingRegisters[54] = 0;

                        // Send LSX to device 01
                        WriteLSXdetailstoDevice01(nguyencongID);

                        // Disable LoadLSX button
                        ModbusServer.coils[3] = false;
                    }
                }
            }
        }

        private void ModbusServer_CoilsChanged(int coil, int numberOfCoils)
        {
            if (ModbusServer != null && IsServerRunning)
            {
                if (coil == 4) // SubmitBit (0x3) // Device 01 submit ket qua gia cong
                {
                    ModbusServer.coils[4] = false;

                    // Read submit data from device 01
                    ReadDataSubmitedFromDevice01();
                }
            }
        }



        // Load LSX thong tin chi tiet
        private void WriteLSXdetailstoDevice01(int nguyencongID)
        {
            if (Common.CurrentKHSX?.KHSXID == null)
            {
                SendAlarmLogToDevice01(6); // send alarmlog : chua co LSX
                return;
            }

            // Get nguyen cong
            var tagertNgCong = SQLServerServices.GetNguyenCong(NguyenCongRegs.NguyenCongs[nguyencongID]);

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
                WriteRegData("49", RegTypes.RegHoldingRegisters, slloichophep.ToString());

                // SLLoihientai to device 01 // 4x55
                WriteRegData("56", RegTypes.RegHoldingRegisters, slLoihientai.ToString());

                // SLDaSanXuat to device 01 // 4x47
                WriteRegData("48", RegTypes.RegHoldingRegisters, sldasanxuat.ToString());

                // SLLotDone to device 01 // 4x49
                WriteRegData("50", RegTypes.RegHoldingRegisters, sllotdone.ToString());

                // MaLSX to device01 // 4x16
                string maLSX = Common.CurrentKHSX?.MaLSX.Value?.ToString() ?? string.Empty;
                WriteRegData("17", RegTypes.RegWriteString, maLSX);

                // Masanpham to device01 // 4x22
                string maSP = Common.CurrentKHSX?.SanPham?.MaSP.Value?.ToString() ?? string.Empty;
                WriteRegData("23", RegTypes.RegWriteString, maSP);

                // LoaiNVL to device01 // 4x28
                string loaiNVL = Common.CurrentKHSX?.LoaiNL.Value?.ToString() ?? string.Empty;
                WriteRegData("29", RegTypes.RegWriteString, loaiNVL);

                // SLSanxuat to device01 // 4x46
                // Maximum num is 32767 for short
                string slSX = Common.CurrentKHSX?.SLSanXuat.Value?.ToString() ?? "0";
                WriteRegData("47", RegTypes.RegHoldingRegisters, slSX);

                // SLLotNVL to device 01 // 4x51
                string slLotNVL = Common.CurrentKHSX?.SLLot.Value?.ToString() ?? "0";
                WriteRegData("52", RegTypes.RegHoldingRegisters, slLotNVL);
            }
            else
            {
                ResetDataLSXDetailstoDevice01();
                SendAlarmLogToDevice01(3); // send alarmlog : nguyen cong khong ton tai trong LSX
            }
        }

        private void ResetDataLSXDetailstoDevice01()
        {
            // SLLoichophep to device 01 // 4x48
            WriteRegData("49", RegTypes.RegHoldingRegisters, "0");

            // SLLoihientai to device 01 // 4x55
            WriteRegData("56", RegTypes.RegHoldingRegisters, "0");

            // SLDaSanXuat to device 01 // 4x47
            WriteRegData("48", RegTypes.RegHoldingRegisters, "0");

            // SLLotDone to device 01 // 4x49
            WriteRegData("50", RegTypes.RegHoldingRegisters, "0");

            // MaLSX to device01 // 4x16
            WriteRegData("17", RegTypes.RegWriteString, "0");

            // Masanpham to device01 // 4x22
            WriteRegData("23", RegTypes.RegWriteString, "0");

            // LoaiNVL to device01 // 4x28
            WriteRegData("29", RegTypes.RegWriteString, "0");

            // SLSanxuat to device01 // 4x46
            // Maximum num is 32767 for short
            WriteRegData("47", RegTypes.RegHoldingRegisters, "0");

            // SLLotNVL to device 01 // 4x51
            WriteRegData("52", RegTypes.RegHoldingRegisters, "0");
        }

        private void SendAlarmLogToDevice01(short alarmCode)
        {
            if (ModbusServer != null && IsServerRunning)
            {
                ModbusServer.holdingRegisters[58] = alarmCode; // AlarmLog register // 4x57 //
            }
        }

        // Read submit data from device 01
        private void ReadDataSubmitedFromDevice01()
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

                if (string.IsNullOrEmpty(maquanlylot) || string.IsNullOrEmpty(manhanvien))
                {
                    SendAlarmLogToDevice01(5); // send alarmlog: chua nhap du thong tin
                    return;
                }

                // Checking maquanlylot
                // Checking manhanvien
                // Checking maquanlylot cua nguyen cong da update hay chua

                if ((slOK == 0 && slNG == 0))
                {
                    SendAlarmLogToDevice01(4); // send alarmlog: chua nhap du thong tin
                    return;
                }

                // so sanh slOK/slNG tuong ung voi sl con lai cua qua trinh sx

                SendAlarmLogToDevice01(1); // send alarmlog success
            }
        }


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

        public void WriteRegData(string address, string regType, string? value)
        {
            if (IsServerRunning && value != null)
            {
                int iadr = int.Parse(address);

                if (regType == RegTypes.RegWriteString)
                {
                    int[] cv = ConvertStringToRegisters(value);

                    ModbusServer.HoldingRegisters? regs = ModbusServer?.holdingRegisters;
                    if (regs != null)
                    {
                        for (int i = 0; i < cv.Length; i++)
                        {
                            short shortvl = (short)cv[i];
                            regs[iadr + i] = shortvl;
                        }
                    }
                }
                else if (regType == RegTypes.RegHoldingRegisters)
                {
                    short ival = short.Parse(value); // Maximum num is 32767 for short

                    ModbusServer.HoldingRegisters? regs = ModbusServer?.holdingRegisters;
                    if (regs != null)
                    {
                        regs[iadr] = ival;
                    }
                }
                else if (regType == RegTypes.RegAnalogueInputs)
                {
                    short ival = short.Parse(value);
                    ModbusServer.InputRegisters? regs = ModbusServer?.inputRegisters;
                    if (regs != null)
                    {
                        regs[iadr] = ival;
                    }
                }
                else if (regType == RegTypes.RegDigitalInputs)
                {
                    bool ival = false;

                    if (value == "1" || value == "True") { ival = true; }

                    ModbusServer.DiscreteInputs? regs = ModbusServer?.discreteInputs;
                    if (regs != null)
                    {
                        regs[iadr] = ival;
                    }
                }
                else if (regType == RegTypes.RegCoilOutputs)
                {
                    bool ival = false;

                    if (value == "1" || value == "True") { ival = true; }

                    ModbusServer.Coils? regs = ModbusServer?.coils;
                    if (regs != null)
                    {
                        regs[iadr] = ival;
                    }
                }
            }
        }

        public void ResetAllHoldingRegisters()
        {
            if (IsServerRunning && ModbusServer != null)
            {
                ModbusServer.holdingRegisters = new ModbusServer.HoldingRegisters(ModbusServer);
            }
        }

        // // // // MODBUS CLIENT // // // //
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
