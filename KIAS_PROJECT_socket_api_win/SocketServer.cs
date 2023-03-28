using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KIAS_PROJECT_socket_api_win
{
    public class SocketServer
    {
        private Form1 window = null;
        public SocketServer(Form1 form)
        {
            this.window = form;
        }
        public async void StartServer()
        {
            // Establish the local endpoint for the socket
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            
            // Create a TCP/IP socket
            using (Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                // Bind the socket to the local endpoint and listen for incoming connections
                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(10);

                    Console.WriteLine("Waiting for a connection...");
                    Socket handler = await listener.AcceptAsync();

                    // Receive data from the client and echo it back
                    byte[] buffer = new byte[1024];
                    string data = null;
                    string eom = "<EOF>";
                    while (true)
                    {
                        int bytesReceived = handler.Receive(buffer);
                        data = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                        if (data.IndexOf(eom) > -1)
                        {
                            Console.WriteLine($"Socket server received message: \"{data.Replace(eom, "")}\"");

                            var ackMessage = "<|ACK|>";
                            var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                            handler.Send(echoBytes, 0);
                            Console.WriteLine(
                                $"Socket server sent acknowledgment: \"{ackMessage}\"");
                            break;
                        }
                    }

                    Console.WriteLine("Received data: {0}", data);

                    //byte[] message = Encoding.ASCII.GetBytes("Echoing back: " + data);
                    byte[] message = Encoding.ASCII.GetBytes("Received");
                    handler.Send(message);

                    // Close handler
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
