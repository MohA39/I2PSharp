using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace I2PSharp
{
    public enum ReadModes
    {
        String,
        Byte
    }
    public class PeerConnection : IDisposable
    {
        
        private static int _NextID { get; set; } = 0;
        public int ID { get; private set; }
        public ReadModes ReadMode { get; private set; } = ReadModes.String;
        public string PeerPublicKey { get; private set; }
        private SAMConnection SAMConnection { get; set; }

        private bool _IsWaitingForMessages = false;
        private Thread _WaitForMessagesThread;
        public delegate void MessageHandler(object sender, MessageEventArgs e);
        public event MessageHandler OnMessage;

        public delegate void DisconnectHandler(object sender, DisconnectEventArgs e);
        public event DisconnectHandler OnDisconnect;

        private CancellationTokenSource WaitForMessagesCancellationSource;
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
        public async Task SendString(string message)
        {
            try
            {
                await SAMConnection.SendString(message);
            }
            catch (IOException)
            {
                DisconnectLogic();
            }
            
        }
        public async Task SendBytes(byte[] message)
        {
            try
            {
                await SAMConnection.SendBytes(message);
            }
            catch (IOException)
            {
                DisconnectLogic();
            }
            
        }

        public void WaitForMessages()
        {
            if (_IsWaitingForMessages || ReadMode != ReadModes.String) return;

            WaitForMessagesCancellationSource = new CancellationTokenSource();
            _WaitForMessagesThread = new Thread(new ThreadStart(async () =>
            {
                
                _IsWaitingForMessages = true;
                while (_IsWaitingForMessages)
                {

                    try
                    {
                        try
                        {
                            await Task.Run(async () =>
                            {

                                string Message = await SAMConnection.ReadString();
                                if (OnMessage != null)
                                {
                                    MessageEventArgs args = new MessageEventArgs(Message);

                                    OnMessage(this, args);
                                }
                            }, WaitForMessagesCancellationSource.Token);
                        }
                        catch(TaskCanceledException)
                        {
                            return;
                        }
                        
                    }
                    catch (IOException)
                    {
                        DisconnectLogic();
                        break;
                    }

                }
            }));
            _WaitForMessagesThread.Start();
        }

        public async Task<byte[]> ReadBytes(int Count, int timeout = 0)
        {
            if (ReadMode == ReadModes.String) throw new InvalidOperationException("Cannot read bytes in string ReadMode");
            try
            {
                return await SAMConnection.ReadBytes(Count, 0);
            }
            catch (IOException)
            {
                DisconnectLogic();
                return null;
            }

        }

        public void StopWaitingForMessages()
        {
            _IsWaitingForMessages = false;
            WaitForMessagesCancellationSource.Cancel();
        }
        public void SetReadMode(ReadModes readMode)
        {
            if (readMode == ReadMode) return;
            ReadMode = readMode;

            if (ReadMode == ReadModes.Byte)
            {
                if (_IsWaitingForMessages)
                {
                    StopWaitingForMessages();
                }
            }
        }

        private void DisconnectLogic()
        {
            if (OnDisconnect != null)
            {
                OnDisconnect(this, new DisconnectEventArgs());
            }

            Dispose();
        }
    }
}