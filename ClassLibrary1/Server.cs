﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

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

    public Server(int port, string ip)
    {
        this.port = port;
        this.ip = ip;
    }

    public void StartListening()
    {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // Establish the local endpoint for the socket.
        // The DNS name of the computer
        // running the listener is "host.contoso.com".

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
            listener.Listen(100);

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
        try { 
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);
        }catch(Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }
    }



    private void ReadCallback(IAsyncResult ar)
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
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read 
            // more data.
            content = state.sb.ToString();
            //Send(handler, "Message Received");
            
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                       new AsyncCallback(ReadCallback), state);
            

        }
        else
        {
           
            // Not all data received. Get more.
            Console.WriteLine("menssagem concluida");
            HandleContentReceived(content,handler);           
        }

    }

    private void HandleContentReceived(string content,Socket handler)
    {
        //put here the code to handle content received from client
        Console.WriteLine("foi recebido esse conteudo: {0} deste socket {1}",content,handler.AddressFamily.ToString());
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