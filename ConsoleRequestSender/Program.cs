using ConsoleRequestSender.SocketServerReciever;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using v8.SocketReciever;

namespace ConsoleRequestSender
{
    class Program
    {
       
        static void Main(string[] args)
        {
           
            var server = new SocketServer();
            server.ReceivedMessage += RecievedMessage;
            server.Run(6543);
        }


        private static void RecievedMessage(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var message = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
                var socketMessage = JsonConvert.DeserializeObject<SocketMessage>(message);
                using (var client = new HttpClient()) {
                    client.DefaultRequestHeaders.Add("x-api-key", "V1CiyMyS3hkAByYK3ihnvMhUOHk4e7uz5Yun8ho8lHKm0tjqPukeNfAFCI48fXBP");
                    switch (socketMessage.Type)
                    {
                        case SocketMessageType.Get:

                           
                            break;

                        case SocketMessageType.Post:
                            var deseriaziledRequestMessage = JsonConvert.DeserializeObject<SerializedPostRequest>(socketMessage.Message);
                            var request = new HttpRequestMessage(HttpMethod.Post, deseriaziledRequestMessage.Url);
                            request.Content = new StringContent(deseriaziledRequestMessage.Content, Encoding.UTF8, "application/json");
                            var result = client.SendAsync(request).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            SendMessage(result);
                            break;

                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        private static void SendMessage(string mes)
        {
            var client = new SocketClient();
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            sock.Connect("8.8.8.8", 0);
            var localhost = sock.LocalEndPoint.ToString().Split(':')[0];
            client.Connect(localhost, 6544);
            var jsonmes = JsonConvert.SerializeObject(mes);
            var socketmes = new SocketMessage(jsonmes, SocketMessageType.WebResponse);
            client.SendMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(socketmes)));
        }
      
       
    }
}
