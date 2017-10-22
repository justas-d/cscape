using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CScape.Basic
{
    public class FakePlayer
    {
        public short Revision { get; }
        public EndPoint Endpoint { get; }
        public IAsymmetricBlockCipher Crypto { get; }
        public string Username { get; }
        public string Password { get; }

        public Socket Socket { get; private set; }

        public FakePlayer(short revision, EndPoint endpoint, IAsymmetricBlockCipher crypto, 
            string username, string password)
        {
            Revision = revision;
            Endpoint = endpoint;
            Crypto = crypto;
            Username = username;
            Password = password;
        }

        public async Task Login()
        {
            Socket?.Dispose();

            Socket = new Socket(SocketType.Stream, ProtocolType.IP);
            Socket.Connect(Endpoint);

            var stream = new Blob(256);
            // header
            stream.Write(14);
            stream.Write(0);
            await Send(stream);

            await Receive(stream, 13);
            // skip 0's
            stream.ReadCaret += 8;
            var status = stream.ReadByte();
            if (status != 0)
            {
                Console.WriteLine($"Server returned status {status} for {Username}");
                return;
            }

            var key = stream.ReadInt64();

            stream.ResetHeads();
            stream.Write(16); // connect type
            stream.Write16(0); // garbage
            stream.Write16(Revision);
            stream.Write(0);  // is low mem
            // crcs
            for (var i = 0; i < 9; i++)
                stream.Write32(0);

            stream.Write(10);
            for (var i = 0; i < 4; i++)
                stream.Write32(0);

            stream.Write32(1); // signlink
            stream.WriteString(Username);
            stream.WriteString(Password);

            var encrypted = Crypto.ProcessBlock(stream.Buffer, 0, stream.WriteCaret);
            await Socket.SendAsync(new ArraySegment<byte>(encrypted, 0, encrypted.Length), SocketFlags.None);
            
            stream.ResetHeads();
            stream.Write(0);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                var seg = new ArraySegment<byte>(stream.Buffer, 0, 1);

                while (true)
                {
                    try
                    {
                        // send garbage
                        await Socket.SendAsync(seg, SocketFlags.None);
                        await Task.Delay(800);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Keepalive fail {Username}: {ex.Message}");
                        break;
                    }
                }
                
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task Receive(Blob blob, int len)
        {
            await Socket.ReceiveAsync(new ArraySegment<byte>(blob.Buffer, 0, len), SocketFlags.None);
            blob.ResetHeads();
        }

        private async Task Send(Blob blob)
        {
            await Socket.SendAsync(new ArraySegment<byte>(blob.Buffer, 0, blob.WriteCaret), SocketFlags.None);
            blob.ResetHeads();
        }
    }
}