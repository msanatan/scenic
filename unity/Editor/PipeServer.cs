using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UniBridge.Editor
{
    public sealed class PipeServer
    {
        private readonly string _projectHash;
        private readonly string _socketPath;
        private readonly string _windowsPipeName;

        private Thread _thread;
        private volatile bool _running;

        private Socket _listener;
        private Socket _client;
        private NamedPipeServerStream _namedPipe;

        public event Action<CommandRequest> OnCommandReceived;

        public PipeServer(string projectHash)
        {
            _projectHash = projectHash;
            _socketPath = Path.Combine(StateManager.BaseStateDirectory(), projectHash, "bridge.sock");
            _windowsPipeName = $"unibridge-{projectHash}";
        }

        public void Start()
        {
            if (_running)
            {
                return;
            }

            _running = true;
            _thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "UniBridgePipeServer",
            };
            _thread.Start();
        }

        public void Stop()
        {
            _running = false;

            try { _namedPipe?.Dispose(); } catch { }
            try { _client?.Dispose(); } catch { }
            try { _listener?.Dispose(); } catch { }

            _namedPipe = null;
            _client = null;
            _listener = null;

            if (_thread != null && _thread.IsAlive)
            {
                _thread.Join(TimeSpan.FromSeconds(1));
            }

            if (Environment.OSVersion.Platform != PlatformID.Win32NT && File.Exists(_socketPath))
            {
                try { File.Delete(_socketPath); } catch { }
            }
        }

        public void Send(CommandResponse response)
        {
            var payload = response.ToJson();
            var body = Encoding.UTF8.GetBytes(payload);
            var frame = new byte[4 + body.Length];
            WriteLengthPrefix(frame, body.Length);
            Buffer.BlockCopy(body, 0, frame, 4, body.Length);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (_namedPipe?.IsConnected == true)
                {
                    _namedPipe.Write(frame, 0, frame.Length);
                    _namedPipe.Flush();
                }

                return;
            }

            if (_client?.Connected == true)
            {
                _client.Send(frame);
            }
        }

        private void Run()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                RunWindowsNamedPipe();
                return;
            }

            RunUnixSocket();
        }

        private void RunWindowsNamedPipe()
        {
            while (_running)
            {
                try
                {
                    _namedPipe = new NamedPipeServerStream(
                        _windowsPipeName,
                        PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    _namedPipe.WaitForConnection();
                    ReadFrames(
                        read: (buffer, offset, count) => _namedPipe.Read(buffer, offset, count),
                        sendClose: () => _namedPipe.Dispose());
                }
                catch
                {
                    Thread.Sleep(100);
                }
                finally
                {
                    try { _namedPipe?.Dispose(); } catch { }
                    _namedPipe = null;
                }
            }
        }

        private void RunUnixSocket()
        {
            var socketDir = Path.GetDirectoryName(_socketPath);
            if (!Directory.Exists(socketDir))
            {
                Directory.CreateDirectory(socketDir);
            }

            if (File.Exists(_socketPath))
            {
                File.Delete(_socketPath);
            }

            _listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            _listener.Bind(new UnixDomainSocketEndPoint(_socketPath));
            _listener.Listen(1);

            while (_running)
            {
                try
                {
                    _client = _listener.Accept();
                    ReadFrames(
                        read: (buffer, offset, count) => _client.Receive(buffer, offset, count, SocketFlags.None),
                        sendClose: () => _client.Dispose());
                }
                catch
                {
                    Thread.Sleep(100);
                }
                finally
                {
                    try { _client?.Dispose(); } catch { }
                    _client = null;
                }
            }
        }

        private void ReadFrames(Func<byte[], int, int, int> read, Action sendClose)
        {
            var header = new byte[4];

            while (_running)
            {
                if (!ReadExactly(read, header, 4))
                {
                    break;
                }

                var length = ReadLengthPrefix(header);
                if (length <= 0 || length > 4 * 1024 * 1024)
                {
                    break;
                }

                var payload = new byte[length];
                if (!ReadExactly(read, payload, length))
                {
                    break;
                }

                UnityEngine.Debug.Log($"Received frame of length {length}");
                var json = Encoding.UTF8.GetString(payload);
                if (CommandRequest.TryParse(json, out var request))
                {
                    OnCommandReceived?.Invoke(request);
                }
            }

            sendClose();
        }

        private static bool ReadExactly(Func<byte[], int, int, int> read, byte[] buffer, int count)
        {
            var total = 0;
            while (total < count)
            {
                var n = read(buffer, total, count - total);
                if (n <= 0)
                {
                    return false;
                }

                total += n;
            }

            return true;
        }

        private static void WriteLengthPrefix(byte[] frame, int length)
        {
            frame[0] = (byte)((length >> 24) & 0xFF);
            frame[1] = (byte)((length >> 16) & 0xFF);
            frame[2] = (byte)((length >> 8) & 0xFF);
            frame[3] = (byte)(length & 0xFF);
        }

        private static int ReadLengthPrefix(byte[] header)
        {
            return (header[0] << 24)
                | (header[1] << 16)
                | (header[2] << 8)
                | header[3];
        }
    }
}
