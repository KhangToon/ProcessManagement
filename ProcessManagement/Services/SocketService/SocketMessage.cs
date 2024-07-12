using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessManagement.Services.SocketService
{
    public class SocketMessage
    {
        public Dictionary<string, object> Command { get; set; }
        public List<Dictionary<string, object>> Data { get; set; }

        public SocketMessage(Dictionary<string, object> command, List<Dictionary<string, object>> data)
        {
            Command = command;
            Data = data;
        }

        public SocketMessage()
        {
            Command = new Dictionary<string, object>();
            Data = new List<Dictionary<string, object>>();
        }
    }
}
