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

            if (CMDTYPE == Common.CHECKOUT)
            {
                HandleCheckOut(ClientMesage, client);
            }
        }

        public string ConvertData(List<IDictionary<string, object>> data)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            return jsonData;
        }

        public void HandleCheckOut(SocketMessage? mess, ConnectedObject client)
        {
            if (mess != null && mess.Data != null)
            {
                string scanCode = mess.Data.FirstOrDefault()?[Common.SCANCODE]?.ToString() ?? string.Empty;

                int slOK = int.Parse(mess.Data.FirstOrDefault()?[Common.SLOK]?.ToString() ?? "0");

                int slNG = int.Parse(mess.Data.FirstOrDefault()?[Common.SLNG]?.ToString() ?? "0");

                if (scanCode != string.Empty && slOK > 0 && slNG >= 0)
                {
                    // Update sl NG OK masp
                    (int result, string err) = SQLServerServices.UpdateNVLmoiCongdoan(slOK, slNG, scanCode, null, Common.CurrentCDid, Common.CurrentKHSXid);

                    if (result == -1)
                    {
                        SendFeedBackToClient(client, Common.RETURNCHECKOUT, Common.FAIL, $"{err}", new Dictionary<string, object>()); return;
                    }
                    else if (result == 0)
                    {
                        SendFeedBackToClient(client, Common.RETURNCHECKOUT, Common.FAIL, $"Mã không nằm trong danh sách", new Dictionary<string, object>()); return;
                    }
                    else
                    {
                        SendFeedBackToClient(client, Common.RETURNCHECKOUT, Common.SUCCESS, $"Cập nhật kết quả thành công!", new Dictionary<string, object>());

                        Im_Ex_Event?.Invoke(null, scanCode);

                        return;
                    }
                }
                else
                {
                    SendFeedBackToClient(client, Common.RETURNCHECKOUT, Common.FAIL, "Thông tin chưa hợp lệ!", new Dictionary<string, object>()); return;
                }
            }
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
    }
}
