using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;

namespace toio.ble.net
{
    public partial class BLENetServer
    {
        public const int STATIC_BUFFER_SIZE = UInt16.MaxValue * 2; // 131070

        private static object memLock = new object();
        // server
        private string listenAddr;
        private int listenPort;
        private UDPServer serverComp;
        // リモートホスト
        private Dictionary<string, BLENetRemoteHost> safeHostTable = new Dictionary<string, BLENetRemoteHost>();
        // 共有メモリ
        private Dictionary<string, ManagedMemoryStream> safeMemoryBook { get; set; }
        // メインスレッドワーカー
        private GameObject mainthreadObject { get; set; }
        private BLENetServer.UnityWorker mainthreadWorker { get; set; }

        public Dictionary<int, Func<bool>> FuncTable { get { return this.mainthreadWorker.FuncTable; } }

        public BLENetServer(GameObject workerObject, string listenAddr, int listenPort)
        {
            this.listenAddr = listenAddr;
            this.listenPort = listenPort;

            this.mainthreadObject = workerObject;
            this.mainthreadWorker = this.mainthreadObject.AddComponent<UnityWorker>();
            this.safeMemoryBook = new Dictionary<string, ManagedMemoryStream>();
            // udp
            this.serverComp = new UDPServer();
            // プロトコル
            this.mainthreadWorker.Initialize(this, this.MakeProtocolTable());
        }

        public void Start()
        {
            if (this.serverComp.wasListened) return;
            // UDPで指定したポートを開く
            this.serverComp.Listen(this.listenAddr, this.listenPort, UDP_OnRecvData);
        }

        public void SendMessageToClient(string hostid, byte[] data)
        {
            this.safeHostTable[hostid].udpClient?.SendData(data);
        }

        public void SendMessageToClient(BLENetRemoteHost remoteHost, byte[] data)
        {
            remoteHost.udpClient?.SendData(data);
        }

        public void Close()
        {
            this.serverComp?.udpClient?.Close();
            foreach(var remote in this.safeHostTable)
            {
                remote.Value.udpClient?.udpClient?.Close();
            }
        }

        public static string GetLocalIPAddress()
        {
            string ipaddress = "";
            IPHostEntry ipentry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in ipentry.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipaddress = ip.ToString();
                    if (ipaddress.Contains("192.168"))
                    {
                        break;
                    }
                }
            }
            return ipaddress;
        }

        private void UDP_OnRecvData(IPEndPoint remoteEP, byte[] recvbuffer, int size)
        {
            lock (memLock)
            {
                var ip = remoteEP.Address.ToString();
                if (!this.safeMemoryBook.ContainsKey(ip))
                {
                    // 静的バッファとして最初にSTATIC_BUFFER_SIZE分のメモリを確保
                    // メモリ自動拡張にした場合、メモリ増加時に内部的にディープメモリコピーが行われ、
                    // メインスレッドから参照するバイト配列の先頭アドレスが割り込み変更される可能性があるため
                    this.safeMemoryBook.Add(ip, new ManagedMemoryStream(STATIC_BUFFER_SIZE));
                }
                if (!this.safeHostTable.ContainsKey(ip))
                {
                    var udp = new UDPClient();
                    this.safeHostTable.Add(ip, new BLENetRemoteHost(ip, remoteEP, udp));
                }

                var mem = this.safeMemoryBook[ip];
                mem.Write(recvbuffer, size);
            }
        }

        public class UnityWorker : MonoBehaviour
        {
            private BLENetServer owner;
            private Dictionary<byte, Action<BLENetRemoteHost, byte[]>> protocolTable;
            private static byte[] workingBuffer = new byte[STATIC_BUFFER_SIZE];
            private static byte[] workbuff = new byte[STATIC_BUFFER_SIZE];
            public Dictionary<int, Func<bool>> FuncTable = new Dictionary<int, Func<bool>>();

            public void Initialize(BLENetServer owner, Dictionary<byte, Action<BLENetRemoteHost, byte[]>> protocolTable)
            {
                this.owner = owner;
                this.protocolTable = protocolTable;
            }

            void Update()
            {
                BLENetRemoteHost remoteHost;
                UInt16 buffsize = 0;
                int read;
                int recvSize = 0;
                byte order;
                foreach(var d in this.owner.safeMemoryBook)
                {
                    lock(memLock)
                    {
                        d.Value.ReadAll(workingBuffer);
                        recvSize = d.Value.writeSize;
                        d.Value.Reset();
                    }
                    if (0 == recvSize) { break; }

                    remoteHost = this.owner.safeHostTable[d.Key];
                    using (ManagedMemoryStream memory = new ManagedMemoryStream(workingBuffer))
                    {
                        // バッファ読み込み
                        // プロトコル単位で受信してる前提でデータ取得
                        while(true)
                        {
                            // 先頭のバッファサイズ符号を取得
                            memory.Read(workbuff, 2);
                            if (recvSize <= memory.readSize) { break; }
                            buffsize = BitConverter.ToUInt16(workbuff, 0);
                            memory.Read(workbuff, 1);
                            order = workbuff[0];
                            read = memory.Read(workbuff, buffsize-3);
                            if (read != buffsize-3) { Debug.Log("error"); break; }
                            //Debug.Log(BLENetProtocol.Bytes2String(workbuff, buffsize-3));
                            // workbuff is only body field
                            this.protocolTable[order].Invoke(remoteHost, workbuff);
                        }
                    }
                }

                List<int> deleteList = new List<int>();
                foreach(var action in this.FuncTable)
                {
                    if (action.Value.Invoke())
                        deleteList.Add(action.Key);
                }
                foreach(var key in deleteList)
                {
                    this.FuncTable.Remove(key);
                }
            }

            void OnApplicationQuit()
            {
                this.owner.Close();
            }
        }
    }
}