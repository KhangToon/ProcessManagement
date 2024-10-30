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
using Microsoft.AspNetCore.Components.Routing;
using ProcessManagement.Models;

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
            /////////////////////
            /////////////// Modify with new method like DialogHandleNhap/XuatKho


            string clientMESS = client.IncomingMessage.Replace(Common.socketEndKey, "").ToString();

            SocketMessage? ClientMesage = JsonConvert.DeserializeObject<SocketMessage>(clientMESS);

            string CMDTYPE = ClientMesage?.Command[Common.CMDTYPE]?.ToString() ?? string.Empty;

            // NHAP KHO
            if (CMDTYPE == Common.PNK_LOAD)
            {
                HandleLoadPhieuNhapKhoDetails(ClientMesage, client);
            }
            else if (CMDTYPE == Common.PNK_HANDLE_LNK)
            {
                HandleExcuteLenhNhapKho(ClientMesage, client);
            }
            else if (CMDTYPE == Common.PNK_HANDLE_LNK_MANUAL)
            {
                HandleExcuteLenhNhapKho_Manual(ClientMesage, client);
            }

            // XUAT KHO
            if (CMDTYPE == Common.PXK_LOAD)
            {
                HandleLoadPhieuXuatKhoDetails(ClientMesage, client);
            }
            else if (CMDTYPE == Common.PXK_HANDLE_LXK)
            {
                HandleExcuteLenhXuatKho(ClientMesage, client);
            }
            else if (CMDTYPE == Common.PXK_HANDLE_LXK_MANUAL)
            {
                HandleExcuteLenhXuatKho_Manual(ClientMesage, client);
            }

            // CHECKING
            if (CMDTYPE == Common.CHECK_LOAD)
            {
                HandleCheckingDataDetails(ClientMesage, client);
            }
        }

        public string ConvertData(List<IDictionary<string, object>> data)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            return jsonData;
        }


        #region BT-HANDY COMMUNICATION

        #region NHAP KHO

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

                    SendFeedBackToClient(client, Common.PNK_RETURN, Common.SUCCESS, string.Empty, NVLofPNKdatas);
                }
            }
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

        // Handle excute lenh nhap kho - Auto
        private void HandleExcuteLenhNhapKho(SocketMessage? mess, ConnectedObject client) // ErrorKey: HELNK
        {
            if (mess != null)
            {
                string maPNK = mess.Data.FirstOrDefault()?[Common.PNK_MPNK].ToString() ?? string.Empty;
                string maVitri = mess.Data.FirstOrDefault()?[Common.PNK_MVT].ToString() ?? string.Empty;
                string maNVL = mess.Data.FirstOrDefault()?[Common.PNK_MNVL].ToString() ?? string.Empty;
                int soluongnhapfromClient = int.TryParse(mess.Data.FirstOrDefault()?[Common.PNK_LNKSL].ToString(), out int lnksl) ? lnksl : 0;
                object? lnkid = mess.Data.FirstOrDefault()?[Common.PNK_LNKID];

                if (string.IsNullOrWhiteSpace(maPNK)) { return; }
                if (string.IsNullOrWhiteSpace(maVitri)) { return; }
                if (string.IsNullOrWhiteSpace(maNVL)) { return; }
                if (soluongnhapfromClient == 0) { return; }

                // Load lenh nhap kho by ID
                LenhNhapKho savedLNK = SQLServerServices.GetLenhNhapKhoByID(lnkid);

                if (savedLNK.LenhNKID.Value == null)
                {
                    SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "Không tồn tại lệnh nhập kho!", new Dictionary<string, object>());
                }
                else
                {
                    // Handle lenh nhap kho

                    // Check trang thai lenh (da hoan thanh hay chua)
                    _ = int.TryParse(savedLNK.LNKIsDone.Value?.ToString(), out int scanlnkIsdone) ? scanlnkIsdone : -1;
                    if (scanlnkIsdone != 0)
                    {
                        SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "Không thể nhập (lệnh đã hoàn thành trước đó)!", new Dictionary<string, object>());
                        return;
                    }

                    // Kiem tra so luong them vao
                    int soluongThemvao = int.TryParse(savedLNK.LNKSoLuong.Value?.ToString(), out int slthem) ? slthem : 0;
                    if (soluongThemvao == 0)
                    {
                        SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "Số lượng không hợp lệ!", new Dictionary<string, object>());
                        return;
                    }

                    // Kiem tra xem nvl da ton tai o vitri hay chua
                    ViTriofNVL viTriofNVL = SQLServerServices.GetViTriOfNgVatLieuByNVLid_VTid(savedLNK.NVLID.Value, savedLNK.VTID.Value);

                    // Kiem tra qua so luong con trong cua vi tri
                    int soluongcontrongvitri = savedLNK.TargertVitri.SLConTrong;
                    if (soluongThemvao > soluongcontrongvitri)
                    {
                        SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, $"Qúa số lượng trống của vị trí ({soluongcontrongvitri})", new Dictionary<string, object>());
                        return;
                    }

                    // NVL da ton tai o vitri nay --> Update so luong
                    if (viTriofNVL != null && viTriofNVL.VTofNVLID.Value != null)
                    {
                        int soluongHiencotaivitri = int.TryParse(viTriofNVL?.VTNVLSoLuong.Value?.ToString(), out int slhc) ? slhc : 0;

                        // gan so luong sau khi nhap cho vi tri da luu
                        int newtonkhotaivitri = soluongHiencotaivitri + soluongThemvao;

                        // Update so luong vi tri da co cua nvl
                        (int updateVTofNVLresult, string updateVTofNVLerror) = SQLServerServices.UpdateSoluongNgVatLieuById(viTriofNVL?.VTofNVLID.Value, newtonkhotaivitri);

                        if (updateVTofNVLresult == -1)
                        {
                            SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "LNK Lỗi: (HELNK0001)!", new Dictionary<string, object>());
                            return;
                        }

                        // Update lenh nhap kho status
                        (int updatelnkResult, string updatelnkError) = SQLServerServices.UpdateLenhNhapKhoStatus(savedLNK.LenhNKID.Value, 1);

                        if (updatelnkResult == -1)
                        {
                            SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "LNK Lỗi: (HELNK0002)!", new Dictionary<string, object>());
                            return;
                        }
                    }
                    else // NVL chua co o vitri --> Them moi
                    {
                        // Tao moi vitriofNVL
                        ViTriofNVL newviTriofNVL = new()
                        {
                            VTID = { Value = savedLNK.VTID.Value },
                            NVLID = { Value = savedLNK.NVLID.Value },
                            VTNVLSoLuong = { Value = soluongThemvao }
                        };

                        // Them vitriofNVL moi vao database
                        (int InsertVTofNVLstatus, string InsertVTofNVLerror) = SQLServerServices.InsertNewViTriOfNgVatLieu(newviTriofNVL);

                        if (InsertVTofNVLstatus == -1)
                        {
                            SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "LNK Lỗi: (HELNK0003)!", new Dictionary<string, object>());
                            return;
                        }

                        // Update lenh nhap kho status
                        (int updatelnkResult, string updatelnkError) = SQLServerServices.UpdateLenhNhapKhoStatus(savedLNK.LenhNKID.Value, 1);

                        if (updatelnkResult == -1)
                        {
                            SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "LNK Lỗi: (HELNK0004)!", new Dictionary<string, object>());
                            return;
                        }
                    }

                    // update status to UI
                    savedLNK.LNKIsDone.Value = 1;

                    // Get nguoi nhap kho
                    string nguoiNhapkho = SQLServerServices.GetNguoiTaoPhieuNhapKhoByID(savedLNK.PNKID.Value);


                    // Logging nhap kho
                    HistoryXNKho logNhapKho = new HistoryXNKho()
                    {
                        LogLoaiPhieu = { Value = Common.LogTypePNK },
                        LogMaPhieu = { Value = maPNK },
                        LogMaViTri = { Value = maVitri },
                        LogNgThucHien = { Value = nguoiNhapkho },
                        LogSoLuong = { Value = savedLNK.LNKSoLuong.Value },
                        LogTonKhoTruoc = { Value = savedLNK.TargetNgLieu.TonKho },
                        LogTonKhoSau = { Value = savedLNK.TargetNgLieu.TonKho + soluongThemvao },
                        LogTenNVL = { Value = savedLNK.TargetNgLieu.TenNVL.Value },
                        LogThoiDiem = { Value = DateTime.Now },
                        NVLID = { Value = savedLNK.NVLID.Value },
                        VTID = { Value = savedLNK.VTID.Value }
                    };
                    // Insert logging to Database
                    (int logId, string logErr) = SQLServerServices.InsertLogingXNKho(logNhapKho);

                    if (logId == -1)
                    {

                    }

                    // Feedback nhap kho thanh cong
                    SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.SUCCESS, $"Đã nhập kho nguyên liệu: \n {maNVL} \n Số lượng : {soluongnhapfromClient} (pcs)", new Dictionary<string, object>());
                }
            }
        }

        // Handle excute lenh nhap kho - Manual
        private void HandleExcuteLenhNhapKho_Manual(SocketMessage? mess, ConnectedObject client) // ErrorKey: HELNKM
        {
            if (mess != null)
            {
                string maVitri = mess.Data.FirstOrDefault()?[Common.PNK_MVT].ToString() ?? string.Empty;
                string maNVL = mess.Data.FirstOrDefault()?[Common.PNK_MNVL].ToString() ?? string.Empty;
                int soluongnhapfromClient = int.TryParse(mess.Data.FirstOrDefault()?[Common.PNK_LNKSL].ToString(), out int lnksl) ? lnksl : 0;

                if (string.IsNullOrWhiteSpace(maVitri)) { return; }
                if (string.IsNullOrWhiteSpace(maNVL)) { return; }
                if (soluongnhapfromClient == 0) { return; }

                // Handle lenh nhap kho

                // Get vitri can nhap kho
                VitriLuuTru targetVitri = SQLServerServices.GetViTriLuuTruByMaVitri(maVitri);
                // Get nvl can nhap kho
                NguyenVatLieu targetNVL = SQLServerServices.GetNguyenVatLieuByTenNVL(maNVL);

                // Kiem tra vi tri co ton tai 
                if (targetVitri.VTID.Value == null)
                {
                    SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "Vị trí lưu kho không tồn tại!", new Dictionary<string, object>());
                    return;
                }

                // Kiem tra NVL co ton tai
                if (targetNVL.NVLID.Value == null)
                {
                    SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "Nguyên vật liệu nhập kho không tồn tại!", new Dictionary<string, object>());
                    return;
                }

                // Kiem tra qua so luong con trong cua vi tri
                int soluongcontrongvitri = targetVitri.SLConTrong;
                if (soluongnhapfromClient > soluongcontrongvitri)
                {
                    SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, $"Qúa số lượng trống của vị trí ({soluongcontrongvitri})", new Dictionary<string, object>());
                    return;
                }

                // Kiem tra xem nvl da ton tai o vitri hay chua
                ViTriofNVL viTriofNVL = SQLServerServices.GetViTriOfNgVatLieuByNVLid_VTid(targetNVL.NVLID.Value, targetVitri.VTID.Value);

                // NVL da ton tai o vitri nay --> Update so luong
                if (viTriofNVL != null && viTriofNVL.VTofNVLID.Value != null)
                {
                    int soluongHiencotaivitri = int.TryParse(viTriofNVL?.VTNVLSoLuong.Value?.ToString(), out int slhc) ? slhc : 0;

                    // gan so luong sau khi nhap cho vi tri da luu
                    int newtonkhotaivitri = soluongHiencotaivitri + soluongnhapfromClient;

                    // Update so luong vi tri da co cua nvl
                    (int updateVTofNVLresult, string updateVTofNVLerror) = SQLServerServices.UpdateSoluongNgVatLieuById(viTriofNVL?.VTofNVLID.Value, newtonkhotaivitri);

                    if (updateVTofNVLresult == -1)
                    {
                        SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "LNK Lỗi: (HELNKM0001)!", new Dictionary<string, object>());
                        return;
                    }
                }
                else // NVL chua co o vitri --> Them moi
                {
                    // Tao moi vitriofNVL
                    ViTriofNVL newviTriofNVL = new()
                    {
                        VTID = { Value = targetVitri.VTID.Value },
                        NVLID = { Value = targetNVL.NVLID.Value },
                        VTNVLSoLuong = { Value = soluongnhapfromClient }
                    };

                    // Them vitriofNVL moi vao database
                    (int InsertVTofNVLstatus, string InsertVTofNVLerror) = SQLServerServices.InsertNewViTriOfNgVatLieu(newviTriofNVL);

                    if (InsertVTofNVLstatus == -1)
                    {
                        SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.FAIL, "LNK Lỗi: (HELNKM0002)!", new Dictionary<string, object>());
                        return;
                    }
                }

                // Logging nhap kho
                HistoryXNKho logNhapKho = new HistoryXNKho()
                {
                    LogLoaiPhieu = { Value = Common.LogTypePNK_Manual },
                    LogMaViTri = { Value = maVitri },
                    LogNgThucHien = { Value = string.Empty },
                    LogSoLuong = { Value = soluongnhapfromClient },
                    LogTonKhoTruoc = { Value = targetNVL.TonKho },
                    LogTonKhoSau = { Value = targetNVL.TonKho + soluongnhapfromClient },
                    LogTenNVL = { Value = targetNVL.TenNVL.Value },
                    LogThoiDiem = { Value = DateTime.Now },
                    NVLID = { Value = targetNVL.NVLID.Value },
                    VTID = { Value = targetVitri.VTID.Value }
                };
                // Insert logging to Database
                (int logId, string logErr) = SQLServerServices.InsertLogingXNKho(logNhapKho);

                if (logId == -1)
                {
                    SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.SUCCESS, $"Đã nhập kho nguyên liệu: \n {maNVL} \n Số lượng : {soluongnhapfromClient} (pcs) \n\n Loggin lỗi: {logErr}", new Dictionary<string, object>());
                    return;
                }

                // Feedback nhap kho thanh cong
                SendFeedBackToClient(client, Common.PNK_HANDLE_LNK_RETURN, Common.SUCCESS, $"Đã nhập kho nguyên liệu: \n {maNVL} \n Số lượng : {soluongnhapfromClient} (pcs)", new Dictionary<string, object>());
            }
        }

        #endregion

        #region XUAT KHO

        // Handle require load phieu xuat kho detail from client
        private void HandleLoadPhieuXuatKhoDetails(SocketMessage? mess, ConnectedObject client)
        {
            if (mess != null)
            {
                string maPXK = mess.Data.FirstOrDefault()?[Common.PXK_MPXK].ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(maPXK)) { return; }

                // Load PXK details
                PhieuXuatKho phieuXuatKho = SQLServerServices.GetPhieuXuatKhoByMaPhieu(maPXK);

                if (phieuXuatKho.PXKID.Value == null)
                {
                    SendFeedBackToClient(client, Common.PXK_RETURN, Common.FAIL, "Không tồn tại phiếu xuất kho!", new Dictionary<string, object>());
                }
                else
                {
                    // Sending list NVLofPXK
                    List<Dictionary<string, object>> NVLofPXKdatas = CreateNVLofPXK_SocketData(phieuXuatKho.DSNVLofPXKs);

                    SendFeedBackToClient(client, Common.PXK_RETURN, Common.SUCCESS, string.Empty, NVLofPXKdatas);
                }
            }
        }

        // Create PXK Info sending to Handy
        private List<Dictionary<string, object>> CreateNVLofPXK_SocketData(List<NVLofPhieuXuatKho> nvlofpxks)
        {
            List<Dictionary<string, object>> datas = new();

            List<LenhXuatKho> dsanhLXKs = new();

            foreach (var nvlpxk in nvlofpxks)
            {
                foreach (var lxk in nvlpxk.DSLenhXKs)
                {
                    dsanhLXKs.Add(lxk);
                }
            }

            string maPXK = SQLServerServices.GetMaPhieuXuatKhoByID(nvlofpxks.FirstOrDefault()?.PXKID.Value);

            foreach (var lenh in dsanhLXKs)
            {
                _ = int.TryParse(lenh.PXKID.Value?.ToString(), out int pxkid) ? pxkid : -1;

                _ = int.TryParse(lenh.LenhXKID.Value?.ToString(), out int lxkid) ? lxkid : -1;

                lenh.ViTriofNVL.NgLieuInfor = SQLServerServices.GetNguyenVatLieuByID(lenh.NVLID.Value);

                string tennvl = lenh.ViTriofNVL.NgLieuInfor.TenNVL.Value?.ToString() ?? string.Empty;

                string mavitri = lenh.ViTriofNVL.VitriInfor.MaViTri.Value?.ToString() ?? string.Empty;

                _ = int.TryParse(lenh.LXKSoLuong.Value?.ToString(), out int soluongxuat) ? soluongxuat : 0;

                _ = int.TryParse(lenh.LXKIsDone.Value?.ToString(), out int lxkstatus) ? lxkstatus : -1;

                string lxkKey = $"LXK{dsanhLXKs.IndexOf(lenh) + 1}";

                Dictionary<string, object> lxkData = new();

                Dictionary<string, object> lxkdetails = new()
                {
                    {Common.PXK_MNVL, tennvl }, {Common.PXK_MVT, mavitri }, {Common.PXKID, pxkid }, {Common.PXK_MPXK, maPXK},
                    {Common.PXK_LXKSL, soluongxuat }, {Common.LXKIsDone, lxkstatus }, {Common.PXK_LXKID, lxkid}
                };
                lxkData.Add(lxkKey, lxkdetails);

                datas.Add(lxkData);
            }

            return datas;
        }

        // Handle excute lenh xuat kho - Auto
        private void HandleExcuteLenhXuatKho(SocketMessage? mess, ConnectedObject client) // ErrorKey: HELXK
        {
            if (mess != null)
            {
                string maPXK = mess.Data.FirstOrDefault()?[Common.PXK_MPXK].ToString() ?? string.Empty;
                string maVitri = mess.Data.FirstOrDefault()?[Common.PXK_MVT].ToString() ?? string.Empty;
                string maNVL = mess.Data.FirstOrDefault()?[Common.PXK_MNVL].ToString() ?? string.Empty;
                int soluongxuatfromClient = int.TryParse(mess.Data.FirstOrDefault()?[Common.PXK_LXKSL].ToString(), out int lxksl) ? lxksl : 0;
                object? lxkid = mess.Data.FirstOrDefault()?[Common.PXK_LXKID];

                if (string.IsNullOrWhiteSpace(maPXK)) { return; }
                if (string.IsNullOrWhiteSpace(maVitri)) { return; }
                if (string.IsNullOrWhiteSpace(maNVL)) { return; }
                if (soluongxuatfromClient == 0) { return; }

                // Load lenh xuat kho by ID
                LenhXuatKho savedLXK = SQLServerServices.GetLenhXuatKhoByID(lxkid);

                if (savedLXK.LenhXKID.Value == null)
                {
                    SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, "Không tồn tại lệnh xuất kho!", new Dictionary<string, object>());
                }
                else
                {
                    // Handle lenh xuat kho

                    // Check trang thai lenh (da hoan thanh hay chua)
                    _ = int.TryParse(savedLXK.LXKIsDone.Value?.ToString(), out int scanlxkIsdone) ? scanlxkIsdone : -1;
                    if (scanlxkIsdone != 0)
                    {
                        SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, "Không thể xuất (lệnh đã hoàn thành trước đó)!", new Dictionary<string, object>());
                        return;
                    }

                    // Kiem tra so luong xuat ra
                    int soluongXuatra = int.TryParse(savedLXK.LXKSoLuong.Value?.ToString(), out int slxuat) ? slxuat : 0;
                    if (soluongXuatra == 0)
                    {
                        SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, "Số lượng không hợp lệ!", new Dictionary<string, object>());
                        return;
                    }

                    // Kiem tra xem nvl co ton tai o vitri hay khong
                    ViTriofNVL viTriofNVL = SQLServerServices.GetViTriOfNgVatLieuByNVLid_VTid(savedLXK.NVLID.Value, savedLXK.VTID.Value);

                    if (viTriofNVL == null || viTriofNVL.VTofNVLID.Value == null)
                    {
                        SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, "Không tồn tại nguyên vật liệu tại vị trí này!", new Dictionary<string, object>());
                        return;
                    }

                    int soluongHiencotaivitri = int.TryParse(viTriofNVL?.VTNVLSoLuong.Value?.ToString(), out int slhc) ? slhc : 0;

                    // Kiem tra so luong xuat co vuot qua so luong hien tai khong
                    if (soluongXuatra > soluongHiencotaivitri)
                    {
                        SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, $"Số lượng xuất vượt quá số lượng hiện tại ({soluongHiencotaivitri})", new Dictionary<string, object>());
                        return;
                    }

                    // Tinh so luong con lai sau khi xuat
                    int newtonkhotaivitri = soluongHiencotaivitri - soluongXuatra;

                    // Update so luong vi tri cua nvl
                    (int updateVTofNVLresult, string updateVTofNVLerror) = SQLServerServices.UpdateSoluongNgVatLieuById(viTriofNVL?.VTofNVLID.Value, newtonkhotaivitri);

                    if (updateVTofNVLresult == -1)
                    {
                        SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, "LXK Lỗi: (HELXK0001)!", new Dictionary<string, object>());
                        return;
                    }

                    // Update lenh xuat kho status
                    (int updatelxkResult, string updatelxkError) = SQLServerServices.UpdateLenhXuatKhoStatus(savedLXK.LenhXKID.Value, 1);

                    if (updatelxkResult == -1)
                    {
                        SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, "LXK Lỗi: (HELXK0002)!", new Dictionary<string, object>());
                        return;
                    }

                    // update status to UI
                    savedLXK.LXKIsDone.Value = 1;

                    // Get nguoi xuat kho
                    string nguoiXuatkho = SQLServerServices.GetNguoiTaoPhieuXuatKhoByID(savedLXK.PXKID.Value);

                    // Logging xuat kho
                    HistoryXNKho logXuatKho = new HistoryXNKho()
                    {
                        LogLoaiPhieu = { Value = Common.LogTypePXK },
                        LogMaPhieu = { Value = maPXK },
                        LogMaViTri = { Value = maVitri },
                        LogNgThucHien = { Value = nguoiXuatkho },
                        LogSoLuong = { Value = savedLXK.LXKSoLuong.Value },
                        LogTonKhoTruoc = { Value = savedLXK.ViTriofNVL.NgLieuInfor.TonKho },
                        LogTonKhoSau = { Value = savedLXK.ViTriofNVL.NgLieuInfor.TonKho - soluongXuatra },
                        LogTenNVL = { Value = savedLXK.ViTriofNVL.NgLieuInfor.TenNVL.Value },
                        LogThoiDiem = { Value = DateTime.Now },
                        NVLID = { Value = savedLXK.NVLID.Value },
                        VTID = { Value = savedLXK.VTID.Value }
                    };
                    // Insert logging to Database
                    (int logId, string logErr) = SQLServerServices.InsertLogingXNKho(logXuatKho);

                    if (logId == -1)
                    {
                        SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.SUCCESS, $"Đã xuất kho nguyên liệu: \n {maNVL} \n Số lượng : {soluongxuatfromClient} (pcs) \n\n Logging Error: {logErr}", new Dictionary<string, object>());
                        return;
                    }

                    // Feedback xuat kho thanh cong
                    SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.SUCCESS, $"Đã xuất kho nguyên liệu: \n {maNVL} \n Số lượng : {soluongxuatfromClient} (pcs)", new Dictionary<string, object>());
                }
            }
        }

        // Handle execute lenh xuat kho - Manual
        private void HandleExcuteLenhXuatKho_Manual(SocketMessage? mess, ConnectedObject client) // ErrorKey: HELXKM
        {
            if (mess != null)
            {
                string maVitri = mess.Data.FirstOrDefault()?[Common.PXK_MVT].ToString() ?? string.Empty;
                string maNVL = mess.Data.FirstOrDefault()?[Common.PXK_MNVL].ToString() ?? string.Empty;
                int soluongxuatfromClient = int.TryParse(mess.Data.FirstOrDefault()?[Common.PXK_LXKSL].ToString(), out int lxksl) ? lxksl : 0;

                if (string.IsNullOrWhiteSpace(maVitri)) { return; }
                if (string.IsNullOrWhiteSpace(maNVL)) { return; }
                if (soluongxuatfromClient == 0) { return; }

                // Handle lenh xuat kho

                // Get vitri can xuat kho
                VitriLuuTru targetVitri = SQLServerServices.GetViTriLuuTruByMaVitri(maVitri);
                // Get nvl can xuat kho
                NguyenVatLieu targetNVL = SQLServerServices.GetNguyenVatLieuByTenNVL(maNVL);

                // Kiem tra vi tri co ton tai 
                if (targetVitri.VTID.Value == null)
                {
                    SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, "Vị trí lưu kho không tồn tại!", new Dictionary<string, object>());
                    return;
                }

                // Kiem tra NVL co ton tai
                if (targetNVL.NVLID.Value == null)
                {
                    SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, "Nguyên vật liệu xuất kho không tồn tại!", new Dictionary<string, object>());
                    return;
                }

                // Kiem tra xem nvl co ton tai o vitri hay khong
                ViTriofNVL viTriofNVL = SQLServerServices.GetViTriOfNgVatLieuByNVLid_VTid(targetNVL.NVLID.Value, targetVitri.VTID.Value);

                if (viTriofNVL == null || viTriofNVL.VTofNVLID.Value == null)
                {
                    SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, "Không tồn tại nguyên vật liệu tại vị trí này!", new Dictionary<string, object>());
                    return;
                }

                int soluongHiencotaivitri = int.TryParse(viTriofNVL?.VTNVLSoLuong.Value?.ToString(), out int slhc) ? slhc : 0;

                // Kiem tra so luong xuat co vuot qua so luong hien tai khong
                if (soluongxuatfromClient > soluongHiencotaivitri)
                {
                    SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, $"Số lượng xuất vượt quá số lượng hiện có ở vị trí ({maVitri}) ({soluongHiencotaivitri})", new Dictionary<string, object>());
                    return;
                }

                // Tinh so luong con lai sau khi xuat
                int newtonkhotaivitri = soluongHiencotaivitri - soluongxuatfromClient;

                // Update so luong vi tri cua nvl
                (int updateVTofNVLresult, string updateVTofNVLerror) = SQLServerServices.UpdateSoluongNgVatLieuById(viTriofNVL?.VTofNVLID.Value, newtonkhotaivitri);

                if (updateVTofNVLresult == -1)
                {
                    SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.FAIL, "LXK Lỗi: (HELXKM0001)!", new Dictionary<string, object>());
                    return;
                }

                // Logging xuat kho
                HistoryXNKho logXuatKho = new HistoryXNKho()
                {
                    LogLoaiPhieu = { Value = Common.LogTypePXK_Manual },
                    LogMaViTri = { Value = maVitri },
                    LogNgThucHien = { Value = string.Empty },
                    LogSoLuong = { Value = soluongxuatfromClient },
                    LogTonKhoTruoc = { Value = targetNVL.TonKho },
                    LogTonKhoSau = { Value = targetNVL.TonKho - soluongxuatfromClient },
                    LogTenNVL = { Value = targetNVL.TenNVL.Value },
                    LogThoiDiem = { Value = DateTime.Now },
                    NVLID = { Value = targetNVL.NVLID.Value },
                    VTID = { Value = targetVitri.VTID.Value }
                };
                // Insert logging to Database
                (int logId, string logErr) = SQLServerServices.InsertLogingXNKho(logXuatKho);

                if (logId == -1)
                {
                    SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.SUCCESS, $"Đã xuất kho nguyên liệu: \n {maNVL} \n Số lượng : {soluongxuatfromClient} (pcs) \n\n Loggin lỗi: {logErr}", new Dictionary<string, object>());
                    return;
                }

                // Feedback xuat kho thanh cong
                SendFeedBackToClient(client, Common.PXK_HANDLE_LXK_RETURN, Common.SUCCESS, $"Đã xuất kho nguyên liệu: \n {maNVL} \n Số lượng : {soluongxuatfromClient} (pcs)", new Dictionary<string, object>());
            }
        }

        #endregion

        #region CHECKING

        // Handle checking data details
        private void HandleCheckingDataDetails(SocketMessage? mess, ConnectedObject client)
        {
            if (mess != null)
            {
                string scanCode = mess.Data.FirstOrDefault()?[Common.CHECK_SCANCODE].ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(scanCode)) { return; }

                // Get NVL
                NguyenVatLieu targetNVL = SQLServerServices.GetNguyenVatLieuByTenNVL(scanCode);

                Dictionary<string, object> dataSend = new();

                if (targetNVL.NVLID.Value != null)
                {

                    // 1 - Add thong tin NVL
                    Dictionary<string, object> dataNVLDetails = new(); // thong tin nvl

                    var nvlDetailsItems = targetNVL?.GetPropertiesValues().Where(nvl => nvl.AlowDisplay == true).ToList();

                    // Add danh muc
                    string dmuckey = "Danh mục";
                    object dmucValue = targetNVL?.DanhMuc?.TenDanhMuc.Value?.ToString() ?? string.Empty;
                    dataNVLDetails.Add(dmuckey, dmucValue);

                    // Add ton kho
                    string tonkhoKey = "Tồn kho";
                    object tonkhoValue = $"{targetNVL?.TonKho ?? 0} ({targetNVL?.DonViTinh.Value?.ToString()})";
                    dataNVLDetails.Add(tonkhoKey, tonkhoValue);

                    // Add main details
                    if (nvlDetailsItems != null)
                    {
                        foreach (var nvldetail in nvlDetailsItems)
                        {
                            if (nvldetail != null)
                            {
                                string key = nvldetail.DisplayName;
                                object value = nvldetail.Value?.ToString()?.Trim() ?? string.Empty;

                                if (!String.IsNullOrEmpty(key))
                                {
                                    dataNVLDetails.Add(key, value);
                                }
                            }
                        }
                    }

                    // Add sub details
                    if (targetNVL?.DSThongTin.Count > 0)
                    {
                        var dsngvldetails = targetNVL?.DSThongTin;

                        if (dsngvldetails != null)
                        {
                            foreach (var nvldetail in dsngvldetails)
                            {
                                string key = nvldetail.LoaiThongTin?.TenLoaiThongTin.Value?.ToString()?.Trim() ?? string.Empty;
                                object value = nvldetail.GiaTri.Value?.ToString() ?? string.Empty;

                                if (!String.IsNullOrEmpty(key))
                                {
                                    dataNVLDetails.Add(key, value);
                                }
                            }
                        }
                    }

                    dataSend.Add(targetNVL?.TenNVL.Value?.ToString() ?? "###", dataNVLDetails);

                    // 2 - Add vitri luu kho
                    if (targetNVL != null && targetNVL.DSViTri.Count > 0)
                    {
                        Dictionary<string, object> dataNVLvitris = new(); // thong tin/vitri

                        int index = 0;

                        foreach (var vitriluu in targetNVL.DSViTri)
                        {
                            string vitriKey = vitriluu.VitriInfor.MaViTri.Value?.ToString() ?? string.Empty;
                            if (!String.IsNullOrEmpty(vitriKey))
                            {
                                _ = int.TryParse(vitriluu.VTNVLSoLuong.Value?.ToString(), out int soluongtaivitri) ? soluongtaivitri : 0;

                                object vitriValue = $"{soluongtaivitri} ({targetNVL.DonViTinh.Value?.ToString()})";

                                if (soluongtaivitri > 0)
                                {
                                    index++;
                                    vitriKey = index.ToString() + "____" + vitriKey;
                                    dataNVLvitris.Add(vitriKey, vitriValue);
                                }
                            }
                        }

                        if (dataNVLvitris.Count > 0) { dataSend.Add("DS vị trí lưu trữ", dataNVLvitris); };
                    }

                }
                else
                {
                    // Get thong tin vi tri
                    VitriLuuTru targetVT = SQLServerServices.GetViTriLuuTruByMaVitri(scanCode);
                    targetVT.DSachViTriofNVL = SQLServerServices.GetListViTriOfNgVatLieuByVTid(targetVT.VTID.Value);

                    if (targetVT.DSachViTriofNVL.Count > 0)
                    {
                        string mavitri = targetVT.MaViTri.Value?.ToString() ?? "###";

                        foreach (var nvl in targetVT.DSachViTriofNVL)
                        {
                            Dictionary<string, object> vitridetails = new();

                            nvl.NgLieuInfor = SQLServerServices.GetNguyenVatLieuByID(nvl.NVLID.Value);

                            string tennvl = nvl.NgLieuInfor.TenNVL.Value?.ToString() ?? string.Empty;

                            int soluongtaivitri = int.TryParse(nvl.VTNVLSoLuong.Value?.ToString(), out int sl) ? sl : 0;

                            if (soluongtaivitri > 0)
                            {
                                vitridetails.Add("SL tại vị trí", soluongtaivitri + $"({nvl.NgLieuInfor.DonViTinh.Value?.ToString()})");

                                if (!String.IsNullOrEmpty(tennvl))
                                {
                                    dataSend.Add(tennvl, vitridetails);
                                }
                            }
                        }
                    }
                }

                // Send feedback data to client
                if (dataSend.Count > 0)
                {
                    SendFeedBackToClient(client, Common.CHECK_LOAD_RETURN, Common.SUCCESS, string.Empty, dataSend);
                }
                else
                {
                    SendFeedBackToClient(client, Common.CHECK_LOAD_RETURN, Common.FAIL, "Không tìm thấy dữ liệu!", dataSend);
                }
            }
        }
        #endregion


        // Sending feedback kho to Client type 1
        private void SendFeedBackToClient(ConnectedObject client, string cmdtype, string returnresult, string resultmess, List<Dictionary<string, object>> data)
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

        // Sending feedback kho to Client type 2
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
        #endregion



    }
}
