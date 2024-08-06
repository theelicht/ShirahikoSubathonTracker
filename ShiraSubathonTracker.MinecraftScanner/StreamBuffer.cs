using System.Text;
using Microsoft.Extensions.Logging;

namespace ShiraSubathonTracker.MinecraftScanner;

public class StreamBuffer(ILogger<StreamBuffer> logger)
{
    private readonly List<byte> _writeBuffer = [];
    private readonly byte[] _readBuffer = new byte[short.MaxValue];

    private int _readBufferOffset;
    
    public void WriteInt(int value)
    {
        while ((value & 128) != 0)
        {
            _writeBuffer.Add((byte) (value & 127 | 128));
            value = (int) ((uint) value) >> 7;
        }
        _writeBuffer.Add((byte) value);
    }
    
    public void WriteShort(short value)
    {
        _writeBuffer.AddRange(BitConverter.GetBytes(value));
    }
    
    public void WriteString(string data)
    {
        var buffer = Encoding.UTF8.GetBytes(data);
        WriteInt(buffer.Length);
        _writeBuffer.AddRange(buffer);
    }
    
    public void WriteToStream(Stream stream, byte b)
    {
        stream.WriteByte(b);
    }
    
    public void FlushToStream(Stream stream, int id = -1)
    {
        var buffer = _writeBuffer.ToArray();
        _writeBuffer.Clear();

        var add = 0;
        var packetData = new[] {(byte) 0x00};
        if (id >= 0)
        {
            WriteInt(id);
            packetData = _writeBuffer.ToArray();
            add = packetData.Length;
            _writeBuffer.Clear();
        }

        WriteInt(buffer.Length + add);
        var bufferLength = _writeBuffer.ToArray();
        _writeBuffer.Clear();

        stream.Write(bufferLength, 0, bufferLength.Length);
        stream.Write(packetData, 0, packetData.Length);
        stream.Write(buffer, 0, buffer.Length);
    }

    public void ReadStreamToBuffer(Stream stream)
    {
        var read = stream.Read(_readBuffer, 0, _readBuffer.Length);
    }

    private byte[] Read(int length)
    {
        var data = new byte[length];
        Array.Copy(_readBuffer, _readBufferOffset, data, 0, length);
        _readBufferOffset += length;
        return data;
    }

    public int ReadInt()
    {
        var value = 0;
        var size = 0;
        int b;
        while (((b = ReadByte()) & 0x80) == 0x80)
        {
            value |= (b & 0x7F) << (size++*7);
            if (size > 5)
            {
                throw new IOException("This VarInt is an imposter!");
            }
        }
        return value | ((b & 0x7F) << (size*7));
    }

    private byte ReadByte()
    {
        var b = _readBuffer[_readBufferOffset];
        _readBufferOffset += 1;
        return b;
    }
    
    public string ReadString(int length)
    {
        var data = Read(length);
        return Encoding.UTF8.GetString(data);
    }
}