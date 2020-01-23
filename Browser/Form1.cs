using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Browser.SocketServerReciever;
using CefSharp;
using CefSharp.WinForms;

using IniParser;
using IniParser.Model;
using Newtonsoft.Json;
using SoftMarket.Devices.BankTerminals;
using SoftMarket.Globals;
using v8.Models;
using v8.SocketReciever;

namespace Browser
{




    public partial class Form1 : Form
    {
        public Parameter[] Parameters { get; set; }

        private string _bankName;
        string _apiKey;
        string _apiLink;
        int _timeout;
        int _requestCount;
        public Form1()
        {
            InitializeComponent();
            var server = new SocketServer();
            server.ReceivedMessage += RecievedMessage;
            server.Run(6544);

            //var cashew = new CashewBank();
            //cashew.RunTransaction(new SoftMarket.Globals.Units.Money(100), false, TransactionType.Payment);
            if (File.Exists("params.ini"))
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile("params.ini");
                var parameters = new List<Parameter>()
                {
                    new Parameter(1, _bankName, 0, 50, "Название банка"),
                    new Parameter(2, _apiKey, 0, 200, "Api key"),
                    new Parameter(3, _apiLink, 0, 200, "Api link"),
                    new Parameter(4, _timeout, 0, 10000, "Timeout"),
                    new Parameter(5, _requestCount, 0, 50, "Request count")





                }.ToArray();
                parameters[0].StringValue = data["Params"]["BankName"];
                parameters[1].StringValue = data["Params"]["ApiKey"];
                parameters[2].StringValue = data["Params"]["ApiLink"];
                parameters[3].IntValue = Convert.ToInt32(data["Params"]["Timeout"]);
                parameters[4].IntValue = Convert.ToInt32(data["Params"]["RequestCount"]);
                Parameters = parameters;
            }



            string filepath = "@debug.txt";
            var stream = File.OpenWrite(filepath);
            stream.Write(Encoding.UTF8.GetBytes("initialize"), 0, 10);
            var settings = new CefSettings();
            stream.Write(Encoding.UTF8.GetBytes("settings"), 0, 8);
            //settings.BrowserSubprocessPath = @"x86\CefSharp.BrowserSubprocess.exe";
            //Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);
            //stream.Write(Encoding.UTF8.GetBytes("init settings"), 0, 8);
            //var browser =
            //    new ChromiumWebBrowser(
            //        "https://pay.cashewpay.by/#00020132420102p110322c7db2350bd54625bbd052bb6d6e39a95409999999.016304a9b1");
            //stream.Write(Encoding.UTF8.GetBytes("creating browser"), 0, 8);
            //this.Controls.Add(browser);
            //stream.Write(Encoding.UTF8.GetBytes("adding control"), 0, 8);
            //browser.Dock = DockStyle.Fill;
            //stream.Write(Encoding.UTF8.GetBytes("set dock"), 0, 8);
            //stream.Close();

            var order = new Order
            {
                Amount = 100.01,
                Currency = 933,
                Description = "тест",
                NotificationUri = "https://webhook.site/417bc9fb-e08e-424c-9452-ed9d160bb8cb",
                //CancelUri = "https://localhost",
                //ErrorUri = "https://localhost",
                //MerchantOrderId = "12345",
                //OrderStaticId = "7451fb6014bc47cb951ed97dd15abed5",
                //SuccessUri = "https://localhost"
            };

            string apiKey = "V1CiyMyS3hkAByYK3ihnvMhUOHk4e7uz5Yun8ho8lHKm0tjqPukeNfAFCI48fXBP";

            using (var client = new WebClient())
            {

                //place order

                string url = "https://api-awew-s112331d21-merchant.cashew.me/v1/orders";
                string content = JsonConvert.SerializeObject(order);

                string strContent =
                    "{\"description\": \"тест\",\"currency\": 933,\"amount\": 12345.67,\"notificationUri\": \"https://webhook.site/417bc9fb-e08e-424c-9452-ed9d160bb8cb\"}";

                var requestMessage = new SerializedPostRequest() { Content = strContent, Url = url };
                var socketMes = new SocketMessage(JsonConvert.SerializeObject(requestMessage), SocketMessageType.Post);

                var processes = Process.GetProcessesByName("ConsoleRequestSender");

                if (processes.Length <= 0)
                {
                    var dirPath = Directory.GetCurrentDirectory();

                    System.Diagnostics.Process.Start(Path.Combine(dirPath, "ConsoleRequestSender.exe"));

                }
                var socket = new SocketClient();
                var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                sock.Connect("8.8.8.8", 0);
                var localhost = sock.LocalEndPoint.ToString().Split(':')[0];
                socket.Connect(localhost, 6543);
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    socket.SendMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(socketMes)));
                }).Start();
         
                try
                {
                    AwaitResponse();
                    var jsonResponse = Message.Message;
                    var response = JsonConvert.DeserializeObject<string>(jsonResponse);
                }
                catch(Exception ex)
                {

                }

                //byte[] bytes = Encoding.UTF8.GetBytes(content);

                //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                //webRequest.Method = "POST";
                //webRequest.ContentType = "application/json";
                //webRequest.Headers.Add("x-api-key", apiKey);
                //webRequest.ContentLength = bytes.Length;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                //ServicePointManager.ServerCertificateValidationCallback +=(object sender, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors) => true;
                //client.Headers[HttpRequestHeader.Accept] = "application/json";
                //client.Headers[HttpRequestHeader.ContentType] = "application/json";
                //client.Headers.Add("x-api-key", apiKey);
                //byte[] result = client.UploadData(url, "POST", bytes);
                //string resultContent = Encoding.UTF8.GetString(result, 0, result.Length);

                //using (Stream str = webRequest.GetRequestStream())
                //{
                //    str.Write(bytes, 0, bytes.Length);

                //}
                //HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                //string responseString = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                //string orderId = JsonConvert.DeserializeObject<PlaceOrderResponse>(responseString).OrderId;
                //string qrUrl = JsonConvert.DeserializeObject<PlaceOrderResponse>(responseString).PaymentUri;
                //var dirPath = Directory.GetCurrentDirectory();

                //System.Diagnostics.Process.Start(Path.Combine(dirPath, "QRS.exe"), qrUrl);
                //Thread.Sleep(5000);
                //foreach (var process in Process.GetProcessesByName("QRS"))
                //{
                //    process.CloseMainWindow();
                //    process.Kill();
                //}

                ////var apiKey = "9T2JjdTWPstzNwpuJORQ3v6szPHtDGej";
                //var cancelUrl = "https://api-awew-s112331d21-merchant.cashew.me/v1/orders/{0}/cancel";
                //var fullUrl = String.Format(cancelUrl, orderId);
                ////using (var client = new WebClient())
                ////{
                ////    client.Headers[HttpRequestHeader.Accept] = "application/json";
                ////    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                ////    client.Headers.Add("x-api-key", apiKey);
                ////    byte[] result = client.UploadData(fullUrl, "POST", new byte[0]);

                ////}

                //HttpWebRequest webCancelRequest = (HttpWebRequest)WebRequest.Create(fullUrl);
                //webRequest.Method = "POST";
                //webRequest.ContentType = "application/json";
                //webRequest.Headers.Add("x-api-key", apiKey);

                //using (Stream str = webRequest.GetRequestStream())
                //{
                //    str.Write(new byte[0], 0, 0);

                //}
                //HttpWebResponse webCancelResponse = (HttpWebResponse)webCancelRequest.GetResponse();
                //if (webCancelResponse.StatusCode == HttpStatusCode.OK)
                //{
                //    //string filepath = "@debug.txt";
                //    //var stream = File.OpenWrite(filepath);
                //    //stream.Write(Encoding.UTF8.GetBytes("canceled"), 0, 8);
                //    //stream.Close();
                //}

                //Debug.WriteLine(responseString);
                ////client.Encoding = Encoding.UTF8;
                //client.Headers[HttpRequestHeader.Accept] = "application/json";
                //client.Headers[HttpRequestHeader.ContentType] = "application/json";

                //client.Headers.Add("x-api-key", apiKey);
                //client.Headers.Add("Content-Type", "application/json");
                //byte[] result = client.UploadData(url, "POST", bytes);
                //string result2 = client.UploadString(url, "POST", content);
                //string resultContent = Encoding.UTF8.GetString(result, 0, result.Length);
                //   string strContent =
                //       "{\"description\": \"тест\",\"currency\": 933,\"amount\": 12345.67,\"notificationUri\": \"https://webhook.site/417bc9fb-e08e-424c-9452-ed9d160bb8cb\"}";
                //   var strBytes = Encoding.UTF8.GetBytes(strContent);

                //    var webRequest = (HttpWebRequest) WebRequest.Create(url);
                //    webRequest.Method = "POST";
                //    webRequest.ContentType = "application/json";
                //    webRequest.Headers.Add("x-api-key", apiKey);
                //    webRequest.ContentLength = strBytes.Length;
                //    using (var str = webRequest.GetRequestStream())
                //    {
                //        str.Write(strBytes, 0, strBytes.Length);

                //    }
                //    var webResponse = (HttpWebResponse) webRequest.GetResponse();
                //    var responseString = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                //}


            }
        }

        public async void AwaitResponse(int timeout = 0)
        {
            if (timeout == 0)
            {
                timeout = 30000;
            }



            while (!IsRecievedMessage && timeout >= 0)
            {
                Thread.Sleep(1000);
                timeout -= 1000;
            }
            if (!IsRecievedMessage)
            {
                throw new Exception("fail to send response");
            }
            return;


        }

        public static bool IsRecievedMessage { get; set; }
        public static SocketMessage Message { get; set; }


        private static void RecievedMessage(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var message = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
                var socketMessage = JsonConvert.DeserializeObject<SocketMessage>(message);


                switch (socketMessage.Type)
                {

                    case SocketMessageType.WebResponse:
                        Message = socketMessage;
                        IsRecievedMessage = true;
                        break;
                }

            }
            catch (Exception ex)
            {

            }
        }

    }
}
