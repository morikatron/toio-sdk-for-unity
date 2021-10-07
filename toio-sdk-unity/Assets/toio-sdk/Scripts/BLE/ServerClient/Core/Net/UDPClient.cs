using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class UDPClient
{
    public string addr { get; private set; }
    public int port { get; private set; }
    public UdpClient udpClient { get; private set; }
    public IPEndPoint endPoint { get; private set; }

    public bool Connect(string addr, int port)
    {
        return this.Connect(new IPEndPoint(IPAddress.Parse(addr), port));
    }
    public bool Connect(IPEndPoint endPoint)
    {
        if (null == this.udpClient)
        {
            this.addr = addr;
            this.port = port;
            this.endPoint = endPoint;
            this.udpClient = new UdpClient();
        }

        try
        {
            this.udpClient.Connect(this.endPoint);
        }
        catch(Exception ex)
        {
            Debug.Log(ex.ToString());
            return false;
        }
        return true;
    }

    public void SendData(byte[] buff)
    {
        this.udpClient.BeginSend(buff, buff.Length, this.SendCallback, this.udpClient);
    }

    private void SendCallback(IAsyncResult ar)
    {
        UdpClient udp = (UdpClient)ar.AsyncState;

        try
        {
            udp.EndSend(ar);
        }
        catch(SocketException ex)
        {
            Debug.LogFormat("send error. msg: {0}, code: {1}", ex.Message, ex.ErrorCode);
            return;
        }
        catch(ObjectDisposedException)
        {
            Debug.Log("既にソケットが閉じられています");
            return;
        }
    }
}