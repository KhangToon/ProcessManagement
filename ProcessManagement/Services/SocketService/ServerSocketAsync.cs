using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProcessManagement.Services;
using ProcessManagement.Commons;
using System.Text.RegularExpressions;
using ProcessManagement.Services.SQLServer;
using ProcessManagement.Services.SocketService;
using ProcessManagement.Models.KHO_NVL.XuatKho;
using Radzen;
using ProcessManagement.Models.KHO_NVL.NhapKho;
using ProcessManagement.Models.KHO_NVL;
using ProcessManagement.Models.KHO_NVL.Tracking;

namespace ParamountBed_Warehouse.Services.SocketService
{
    public class ServerSocketAsync
    {
        // Thread Signal
        private static ManualResetEvent _connected = new(false);
        // Server socket
        public Socket? _server = null;

        SQLServerServices SQLServerServices = new();

        #region UI Shared properties
        public bool serverrunning = false;
        public List<ConnectedObject> ConnectedClients = new();
        #endregion

        //public event EventHandler<string> CheckingEvent;
        public event EventHandler<object>? Im_Ex_Event;

        public bool IsIm_Ex_EventRegistered()
        {
            return Im_Ex_Event != null;
        }

        /// <summary>
        /// Listen for client connections
        /// </summary>
        public void StartListening()
        {
            try
            {
                _server = ConnectionManager.CreateListener();
                serverrunning = true;

                Thread acceptClientThread = new(() =>
                {
                    while (true)
                    {
                        // Set the event to nonsignaled state
                        _connected.Reset();

                        // Start an asynchronous socket to listen for connections
                        _server.BeginAccept(new AsyncCallback(AcceptCallback), _server);

                        // Wait until a connection is made before continuing
                        _connected.WaitOne();
                    }
                })
                {
                    IsBackground = true
                };
                acceptClientThread.Start();
            }
            catch (Exception ex)
            {
                string exMess = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// Handler for new connections
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue accepting new connections
            _connected.Set();

            // Accept new client socket connection
            Socket? socket = _server?.EndAccept(ar);

            // Create a new client connection object and store the socket
            ConnectedObject? client = new()
            {
                Socket = socket
            };

            // Store all clients
            ConnectedClients.Add(client);

            // Notify connected

            // Begin receiving messages from new connection
            try
            {
                client.Socket?.BeginReceive(client.Buffer, 0, client.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
            }
            catch (SocketException)
            {
                // Client was forcebly closed on the client side

                CloseClient(client);
            }
            catch (Exception ex)
            {
                string exMess = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// Handler for received messages
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            int bytesRead;

            // Check for null values
            if (!CheckState(ar, out string err, out ConnectedObject? client))
            {
                return;
            }

            if (client == null || client.Socket == null) return;

            // Read message from the client socket
            try
            {
                bytesRead = client.Socket.EndReceive(ar);
            }
            catch (SocketException)
            {
                // Client was forcebly closed on the client side
                CloseClient(client);
                return;
            }
            catch (Exception ex) { string exMess = ex.Message; throw; }

            // Check message
            if (bytesRead > 0)
            {
                // Build message as it comes in
                client.BuildIncomingMessage(bytesRead);

                // Check if we received the full message
                if (client.MessageReceived())
                {
                    // Handle required
                    HandleClientRequired(client);

                    // Update UI

                    // Reset message
                    client.ClearIncomingMessage();

                    // Acknowledge message
                    //SendReply(client, data);
                }
            }
            else
            {
                CloseClient(client);
                return;
            }

            // Listen for more incoming messages
            try
            {
                client.Socket.BeginReceive(client.Buffer, 0, client.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
            }
            catch (SocketException)
            {
                // Client was forcebly closed on the client side
                CloseClient(client);
            }
            catch (Exception ex) { string exMess = ex.Message; }
        }



        /// <summary>
        /// Sends a reply to client
        /// </summary>
        /// <param name="client"></param>
        public void SendReply(ConnectedObject client, string mess)
        {
            if (client == null)
            {
                return;
            }

            // Create reply
            client.CreateOutgoingMessage(mess);
            var byteReply = client.OutgoingMessageToBytes();

            // Listen for more incoming messages
            try
            {
                client.Socket?.BeginSend(byteReply, 0, byteReply.Length, SocketFlags.None, new AsyncCallback(SendReplyCallback), client);
            }
            catch (SocketException)
            {
                // Client was forcebly closed on the client side
                CloseClient(client);
            }
            catch (Exception ex) { string exMess = ex.Message; throw; }
        }

        /// <summary>
        /// Handler after a reply has been sent
        /// </summary>
        /// <param name="ar"></param>
        private static void SendReplyCallback(IAsyncResult ar)
        {
            //Console.WriteLine("Reply Sent");
        }

        /// <summary>
        /// Checks IAsyncResult for null value
        /// </summary>
        /// <param name="ar"></param>
        /// <param name="err"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        private static bool CheckState(IAsyncResult ar, out string err, out ConnectedObject? client)
        {
            // Initialise
            client = null;
            err = "";

            // Check ar
            if (ar == null)
            {
                err = "Async result null";
                return false;
            }

            // Check client
            client = ar?.AsyncState as ConnectedObject;

            if (client == null)
            {
                err = "Client null";
                return false;
            }

            return true;

        }

        /// <summary>
        /// Closes a client socket and removes it from the client list
        /// </summary>
        /// <param name="client"></param>
        private void CloseClient(ConnectedObject? client)
        {
            // Notify disconnected
            client?.Close();

            if (client != null)
            {
                if (ConnectedClients.Contains(client))
                {
                    client.ReceivedMessages?.Clear();
                    ConnectedClients.Remove(client);
                }
            }
        }

        /// <summary>
        /// Closes all client and server connections
        /// </summary>
        private void CloseAllSockets()
        {
            // Close all clients
            foreach (ConnectedObject connection in ConnectedClients)
            {
                connection.Close();
            }
            // Close server
            _server?.Close();
        }

        /// <summary>
        /// Handle client required
        /// </summary>
        /// <param name="incomingMessage"></param>
        private void HandleClientRequired(ConnectedObject client)
        {
            string clientMESS = client.IncomingMessage.Replace(Common.socketEndKey, "").ToString();

            SocketMessage? ClientMesage = JsonConvert.DeserializeObject<SocketMessage>(clientMESS);

            string CMDTYPE = ClientMesage?.Command[Common.CMDTYPE]?.ToString() ?? string.Empty;

            if (CMDTYPE == Common.PNK_LOAD)
            {
                HandleLoadPhieuNhapKhoDetails(ClientMesage, client);
            }
            else if (CMDTYPE == Common.PNK_HANDLE_LNK)
            {
                HandleExcuteLenhNhapKho(ClientMesage, client);
            }


        }

        public string ConvertData(List<IDictionary<string, object>> data)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            return jsonData;
        }

        public void HandleXuatKhoRequire(SocketMessage? mess, ConnectedObject client)
        {
            if (mess != null && mess.Data != null)
            {
                string maPXK = mess.Data.FirstOrDefault()?[Common.PXK_MPXK]?.ToString() ?? string.Empty;
                string maViTri = mess.Data.FirstOrDefault()?[Common.PXK_MVT]?.ToString() ?? string.Empty;
                string maNVL = mess.Data.FirstOrDefault()?[Common.PXK_MNVL]?.ToString() ?? string.Empty;

                (int lenhxkid, string errorMess) = OnExcuteLenhXuatKho(maPXK, maViTri, maNVL);

                if (lenhxkid == -1)
                {
                    SendFeedBackToClient(client, Common.PXK_RETURN, Common.FAIL, errorMess, new Dictionary<string, object>()); return;
                }
                else if (lenhxkid > 0)
                {
                    // Raising reload 
                    Common.RaisePXK_Event(lenhxkid);

                    SendFeedBackToClient(client, Common.PXK_RETURN, Common.SUCCESS, "Xuất kho thành công!", new Dictionary<string, object>()); return;
                }
            }
        }


        private (int, string) OnExcuteLenhXuatKho(string maphieu, string mavitri, string tennvl)
        {
            if (String.IsNullOrEmpty(maphieu) || String.IsNullOrEmpty(mavitri) || String.IsNullOrEmpty(tennvl))
            {
                return (-1, "Chưa đủ thông tin quét! (XKErr: 001)");
            }

            LenhXuatKho scanLXK = new();

            // Load phieu xuat kho id
            List<int> pxkIds = SQLServerServices.GetListPXKIds(maphieu.Trim());
            if (pxkIds.Count == 0) { return (-1, "Không tìm thấy phiếu xuất kho! (XKErr: 002)"); }
            int scanpxkID = pxkIds[0];


            // Load nvl id
            List<int> nvlIds = SQLServerServices.GetListNVLIds(tennvl.Trim());
            if (nvlIds.Count == 0) { return (-1, "Không tìm thấy nguyên liệu! (XKErr: 003)"); }
            int scannvlID = nvlIds[0];

            // Load vitri ID
            List<int> vitriIds = SQLServerServices.GetListVTriIds(mavitri.Trim());
            if (vitriIds.Count == 0) { return (-1, "Không tìm thấy vị trí lưu kho! (XKErr: 004)"); }
            int scanvitriID = vitriIds[0];

            if (scanpxkID == 0 || scannvlID == 0 || scanvitriID == 0)
            {
                { return (-1, "Không thể xuất kho! (XKErr: 005)"); }
            }

            // Get scan lenh xuat kho
            LenhXuatKho temLXK = new() { PXKID = { Value = scanpxkID }, NVLID = { Value = scannvlID }, VTID = { Value = scanvitriID } };

            scanLXK = SQLServerServices.GetLenhXuatKho(temLXK);
            scanLXK.ViTriofNVL = SQLServerServices.GetViTriOfNgVatLieuByNVLid_VTid(scanLXK.NVLID.Value, scanLXK.VTID.Value);

            if (scanLXK.LenhXKID.Value == null)
            {
                { return (-1, "Không tìm thấy lệnh xuất kho! (XKErr: 005)"); }
            }

            // Update so luong nguyen vat lieu
            int soluongXuatdi = int.TryParse(scanLXK.LXKSoLuong.Value?.ToString(), out int slxuat) ? slxuat : 0;

            // Tinh so luong hien co cua nvl o vitri
            int soluongHientai = int.TryParse(scanLXK.ViTriofNVL.VTNVLSoLuong.Value?.ToString(), out int slht) ? slht : 0;

            // gan so luong sau khi xuat cho vi tri da luu
            int newtonkho = soluongHientai - soluongXuatdi;

            if (newtonkho < 0)
            {
                { return (-1, "Số lượng xuất kho không hợp lệ! (XKErr: 006)"); }
            }


            // Check trang thai lenh (da hoan thanh hay chua)
            _ = int.TryParse(scanLXK.LXKIsDone.Value?.ToString(), out int scanlxkIsdone) ? scanlxkIsdone : -1;
            if (scanlxkIsdone != 0)
            {
                { return (-1, "Lệnh đã xuất trước đó! (XKErr: 007)"); }
            }

            // Update so luong vi tri da co cua nvl
            (int updateVTofNVLresult, string updateVTofNVLerror) = SQLServerServices.UpdateSoluongNgVatLieuById(scanLXK.ViTriofNVL.VTofNVLID.Value, newtonkho);

            if (updateVTofNVLresult == -1)
            {
                { return (-1, "Không thể xuất kho! (XKErr: 008)"); }
            }

            // Update lenh xuat kho status
            (int updatelxkResult, string updatelxkError) = SQLServerServices.UpdateLenhXuatKhoStatus(scanLXK.LenhXKID.Value, 1);

            if (updatelxkResult != -1)
            {
                // update status to UI
                scanLXK.LXKIsDone.Value = 1;

                _ = int.TryParse(scanLXK.LenhXKID.Value.ToString(), out int lenhxkid) ? lenhxkid : -1;

                { return (lenhxkid, "Xuất kho thành công!"); }
            }
            else { { return (-1, "Không thể xuất kho! (XKErr: 009)"); } }
        }


        public void HandleCheckOut(SocketMessage? mess, ConnectedObject client)
        {
            //if (mess != null && mess.Data != null)
            //{
            //    string scanCode = mess.Data.FirstOrDefault()?[Common.SCANCODE]?.ToString() ?? string.Empty;

            //    int slOK = int.Parse(mess.Data.FirstOrDefault()?[Common.SLOK]?.ToString() ?? "0");

            //    int slNG = int.Parse(mess.Data.FirstOrDefault()?[Common.SLNG]?.ToString() ?? "0");

            //    if (scanCode != string.Empty && slOK > 0 && slNG >= 0)
            //    {
            //        // Update sl NG OK masp
            //        (int result, string err) = SQLServerServices.UpdateNVLmoiCongdoan(slOK, slNG, scanCode, null, Common.CurrentCDid, Common.CurrentKHSXid);

            //        if (result == -1)
            //        {
            //            SendFeedBackToClient(client, Common.RETURNCHECKOUT, Common.FAIL, $"{err}", new Dictionary<string, object>()); return;
            //        }
            //        else if (result == 0)
            //        {
            //            SendFeedBackToClient(client, Common.RETURNCHECKOUT, Common.FAIL, $"Mã không nằm trong danh sách", new Dictionary<string, object>()); return;
            //        }
            //        else
            //        {
            //            SendFeedBackToClient(client, Common.RETURNCHECKOUT, Common.SUCCESS, $"Cập nhật kết quả thành công!", new Dictionary<string, object>());

            //            Im_Ex_Event?.Invoke(null, scanCode);

            //            return;
            //        }
            //    }
            //    else
            //    {
            //        SendFeedBackToClient(client, Common.RETURNCHECKOUT, Common.FAIL, "Thông tin chưa hợp lệ!", new Dictionary<string, object>()); return;
            //    }
            //}
        }

        private void SendFeedBackToClient(ConnectedObject client, string cmdtype, string returnresult, string resultmess, Dictionary<string, object> data)
        {
            Dictionary<string, object> HeaderSEND = new()
            {
                { Common.CMDTYPE, cmdtype },

                { Common.RETURNRESULT, returnresult },

                { Common.RESULTMESS, resultmess }
            };

            SocketMessage sendingMess = new()
            {
                Command = HeaderSEND,

                Data = new List<Dictionary<string, object>>() { data }
            };
            string datasending = JsonConvert.SerializeObject(sendingMess);

            SendReply(client, datasending);
        }



        // Create PNK Infor sending to Handy
        private List<Dictionary<string, object>> CreateNVLofPNK_SocketData(List<NVLofPhieuNhapKho> nvlofpnks)
        {
            List<Dictionary<string, object>> datas = new();

            List<LenhNhapKho> dsanhLNKs = new();

            foreach (var nvlpnk in nvlofpnks)
            {
                foreach (var lnk in nvlpnk.DSLenhNKs)
                {
                    dsanhLNKs.Add(lnk);
                }
            }

            string maPNK = SQLServerServices.GetMaPhieuNhapKhoByID(nvlofpnks.FirstOrDefault()?.PNKID.Value);

            foreach (var lenh in dsanhLNKs)
            {
                _ = int.TryParse(lenh.PNKID.Value?.ToString(), out int pnkid) ? pnkid : -1;
                _ = int.TryParse(lenh.LenhNKID.Value?.ToString(), out int lnkid) ? lnkid : -1;
                string tennvl = lenh.TargetNgLieu.TenNVL.Value?.ToString() ?? string.Empty;

                string mavitri = lenh.TargertVitri.MaViTri.Value?.ToString() ?? string.Empty;
                _ = int.TryParse(lenh.LNKSoLuong.Value?.ToString(), out int soluongnhap) ? soluongnhap : 0;
                _ = int.TryParse(lenh.LNKIsDone.Value?.ToString(), out int lnkstatus) ? lnkstatus : -1;

                string lnkKey = $"LNK{dsanhLNKs.IndexOf(lenh) + 1}";

                Dictionary<string, object> lnkData = new();

                Dictionary<string, object> lnkdetails = new()
                    {
                        {Common.PNK_MNVL, tennvl }, {Common.PNK_MVT, mavitri }, {Common.PNKID, pnkid }, {Common.PNK_MPNK, maPNK},
                        {Common.PNK_LNKSL, soluongnhap }, {Common.LNKIsDone, lnkstatus }, {Common.PNK_LNKID, lnkid}
                    };

                lnkData.Add(lnkKey, lnkdetails);

                datas.Add(lnkData);
            }

            return datas;
        }

        // Handle require load phieu nhap kho detail from client
        private void HandleLoadPhieuNhapKhoDetails(SocketMessage? mess, ConnectedObject client)
        {
            if (mess != null)
            {
                string maPNK = mess.Data.FirstOrDefault()?[Common.PNK_MPNK].ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(maPNK)) { return; }

                // Load PNK details
                PhieuNhapKho phieuNhapKho = SQLServerServices.GetPhieuNhapKhoByMaPhieu(maPNK);

                if (phieuNhapKho.PNKID.Value == null)
                {
                    SendFeedBackToClient(client, Common.PNK_RETURN, Common.FAIL, "Không tồn tại phiếu nhập kho!", new Dictionary<string, object>());
                }
                else
                {
                    // Sending list NVLofPNK
                    List<Dictionary<string, object>> NVLofPNKdatas = CreateNVLofPNK_SocketData(phieuNhapKho.DSNVLofPNKs);

                    SendPhieuXuatKhoDataToClient(client, Common.PNK_RETURN, Common.SUCCESS, string.Empty, NVLofPNKdatas);
                }
            }
        }

        // Handle excute lenh nhap kho
        private void HandleExcuteLenhNhapKho(SocketMessage? mess, ConnectedObject client)
        {
            if (mess != null)
            {
                string maPNK = mess.Data.FirstOrDefault()?[Common.PNK_MPNK].ToString() ?? string.Empty;
                string maVitri = mess.Data.FirstOrDefault()?[Common.PNK_MVT].ToString() ?? string.Empty;
                string maNVL = mess.Data.FirstOrDefault()?[Common.PNK_MNVL].ToString() ?? string.Empty;
                int lnkSoluong = int.TryParse(mess.Data.FirstOrDefault()?[Common.PNK_LNKSL].ToString(), out int lnksl) ? lnksl : 0;
                object? lnkid = mess.Data.FirstOrDefault()?[Common.PNK_LNKID];

                if (string.IsNullOrWhiteSpace(maPNK)) { return; }
                if (string.IsNullOrWhiteSpace(maVitri)) { return; }
                if (string.IsNullOrWhiteSpace(maNVL)) { return; }
                if (lnkSoluong == 0) { return; }

                // Load lenh nhap kho by ID
                LenhNhapKho scanLNK = SQLServerServices.GetLenhNhapKhoByID(lnkid);

                // Asign so luong from Handy
                scanLNK.LNKSoLuong.Value = lnkSoluong;

                if (scanLNK.LenhNKID.Value == null)
                {
                    SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "Không tồn tại lệnh nhập kho!", new Dictionary<string, object>());
                }
                else
                {
                    // Handle lenh nhap kho

                    // Check trang thai lenh (da hoan thanh hay chua)
                    _ = int.TryParse(scanLNK.LNKIsDone.Value?.ToString(), out int scanlnkIsdone) ? scanlnkIsdone : -1;

                    if (scanlnkIsdone != 0)
                    {
                        SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "Không thể nhập (lệnh đã hoàn thành trước đó)!", new Dictionary<string, object>());
                        return;
                    }

                    // Kiem tra so luong them vao
                    int soluongThemvao = int.TryParse(scanLNK.LNKSoLuong.Value?.ToString(), out int slthem) ? slthem : 0;

                    if (soluongThemvao == 0)
                    {
                        SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "Số lượng không hợp lệ!", new Dictionary<string, object>());
                        return;
                    }

                    // Kiem tra xem nvl da ton tai o vitri hay chua
                    ViTriofNVL viTriofNVL = SQLServerServices.GetViTriOfNgVatLieuByNVLid_VTid(scanLNK.NVLID.Value, scanLNK.VTID.Value);

                    // NVL da ton tai o vitri nay --> Update so luong
                    if (viTriofNVL != null && viTriofNVL.VTofNVLID.Value != null)
                    {
                        int soluongHientaivitri = int.TryParse(viTriofNVL?.VTNVLSoLuong.Value?.ToString(), out int slhc) ? slhc : 0;

                        // gan so luong sau khi nhap cho vi tri da luu
                        int newtonkhotaivitri = soluongHientaivitri + soluongThemvao;

                        // Update so luong vi tri da co cua nvl
                        (int updateVTofNVLresult, string updateVTofNVLerror) = SQLServerServices.UpdateSoluongNgVatLieuById(viTriofNVL?.VTofNVLID.Value, newtonkhotaivitri);

                        if (updateVTofNVLresult == -1)
                        {
                            SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "LNK Lỗi: (0001)!", new Dictionary<string, object>());
                            return;
                        }

                        // Update lenh nhap kho status
                        (int updatelnkResult, string updatelnkError) = SQLServerServices.UpdateLenhNhapKhoStatus(scanLNK.LenhNKID.Value, 1);

                        if (updatelnkResult == -1)
                        {
                            SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "LNK Lỗi: (0002)!", new Dictionary<string, object>());
                            return;
                        }
                    }
                    else // NVL chua co o vitri --> Them moi
                    {
                        // Tao moi vitriofNVL
                        ViTriofNVL newviTriofNVL = new()
                        {
                            VTID = { Value = scanLNK.VTID.Value },
                            NVLID = { Value = scanLNK.NVLID.Value },
                            VTNVLSoLuong = { Value = soluongThemvao }
                        };

                        // Them vitriofNVL moi vao database
                        (int InsertVTofNVLstatus, string InsertVTofNVLerror) = SQLServerServices.InsertNewViTriOfNgVatLieu(newviTriofNVL);

                        if (InsertVTofNVLstatus == -1)
                        {
                            SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "LNK Lỗi: (0003)!", new Dictionary<string, object>());
                            return;
                        }

                        // Update lenh nhap kho status
                        (int updatelnkResult, string updatelnkError) = SQLServerServices.UpdateLenhNhapKhoStatus(scanLNK.LenhNKID.Value, 1);

                        if (updatelnkResult == -1)
                        {
                            SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "LNK Lỗi: (0004)!", new Dictionary<string, object>());
                            return;
                        }
                    }

                    // update status to UI
                    scanLNK.LNKIsDone.Value = 1;

                    // Logging nhap kho
                    HistoryXNKho logNhapKho = new HistoryXNKho()
                    {
                        LogLoaiPhieu = { Value = Common.LogTypePNK },
                        LogMaPhieu = { Value = maPNK },
                        LogMaViTri = { Value = maVitri },
                        LogNgThucHien = { Value = "Khang" },
                        LogSoLuong = { Value = scanLNK.LNKSoLuong.Value },
                        LogTonKhoTruoc = { Value = scanLNK.TargetNgLieu.TonKho },
                        LogTonKhoSau = { Value = scanLNK.TargetNgLieu.TonKho + soluongThemvao },
                        LogTenNVL = { Value = scanLNK.TargetNgLieu.TenNVL.Value },
                        LogThoiDiem = { Value = DateTime.Now },
                        NVLID = { Value = scanLNK.NVLID.Value },
                        VTID = { Value = scanLNK.VTID.Value }
                    };
                    // Insert logging to Database
                    (int logId, string logErr) = SQLServerServices.InsertLogingXNKho(logNhapKho);

                    if (logId == -1)
                    {

                    }

                    // Feedback nhap kho thanh cong
                    SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.SUCCESS, $"Đã nhập kho nguyên liệu: \n {maNVL} \n Số lượng : {lnkSoluong} (pcs)", new Dictionary<string, object>());
                }
            }
        }

        private void SendPhieuXuatKhoDataToClient(ConnectedObject client, string cmdtype, string returnresult, string resultmess, List<Dictionary<string, object>> data)
        {
            Dictionary<string, object> HeaderSEND = new()
            {
                { Common.CMDTYPE, cmdtype },

                { Common.RETURNRESULT, returnresult },

                { Common.RESULTMESS, resultmess }
            };

            SocketMessage sendingMess = new()
            {
                Command = HeaderSEND
            };

            sendingMess.Data.AddRange(data);

            string datasending = JsonConvert.SerializeObject(sendingMess);

            SendReply(client, datasending);
        }
    }
}
