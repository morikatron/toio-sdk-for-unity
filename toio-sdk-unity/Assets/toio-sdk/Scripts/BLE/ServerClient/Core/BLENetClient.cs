using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using toio;

namespace toio.ble.net
{
    public class BLENetClient
    {
        public class CubeStatus
        {
            public CubeReal cube;
            public Dictionary<BLECharacteristicInterface, byte[]> status = new Dictionary<BLECharacteristicInterface, byte[]>();
            public CubeStatus(CubeReal cube)
            {
                this.cube = cube;
            }
        }

        // public
        public UDPServer listener { get; private set; } = new UDPServer();
        public UDPClient client { get; private set; } = new UDPClient();
        public Dictionary<byte, Action<byte[]>> protocolTable { get; set; } = new Dictionary<byte, Action<byte[]>>();
        // private
        private Dictionary<int, CubeStatus> cubeStatusTable = new Dictionary<int, CubeStatus>();
        private static object memlock = new object();
        private Queue<Action> backFuncQueue = new Queue<Action>();
        private Queue<Action> frontFuncQueue = new Queue<Action>();
        private byte[] workbuff = new byte[BLENetServer.STATIC_BUFFER_SIZE];

        public BLENetClient()
        {
            this.protocolTable.Add(BLENetProtocol.S2C_WRITE, this.OnRecv_WriteValue);
            this.protocolTable.Add(BLENetProtocol.S2C_READ, this.OnRecv_ReadValue);
        }

        public void Start(string listenAddr, int listenPort, string serverAddr, int serverPort)
        {
            // サーバー起動
            this.listener.Listen(listenAddr, listenPort, OnRecvData);
            // クライアント接続
            this.client.Connect(serverAddr, serverPort);
            this.client.SendData(BLENetProtocol.Encode_C2S_CONNECT(Convert.ToUInt16(listenPort)));
        }

        public void Update()
        {
            lock (memlock)
            {
                while(0 < this.backFuncQueue.Count)
                {
                    this.frontFuncQueue.Enqueue(this.backFuncQueue.Dequeue());
                }
            }
            while(0 < this.frontFuncQueue.Count)
            {
                this.frontFuncQueue.Dequeue().Invoke();
            }
            this.SendCubeStatus();
        }

        public void JoinCube(int index, CubeReal cube)
        {
            if (this.cubeStatusTable.ContainsKey(index) && this.cubeStatusTable[index].cube == cube)
            {
                Debug.LogError("index:" + index + " already exists.");
                return;
            }

            Action sendJoin = ()=>
            {
                var buff = BLENetProtocol.Encode_C2S_JOIN(index, cube);
                this.client.SendData(buff);
            };
            Action sendDisconnect = () =>
            {

            };

            cube.peripheral.AddConnectionListener("BLENetClient", (peri) =>
            {
                if (peri.isConnected)
                {
                    sendJoin();
                }
                if (!peri.isConnected)
                {
                    sendDisconnect();
                }
            });

            this.cubeStatusTable[index] = new CubeStatus(cube);
            foreach(var c in cube.characteristicTable)
            {
                c.Value.notifiedCallback.AddListener("BLENetClient", (chraID, data) =>
                {
                    this.cubeStatusTable[index].status[c.Value] = data;
                });
            }

            sendJoin();
        }

        private void OnRecvData(IPEndPoint remoteEP, byte[] recvbuffer, int size)
        {
            int recvSize = recvbuffer.Length;
            using (ManagedMemoryStream memory = new ManagedMemoryStream(recvbuffer))
            {
                UInt16 buffsize = 0;
                int read;
                byte order;
                while(true)
                {
                    memory.Read(this.workbuff, 2);
                    if (recvSize <= memory.readSize) { break; }
                    buffsize = BitConverter.ToUInt16(this.workbuff, 0);
                    memory.Read(this.workbuff, 1);
                    order = this.workbuff[0];
                    read = memory.Read(this.workbuff, buffsize-3);
                    if (read != buffsize-3) { Debug.Log("error"); break; }
                    this.protocolTable[order](this.workbuff);
                }
            }
        }

        private void SendCubeStatus()
        {
            if (0 == this.cubeStatusTable.Count) { return ; }

            var list = new List<(int localCubeIndex, BLECharacteristicInterface chara, byte[] data)>();
            foreach(var d in this.cubeStatusTable)
            {
                if (!d.Value.cube.isConnected) continue;
                foreach(var c in d.Value.status)
                {
                    list.Add((d.Key, c.Key, c.Value));
                }
                d.Value.status.Clear();
            }
            if (0 < list.Count)
            {
                var data = BLENetProtocol.Encode_C2S_SUBSCRIBE(list);
                this.client.SendData(data);
            }
        }

        private void SendReadCallback(int localCubeIndex, string characteristicUUID, byte[] data)
        {
            var buff = BLENetProtocol.Encode_C2S_READ_CALLBACK(localCubeIndex, characteristicUUID, data);
            this.client.SendData(buff);
        }

        private void OnRecv_WriteValue(byte[] data)
        {
            try
            {
                var readdata = BLENetProtocol.Decode_S2C_WRITE(data);
                foreach(var (localCubeIndex, charaID, buffer, withResponse) in readdata)
                {
                    // メインスレッドから呼び出し
                    this.backFuncQueue.Enqueue(() =>
                    {
                        if (this.cubeStatusTable.ContainsKey(localCubeIndex) && this.cubeStatusTable[localCubeIndex].cube.isConnected)
                            this.cubeStatusTable[localCubeIndex].cube.characteristicTable[charaID].WriteValue(buffer, withResponse);
                    });
                }
            }
            catch
            {
                // 握りつぶす…
            }
        }

        private void OnRecv_ReadValue(byte[] data)
        {
            try
            {
                var (localCubeIndex, charaID) = BLENetProtocol.Decode_S2C_READ(data);
                // メインスレッドから呼び出し => ReadValue実行
                this.backFuncQueue.Enqueue(() =>
                {
                    if (this.cubeStatusTable.ContainsKey(localCubeIndex) && this.cubeStatusTable[localCubeIndex].cube.isConnected)
                        this.cubeStatusTable[localCubeIndex].cube.characteristicTable[charaID].ReadValue((_, retData) =>
                        {
                            // メインスレッドから呼び出し => ReadValue結果を送信
                            this.backFuncQueue.Enqueue(() =>
                            {
                                SendReadCallback(localCubeIndex, charaID, retData);
                            });
                        });
                });
            }
            catch
            {
                // 握りつぶす…
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
    }
}