using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/*
 * SocketServer.cs
 * ソケット通信（サーバ）
 * Unityアプリ内にサーバを立ててメッセージの送受信を行う
 * https://blog.applibot.co.jp/2018/08/13/socket-communication-with-unity/
 */
public class TCPServer
{
    protected TcpListener _listener;
    protected readonly List<TcpClient> clients = new List<TcpClient>();
    protected readonly Dictionary<ulong, TcpClient> clientTable = new Dictionary<ulong, TcpClient>();
    public bool IsConnected { get { return 0 <clients.Count; } }
    public bool IsStarted { get; private set; }

    public TCPServer()
    {
        this.IsStarted = false;
    }

    // ソケット接続準備、待機
    protected void Listen(string host, int port)
    {
        //Debug.LogFormat("[SocketServer]Listen. addr={0} port={1}", host, port);
        var ip = IPAddress.Parse(host);
        _listener = new TcpListener(ip, port);
        _listener.Start();
        _listener.BeginAcceptSocket(DoAcceptTcpClientCallback, _listener);
        _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
        this.IsStarted = true;
    }

    protected string ListenV6(string host, int port)
    {
        //Debug.LogFormat("[SocketServer]Listen. addr={0} port={1}", host, port);
        _listener = new TcpListener(IPAddress.IPv6Any, port);
        _listener.Start();
        _listener.BeginAcceptSocket(DoAcceptTcpClientCallback, _listener);
        _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
        return ((IPEndPoint)_listener.LocalEndpoint).Address.ToString();
    }

    public static string GetIPAddress()
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

    // クライアントからの接続処理
    private void DoAcceptTcpClientCallback(IAsyncResult ar)
    {
        var listener = (TcpListener)ar.AsyncState;
        var client = listener.EndAcceptTcpClient(ar);

        var endpoint = (IPEndPoint)client.Client.RemoteEndPoint;

        var hostid = GetHostID(endpoint.Address.ToString(), endpoint.Port);

        if(!clientTable.ContainsKey(hostid))
        {
            clientTable.Add(hostid, client);
            clients.Add(client);
        }

        //Debug.LogFormat("[SocketServer]Connected. addr={0} port={1}", endpoint.Address, endpoint.Port);
        this.OnConnected(hostid, endpoint);

        // 接続が確立したら次の人を受け付ける
        listener.BeginAcceptSocket(DoAcceptTcpClientCallback, listener);

        // 今接続した人とのネットワークストリームを取得
        var stream = client.GetStream();

        var buf = new byte[UInt16.MaxValue];

        // 接続が切れるまで送受信を繰り返す
        while (client.Connected)
        {
            // ストリームから一時バッファに読み込む
            // NOTE:直接memorystreamに書き込んだほうがメモリ効率が良い
            int read = stream.Read(buf, 0, buf.Length);
            if (0 < read)
            {
                //Debug.Log("recv : " + Bytes2String(buf, read));
                OnRecvData(hostid, buf, read);
            }

            // クライアントの接続が切れたら
            if (client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0))
            {
                Debug.Log("Disconnect: " + client.Client.RemoteEndPoint);
                var ip = client.Client.RemoteEndPoint as IPEndPoint;
                this.OnDisconnected(ip);
                client.Close();
                clients.Remove(client);
                clientTable.Remove(hostid);
                break;
            }
        }
    }

    protected virtual void OnConnected(ulong hostid, IPEndPoint endpoint) { }

    protected virtual void OnDisconnected(IPEndPoint endpoint) { }

    // メッセージ受信
    protected virtual void OnRecvData(ulong hostid, byte[] recvbuffer, int size) { }

    // クライアントにメッセージ送信
    public void SendMessageToClient(TcpClient client, byte[] data)
    {
        if (clients.Count == 0)
        {
            return;
        }

        try
        {
            var stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }
        catch
        {
            clients.Remove(client);
        }
    }

    public void SendMessageToAllClient(byte[] data)
    {
        if (clients.Count == 0)
        {
            return;
        }

        // 全員に同じメッセージを送る
        foreach (var client in clients)
        {
            SendMessageToClient(client, data);
        }
    }

    // 終了処理
    public void Close()
    {
        if (_listener == null)
        {
            return;
        }

        if (clients.Count != 0)
        {
            foreach (var client in clients)
            {
                client.Close();
            }
        }
        _listener.Stop();
    }

    public static ulong GetHostID(string addr, int port)
    {
        return ulong.Parse(addr.Replace(".", "") + port.ToString());
    }

    public static string Bytes2String(byte[] buff, int size)
    {
        if (0 == size) { return ""; }
        var sb = new StringBuilder(size);
        for (int i = 0; i < size; i++)
        {
            sb.Append(buff[i] + ",");
        }
        sb = sb.Remove(sb.Length-1, 1);
        return sb.ToString();
    }
}