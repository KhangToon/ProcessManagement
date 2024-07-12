using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProcessManagement.Services.SocketService
{
    public class MessageAgrs
    {
        public const string EXPORT = "EXPORT";
        public const string IMPORT = "IMPORT";
        public const string CHECK = "CHECK";

        public MessageAgrs(Socket client, string message)
        {
            Client = client;

            ClientAddress = client.RemoteEndPoint?.ToString();

            List<string> clientcontent = SplitMessage(message);
        }

        public Socket Client { get; set; }
        public string? Code { get; set; }
        public string? LotNo { get; set; }
        public string? Quantity { get; set; }
        public string? MessType { get; set; }
        public DateTime MessTime { get; set; }
        public string? ClientAddress { get; set; }


        private List<string> SplitMessage(string message)
        {
            List<string> contentlist = new List<string>();

            string parttern = @"\[(.*?)\]";

            Regex regex = new Regex(parttern);

            MatchCollection matches = regex.Matches(message);

            foreach (Match match in matches)
            {
                contentlist.Add(match.Groups[1].Value);
            }

            return contentlist;
        }
    }
}
