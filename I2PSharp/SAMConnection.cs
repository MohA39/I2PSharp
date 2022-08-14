
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;

namespace I2PSharp
{
    public class SAMConnection : IDisposable
    {

        public bool IsConnected { get; private set; } = false;
        public bool IsStringReadingPaused { get; set; } = false;
        private readonly int _port;
        private TcpClient _client = new TcpClient();
        private NetworkStream _networkstream;
        private StreamReader _streamreader;
        private bool _IsDisposed = false;
        public SAMConnection(int SAMPort)
        {
            _port = SAMPort;
        }

        public void Dispose()
        {
            _IsDisposed = true;
            _networkstream.Dispose();
            _client.Dispose();
        }
        public async Task ConnectAsync()
        {
            await _client.ConnectAsync(IPAddress.Loopback, _port);

            _networkstream = _client.GetStream();
            _streamreader = new StreamReader(_networkstream, Encoding.UTF8);
            var CommandResult = await SendCommandAsync("HELLO VERSION");
            if (Utils.TryParseResponse(CommandResult).result == SAMResponseResults.OK)
            {
                IsConnected = true;
            }
        }

        public async Task<string> SendCommandAsync(string Command)
        {
            byte[] CommandBytes = Encoding.UTF8.GetBytes(Command + "\n");
            await _networkstream.WriteAsync(CommandBytes, 0, CommandBytes.Length);
            return await ReadString();
        }
        public async Task SendString(string message)
        {
            byte[] MessageBytes = Encoding.UTF8.GetBytes(message + "\n");
            await _networkstream.WriteAsync(MessageBytes, 0, MessageBytes.Length);
        }
        public async Task SendBytes(byte[] message)
        {
            await _networkstream.WriteAsync(message, 0, message.Length);
        }

        public async Task<byte[]> ReadBytes(int count)
        {
            while (!_IsDisposed)
            {
                try
                {
                    byte[] BytesRead = new byte[count];
                    await _networkstream.ReadAsync(BytesRead, 0, count);
                    return BytesRead;
                }
                catch (InvalidOperationException) // To mitigate "The stream is currently in use by a previous operation on the stream."
                {
                    await Task.Delay(50);
                }
            }

            return null;
        }

        public async Task<string> ReadString()
        {
            while (!_IsDisposed)
            {
                try
                {
                    return await Readline();
                }
                catch (InvalidOperationException) // To mitigate "The stream is currently in use by a previous operation on the stream."
                {
                    await Task.Delay(50);
                }
            }

            return null;
        }

        private async Task<string> Readline()
        {
            return await _streamreader.ReadLineAsync();
        }
    }
}
