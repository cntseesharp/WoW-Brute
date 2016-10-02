using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Packet
{
    byte[] content;
    int pointer;

    public Packet()
    {
        content = new byte[0];
        pointer = 0;
    }

    public void Flush()
    {
        content = new byte[0];
        pointer = 0;
    }



    public byte[] Content
    {
        get { return content; }
    }

    public void Write(string content)
    {
        byte[] temp = new byte[content.Length];
        int i = 0;
        foreach (char c in content)
            temp[i++] = (byte)c;
        ContentUpdater(temp);
    }

    public void Write(int content)
    {
        ContentUpdater(BitConverter.GetBytes(content));
    }

    public void Write(double content)
    {
        ContentUpdater(BitConverter.GetBytes(content));
    }

    public void Write(UInt16 content)
    {
        ContentUpdater(BitConverter.GetBytes(content));
    }

    public void Write(byte[] content)
    {
        ContentUpdater(content);
    }

    public void Write(byte content)
    {
        ContentUpdater(new byte[1] { content });
    }

    void ContentUpdater(byte[] content)
    {
        byte[] temp = new byte[this.content.Length + content.Length];
        this.content.CopyTo(temp, 0);
        content.CopyTo(temp, this.content.Length);
        this.content = temp;
    }

    public byte ReadByte()
    {
        return content[pointer++];
    }

    public int ReadInt()
    {
        pointer += 4;
        return BitConverter.ToInt32(content, pointer - 4);
    }

    public int ReadInt32()
    {
        return ReadInt();
    }

    public UInt32 ReadUInt32()
    {
        pointer += 4;
        return BitConverter.ToUInt32(content, pointer - 4);
    }

    public UInt16 ReadUInt16()
    {
        pointer += 2;
        return BitConverter.ToUInt16(content, pointer - 2);
    }

    public long ReadInt64()
    {
        pointer += 8;
        return BitConverter.ToInt64(content, pointer - 8);
    }

    public double ReadDouble()
    {
        pointer += 8;
        return BitConverter.ToDouble(content, pointer - 8);
    }

    public float ReadFloat()
    {
        pointer += 4;
        return BitConverter.ToSingle(content, pointer - 4);
    }

    public string ReadString()
    {
        StringBuilder sb = new StringBuilder();
        while (true)
        {
            byte b;
            //if (Remaining > 0)
            b = ReadByte();
            //else
            //   b = 0;

            if (b == 0) break;
            sb.Append((char)b);
        }
        return sb.ToString();
    }

    public bool ReadBoolean()
    {
        return BitConverter.ToBoolean(content, pointer++);
    }

    public char ReadChar()
    {
        return BitConverter.ToChar(content, pointer++);
    }

    public byte[] ReadBytes(int count)
    {
        byte[] temp = new byte[count];
        for (int i = 0; i < count; i++)
            temp[i] = content[pointer + i];
        pointer += count;
        return temp;
    }
}

