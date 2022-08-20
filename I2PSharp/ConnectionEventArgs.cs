using System;

namespace I2PSharp
{
    public class ConnectionEventArgs : EventArgs
    {
        public PeerConnection PeerConnection { get; private set; }
        public ConnectionTypes ConnectionType { get; private set; }
        public ConnectionEventArgs(PeerConnection connectedSession, ConnectionTypes connectionType)
        {
            PeerConnection = connectedSession;
            ConnectionType = connectionType;
        }
    }
}
