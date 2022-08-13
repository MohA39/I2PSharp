using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Linq;

namespace I2PSharp
{
    public class SAMSession
    {

        private int _SAMPort;
        private SAMConnection _Connection;
        private List<int> _UsedListenPorts = new List<int>();

        private Random _random = new Random();
        public SAMSession(int SAMPort = 7656)
        {

            _SAMPort = SAMPort;
            _Connection = new SAMConnection(SAMPort);
        }

        public async Task ConnectAsync()
        {
            await _Connection.ConnectAsync();
        }

        public async Task CreateSessionAsync(string Destination = "TRANSIENT")
        {
            if (!_Connection.IsConnected)
            {
                throw new InvalidOperationException("Error: Not connected to I2P SAM. Please use ConnectAsync() to connect.");
            }
            await _Connection.SendCommandAsync($"SESSION CREATE STYLE=PRIMARY ID={Utils.GenID(32, 64)} DESTINATION={Destination}");
        }

        public async Task<SAMSubsession> CreateSTREAMSubsessionAsync(string ID = null, int? FromPort = null, int? ToPort = null)
        {
            if (!_Connection.IsConnected)
            {
                throw new InvalidOperationException("Error: Not connected to I2P SAM. Please use ConnectAsync() to connect.");
            }

            string SubsessionID = string.IsNullOrEmpty(ID) ? Utils.GenID(32, 64) : ID;

            var FromPortArgument = PortToCommandArgument("FROM_PORT", FromPort);
            var ToPortArgument = PortToCommandArgument("TO_PORT", ToPort);

            if (FromPortArgument.port.HasValue)
            {
                _UsedListenPorts.Add(FromPortArgument.port.Value);
            }
            else
            {
                _UsedListenPorts.Add(0);
            }    

            string response = await _Connection.SendCommandAsync($"SESSION ADD STYLE=STREAM ID={SubsessionID} {FromPortArgument.argument} {ToPortArgument.argument}"); 
             var ParsedResponse = Utils.TryParseResponse(response);
            if (ParsedResponse.result == SAMResponseResults.OK)
            {
                SAMSubsession subsession = new SAMSubsession(_SAMPort, SubsessionID, FromPortArgument.port, ToPortArgument.port);
                return subsession;
            }
            else
            {
                throw new SAMBadResultException($"Error creating subsession(Bad result): {ParsedResponse.result} {Environment.NewLine}Message: {ParsedResponse.response["MESSAGE"]}");
            }
        }
        public async Task<string> LookupAsync(string name)
        {
            string response = await _Connection.SendCommandAsync($"NAMING LOOKUP NAME={name}");
            var ParsedResponse = Utils.TryParseResponse(response);

            if (ParsedResponse.result == SAMResponseResults.OK)
            {
                return ParsedResponse.response["NAME"];
            }
            else
            {
                throw new SAMBadResultException($"Bad result: {ParsedResponse.result} \r\n Message: {ParsedResponse.response["MESSAGE"]}");
            }

        }

        public async Task EndSubsession(string ID)
        {
            await _Connection.SendCommandAsync($"SESSION REMOVE ID={ID}");
        }

        public async Task<(string PrivateKey, string PublicKey)> GenerateDestinationAsync(SAMSignatures SignatureType = SAMSignatures.DSA_SHA1)
        {
            string response = await _Connection.SendCommandAsync($"DEST GENERATE {SignatureType}");
            var ParsedResponse = Utils.TryParseResponse(response);

            return (ParsedResponse.response["PRIV"], ParsedResponse.response["PUB"]);
        }

        public int GenerateUnusedPort()
        {
            int MaxPort = 65535;
            int Port = _random.Next(1, MaxPort);

            if (_UsedListenPorts.Contains(Port))
            {
                return GenerateUnusedPort();
            }
            return Port;
        }
        private (string argument, int? port) PortToCommandArgument(string arg, int? port)
        {
            if (port.HasValue)
            {
                if (port < 0)
                {
                    int GeneratedPort = GenerateUnusedPort();
                    return ($"{arg}={GeneratedPort}", GeneratedPort);
                }
                else
                {
                    return ($"{arg}{port.Value}", port.Value);
                }
            }
            else
            {
                return ("", null);
            }
        }
    }
}
