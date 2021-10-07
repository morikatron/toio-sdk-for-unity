using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using toio;

//https://qiita.com/haminiku/items/0661568d0e311c8e8381
//[ExecuteAlways]
public class TCPClient : MonoBehaviour
{
    // メッセージを管理するリスト
    //private List<string> messages = new List<string>();
    // Server
    private TcpClient tcp;
    private NetworkStream stream = null;
    private bool isStopReading = false;
    private byte[] readbuf;
    private int errorCnt = 0;
    private const int TRYLIMIT = 1;
    public bool error = false;
    public string addr;
    public int port;
    public bool ready = false;
    public TCallbackProvider<TCPClient, byte[]> recvCallback = new TCallbackProvider<TCPClient, byte[]>();

    public void SendData(byte[] data)
    {
        if (!ready || null == this) { return; }

        // サーバに送信
        StartCoroutine(SendMessageImpl(data));
    }

    public void Close()
    {
        this.stream.Close();
        this.tcp.Close();
    }

    private System.Collections.IEnumerator Start()
    {
        this.readbuf = new byte[32768];

        while (true)
        {
            if (this.error) { break; }
            if (!this.isStopReading) { StartCoroutine(ReadMessage()); }
            yield return null;
        }
    }

    private System.Collections.IEnumerator SendMessageImpl(byte[] data)
    {
        if (this.stream == null)
        {
            this.stream = GetNetworkStream();
            if (null == stream) { yield break; }
        }
        //サーバーにデータを送信する
        stream.Write(data, 0, data.Length);
        yield break;
    }

    private System.Collections.IEnumerator ReadMessage()
    {
        this.stream = GetNetworkStream();
        if (null != this.stream)
        {
            // 非同期で待ち受けする
            this.stream.BeginRead(this.readbuf, 0, this.readbuf.Length, new AsyncCallback(this.ReadCallback), null);
            this.isStopReading = true;
            this.ready = true;
        }
        yield return null;
    }

    private void ReadCallback(IAsyncResult ar)
    {
        int bytes = stream.EndRead(ar);
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(this.readbuf, 0, bytes);
            this.recvCallback.Notify(this, ms.ToArray());
        }
        this.isStopReading = false;
        return;
    }

    private void OnApplicationQuit()
    {
        if (null != this.stream)
        {
            this.stream.Close();
        }
    }

    private NetworkStream GetNetworkStream()
    {
        if (TRYLIMIT <= this.errorCnt) { this.error = true; return null; }

        if (this.stream != null && this.stream.CanRead)
        {
            return this.stream;
        }

        bool _error = false;
        try
        {
            Debug.Log("try create tcpclient addr=" + addr);

            //TcpClientを作成し、サーバーと接続する
            this.tcp = new TcpClient(addr, port);
        }
        catch(SocketException e)
        {
            _error = true;
            this.errorCnt++;
            Debug.Log(e.Message);
        }

        if (_error) { Debug.Log("failed craete tcpclient."); return null; }

        Debug.Log("success create tcpclient");

        //NetworkStreamを取得する
        return tcp.GetStream();
    }
}