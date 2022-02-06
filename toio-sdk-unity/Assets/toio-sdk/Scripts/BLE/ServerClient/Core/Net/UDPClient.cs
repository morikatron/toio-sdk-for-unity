using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace toio.ble.net
{
    public class UDPClient
    {
        public string addr { get; private set; }
        public int port { get; private set; }
        public UdpClient udpClient { get; private set; }
        public IPEndPoint endPoint { get; private set; }
        public bool IsConnected { get { return null !=this.udpClient.Client && this.udpClient.Client.Connected; } }
        public Action SocketExceptAction { get; set; }

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
            if (this.IsConnected)
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
                this.udpClient.Close();
                this.udpClient = new UdpClient();
                this.SocketExceptAction?.Invoke();
                return;
            }
            catch(ObjectDisposedException)
            {
                Debug.Log("既にソケットが閉じられています");
                this.SocketExceptAction?.Invoke();
                return;
            }
        }
    }
}