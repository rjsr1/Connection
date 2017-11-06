using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Connection;

public class StateObject
{
    // Client  socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 1024;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
    // Received data string.
    public StringBuilder sb = new StringBuilder();
}

public class Server
{
    // Thread signal.
    public ManualResetEvent allDone = new ManualResetEvent(false);
    //permite dizer qual porta e ip
    private int port;
    private string ip;
    private int maxNumberOfClients;

    public Server(int port, string ip, int maxNumberOfClients)
    {
        this.port = port;
        this.ip = ip;
        this.maxNumberOfClients = maxNumberOfClients;
    }


    public void StartListening()
    {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // Establish the local endpoint for the socket.        
        //faz parse com string ip
        IPAddress ipAddress = IPAddress.Parse(this.ip);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, this.port);

        // Create a TCP/IP socket.
        Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(this.maxNumberOfClients);

            while (true)
            {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.               
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);

                // Wait until a connection is made before continuing.
                allDone.WaitOne();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

    }

    public void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.
        allDone.Set();

        // Get the socket that handles the client request.
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.
        StateObject state = new StateObject();
        state.workSocket = handler;
        try
        {
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }
    }



    private void ReadCallback(IAsyncResult ar)
    {
        try
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                content = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                state.sb.Append(content);
                /*   
                *   int unicode = 4;
                *    char character = (char)unicode;
                *   string endOfMessage = character.ToString();
                */
                //add endof message tag to content
                String endOfMessage = Connection_Util.ASCIITag(4);

                content = state.sb.ToString();

                //Send(handler, "Message Received");
                //check if message ends with endOfMessage. 
                if (content.EndsWith(endOfMessage))
                {
                    //call method that handler content received                
                    string result = state.sb.ToString();
                    result = RemoveEndOfMessage(result);
                    HandleContentReceived(result, handler);
                }
                else
                {
                    //read more data from client
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                }

            }
        }
        catch (Exception e)
        {
            Console.WriteLine("erro receive");
        }
    }
    private string RemoveEndOfMessage(string s)
    {
        return s.Remove(s.Length-1);
    }

    private void HandleContentReceived(string content, Socket handler)
    {

        Console.WriteLine("foi recebido esse conteudo: {0} deste socket {1}", content, handler.LocalEndPoint.ToString());
        //***************lembrar que criar metodo para endofmessage*********talvez seja isso que causa client não ler resultado

        String endOfMessage = Connection_Util.ASCIITag(4);
        String messageToClient = "recebi : " + content + endOfMessage;
        //EndMessage(handler);        


        //Send response to client
        Send(handler, messageToClient);

        //create new state from new messages
        StateObject state = new StateObject();
        state.workSocket = handler;


        //continue to receive data from client       
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                                      new AsyncCallback(ReadCallback), state);


    }
    public void EndMessage(Socket handler)
    {

        String endOfMessage = Connection_Util.ASCIITag(4);

        //get bytes for endMassage code ASCII
        byte[] byteData = Encoding.ASCII.GetBytes(endOfMessage);

        //send endOfMessage code to server
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }

    public void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }


    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = handler.EndSend(ar);

            Console.WriteLine("Sent {0} bytes to client.", bytesSent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public void Disconnect(Socket handler)
    {
        handler.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), handler);
    }

    private void DisconnectCallback(IAsyncResult ar)
    {
        Socket handler = (Socket)ar.AsyncState;
        handler.EndConnect(ar);
    }

}