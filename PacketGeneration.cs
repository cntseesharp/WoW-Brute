using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

public class Logon
{

    private Srp6 srp;       // http://srp.stanford.edu/design.html  <- SRP6 information

    private BigInteger A;   // My public key?
    private BigInteger B;   // Server's public key
    private BigInteger a;   // my random number, used to initalize A from g and N.
    private byte[] I;       // Hash of "username:password"
    private BigInteger M;   // Combination of... everything!
    private byte[] M2;      // M2 is the combination of the server's everything to proof with ours (which we don't actually do, cause we trust blizzard, right?)


    private byte[] N;       // Modulus for A and B
    private byte[] g;       // base for A and B

    private BigInteger Salt;    // Server provided salt
    private byte[] crcsalt;     // Server provided crcsalt for file crc's.

    public byte[] K;        // Our combined key used for encryption of all traffic

    private string mUsername;
    private string mPassword;

    public Logon(string username, string password)
    {
        mUsername = username.ToUpper();
        mPassword = password;
    }

    public Packet DoLogonChallenge()
    {
        Packet p = new Packet();

        p.Write((byte)RLOp.AUTH_LOGON_CHALLENGE);
        p.Write((byte)8);                             // Used to be 2, now its 3.

        p.Write((UInt16)(30 + mUsername.Length));                                                        // Packet size + name length

        p.Write((byte)'W'); p.Write((byte)'o'); p.Write((byte)'W'); p.Write((byte)'\0');        // WoW

        p.Write((byte)0x03);
        p.Write((byte)0x03);
        p.Write((byte)0x05);
        //Build, 12340
        p.Write((byte)0x34);
        p.Write((byte)0x30);
        //Wow type
        p.Write((byte)0x36);
        p.Write((byte)0x38);
        p.Write((byte)0x78);
        p.Write((byte)0x00);
        //OS
        p.Write((byte)0x6e);
        p.Write((byte)0x69);
        p.Write((byte)0x57);
        p.Write((byte)0x00);

        //Language
        p.Write((byte)0x55);
        p.Write((byte)0x52);
        p.Write((byte)0x75);
        p.Write((byte)0x72);
        //timezone bias
        p.Write((byte)0x78);
        p.Write((byte)0x00);
        p.Write((byte)0x00);
        p.Write((byte)0x00);

        p.Write((byte)127); p.Write((byte)0); p.Write((byte)0); p.Write((byte)1);       // Interestingly, mac sends IPs in reverse order.

        p.Write((byte)mUsername.Length);
        p.Write(Encoding.Default.GetBytes(mUsername)); // Name - NOT null terminated
        return p;
    }

    public bool HandleLogonChallenge(Packet p)
    {
        byte op = p.ReadByte();
        byte unk = p.ReadByte();

        Logger.Log(LogType.NeworkComms, "Login Challenge: Response Type = {0}", (LogonOpCodes)unk);

        byte error = p.ReadByte();
        if (error > 0)
        {
            Logger.Log(LogType.System, "Login Challenge: Error = {0}", (LogonOpCodes)error);
            if (error == (byte)LogonOpCodes.LOGIN_BANNED) throw new BanException();
            return false;
        }
        B = new BigInteger(p.ReadBytes(32));               // Varies
        byte glen = p.ReadByte();                          // Length = 1
        g = p.ReadBytes(glen);                             // g = 7
        byte Nlen = p.ReadByte();                          // Length = 32
        N = p.ReadBytes(Nlen);                             // N = B79B3E2A87823CAB8F5EBFBF8EB10108535006298B5BADBD5B53E1895E644B89
        Salt = new BigInteger(p.ReadBytes(32));            // Salt = 3516482AC96291B3C84B4FC204E65B623EFC2563C8B4E42AA454D93FCD1B56BA
        crcsalt = p.ReadBytes(16);                         // Varies

        srp = new Srp6(new BigInteger(N), new BigInteger(g));

        // A hack, yes. We just keep trying till we get an S thats not negative so we get rid of auth=4 error logging on.
        BigInteger S;
        do
        {
            a = BigInteger.Random(19 * 8);
            A = srp.GetA(a);

            I = Srp6.GetLogonHash(mUsername, mPassword);

            BigInteger x = Srp6.Getx(Salt, I);
            BigInteger u = Srp6.Getu(A, B);
            S = srp.ClientGetS(a, B, x, u);
        }
        while (S < 0);
        System.Threading.Thread.Sleep(2000);
        K = Srp6.ShaInterleave(S);
        M = srp.GetM(mUsername, Salt, A, B, new BigInteger(K));

        unk = p.ReadByte();

        return true;
    }

    public Packet DoLogonProof()
    {
        Packet p = new Packet();
        Sha1Hash sha;
        byte[] files_crc;


        // Generate CRC/hashes of the Game Files
        files_crc = GenerateCrc(crcsalt);

        // get crc_hash from files_crc
        sha = new Sha1Hash();
        sha.Update(A);
        sha.Update(files_crc);
        byte[] crc_hash = sha.Final();

        p.Write((byte)RLOp.AUTH_LOGON_PROOF);
        p.Write(A); // 32 bytes
        p.Write(M); // 20 bytes
        p.Write(crc_hash); // 20 bytes
        p.Write((byte)0); // number of keys
        p.Write((byte)0); // unk (1.11.x)
        return p;
    }


    public bool HandleLogonProof(Packet p)
    {
        byte op = p.ReadByte();
        byte error = p.ReadByte();

        if (error > 0)
        {
            Logger.Log(LogType.System, "Login Proof: Error = {0} on {1}", error, mUsername);
            return false;
        }

        M2 = p.ReadBytes(20);
        int unknown = p.ReadInt32();
        UInt16 unk2 = p.ReadUInt16();
        p.ReadUInt32();

        return true;
    }

    ///

    private byte[] GenerateCrc(byte[] crcsalt)
    {
        Sha1Hash sha;

        byte[] buffer1 = new byte[0x40];
        byte[] buffer2 = new byte[0x40];

        for (int i = 0; i < 0x40; ++i)
        {
            buffer1[i] = 0x36;
            buffer2[i] = 0x5c;
        }

        for (int i = 0; i < crcsalt.Length; ++i)
        {
            buffer1[i] ^= crcsalt[i];
            buffer2[i] ^= crcsalt[i];
        }

        sha = new Sha1Hash();
        sha.Update(buffer1);


        try
        {
            FileStream fs = new FileStream("hash.bin", FileMode.Open, FileAccess.Read);
            byte[] Buffer = new byte[fs.Length];
            fs.Read(Buffer, 0, (int)fs.Length);
            sha.Update(Buffer);
        }
        catch (Exception e)
        {
            Logger.Log(LogType.Error, e.Message);
        }

        byte[] hash1 = sha.Final();

        sha = new Sha1Hash();
        sha.Update(buffer2);
        sha.Update(hash1);
        return sha.Final();
    }

    public Packet SendRealmlistRequest()
    {
        Packet p = new Packet();
        p.Write((byte)RLOp.REALM_LIST);
        p.Write(0);
        return p;
    }

    public Realm[] RetrieveRealmList(Packet p)
    {
        Realm[] Realms;

        byte op = p.ReadByte();
        UInt16 Length = p.ReadUInt16();
        UInt32 Request = p.ReadUInt32();
        UInt16 NumOfRealms = p.ReadUInt16();

        Realms = new Realm[NumOfRealms];

        for (int i = 0; i < NumOfRealms; i++)
        { 
            Realms[i].Type = p.ReadByte();
            Realms[i].Color = p.ReadByte();
            p.ReadByte(); // unk
            Realms[i].Name = p.ReadString();
            Realms[i].Address = p.ReadString();
            Realms[i].Population = p.ReadFloat();
            Realms[i].NumChars = p.ReadByte();
            Realms[i].Language = p.ReadByte();
            Realms[i].Unk = p.ReadByte();
        }

        byte Unk1 = p.ReadByte();
        byte Unk2 = p.ReadByte();

        Logger.Log(LogType.SystemDebug, "Realm reading - Done.");

        return Realms;
    }
}
