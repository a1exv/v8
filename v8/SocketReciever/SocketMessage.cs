using System;
using System.Collections.Generic;
using System.Text;

namespace v8.SocketReciever
{
    public class SocketMessage
    {
        public string Message { get; set; }
        public SocketMessageType Type { get; set; }

        public SocketMessage(string message, SocketMessageType type)
        {
            Message = message;
            Type = type;
        }
    }
}
