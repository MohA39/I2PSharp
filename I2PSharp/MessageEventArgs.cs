using System;

namespace I2PSharp
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public MessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
