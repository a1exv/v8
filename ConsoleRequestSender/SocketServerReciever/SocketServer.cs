using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using v8.SocketReciever;

namespace ConsoleRequestSender.SocketServerReciever
{
    public class SocketServer
    {
        private int _port;

        private SocketServerState _state;

        private Socket _mainSocket;

        public event EventHandler<SocketServerState> StateChanged;

        public event EventHandler<MessageReceivedEventArgs> ReceivedMessage;

        public event EventHandler<RemoteHost> HostAcceptConnection;

        public event EventHandler<RemoteHost> HostClosedConnection;

        public int Port
        {
            get { return _port; }
        }

        public SocketServerState State
        {
            get { return _state; }
        }

        public SocketServer()
        {
            _state = SocketServerState.Stopped;
        }

        public void Run(int port)
        {
            if (State == SocketServerState.Stopped)
            {
                _port = port;
                OnStateChanged(SocketServerState.Starting);
                var thread = new Thread(Listening);
                thread.IsBackground = false;
                thread.Start();
            }
        }

        public void Stop()
        {
            if (_mainSocket != null)
            {
                _mainSocket.Close();
                _mainSocket = null;
            }
        }

        private void Listening()
        {
            var hosts = new List<RemoteHost>();
            try
            {
                var endPoint = new IPEndPoint(IPAddress.Any, _port);
                _mainSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _mainSocket.Bind(endPoint);

                _mainSocket.Listen((int)SocketOptionName.MaxConnections);

                OnStateChanged(SocketServerState.Running);

                while (_state == SocketServerState.Running)
                {
                    var host = new RemoteHost(_mainSocket.Accept());
                    OnHostAcceptConnection(host);
                    hosts.Add(host);
                    var thread = new Thread(HostConnection);
                    thread.IsBackground = true;
                    thread.Start(host);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                OnStateChanged(SocketServerState.Stopped);
                for (int i = 0; i < hosts.Count; i++)
                {
                    var host = hosts[i];
                    if (host.Connection.Connected)
                    {
                        host.Connection.Close();
                    }
                }
                hosts.Clear();
            }
        }

        private void HostConnection(object obj)
        {
            var host = (RemoteHost)obj;
            try
            {
                var message = new List<byte>();

                while (host.Connection.Connected && State == SocketServerState.Running)
                {
                    int bufferLenght = 1024;
                    var buffer = new byte[bufferLenght];
                    var count = host.Connection.Receive(buffer);

                    message.AddRange(buffer);

                    if (count < bufferLenght || _state == SocketServerState.Stopped)
                    {
                        break;
                    }


                }
                OnReceivedMessage(host, message.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                OnHostClosedConnection(host);
            }
        }

        protected void OnStateChanged(SocketServerState state)
        {
            _state = state;
            var handler = StateChanged;
            if (handler != null)
            {
                handler(this, state);
            }


        }

        protected void OnReceivedMessage(RemoteHost host, byte[] message)
        {
            var handler = ReceivedMessage;
            if (handler != null)
            {
                handler(this, new MessageReceivedEventArgs(host, message));
            }
        }


        protected void OnHostAcceptConnection(RemoteHost host)
        {
            var handler = HostAcceptConnection;
            if (handler != null)
            {
                handler(this, host);
            }
        }

        protected void OnHostClosedConnection(RemoteHost host)
        {
            var handler = HostClosedConnection;
            if (handler != null)
            {
                handler(this, host);
            }
        }
    }
}
