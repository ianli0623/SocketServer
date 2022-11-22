// https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example
using System;
using static System.Console;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// State object for reading client data asynchronously  
public class StateObject
{
    // Size of receive buffer.  
    public const int BufferSize = 4096;
    public static int count = 0;

    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];

    // Received data string.
    public StringBuilder sb = new StringBuilder();

    // Client socket.
    public Socket workSocket = null;
}  
  
public class AsynchronousSocketListener
{
    // Thread signal.  
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public AsynchronousSocketListener()
    {
    }

    public static void StartListening()
    {
        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com".  
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());  
        IPAddress ipAddress = ipHostInfo.AddressList[0];  
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8800);  
  
        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,  
            SocketType.Stream, ProtocolType.Tcp );  
  
        // Bind the socket to the local endpoint and listen for incoming connections.  
        try {  
            listener.Bind(localEndPoint);  
            listener.Listen(100);  
  
            while (true) {  
                // Set the event to nonsignaled state.  
                allDone.Reset();  
  
                // Start an asynchronous socket to listen for connections.  
                Console.WriteLine("Waiting for a connection...");  
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),  
                    listener );  
  
                // Wait until a connection is made before continuing.  
                allDone.WaitOne();  
            }  
  
        } catch (Exception e) {  
            Console.WriteLine(e.ToString());  
        }  
  
        Console.WriteLine("\nPress ENTER to continue...");  
        Console.Read();  
  
    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        allDone.Set();  
  
        // Get the socket that handles the client request.  
        Socket listener = (Socket) ar.AsyncState;  
        Socket handler = listener.EndAccept(ar);  
  
        // Create the state object.  
        StateObject state = new StateObject();  
        state.workSocket = handler;  
        handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,  
            new AsyncCallback(ReadCallback), state);  

    }

    public static void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket
        // from the asynchronous state object.
        StateObject state = (StateObject) ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket.
        int bytesRead = handler.EndReceive(ar);
        
        if (bytesRead > 0) {
            
            // There  might be more data, so store the data received so far.
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read
            // more data.

            //content = state.sb.ToString();
            //content = @"0019700000000000000000000PB   00001000000000000000000000001410100991091229164457Y10912291645370000101000D99訊息內容有誤!! 業務類別錯誤                                                             0000000";
            content = @"0415200000000000000000000DA   0000100000000000000000000004096DA10DA202011000037191091116161154P120XXX048000                                                                              000001    0449344                                            0050505001225100                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       ";
            Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
            Thread.Sleep(5000);
            // Echo the data back to the client.
            Send(handler, content);

            //if (content.IndexOf("<EOF>") > -1)
            //{
            //    // All the data has been read from the
            //    // client. Display it on the console.
            //    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
            //        content.Length, content);
            //    // Echo the data back to the client.
            //    //Thread.Sleep(6000);
            //    Send(handler, content);
            //}

            //var a = StateObject.count;
            //var b = StateObject.BufferSize;
            //if (StateObject.count == StateObject.BufferSize)
            //{
            //    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
            //        content.Length, content);
            //    // Echo the data back to the client.
            //    Send(handler, content);
            //}
            //else
            //{
            //    // Not all data received. Get more.
            //    StateObject.count++;
            //    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            //    new AsyncCallback(ReadCallback), state);
            //}
        }
    }

    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);  
  
        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,  
            new AsyncCallback(SendCallback), handler);
        //Thread.Sleep(3000);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket) ar.AsyncState;  
  
            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);  
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);  
  
            handler.Shutdown(SocketShutdown.Both);  
            handler.Close();  
  
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());  
        }  
    }
    public static int Main(string[] args)
    {
        StartListening();
        return 0;
    }
}