using A2S_Proto.Models;
using A2S_Proto.Services;
using CommandLine.Interfaces;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using static A2S_Proto.Models.A2S_INFO;

namespace Host.Services
{
    public class Worker : IHostedService
    {
        private readonly ICommands _commands;
        private readonly GetServerListCall _serverListCall;

        public Worker(ICommands commands, GetServerListCall serverListCall)
        {
            _commands = commands;
            _serverListCall = serverListCall;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _commands
                .Add("vq", VRisingQuery, "Query the V Rising Server.")
                .Add("info", A2sInfo, "A2S_INFO Query the V Rising Server.");

            _commands.PrintHelp();
            await A2sInfoTest(new[] { "192.168.0.22:27016" });
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public async Task VRisingQuery(string[] parameters)
        {
            var serverList = await _serverListCall.GetServerList(@"\gameaddr\82.127.111.71:27015");
            Console.WriteLine("Server List:\r\n{0}", JsonSerializer.Serialize(serverList, new JsonSerializerOptions { WriteIndented = true }));
        }

        public async Task A2sInfo(string[] parameters)
        {
            var ep = IPEndPoint.Parse(parameters[0]);
            try
            {
                var a2sInfo = await A2S_INFO.Get(ep);
                Console.WriteLine(@"A2S_INFO ""{0}"":\r\n{1}", ep, JsonSerializer.Serialize(a2sInfo, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void PrintBytesToChar(byte[] bytes)
        {
            for(int i = 0; i < bytes.Length; i++)
                Console.Write($"{i}  ");
            Console.WriteLine();

            for (int i = 0; i < bytes.Length; i++)
                Console.Write($"{bytes[i]:X2} {IntToSpace(i)}");
            Console.WriteLine();

            for (int i = 0; i < bytes.Length; i++)
                Console.Write($"{Convert.ToChar(bytes[i] == 0x00 ? 0x20 : bytes[i])}  {IntToSpace(i)}");
            Console.WriteLine();
        }

        private static string IntToSpace(int i)
        {
            return new string(' ',
                i == 0 ? 1 :
                i % 1000000000 == 0 ? 10 :
                i % 100000000 == 0 ? 9 :
                i % 10000000 == 0 ? 8 :
                i % 1000000 == 0 ? 7 :
                i % 100000 == 0 ? 6 :
                i % 10000 == 0 ? 5 :
                i % 1000 == 0 ? 4 :
                i % 100 == 0 ? 3 :
                i % 10 == 0 ? 2 : 1);
        }
        public async Task A2sInfoTest(string[] parameters)
        {
            var request = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 };
            var ep = IPEndPoint.Parse(parameters[0]);
            using var udp = new UdpClient();
            Console.WriteLine(@"Sent {0} bytes: {1}", await udp.SendAsync(request, request.Length, ep), ByteArrayToHexViaLookup32(request));
            PrintBytesToChar(request);

            var buffer = new byte[ushort.MaxValue];
            var recv = await udp.Client.ReceiveFromAsync(buffer, SocketFlags.Peek, ep);
            var data = new byte[recv.ReceivedBytes - 4];
            Buffer.BlockCopy(buffer, 4, data, 0, recv.ReceivedBytes - 4);
            Console.WriteLine(@"Received {0} bytes: {1}", recv.ReceivedBytes, ByteArrayToHexViaLookup32(data));
            PrintBytesToChar(data);

            // 0x41 = Challenge
            if (data[0] == 0x41)
            {
                // 0xFF, 0xFF, 0xFF, 0xFF, 0x41, [0x64, 0xC0, 0xCF, 0x7E]
                var challenge = data[1..];
                Console.WriteLine(@"Challenge: {0}", ByteArrayToHexViaLookup32(challenge));
                PrintBytesToChar(challenge);

                var requestWithChallenge = new byte[request.Length + challenge.Length];
                Buffer.BlockCopy(request, 0, requestWithChallenge, 0, request.Length);
                Buffer.BlockCopy(challenge, 0, requestWithChallenge, request.Length, challenge.Length);
                Console.WriteLine(@"Sent {0} bytes with challenge: {1}", await udp.SendAsync(requestWithChallenge, requestWithChallenge.Length, ep), ByteArrayToHexViaLookup32(requestWithChallenge));
                PrintBytesToChar(requestWithChallenge);

                recv = await udp.Client.ReceiveFromAsync(buffer, SocketFlags.Peek, ep);
                data = new byte[recv.ReceivedBytes - 4];
                Buffer.BlockCopy(buffer, 4, data, 0, recv.ReceivedBytes - 4);
                Console.WriteLine(@"Received {0} bytes: {1}", recv.ReceivedBytes, ByteArrayToHexViaLookup32(data));
                PrintBytesToChar(data);
            }

            recv = await udp.Client.ReceiveFromAsync(buffer, SocketFlags.Peek, ep);
            data = new byte[recv.ReceivedBytes - 4];
            Buffer.BlockCopy(buffer, 4, data, 0, recv.ReceivedBytes - 4);
            Console.WriteLine(@"Received {0} bytes: {1}", recv.ReceivedBytes, ByteArrayToHexViaLookup32(data));
            PrintBytesToChar(data);

            //using var ms = new MemoryStream(await udp.ReceiveAsync(ref ep));   // Saves the received data in a memory buffer
            //var br = new BinaryReader(ms, Encoding.UTF8);     // A binary reader that treats characters as Unicode 8-bit
            //ms.Seek(4, SeekOrigin.Begin);                           // skip the 4 0xFFs

            //br.Close();
            //ms.Close();
            udp.Close();
        }

        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        private static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }
    }
}
