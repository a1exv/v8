using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace v8.SocketReciever
{
    public class RemoteHost
    {
        public string Address { get; private set; }

        public Socket Connection { get; private set; }

        public RemoteHost(Socket socket)
        {
            var connection = socket;

            Connection = connection;

            if (Connection.RemoteEndPoint != null)
            {
                Address = Connection.RemoteEndPoint.ToString();
            }
            else
            {
                Address = "none";
            }
        }
    }
}
