using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Linq;

namespace I2PSharp
{

    public class SAMSubsession
    {
        private int _SAMPort;
        public string ID { get; private set; }
        private int? _FromPort;
        private int? _ToPort;


        public bool IsAcceptingConnections { get; private set; }
        public bool IsConnected { get; private set; }
        private List<PeerConnection> _ConnectedSessions = new List<PeerConnection>();
        public delegate void PeerConnectionHandler(object sender, ConnectionEventArgs e);
        public event PeerConnectionHandler OnConnect;

        internal SAMSubsession(int SAMPort, string SubsessionID, int? FromPort, int? ToPort)
        {
            _SAMPort = SAMPort;
            ID = SubsessionID;

            _FromPort = FromPort;
            _ToPort = ToPort;

            IsConnected = false;
            IsAcceptingConnections = false;
        }

        public async Task<PeerConnection> Connect(string Destination, int FromPort = 0, int ToPort = 0, bool WaitForMessages = true)
        {
            SAMConnection connection = new SAMConnection(_SAMPort);
            await connection.ConnectAsync();
            string Response = await connection.SendCommandAsync($"STREAM CONNECT ID={ID} DESTINATION={Destination} FROM_PORT={FromPort} TO_PORT={ToPort}");
            
            var ParsedResponse = Utils.TryParseResponse(Response);
            if (ParsedResponse.result == SAMResponseResults.OK)
            {
                
                IsConnected = true;
                PeerConnection connectedSession = new PeerConnection(Destination, connection);
                _ConnectedSessions.Add(connectedSession);
                if (OnConnect != null)
                {
                    ConnectionEventArgs args = new ConnectionEventArgs(connectedSession, ConnectionTypes.AcceptedByPeer);
                    OnConnect(this, args);
                }

                if (WaitForMessages)
                {
                    connectedSession.WaitForMessages();
                }

                return connectedSession;
            }
            else
            {

                return null;
            }
        }

        public async Task<PeerConnection> AcceptConnection(bool WaitForMessages = true)
        {
            SAMConnection connection = new SAMConnection(_SAMPort);
            await connection.ConnectAsync();
            IsAcceptingConnections = true;
            string Response = await connection.SendCommandAsync($"STREAM ACCEPT ID={ID}");
            var ParsedResponse = Utils.TryParseResponse(Response);
            if (ParsedResponse.result == SAMResponseResults.OK)
            {

                string PeerPublicKey = (await connection.ReadString()).Split(' ')[0];
                IsConnected = true;

                PeerConnection connectedSession = new PeerConnection(PeerPublicKey, connection);
                if (OnConnect != null)
                {
                    ConnectionEventArgs args = new ConnectionEventArgs(connectedSession, ConnectionTypes.AcceptedPeer);
                    OnConnect(this, args);
                }
                    
                if (WaitForMessages)
                {
                    connectedSession.WaitForMessages();
                }
                _ConnectedSessions.Add(connectedSession);
                return connectedSession;
            }
            return null;
        }
        public List<PeerConnection> GetConnectedSessions()
        {
            return _ConnectedSessions;
        }
        public List<PeerConnection> GetConnectedSessionsByPeerPublicKey(string PeerPublicKey)
        {
            return _ConnectedSessions.Where(x => x.PeerPublicKey == PeerPublicKey).ToList();
        }
        public List<PeerConnection> GetConnectedSessionsByID(int ID)
        {
            return _ConnectedSessions.Where(x => x.ID == ID).ToList();
        }


    }
}
