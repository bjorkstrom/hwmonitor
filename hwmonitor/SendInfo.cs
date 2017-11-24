using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hwmonitor
{
    public static class SendInfo
    {
        static TcpClient tcpClient;
        static Socket theSocket;
        static int offset = 0;
        static int timeout = 10000;

        public static void Init()
        {
            try
            {
                tcpClient = new TcpClient();
                string ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[2].ToString();
                tcpClient.Connect(ipAddress, 8001);
                theSocket = tcpClient.Client;
            }
           catch(Exception e)
            {
                Console.WriteLine(e.Message);
            } 
        }

        public static void Send(string infoToSend)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(infoToSend);
            int size = infoToSend.Length;
            int startTickCount = Environment.TickCount;
            int sent = 0;  // how many bytes is already sent

            do
            {
                if (Environment.TickCount > startTickCount + timeout)
                    throw new Exception("Timeout.");
                try
                {
                    sent += theSocket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        Console.WriteLine(ex.Message);
                //        throw ex;  // any serious error occurr
                }
            } while (sent < size);
        }

    }
}
