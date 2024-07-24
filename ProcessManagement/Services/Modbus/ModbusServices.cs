using EasyModbus;
using ProcessManagement.Commons;
using ProcessManagement.Models;
using ProcessManagement.Pages.KehoachSX;
using ProcessManagement.Services.SQLServer;
using Radzen;
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
                // Read Tennguyencong // 4x 71
                var clientngcongID = ModbusServer.holdingRegisters[72];

                var nguyencongID = Regs.NguyenCongID.IDs[clientngcongID];

                NguyenCong targetngcong = SQLServerServices.GetNguyenCong(nguyencongID);

                string tenngcong = targetngcong.TenNguyenCong.Value?.ToString()?.Trim() ?? string.Empty;

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

                //ModbusServer.coils[1] = false; // disable device 01 // 0x0

                // calling update method
                Regs.AlarmCode result = UpdateCalamviec(tenngcong, calamviec, maquanlylot, manhanvien, slOK, slNG);

                // feedback update result to client
                SendAlarmLogToDevice01(result);
            }
        } 

        private Regs.AlarmCode UpdateCalamviec(string tenngcong, bool iscadem, string maquanlylot, string manhanvien, int slOK, int slNG)
        {
            // iscadem == 1 (ca dem) // iscadem = 0 (ca ngay)

            if (string.IsNullOrEmpty(maquanlylot) || string.IsNullOrEmpty(manhanvien))
            {
                return Regs.AlarmCode.NotEnoughInfor;
            }

            if ((slOK == 0 && slNG == 0))
            {
                return Regs.AlarmCode.ErrorSLOKNG;
            }

            // Get target NVLmoiNguyencong by maquanly and tennguyencong
            var targetLotNVLItems = Common.CurrentKHSX?.DSachCongDoans.SelectMany(item => item.DSachNVLCongDoans)
                            .FirstOrDefault(mql => mql.MaQuanLy.Value?.ToString()?.Trim() == maquanlylot && mql.TenCongDoan.Value?.ToString()?.Trim() == tenngcong) ?? null;

            // Checking maquanlylot
            if (targetLotNVLItems == null) return Regs.AlarmCode.MQLNotexist; // ma quan ly khong ton tai

            // Checking manhanvien

            // Checking maquanlylot cua nguyen cong da update hay chua
            var isupdated = (targetLotNVLItems.IsUpdated.Value?.ToString() == "1");
            if (isupdated) return Regs.AlarmCode.IsUpdated;

            // checking NG/OK error before update //
            int slgocLOTngcong = int.TryParse(targetLotNVLItems?.SLGoccuaLOTNVL.Value?.ToString(), out int slcgc) ? slcgc : 0;
            int slLOTtruocgiacong = int.TryParse(targetLotNVLItems?.SLTruocGiaCong.Value?.ToString(), out int sltgc) ? sltgc : 0;

            // lay so luong cua moi ca truoc do
            int cangayOK = int.TryParse(targetLotNVLItems?.CaNgay.OK.Value?.ToString(), out int cangayok) ? cangayok : 0;
            int cangayNG = int.TryParse(targetLotNVLItems?.CaNgay.NG.Value?.ToString(), out int cangayng) ? cangayng : 0;
            int cademOK = int.TryParse(targetLotNVLItems?.CaDem.OK.Value?.ToString(), out int candemok) ? candemok : 0;
            int cademNG = int.TryParse(targetLotNVLItems?.CaDem.NG.Value?.ToString(), out int candemng) ? candemng : 0;

            // gan lai gia tri NG/OK cua ca lam viec dang submit
            if (iscadem) { cademOK = slOK; cademNG = slNG; } else { cangayOK = slOK; cangayNG = slNG; }

            // gan sl truoc gia cong la sl goc cua lot nguyen cong, neu no == 0
            if (slLOTtruocgiacong == 0) { slLOTtruocgiacong = slgocLOTngcong; }

            // tinh sl con lai sau khi updated
            int slconlaiCD = slLOTtruocgiacong - (cangayOK + cangayNG) - (cademOK + cademNG);

            // so sanh slOK/slNG tuong ung voi sl con lai cua qua trinh sx
            if (slconlaiCD < 0 || slconlaiCD == slLOTtruocgiacong)
            {
                return Regs.AlarmCode.ErrorSLOKNG;
            }
            else
            {
                // create data for update calamviec
                TemNVLMCDValues targetCalamviec = new((iscadem) ? Common.Cadem : Common.Cangay)
                {
                    slConlai = slconlaiCD,
                    ngayGC = DateTime.TryParse(((iscadem) ? targetLotNVLItems?.CaDem : targetLotNVLItems?.CaNgay)?.NgayGiaCong.Value?.ToString(), out DateTime ngc) ? ngc : DateTime.Now,
                    slOK = slOK,
                    slNG = slNG,
                    tenNV = manhanvien
                };

                // update target calamviec
                (int updateResult, string updateErrMess) = SQLServerServices.UpdateCalamviec(targetLotNVLItems, targetCalamviec);

                if (updateResult == -1)
                {
                    return Regs.AlarmCode.UpdateFailed;
                }
                else
                {
                    int slOKsaukhigiacong = cangayOK + cademOK; // so luong OK(so luong con lai) sau khi gia cong

                    int slNGsaukhigiacong = cangayNG + cademNG; // so luong loi sau khi gia cong

                    int isUpdated = 0;

                    if (slconlaiCD == 0) { isUpdated = 1; } // khi da gia cong het sltruocgiacong

                    // Update slsaugiacong nguyen cong hien tai
                    (updateResult, updateErrMess) = SQLServerServices.UpdateSLsaugiacongNVLMCD(targetLotNVLItems?.NVLMCDID.Value, slOKsaukhigiacong, slNGsaukhigiacong, isUpdated);

                    // Update sltruocgiacong nguyen cong tiep theo
                    (updateResult, updateErrMess) = UpdateSLtruocgiacongOfNextNguyencong(targetLotNVLItems, slOKsaukhigiacong);

                    if (updateResult == -1)
                    {
                        return Regs.AlarmCode.UpdateFailed;
                    }
                    else
                    {
                        Common.RaiseCongdoanUpdatedEvent(targetLotNVLItems?.NVLMCDID.Value);

                        // Trigger LSX infor for client after updated

                        return Regs.AlarmCode.UpdateSuccess;
                    }
                }
            }
        }

        private (int, string) UpdateSLtruocgiacongOfNextNguyencong(NVLmoiNguyenCong? currentNVLMCD, int slsaukhigiacong)
        {
            int result = -1; string errorMess = string.Empty;

            int slgocnguyencong = int.TryParse(currentNVLMCD?.SLGoccuaLOTNVL.Value?.ToString(), out int slgoc) ? slgoc : 0;

            string maquanly = currentNVLMCD?.MaQuanLy.Value?.ToString() ?? string.Empty;

            int currentCDid = int.TryParse(currentNVLMCD?.CDID.Value?.ToString(), out int cdid) ? cdid : 0;

            int nextCDid = (currentCDid > 0) ? (currentCDid + 1) : 0;

            // index cua nguyen cong hien tai trong danh sach nguyen cong
            int indexCurrentNC = int.TryParse(currentNVLMCD?.IndexNguyenCong?.ToString(), out int indexnc) ? indexnc : -1;

            if (nextCDid == 0 || maquanly == string.Empty || indexCurrentNC == -1) { return (result, errorMess); }

            if (indexCurrentNC == 0) // nguyen cong dau tien
            {
                // Update nguyen cong tiep theo
                (result, errorMess) = SQLServerServices.UpdateSLTruocgiacongNextNguyenCong(nextCDid, maquanly, slsaukhigiacong);

                // Update sltruocgiacong nguyencong dau tien
                (result, errorMess) = SQLServerServices.UpdateSLTruocgiacongNextNguyenCong(currentCDid, maquanly, slgocnguyencong);
            }
            else
            {
                // Update nguyen cong tiep theo
                (result, errorMess) = SQLServerServices.UpdateSLTruocgiacongNextNguyenCong(nextCDid, maquanly, slsaukhigiacong);
            }

            return (result, errorMess);
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
