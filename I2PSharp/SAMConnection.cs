
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
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private bool _IsDisposed = false;
        public SAMConnection(int SAMPort)
        {
            _port = SAMPort;
        }

        public void Dispose()
        {
            _IsDisposed = true;
            _reader.Dispose();
            _writer.Dispose();
            _client.Dispose();
        }
        public async Task ConnectAsync()
        {
            await _client.ConnectAsync(IPAddress.Loopback, _port);

            NetworkStream stream = _client.GetStream();

            _reader = new BinaryReader(stream);
            _writer = new BinaryWriter(stream);

            var CommandResult = await SendCommandAsync("HELLO VERSION");
            if (Utils.TryParseResponse(CommandResult).result == SAMResponseResults.OK)
            {
                IsConnected = true;
            }
        }

        public async Task<string> SendCommandAsync(string Command)
        {
            _writer.Write(Encoding.UTF8.GetBytes(Command + "\n"));
            return await ReadString();
        }
        public void SendString(string message)
        {
            _writer.Write(Encoding.UTF8.GetBytes(message + "\n"));
        }
        public void SendBytes(byte[] message)
        {
            _writer.Write(message);
        }

        public async Task<byte[]> ReadBytes(int count)
        {
            while (!_IsDisposed)
            {
                try
                {
                    return _reader.ReadBytes(count);
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
                    return Readline();
                }
                catch (InvalidOperationException) // To mitigate "The stream is currently in use by a previous operation on the stream."
                {
                    await Task.Delay(50);
                }
            }

            return null;
        }

        private string Readline()
        {

            StringBuilder SB = new StringBuilder();
            while (true)
            {
                if (IsStringReadingPaused)
                {
                    return null;
                }
                char readchar = _reader.ReadChar();

                if (readchar == '\n')
                {
                    break;
                }
                SB.Append(readchar);
            }
            return SB.ToString();

        }

    }
}
