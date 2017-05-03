using System;
using System.IO;
using System.Net;
using CScape.Dev.Providers;
using Nito.AsyncEx;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace CScape.Dev.Runtime.PlayerSpammer
{
    class Program
    {
        static void Main(string[] args)
        {
            AsymmetricCipherKeyPair key;
            using (var file = File.Open("cryptokey-private.pem", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var stream = new StreamReader(file))
                key = (AsymmetricCipherKeyPair) new PemReader(stream).ReadObject();

            //todo: maybe switch to SHA256?
            var crypto = new OaepEncoding(new RsaEngine(), new Sha1Digest());
            crypto.Init(true, key.Public);

            var nPlayer = 0;
            while (true)
            {
                Console.ReadLine();
                var username = $"Fake{nPlayer++}";
                Console.WriteLine($"Spawning {username}");

                AsyncContext.Run(async () =>
                {
                    var p = new FakePlayer(317, new IPEndPoint(IPAddress.Loopback, 43594), crypto, username, "123");
                    await p.Login();
                });
            }
        }
    }
}