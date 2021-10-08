using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using toio;

public class BLENetService : BLEServiceInterface
{
    public BLENetServer server { get; private set; }
    public string tcpAddr { get; private set; }
    public int tcpPort { get; private set; }
    public string udpAddr { get; private set; }
    public int udpPort { get; private set; }

    public BLENetService(string udpAddr, int udpPort)
    {
        this.udpAddr = udpAddr;
        this.udpPort = udpPort;
    }

    public void RequestDevice(Action<BLEDeviceInterface> action)
    {
        this.server = new BLENetServer(this.udpAddr, this.udpPort, BLENetProtocol.C_UDP_PORT);
        if (null != action) action(new BLENetDevice(this, server));
    }

    public UniTask Enable(bool enable, Action action)
    {
        if (!enable) this.server.Close();
        return UniTask.FromResult<object>(null);
    }

    public void DisconnectAll()
    {
        this.server.Close();
    }
}
