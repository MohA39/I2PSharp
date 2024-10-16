﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace I2PSharp
{

    public class SAMSubsession
    {
        private readonly int _SAMPort;
        public string ID { get; private set; }
        private readonly int? _FromPort;
        private readonly int? _ToPort;


        public bool IsAcceptingConnections { get; private set; }
        public bool IsConnected { get; private set; }
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

        public async Task<PeerConnection> ConnectAsync(string Destination, int FromPort = 0, int ToPort = 0, bool WaitForMessages = true)
        {
            SAMConnection connection = new SAMConnection(_SAMPort);
            await connection.ConnectAsync();
            string Response = await connection.SendCommandAsync($"STREAM CONNECT ID={ID} DESTINATION={Destination} FROM_PORT={FromPort} TO_PORT={ToPort}");

            (SAMResponseResults result, Dictionary<string, string> response) ParsedResponse = Utils.TryParseResponse(Response);
            if (ParsedResponse.result == SAMResponseResults.OK)
            {

                IsConnected = true;
                PeerConnection connectedSession = new PeerConnection(Destination, connection);

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

        public async Task<PeerConnection> AcceptConnectionAsync(bool WaitForMessages = true)
        {
            SAMConnection connection = new SAMConnection(_SAMPort);
            await connection.ConnectAsync();
            IsAcceptingConnections = true;
            string Response = await connection.SendCommandAsync($"STREAM ACCEPT ID={ID}");
            (SAMResponseResults result, Dictionary<string, string> response) ParsedResponse = Utils.TryParseResponse(Response);
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

                return connectedSession;
            }
            return null;
        }

    }
}
