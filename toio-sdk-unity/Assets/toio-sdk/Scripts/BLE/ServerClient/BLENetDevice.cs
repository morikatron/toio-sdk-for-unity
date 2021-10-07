using System;
using Cysharp.Threading.Tasks;
using toio;

public class BLENetDevice : BLEDeviceInterface
{
    public BLENetService service { get; private set; }
    public BLENetServer server { get; private set; }

    private Action<BLEPeripheralInterface> scanAction;

    public BLENetDevice(BLENetService _service, BLENetServer _server)
    {
        this.service = _service;
        this.server = _server;
        this.server.RegisterJoinPeripheralCallback("device", OnJoinPeripheral);
    }

    public void Scan(String[] serviceUUIDs, bool rssiOnly, Action<BLEPeripheralInterface> action)
    {
        this.server.Start();
        this.scanAction = action;
    }

    public void StopScan()
    {

    }

    public UniTask Disconnect(Action action)
    {
        return UniTask.FromResult<object>(null);
    }

    public UniTask Enable(bool enable, Action action)
    {
        return UniTask.FromResult<object>(null);
    }

    private void OnJoinPeripheral(BLENetServer _, BLEPeripheralInterface peripheral)
    {
        if (null != this.scanAction)
        {
            this.scanAction.Invoke(peripheral);
        }
    }
}
