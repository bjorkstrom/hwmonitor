using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hwmonitor
{
    static public class SendInfo
    {
        static NamedPipeServerStream communicationPipe;
        static StreamWriter writer;

        public static void Init()
        {
            communicationPipe = new NamedPipeServerStream("PipeForBatteryCommunication", PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            try
            {
                IAsyncResult connectionResult = communicationPipe.BeginWaitForConnection(ConnectedFunction, communicationPipe);
            }
            catch (IOException ioe)
            {
                Log.Exception(ioe);
                communicationPipe.Close();
            }
            catch(Exception e)
            {
                Log.Exception(e);
            }
            writer = new StreamWriter(communicationPipe);
        }

        static void ConnectedFunction(IAsyncResult result)
        {
            try
            {
                communicationPipe.EndWaitForConnection(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Pipe Connection failed: " + ex.Message);
            }
        }

        public static void Send(string infoToSend)
        {
            if (communicationPipe.IsConnected)
            {
                writer.WriteLine(infoToSend);
                writer.Flush();
            }
        }
    }
}
