using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Reflection.PortableExecutable;

namespace A2S_Proto.Models;

public class A2S_INFO
{
    // \xFF\xFF\xFF\xFFTSource Engine Query\x00 because UTF-8 doesn't like to encode 0xFF
    public static readonly byte[] REQUEST = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 };
    #region Strong Typing Enumerators
    [Flags]
    public enum ExtraDataFlags : byte
    {
        GameID = 0x01,
        SteamID = 0x10,
        Keywords = 0x20,
        Spectator = 0x40,
        Port = 0x80
    }
    public enum VACFlags : byte
    {
        Unsecured = 0,
        Secured = 1
    }
    public enum VisibilityFlags : byte
    {
        Public = 0,
        Private = 1
    }
    public enum EnvironmentFlags : byte
    {
        Linux = 0x6C,   //l
        Windows = 0x77, //w
        Mac = 0x6D,     //m
        MacOsX = 0x6F   //o
    }
    public enum ServerTypeFlags : byte
    {
        Dedicated = 0x64,     //d
        Nondedicated = 0x6C,   //l
        SourceTV = 0x70   //p
    }
    #endregion

    public byte? Header { get; set; }        // I
    public byte? Protocol { get; set; }
    public string? Name { get; set; }
    public string? Map { get; set; }
    public string? Folder { get; set; }
    public string? Game { get; set; }
    public short? ID { get; set; }
    public byte? Players { get; set; }
    public byte? MaxPlayers { get; set; }
    public byte? Bots { get; set; }
    public ServerTypeFlags ServerType { get; set; }
    public EnvironmentFlags Environment { get; set; }
    public VisibilityFlags Visibility { get; set; }
    public VACFlags VAC { get; set; }
    public string? Version { get; set; }
    public ExtraDataFlags ExtraDataFlag { get; set; }

    #region Extra Data Flag Members
    public ulong? GameID { get; set; }           //0x01
    public ulong? SteamID { get; set; }          //0x10
    public string? Keywords { get; set; }        //0x20
    public string? Spectator { get; set; }       //0x40
    public short? SpectatorPort { get; set; }   //0x40
    public short? Port { get; set; }             //0x80
    #endregion

    public static async Task<A2S_INFO> Get(IPEndPoint ep)
    {
        var a2sInfo = new A2S_INFO();

        using var udp = new UdpClient();
        await udp.SendAsync(REQUEST, REQUEST.Length, ep);
        using var ms = new MemoryStream(udp.Receive(ref ep));   // Saves the received data in a memory buffer
        var br = new BinaryReader(ms, Encoding.UTF8);     // A binary reader that treats characters as Unicode 8-bit
        ms.Seek(4, SeekOrigin.Begin);                           // skip the 4 0xFFs

        a2sInfo.Header = br.ReadByte();
        a2sInfo.Protocol = br.ReadByte();
        a2sInfo.Name = ReadNullTerminatedString(ref br);
        a2sInfo.Map = ReadNullTerminatedString(ref br);
        a2sInfo.Folder = ReadNullTerminatedString(ref br);
        a2sInfo.Game = ReadNullTerminatedString(ref br);
        a2sInfo.ID = br.ReadInt16();
        a2sInfo.Players = br.ReadByte();
        a2sInfo.MaxPlayers = br.ReadByte();
        a2sInfo.Bots = br.ReadByte();
        a2sInfo.ServerType = (ServerTypeFlags)br.ReadByte();
        a2sInfo.Environment = (EnvironmentFlags)br.ReadByte();
        a2sInfo.Visibility = (VisibilityFlags)br.ReadByte();
        a2sInfo.VAC = (VACFlags)br.ReadByte();
        a2sInfo.Version = ReadNullTerminatedString(ref br);
        a2sInfo.ExtraDataFlag = (ExtraDataFlags)br.ReadByte();

        #region These EDF readers have to be in this order because that's the way they are reported
        if (a2sInfo.ExtraDataFlag.HasFlag(ExtraDataFlags.Port))
            a2sInfo.Port = br.ReadInt16();
        if (a2sInfo.ExtraDataFlag.HasFlag(ExtraDataFlags.SteamID))
            a2sInfo.SteamID = br.ReadUInt64();
        if (a2sInfo.ExtraDataFlag.HasFlag(ExtraDataFlags.Spectator))
        {
            a2sInfo.SpectatorPort = br.ReadInt16();
            a2sInfo.Spectator = ReadNullTerminatedString(ref br);
        }
        if (a2sInfo.ExtraDataFlag.HasFlag(ExtraDataFlags.Keywords))
            a2sInfo.Keywords = ReadNullTerminatedString(ref br);
        if (a2sInfo.ExtraDataFlag.HasFlag(ExtraDataFlags.GameID))
            a2sInfo.GameID = br.ReadUInt64();
        #endregion

        br.Close();
        ms.Close();
        udp.Close();

        return a2sInfo;
    }

    /// <summary>Reads a null-terminated string into a .NET Framework compatible string.</summary>
    /// <param name="input">Binary reader to pull the null-terminated string from.  Make sure it is correctly positioned in the stream before calling.</param>
    /// <returns>String of the same encoding as the input BinaryReader.</returns>
    public static string ReadNullTerminatedString(ref BinaryReader reader)
    {
        var sb = new StringBuilder();
        char? read = Convert.ToChar(reader.ReadByte());
        while (read != '\x00')
        {
            sb.Append(read);
            read = Convert.ToChar(reader.ReadByte());
        }
        return sb.ToString();
    }
}
