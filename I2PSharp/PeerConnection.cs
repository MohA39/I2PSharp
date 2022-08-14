using System;
using System.Threading.Tasks;
using System.Threading;

namespace I2PSharp
{
    public class PeerConnection : IDisposable
    {
        private static int _NextID { get; set; } = 0;
        public int ID { get; private set; }
        public string PeerPublicKey { get; private set; }
        private SAMConnection SAMConnection { get; set; }

        private bool _IsWaitingForMessages = false;
        private Thread _WaitForMessagesThread;
        public delegate void MessageHandler(object sender, MessageEventArgs e);
        public event MessageHandler OnMessage;

        public delegate void DisconnectHandler(object sender, DisconnectEventArgs e);
        public event DisconnectHandler OnDisconnect;
        public PeerConnection(string peerPublicKey, SAMConnection sAMConnection)
        {
            ID = _NextID;
            PeerPublicKey = peerPublicKey;
            SAMConnection = sAMConnection;

            _NextID++;
        }

        public void Dispose()
        {
            StopWaitingForMessages();
            SAMConnection.Dispose();
        }
        public async void SendString(string message)
        {
            await SAMConnection.SendString(message);
        }
        public async void SendBytes(byte[] message)
        {
            await SAMConnection.SendBytes (message);
        }

        public void WaitForMessages()
        {
            if (_IsWaitingForMessages) return;
            _IsWaitingForMessages=true;
            SAMConnection.IsStringReadingPaused = false;
            _WaitForMessagesThread = new Thread(new ThreadStart(async () =>
            {

                while (_IsWaitingForMessages)
                {
                    try
                    {
                        string Message = await SAMConnection.ReadString();

                        if (OnMessage != null)
                        {
                            MessageEventArgs args = new MessageEventArgs(Message);
                            
                            OnMessage(this, args);
                        }
                    }
                    catch (System.IO.IOException)
                    {
                        if (OnDisconnect != null)
                        {
                            OnDisconnect(this, new DisconnectEventArgs());
                        }
                        
                        Dispose();
                        break;
                    }
                    
                }
            }));
            _WaitForMessagesThread.Start();
        }

        public async Task<byte[]> ReadBytes(int Count)
        {
            return await SAMConnection.ReadBytes(Count);
        }

        public void StopWaitingForMessages()
        {
            if (!_IsWaitingForMessages) return;
            SAMConnection.IsStringReadingPaused = true;
            _IsWaitingForMessages = false;
        }
        
    }
}
