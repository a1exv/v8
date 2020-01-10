using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using SoftMarket.Globals.Units;
using SoftMarket.Globals;
using System.Threading;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using Newtonsoft.Json;
using SoftMarket.Devices.Printers;
using SoftMarket.Devices;
using SoftMarket.Globals.Forms;
using SoftMarket.Devices.BankTerminals;
using v8.Models;
using System.Security.Cryptography.X509Certificates;


namespace SoftMarket.Devices.BankTerminals
{
    public class CashewBank : IBankTerminal, IDeviceMessage, IDevice
    {
        private string _bankName = "банк";
        protected bool isStop = false;
        protected string _apiKey = string.Empty;
        protected string _apiLink = string.Empty;
        protected int _timeout = 0;
        protected int _requestCount = 0;
        protected BankTerminalTransactionMode transactionMode =
       BankTerminalTransactionMode.Sales;
        protected string yes = "yes";
        protected string no = "no";
        private readonly Parameter[] _parametersDefault;
        private bool isOpen;
        #region IBankTerminal Members

        public CashewBank()
        {
            var parameters = new List<Parameter>()
            {
                new Parameter(1, _bankName, 0, 50, "Название банка"),
                new Parameter(2, _apiKey, 0, 200, "Api key"),
                new Parameter(3, _apiLink, 0, 200, "Api link"),
                new Parameter(4, _timeout, 0, 10000, "Timeout"),
                new Parameter(5, _requestCount, 0, 50, "Request count")





            }.ToArray();
            if (File.Exists("params.ini"))
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile("params.ini");

                parameters[0].StringValue = data["Params"]["BankName"];
                parameters[1].StringValue = data["Params"]["ApiKey"];
                parameters[2].StringValue = data["Params"]["ApiLink"];
                parameters[3].IntValue = Convert.ToInt32(data["Params"]["Timeout"]);
                parameters[4].IntValue = Convert.ToInt32(data["Params"]["RequestCount"]);
                _parametersDefault = parameters;

            }
            else
            {
                _parametersDefault = parameters;
            }
        }

        public object ExecuteUserCommand(int commandId, object commandData)
        {
            return null;
        }
        public string BankName
        {
            get
            {
                return _bankName;

            }
            set { _bankName = value; }
        }
        public SoftMarket.Globals.Parameter[] Parameters
        {
            get { return _parametersDefault; }
            set
            {


                foreach (Parameter parameter in value)
                {
                    switch (parameter.Key)
                    {
                        case (int)SettingsParameter.BankName:
                            _bankName = parameter.StringValue;
                            break;
                        case (int)SettingsParameter.ApiKey:
                            _apiKey = parameter.StringValue;
                            break;
                        case (int)SettingsParameter.ApiLink:
                            _apiLink = parameter.StringValue;
                            break;
                        case (int)SettingsParameter.Timeout:
                            _timeout = parameter.IntValue;
                            break;
                        case (int)SettingsParameter.RequestCount:
                            _requestCount = parameter.IntValue;
                            break;
                    }
                }
            }
        }
        public Port DefaultPort
        {
            get
            {
                return new Port(PortType.KEYBOARD);
            }
        }
        public PortType[] PortTypes
        {
            get
            {
                return new PortType[] { PortType.KEYBOARD };
            }
        }
        public bool CanExecuteTransaction(TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.Payment:
                case TransactionType.Refund:
                case TransactionType.Cancel:
                    return transactionMode == BankTerminalTransactionMode.Sales;
                case TransactionType.BonusPayment:
                case TransactionType.BonusRefund:
                case TransactionType.BonusCancel:
                    return transactionMode == BankTerminalTransactionMode.Bonus;
                case TransactionType.ClosedLoopPayment:
                case TransactionType.ClosedLoopRefund:

                case TransactionType.ClosedLoopCancel:
                    return transactionMode == BankTerminalTransactionMode.ClosedLoop;
                case TransactionType.Ext1Payment:
                case TransactionType.Ext1Refund:
                case TransactionType.Ext1Cancel:
                    return transactionMode == BankTerminalTransactionMode.Ext1;
                case TransactionType.Ext2Payment:
                case TransactionType.Ext2Refund:
                case TransactionType.Ext2Cancel:
                    return transactionMode == BankTerminalTransactionMode.Ext2;
                case TransactionType.Ext3Payment:
                case TransactionType.Ext3Refund:
                case TransactionType.Ext3Cancel:
                    return transactionMode == BankTerminalTransactionMode.Ext3;
                default:
                    return false;
            }
        }
        public PaymentParameters ExecuteTransaction(Money summa, PaymentParameters
       paymentParameters, TransactionType transactionType, TransactionInfoParameter[] extParameters,
       decimal currencySum, int currencyCode, byte decimalPlace)
        {
            if (!CanExecuteTransaction(transactionType))
                throw new
               DeviceException("Чек содержит скидки. Использование типа оплаты запрещено.");

            switch (transactionType)
            {
                case TransactionType.Payment:
                    return RunTransaction(summa, false, transactionType);
                case TransactionType.Refund:
                    return RunTransaction(summa, true, transactionType);
                case TransactionType.BonusPayment:
                    return RunTransaction(summa, false, transactionType);
                case TransactionType.BonusRefund:
                    return RunTransaction(summa, true, transactionType);
                case TransactionType.ClosedLoopPayment:
                    return RunTransaction(summa, false, transactionType);
                case TransactionType.ClosedLoopRefund:
                    return RunTransaction(summa, true, transactionType);
                case TransactionType.Ext1Payment:
                    return RunTransaction(summa, false, transactionType);
                case TransactionType.Ext1Refund:
                    return RunTransaction(summa, true, transactionType);
                case TransactionType.Ext2Payment:
                    return RunTransaction(summa, false, transactionType);
                case TransactionType.Ext2Refund:
                    return RunTransaction(summa, true, transactionType);
                case TransactionType.Ext3Payment:
                    return RunTransaction(summa, false, transactionType);
                case TransactionType.Ext3Refund:
                    return RunTransaction(summa, true, transactionType);
                case TransactionType.Ext1Cancel:

                case TransactionType.Ext2Cancel:

                case TransactionType.Ext3Cancel:
                    if (paymentParameters.TransactionType == TransactionType.Ext3Payment)
                        return RunTransaction(paymentParameters.Summa, true,
                       TransactionType.Ext3Refund);
                    else
                        throw new
                       DeviceException("Данная транзакция не поддерживается");
                case TransactionType.Cancel:
                    if (paymentParameters.TransactionType == TransactionType.Payment)
                        return RunTransaction(paymentParameters.Summa, true,
                       TransactionType.Refund);
                    else
                        throw new
                       DeviceException("Данная транзакция не поддерживается");
                //case TransactionType.BonusCancel:
                //    if (paymentParameters.TransactionType == TransactionType.BonusPayment)
                //        return RunTransaction(paymentParameters.Summa, true,
                //       TransactionType.BonusRefund);
                //    else
                //        throw new
                //       DeviceException(BankTerminalStrings.GetString((int)Message.TransactionTypeNotSupport));
                //case TransactionType.ClosedLoopCancel:
                //    if (paymentParameters.TransactionType ==
                //   TransactionType.ClosedLoopPayment)
                //        return RunTransaction(paymentParameters.Summa, true,
                //       TransactionType.ClosedLoopRefund);
                //    else
                //        throw new
                //       DeviceException(BankTerminalStrings.GetString((int)Message.TransactionTypeNotSupport));
                default:
                    throw new
                   DeviceException("Данная транзакция не поддерживается");
            }
        }

        public TransactionInfoParameter[] GetExtendedParameters(TransactionType
       transactionType)
        {
            return new TransactionInfoParameter[0];
        }

        public void CheckExtendedParameters(TransactionInfoParameter[] extParameters,
       TransactionType transactionType)
        {
        }
        public void Init(int sareaId, int systemId, int currencyId, int deviceId)
        {

        }
        public void Stop()
        {
            isStop = true;
        }
        public void Open(Port port)
        {
            isStop = false;
            isOpen = true;
        }
        public void Close()
        {
            isOpen = false;
        }

        public string DeviceName
        {
            get
            {
                return "Cashew";
            }
        }
        public String DeviceFriendlyName
        {
            get
            {
                return
               "Cashew";
            }
        }
        public bool IsOfflineMode
        {
            get
            {
                return false;
            }
        }
        public void OfflineAuthorization(string authorizationCode)
        {
        }
        public void VirtualPort(string message)
        {
        }
        public event StatusMessageEventHandler StatusMessage;
        public event PrintMessageEventHandler PrintMessage;
        public event StringDataInputedEventHandler StringDataInputed;

        protected void OnStatusMessage(string message)
        {
            MessageEventArgs args = new MessageEventArgs(message);
            if (StatusMessage != null)
                StatusMessage(this, args);
        }
        public AccountInfo[] GetAccountsInfo(string cardCode)
        {
            return null;
        }
        public void OpenWorkDay()
        {
        }
        public void CloseWorkDay()
        {
        }
        #endregion
        #region IDevice Members
        public Parameter[] GetParameters(PortType portType)
        {
            return Parameters;
        }
        public Parameter[] GetDefaultParameters(PortType portType)
        {
            return GetParameters(portType);
        }

        public bool IsPortSupported(PortType portType)
        {
            foreach (PortType supPort in PortTypes)
            {
                if (supPort == portType)
                    return true;
            }
            return false;
        }
        public bool IsOpened
        {
            get
            {
                return isOpen;
            }
        }
        #endregion

        //private string [] orderStatuses = new string[]{"opened", "prepared", "confirmed", "canceled", "expired"};


        public PaymentParameters RunTransaction(Money summa, bool refund, TransactionType
            transactionType)
        {
            if (string.IsNullOrEmpty(_apiKey) ||
                string.IsNullOrEmpty(_apiLink) ||
                string.IsNullOrEmpty(_bankName) ||
                _timeout == 0 ||
                _requestCount == 0)
            {
                throw new DeviceException("Установите необходимые параметры");
            }



            string filePath = "@log.txt";
            var logStream = File.OpenWrite(filePath);

            try
            {
                var order = new Order
                {
                    Amount = (double)summa.Amount / 100,
                    Currency = 933,
                    Description = "QR payment",
                    NotificationUri = "https://webhook.site/417bc9fb-e08e-424c-9452-ed9d160bb8cb"
                };


                string apiKey = _apiKey;
                using (var client = new WebClient())
                {

                    // Set protocol
                    //var setProtocolLog = "set rpotocol to tls 1.2 \n";
                    //logStream.Write(Encoding.Default.GetBytes(setProtocolLog), 0, setProtocolLog.Length);
                    //ServicePointManager.SecurityProtocol = SecurityProtocolTypeExtensions.Tls12;

                    //place order

                    var placeOrderLogStart = "start placing order \n";
                    logStream.Write(Encoding.Default.GetBytes(placeOrderLogStart), 0, placeOrderLogStart.Length);

                    var url = _apiLink;
                    logStream.Write(Encoding.Default.GetBytes(url+"\n"), 0, url.Length+1);
                    var content = JsonConvert.SerializeObject(order);
                    logStream.Write(Encoding.Default.GetBytes(content+"\n"), 0, content.Length+1);
                    var bytes = Encoding.Default.GetBytes(content);




                    client.Headers[HttpRequestHeader.Accept] = "application/json";
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers.Add("x-api-key", apiKey);



                      
                    byte[] result = client.UploadData(url, "POST", bytes);
                    string resultContent = Encoding.UTF8.GetString(result, 0, result.Length);
                    var resultContentPlaceOrder = JsonConvert.DeserializeObject<PlaceOrderResponse>(resultContent);
                    LaunchBrowser(resultContentPlaceOrder.PaymentUri);

                    var placeOrderLogEnd = "order placed \n ";
                    logStream.Write(Encoding.Default.GetBytes(placeOrderLogEnd), 0, placeOrderLogEnd.Length);


                    //check status 

                    var statusUrl = _apiLink + "/{0}/status";
                    logStream.Write(Encoding.UTF8.GetBytes(statusUrl), 0, statusUrl.Length);
                    PlaceOrderResponse response = JsonConvert.DeserializeObject<PlaceOrderResponse>(resultContent);
                    int timeout = _timeout * 1000;
                    int requestFrequency = timeout / _requestCount;
                    string orderStatus = "";
                    while (timeout > 0)
                    {
                        Thread.Sleep(requestFrequency);
                        timeout -= requestFrequency;
                        var request = (HttpWebRequest)WebRequest.Create(String.Format(statusUrl, response.OrderId));
                        request.Headers.Add("x-api-key", apiKey);
                        request.ContentType = "application/json";
                        //request.Headers.Add("Content-Type", "application/json");
                        HttpWebResponse statusResponse = (HttpWebResponse)request.GetResponse();

                        using (Stream stream = statusResponse.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                var statusResponseString = reader.ReadToEnd();
                                logStream.Write(Encoding.UTF8.GetBytes(statusResponseString), 0,
                                    statusResponseString.Length);
                                StatusResponse status =
                                    JsonConvert.DeserializeObject<StatusResponse>(statusResponseString);
                                if (status.Status.Equals("confirmed"))
                                {
                                    orderStatus = status.Status;

                                    #region Confirmation 


                                    var confirmationLogStart = "start confirmation \n";
                                    logStream.Write(Encoding.Default.GetBytes(confirmationLogStart), 0, confirmationLogStart.Length);


                                    HttpStatusCode confirmationResult = new HttpStatusCode();
                                    while (timeout > 0)
                                    {
                                        confirmationResult = ConfirmOrder(response.OrderId);
                                        if (confirmationResult == HttpStatusCode.OK)
                                        {
                                           
                                            var confirmationLogEndSuccess = "end confirmation with success \n";
                                            logStream.Write(Encoding.Default.GetBytes(confirmationLogEndSuccess), 0, confirmationLogEndSuccess.Length);
                                            break;
                                        }
                                        else
                                        {
                                            timeout -= requestFrequency;
                                        }
                                    }
                                    if (confirmationResult != HttpStatusCode.OK)
                                    {
                                        var confirmationLogEndFail = "end confirmation with failure, status code = "+confirmationResult.ToString()+" \n";
                                        logStream.Write(Encoding.Default.GetBytes(confirmationLogEndFail), 0, confirmationLogEndFail.Length);
                                        throw new DeviceException("Подтверждение не получено");
                                    }
                                    else if(confirmationResult == HttpStatusCode.OK)
                                    {
                                        var confirmationLogEndSuccess = "end confirmation with success \n";
                                        logStream.Write(Encoding.Default.GetBytes(confirmationLogEndSuccess), 0, confirmationLogEndSuccess.Length);

                                        break;

                                    }




                                    #endregion Confirmation
                                }
                                else
                                {
                                    var processes = Process.GetProcessesByName("QRS");

                                    if (processes.Length <= 0)
                                    {
                                        timeout = -1;

                                        break;

                                    }
                                }

                                orderStatus = status.Status;
                            }
                        }
                    }

                    foreach (var process in Process.GetProcessesByName("QRS"))
                    {
                        process.CloseMainWindow();
                        process.Kill();
                    }

                    if (orderStatus.Equals("confirmed"))
                    {
                        logStream.Write(Encoding.UTF8.GetBytes("ORDER IS PREPARED"), 0, 17);
                        logStream.Close();
                    }
                    else
                    {

                        CancelOrder(response.OrderId);

                        throw new DeviceException("Операция не проведена");
                    }



                }


            }
            catch (Exception e)
            {

                logStream.Write(Encoding.Default.GetBytes(e.Message+"\n"), 0, e.Message.Length+1);
                logStream.Write(Encoding.Default.GetBytes(e.StackTrace), 0, e.StackTrace.Length);

                throw new DeviceException("Операция не проведена");
            }
            finally
            {
                logStream.Close();
            }

            return new PaymentParameters(summa, "", "", "", _bankName, transactionType);

        }


        public bool OnKeyDown(char symbol)
        {
            return false;
        }

        public void CheckMessage()
        {

        }

        public void ClearMessage()
        {

        }

        protected enum SettingsParameter
        {
            BankName = 1,
            ApiKey = 2,
            ApiLink = 3,
            Timeout = 4,
            RequestCount = 5
        }

        #region Browser


        public void LaunchBrowser(string url)
        {
            string filepath = "@debug.txt";
            var stream = File.OpenWrite(filepath);
            stream.Write(Encoding.UTF8.GetBytes(url), 0, url.Length);
            stream.Close();

            var dirPath = Directory.GetCurrentDirectory();

            System.Diagnostics.Process.Start(Path.Combine(dirPath, "QRS.exe"), url);
            //System.Diagnostics.Process.Start("notepad.exe", url);
            
        }

        

        public void CancelOrder(string orderId)
        {
            string apiKey = _apiKey;
            var cancelUrl = _apiLink + "/{0}/cancel";
            var fullUrl = String.Format(cancelUrl, orderId);
            //using (var client = new WebClient())
            //{
            //    client.Headers[HttpRequestHeader.Accept] = "application/json";
            //    client.Headers[HttpRequestHeader.ContentType] = "application/json";
            //    client.Headers.Add("x-api-key", apiKey);
            //    byte[] result = client.UploadData(fullUrl, "POST", new byte[0]);

            //}

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(fullUrl);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.Headers.Add("x-api-key", apiKey);

            using (Stream str = webRequest.GetRequestStream())
            {
                str.Write(new byte[0], 0, 0);

            }
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            if (webResponse.StatusCode == HttpStatusCode.OK)
            {
                string filepath = "@debug.txt";
                var stream = File.OpenWrite(filepath);
                stream.Write(Encoding.UTF8.GetBytes("canceled"), 0, 8);
                stream.Close();
            }
            else
            {

                string filepath = "@debug.txt";
                var stream = File.OpenWrite(filepath);
                stream.Write(Encoding.UTF8.GetBytes("smth goes wrong"), 0, 14);
                stream.Close();
            }
        }

        public HttpStatusCode ConfirmOrder(string orderId)
        {
            var confirmUrl = _apiLink + "/{0}/confirm";
            var fullUrl = String.Format(confirmUrl, orderId);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(fullUrl);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.Headers.Add("x-api-key", _apiKey);
            using (Stream str = webRequest.GetRequestStream())
            {
                str.Write(new byte[0], 0, 0);

            }
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            return webResponse.StatusCode;


            #endregion Browser
        }
    }
}