using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace v8.SocketReciever
{
    public class SocketClient
    {
        private int _port;

        private bool _connecting;

        private bool _connected;

        private SocketClientState _state;

        public Socket _mainSocket;

        private Thread _messageThread;

        private QueueWithBlock<byte[]> _messageQueue;

     //   public event Action<object, SocketClientState> StateChanged;

        public int Port
        {
            get { return _port; }
        }

        public SocketClientState State
        {
            get { return _state; }
        }

        public SocketClient()
        {
            _state = SocketClientState.Disconnected;
        }

        public void Connect(string ip, int port)
        {
            _port = port;
            var thread = new Thread(ConnectionWork);
            thread.IsBackground = true;
            thread.Start(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public void Disconnect()
        {
            if (_mainSocket != null)
            {
                _mainSocket.Close();
                _mainSocket = null;
            }
        }

        private void ConnectionWork(object obj)
        {
            try
            {
                var endPoint = (IPEndPoint)obj;
                _mainSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _connecting = true;
                _mainSocket.Connect(endPoint);
                
                _connecting = false;

                OnStateChanged(SocketClientState.Connected);
                while (_mainSocket.Connected)
                {
                    Thread.Sleep(1000);
                    //    var buffer = new byte[1024];
                    //    var count = _mainSocket.Receive(buffer);
                    //    if (count == 0 || _state == SocketClientState.Disconnected)
                    //    {
                    //        break;
                    //    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw ex;
            }
            finally
            {
                _connecting = false;
                if (_state == SocketClientState.Connected)
                {
                    OnStateChanged(SocketClientState.Disconnected);
                }
            }
        }

        public void SendMessage(byte[] message)
        {
            int timeout = 300;
            while (_state == SocketClientState.Disconnected && timeout > 0)
            {
                Thread.Sleep(70);
                timeout -= 70;
            }
            //if (_messageThread == null)
            //{
            //    _messageThread = new Thread(MessageThreadWork);
            //    _messageThread.IsBackground = true;
            //    _messageQueue = new QueueWithBlock<byte[]>();
            //    _messageThread.Start();
            //}
            //_messageQueue.Enqueue(message);
            SendMessageWork(message);
        }

        //private void MessageThreadWork()
        //{



        //    while (_state == SocketClientState.Connected)
        //    {
        //        var message = _messageQueue.Dequeue();
        //        if (message != null)
        //        {
        //            SendMessageWork(message);
        //        }
        //    }
        //}

        private void SendMessageWork(object obj)
        {
            try
            {
                var message = (byte[])obj;
                var sent = 0;
                var offset = 0;
                var size = message.Length;
                while (_connecting) { Thread.Sleep(70); }
                if (_mainSocket != null && _mainSocket.Connected && message.Length > 0 &&
                    _state == SocketClientState.Connected)
                {
                    do
                    {
                        try
                        {
                            sent += _mainSocket.Send(message, offset + sent, size - sent, SocketFlags.None);
                        }
                        catch (SocketException ex)
                        {
                            if (ex.SocketErrorCode == SocketError.WouldBlock ||
                                ex.SocketErrorCode == SocketError.IOPending ||
                                ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                            {
                                Thread.Sleep(30);
                            }
                            else
                            {
                                throw ex;
                            }
                        }



                    } while (sent < size);

                    OnStateChanged(SocketClientState.Disconnected);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                if (_state == SocketClientState.Connected)
                {
                    OnStateChanged(SocketClientState.Disconnected);
                }
                throw ex;
            }
        }

        protected void OnStateChanged(SocketClientState state)
        {
            _state = state;
            if (_state == SocketClientState.Disconnected)
            {
                if (_messageQueue != null)
                {

                    _messageQueue.Release();
                    _messageQueue = null;

                }

                if (_mainSocket != null && _mainSocket.Connected)
                {
                    _mainSocket.Disconnect(true);
                }

                _messageThread = null;
            }

            //var handler = StateChanged;
            //if (handler != null)
            //{
            //    handler(this, state);
            //}
        }
    }
}
