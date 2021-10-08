using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using toio;


public partial class BLENetServer
{
    public const int STATIC_BUFFER_SIZE = UInt16.MaxValue * 2; // 131070

    private static object memLock = new object();
    // server
    private string listenAddr;
    private int listenPort;
    private int sendPort;
    private UDPServer serverComp;
    // リモートホスト
    private Dictionary<string, BLENetRemoteHost> safeHostTable = new Dictionary<string, BLENetRemoteHost>();
    // 共有メモリ
    private Dictionary<string, ManagedMemoryStream> safeMemoryBook { get; set; }
    // メインスレッドワーカー
    private GameObject mainthreadObject { get; set; }
    private FunctionWorker mainthreadFuncWorker { get; set; }
    private BLENetServer.UnityWorker mainthreadProvider { get; set; }

    public BLENetServer(string listenAddr, int listenPort, int sendPort)
    {
        this.listenAddr = listenAddr;
        this.listenPort = listenPort;
        this.sendPort = sendPort;

        this.mainthreadObject = new GameObject();
        this.mainthreadObject.name = "~BLE.Net.Server.mainthreadObject";
        this.mainthreadFuncWorker = this.mainthreadObject.AddComponent<FunctionWorker>();
        this.mainthreadProvider = this.mainthreadObject.AddComponent<UnityWorker>();
        this.safeMemoryBook = new Dictionary<string, ManagedMemoryStream>();
        // udp
        this.serverComp = new UDPServer();
        // プロトコル
        this.mainthreadProvider.Initialize(this, this.MakeProtocolTable());
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

    private void UDP_OnRecvData(IPEndPoint remoteEP, byte[] recvbuffer, int size)
    {
        lock (memLock)
        {
            var ip = remoteEP.Address.ToString();
            if (!this.safeMemoryBook.ContainsKey(ip))
            {
                Debug.LogFormat("<color=#00ff00><b>■</b>---  接続開始 ip={0}, port={1}  ---<b>■</b></color>", remoteEP.Address, remoteEP.Port);
                // 静的バッファとして最初にSTATIC_BUFFER_SIZE分のメモリを確保
                // メモリ自動拡張にした場合、メモリ増加時に内部的にディープメモリコピーが行われ、
                // メインスレッドから参照するバイト配列の先頭アドレスが割り込み変更される可能性があるため
                this.safeMemoryBook.Add(ip, new ManagedMemoryStream(STATIC_BUFFER_SIZE));
            }
            if (!this.safeHostTable.ContainsKey(ip))
            {
                var udp = new UDPClient();
                udp.Connect(ip, this.sendPort);
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
                        this.protocolTable[order].Invoke(remoteHost, workbuff);
                    }
                }
            }
        }

        void OnApplicationQuit()
        {
            this.owner.Close();
        }
    }
}
