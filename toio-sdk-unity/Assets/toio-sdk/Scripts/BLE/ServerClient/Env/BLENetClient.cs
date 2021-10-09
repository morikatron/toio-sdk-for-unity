using System;
using UnityEngine;
using toio;
using System.Collections.Generic;
using System.Net;

public class BLENetClient : MonoBehaviour
{
    private CubeManager cubeManager = null;
    private List<Cube> joinedCubeList = new List<Cube>();

    private UDPServer listener = new UDPServer();
    private UDPClient client = null;
    private Dictionary<byte, Action<byte[]>> protocolTable = new Dictionary<byte, Action<byte[]>>();
    private FunctionWorker mainthreadFuncWorker = null;
    private byte[] workbuff = new byte[BLENetServer.STATIC_BUFFER_SIZE];
    private bool started = false;

    //private List<(int localCubeIndex, BLECharacteristicInterface chara, byte[] data)> recvDataList = new List<(int localCubeIndex, BLECharacteristicInterface chara, byte[] data)>();
    private Dictionary<int, Dictionary<BLECharacteristicInterface, byte[]>> recvDataTable = new Dictionary<int, Dictionary<BLECharacteristicInterface, byte[]>>();
    private float previousTIme = 0;

    void Awake()
    {
#if UNITY_EDITOR
        gameObject.SetActive(false);
        return;
#else
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;

        this.mainthreadFuncWorker = gameObject.AddComponent<FunctionWorker>();
    }

    async void Start()
    {
        cubeManager = new CubeManager();

        // サーバー起動
        var addr = GetIPAddress();
        var port = BLENetProtocol.C_UDP_PORT;
        this.listener.Listen(addr, port,　OnRecvData);
        // クライアント
        addr = "192.168.0.9";
        port = BLENetProtocol.S_PORT;
        this.client = new UDPClient();
        this.client.Connect(addr, port);

        this.protocolTable.Add(BLENetProtocol.S2C_WRITE, this.OnRecv_WriteValue);
        this.protocolTable.Add(BLENetProtocol.S2C_READ, this.OnRecv_ReadValue);
        if (false)
        {
            cubeManager.MultiConnectAsync(4, this, this.OnConnected);
        }
        else
        {
            await cubeManager.MultiConnect(4);
            List<CubeReal> reals = new List<CubeReal>();
            foreach(var c in cubeManager.cubes) { reals.Add(c as CubeReal); }
            SendJoinCubes(reals);

            //////// Characteristic Recv Data Table
            for(int i = 0; i < reals.Count; i++)
            {
                var r = reals[i];
                var idx = i;
                foreach(var c in r.characteristicTable)
                {
                    c.Value.notifiedCallback.AddListener("BLENetClient", (chraID, data) =>
                    {
                        if (!this.recvDataTable.ContainsKey(idx)) { this.recvDataTable[idx] = new Dictionary<BLECharacteristicInterface, byte[]>(); }
                        this.recvDataTable[idx][c.Value] = data;
                    });
                }
            }
        }
        started = true;
#endif
    }

    void Update()
    {
        if (!started) return;

        if (CubeOrderBalancer.intervalSec < Time.time - previousTIme)
        {
            previousTIme = Time.time;
        }
        this.SendCubeStatus();

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
                //Debug.Log(BLENetProtocol.Bytes2String(this.workbuff, buffsize-3));
                this.protocolTable[order](this.workbuff);
            }
        }
    }

    void OnConnected(Cube cube, CONNECTION_STATUS status)
    {
        if (status.IsNewConnected)
        {
            Cube c;
            for (int i = 0; i < cubeManager.cubes.Count; i++)
            {
                c = cubeManager.cubes[i];
                if (cube == c)
                {
                    SendJoinCube(i, c as CubeReal);
                    break;
                }
            }
        }
        else if (status.IsReConnected)
        {
            Debug.Log("re-connected!!");
        }
    }

    //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
    //      RPC API < recv >
    //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

    private void OnRecv_WriteValue(byte[] data)
    {
        try
        {
            var readdata = BLENetProtocol.Decode_S2C_WRITE(data);
            foreach(var (idx, charaID, buffer, withResponse) in readdata)
            {
                var cube = (this.cubeManager.cubes[idx] as CubeReal);
                // メインスレッドから呼び出し
                this.mainthreadFuncWorker.EnqueueFunc(() =>
                {
                    cube.characteristicTable[charaID].WriteValue(buffer, withResponse);
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
            var cube = (this.cubeManager.cubes[localCubeIndex] as CubeReal);
            // メインスレッドから呼び出し
            this.mainthreadFuncWorker.EnqueueFunc(() =>
            {
                cube.characteristicTable[charaID].ReadValue((_, retData) =>
                {
                    this.mainthreadFuncWorker.EnqueueFunc(() =>
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

    //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
    //      RPC API < send >
    //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

    private void SendCubeStatus()
    {
        if (0 == cubeManager.cubes.Count) { return ; }

        var list = new List<(int localCubeIndex, BLECharacteristicInterface chara, byte[] data)>();
        foreach(var d in this.recvDataTable)
        {
            foreach(var c in d.Value)
            {
                list.Add((d.Key, c.Key, c.Value));
            }
            d.Value.Clear();
        }
        if (0 < list.Count)
        {
            var data = BLENetProtocol.Encode_C2S_SUBSCRIBE(list);
            this.client.SendData(data);
        }
    }

    private void SendJoinCube(int index, CubeReal c)
    {
        var buff = BLENetProtocol.Encode_C2S_JOIN(index, c);
        this.client.SendData(buff);
    }

    private void SendJoinCubes(List<CubeReal> cubes)
    {
        var buff = BLENetProtocol.Encode_C2S_JOINS(cubes);
        this.client.SendData(buff);
    }

    private void SendReadCallback(int localCubeIndex, string characteristicUUID, byte[] data)
    {
        var buff = BLENetProtocol.Encode_C2S_READ_CALLBACK(localCubeIndex, characteristicUUID, data);
        this.client.SendData(buff);
    }

    //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
    //      static functions
    //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

    private byte[] Position2Bytes()
    {
        var cubes = cubeManager.cubes;

        int head = 3;
        int cubebody = 3 + 13;
        int body = 1 + (cubes.Count * cubebody);

        Cube c;
        byte[] buff = new byte[head+body];
#if !RELEASE
        if (UInt16.MaxValue < buff.Length) { Debug.LogErrorFormat("最大バッファサイズを超えました. プロトコルを変更して下さい."); }
#endif
        BLENetProtocol.WriteBytesU16(buff, 0, Convert.ToUInt16(buff.Length));
        buff[2] = BLENetProtocol.C2S_SUBSCRIBE;
        buff[3] = (byte)cubes.Count;
        int offset, suboffset;
        for (int i = 0; i < cubes.Count; i++)
        {
            c = cubes[i];

            offset = head + 1 + (i * cubebody);
            buff[offset] = (byte)i;
            buff[offset+1] = BLENetProtocol.Characteristic2ShortID(CubeReal.CHARACTERISTIC_ID);
            buff[offset+2] = 13;
            suboffset = 3;
            buff[offset+suboffset] = 1;
            BLENetProtocol.WriteBytesS16(buff, offset+suboffset+1, (Int16)c.pos.x);
            BLENetProtocol.WriteBytesS16(buff, offset+suboffset+3, (Int16)c.pos.y);
            BLENetProtocol.WriteBytesS16(buff, offset+suboffset+5, (Int16)c.angle);
            BLENetProtocol.WriteBytesS16(buff, offset+suboffset+7, (Int16)c.sensorPos.x);
            BLENetProtocol.WriteBytesS16(buff, offset+suboffset+9, (Int16)c.sensorPos.y);
            BLENetProtocol.WriteBytesS16(buff, offset+suboffset+11, (Int16)c.sensorAngle);
        }
        return buff;
    }

    private static string GetIPAddress()
    {
        string ipaddress = "";
        Debug.Log(Dns.GetHostName());
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