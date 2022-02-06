using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace toio.ble.net
{
    public class UDPServer
    {
        public string addr { get; private set; }
        public int port { get; private set; }
        public UdpClient udpClient { get; private set; }
        public IPEndPoint endPoint { get; private set; }
        private Action<IPEndPoint, byte[], int> callback;
        public bool wasListened { get; private set; } = false;

        public void Listen(string addr, int port, Action<IPEndPoint, byte[], int> callback)
        {
            this.addr = addr;
            this.port = port;

            try
            {
                this.endPoint = new IPEndPoint(IPAddress.Parse(this.addr), this.port);
                this.udpClient = new UdpClient(this.endPoint);
                this.udpClient.BeginReceive(this.RecvCallback, this.udpClient);
            }
            catch
            {
                Debug.Log("listen failed");
            }

            this.callback = callback;
            this.wasListened = true;
        }

        protected void OnRecvData(IPEndPoint remoteEP, byte[] recvbuffer, int size)
        {
            if (null != callback)
            {
                this.callback.Invoke(remoteEP, recvbuffer, size);
            }
        }

        private void RecvCallback(IAsyncResult ar)
        {
            UdpClient udp = (UdpClient)ar.AsyncState;

            byte[] recvBuff = null;
            IPEndPoint remoteEP = null;
            try
            {
                recvBuff = udp.EndReceive(ar, ref remoteEP);
            }
            catch(SocketException ex)
            {
                Debug.LogFormat("recv error. msg: {0}, code: {1}", ex.Message, ex.ErrorCode);
                return;
            }
            catch(ObjectDisposedException)
            {
                Debug.Log("既にソケットが閉じられています");
                return;
            }

            this.OnRecvData(remoteEP, recvBuff, recvBuff.Length);

            udp.BeginReceive(this.RecvCallback, udp);
        }

        public static ulong GetHostID(string addr, int port)
        {
            return ulong.Parse(addr.Replace(".", "") + port.ToString());
        }
    }
}