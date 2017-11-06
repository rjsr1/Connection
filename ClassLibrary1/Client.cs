using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using Connection;

/*
public class StateObject
{
    // Client socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 1024;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
    // Received data string.
    public StringBuilder sb = new StringBuilder();
}
*/



public class Client
{
    private Socket clientSocket;
    private string ip;
    private int port;

    // ManualResetEvent instances signal completion.
    private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);

    // The response from the remote device.
    private string response = string.Empty;

    public Client(int port, string ip)
    {
        this.ip = ip;
        this.port = port;
    }
    public void StartClient()
    {
        // Connect to a remote device.
        try
        {
            // Establish the remote endpoint for the socket.
            // The name of the             

            IPAddress ipAddress = IPAddress.Parse(this.ip);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, this.port);

            // Create a TCP/IP socket.
            this.clientSocket = new Socket(AddressFamily.InterNetwork,
                 SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            clientSocket.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), clientSocket);
            connectDone.WaitOne();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }



    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.
            client.EndConnect(ar);
            Console.WriteLine("Conectado!!");
            Receive(this.clientSocket);


            // Signal that the connection has been made.
            connectDone.Set();
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public void Receive(Socket client)
    {
        try
        {
            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = client;
            if (this.GetSocket().Connected)
            {
                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
        }
        catch (ObjectDisposedException ode)
        {
            HandleObjectDesposed(ode);
        }
        catch (SocketException e)
        {
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        if (this.clientSocket.Connected)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    string content = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                    state.sb.Append(content);


                    String endOfMessage = Connection_Util.ASCIITag(4);

                    if (content.EndsWith(endOfMessage))
                    {
                        // All the data has arrived; put it in response.
                        if (state.sb.Length > 1)
                        {
                            this.response = state.sb.ToString();
                            this.response = RemoveEndOfMessage(response);
                            HandleReceivedData();
                        }
                        // Signal that all bytes have been received.
                        receiveDone.Set();

                    }
                    else
                    {
                        // Get the rest of the data.
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state);

                    }

                }

            }
            catch (ObjectDisposedException ode)
            {
                throw;
            }
            catch (SocketException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
    private string RemoveEndOfMessage(string s)
    {
        return s.Remove(s.Length - 1);
    }

    public void EndMessage()
    {

        Socket handler = this.clientSocket;

        String endOfMessage = Connection_Util.ASCIITag(4);

        //get bytes for endMassage code ASCII
        byte[] byteData = Encoding.ASCII.GetBytes(endOfMessage);
        try
        {
            if (this.clientSocket.Connected)
            {
                //send endOfMessage code to server
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
            }
            else
            {
                HandleSocketDisconnected();
            }
        }
        catch (ObjectDisposedException ode)
        {
            throw;
        }
        catch (SocketException e)
        {
            throw;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public void Send(String data)
    {
        Socket client = this.clientSocket;
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);
        try
        {
            if (this.clientSocket.Connected)
            {
                // Begin sending the data to the remote device.
                client.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), client);
            }
           
        }
        catch (ObjectDisposedException ode)
        {
            throw;
        }
        catch (SocketException e)
        {
            throw;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = client.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to server.", bytesSent);

            // Signal that all bytes have been sent.
            sendDone.Set();
        }
        catch (SocketException e)
        {
            throw;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private void HandleSocketDisconnected()
    {
        Console.WriteLine("Você está desconectado do Servidor.Deseja se reconectar? sim/nao");
        String response = Console.ReadLine();
        if (response == "sim")
        {
            StartClient();
        }
    }

    public void HandleReceivedData()
    {
        Console.WriteLine(this.GetSocketReceiveResponse());
        Receive(this.clientSocket);
    }
    public string GetSocketReceiveResponse()
    {
        // Console.WriteLine(this.response+"essa eh a resposta");
        return this.response;
    }

    public Socket GetSocket()
    {
        return this.clientSocket;
    }
    public void ReleaseSocket()
    {        
        this.clientSocket.Shutdown(SocketShutdown.Both);
        this.clientSocket.Close();

    }
    public void Disconnect()
    {
        try
        {
            clientSocket.BeginDisconnect(false, new AsyncCallback(DisconnectCallBack), this.clientSocket);
        }
        catch (ObjectDisposedException ode)
        {
            throw;
        }
        catch (Exception e)
        {
            throw;
        }
    }
    private void DisconnectCallBack(IAsyncResult ar)
    {
        try
        {
            Socket handler = (Socket)ar.AsyncState;
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public void HandleObjectDesposed(ObjectDisposedException ode)
    {
        Console.WriteLine(ode.Message);
        Console.WriteLine(ode.StackTrace);

    }

}
