using System;
using System.Collections.Generic;
using System.Text;

namespace v8.SocketReciever
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public RemoteHost Host { get; private set; }

        public byte[] Message { get; private set; }

        public MessageReceivedEventArgs(RemoteHost host, byte[] message)
        {
            Message = message;
            Host = host;
        }
    }
}
