using System;
using System.Net;
using System.Collections.Generic;

public class BLENetRemoteHost
{
    public string hostid { get; private set; }
    public IPEndPoint endpoint { get; private set; }
    public UDPClient udpClient { get; private set; }
    // ペリフェラル
    private Dictionary<int, BLENetPeripheral> peripheralTable = new Dictionary<int, BLENetPeripheral>();

    public BLENetRemoteHost(string hostid, IPEndPoint _endpoint, UDPClient client)
    {
        this.hostid = hostid;
        this.endpoint = _endpoint;
        this.udpClient = client;
    }

    public void AddPeripheral(int localIndex, BLENetPeripheral peripheral)
    {
        this.peripheralTable.Add(localIndex, peripheral);
    }

    public void RemovePeripheral(int localIndex)
    {
        this.peripheralTable.Remove(localIndex);
    }

    public BLENetPeripheral GetPeripheral(int localIndex)
    {
        if (this.peripheralTable.ContainsKey(localIndex))
        {
            return this.peripheralTable[localIndex];
        }
        return null;
    }
}