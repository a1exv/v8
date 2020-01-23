using ConsoleRequestSender.SocketServerReciever;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
       public static FileStream logstream;
        static void Main(string[] args)
        {
            try
            {
                string filePath = "@senderLog.txt";
                logstream = File.OpenWrite(filePath);
                logstream.Close();
                WriteInLog("constructor...");
                var server = new SocketServerReciever.SocketServer();
                server.ReceivedMessage += RecievedMessage;
                server.Run(65433);
            }
            catch(Exception ex)
            {
                WriteInLog(ex.StackTrace);
            }
        }

        


        private static void RecievedMessage(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                WriteInLog("starting recieving message");
                var message = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
                var socketMessage = JsonConvert.DeserializeObject<SocketMessage>(message);
                WriteInLog("message : " + socketMessage);
                using (var client = new HttpClient()) {
                    client.DefaultRequestHeaders.Add("x-api-key", "V1CiyMyS3hkAByYK3ihnvMhUOHk4e7uz5Yun8ho8lHKm0tjqPukeNfAFCI48fXBP");
                    switch (socketMessage.Type)
                    {
                        case SocketMessageType.Get:
                            var deserializedGetRequestMessage = JsonConvert.DeserializeObject<SerializedGetRequest>(socketMessage.Message);
                            var getRequest = new HttpRequestMessage(HttpMethod.Get, deserializedGetRequestMessage.Url);

                            var getResponse = client.SendAsync(getRequest).GetAwaiter().GetResult();
                            var getResult = getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            if (!string.IsNullOrEmpty(getResult))
                            {
                                SendMessage(getResult);
                            }
                            else if (getResponse.StatusCode == HttpStatusCode.OK)
                            {
                                SendMessage("OK");
                            }
                            else
                            {
                                SendMessage("smth goes wrong");
                            }
                           
                            break;

                        case SocketMessageType.Post:
                            var deseriaziledRequestMessage = JsonConvert.DeserializeObject<SerializedPostRequest>(socketMessage.Message);
                            var request = new HttpRequestMessage(HttpMethod.Post, deseriaziledRequestMessage.Url);

                            request.Content = new StringContent(deseriaziledRequestMessage.Content, Encoding.UTF8, "application/json");
                            WriteInLog("post request to server...");
                            var response = client.SendAsync(request).GetAwaiter().GetResult();
                            WriteInLog("done");
                            var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            WriteInLog("result: " + result);
                            if (!string.IsNullOrEmpty(result))
                            {
                                SendMessage(result);
                            }
                            else if (response.StatusCode == HttpStatusCode.OK)
                            {
                                SendMessage("OK");
                            }
                            else
                            {
                                SendMessage("smth goes wrong");
                            }
                            break;

                    }
                }
            }
            catch(Exception ex)
            {
                logstream.Write(Encoding.Default.GetBytes(ex.StackTrace + '\n'), 0, ex.StackTrace.Length + 1);
            }
        }

        private static void SendMessage(string mes)
        {
            try
            {
                WriteInLog("starting sending back...");
                var client = new SocketClient();
                var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                sock.Connect("8.8.8.8", 0);
                var localhost = sock.LocalEndPoint.ToString().Split(':')[0];
                client.Connect(localhost, 65449);
                WriteInLog("connected");
                var jsonmes = JsonConvert.SerializeObject(mes);
                var socketmes = new SocketMessage(jsonmes, SocketMessageType.WebResponse);
                client.SendMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(socketmes)));
                client.Disconnect();
                WriteInLog("message goes to cashew"+socketmes);
            }
            catch(Exception ex)
            {
                WriteInLog(ex.Message);
                WriteInLog(ex.StackTrace);

            }
         
        }

        private static void WriteInLog(string log)
        {
            try
            {
                File.AppendAllText("@senderLog.txt", log+"\n", Encoding.Default);
               // logstream.Write(Encoding.Default.GetBytes(log + '\n'), 0, log.Length + 1);
            }
            catch (Exception ex)
            {

            }
        }
      
       
    }
}
