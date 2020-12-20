using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;
using toio.Android;
using UnityEngine.Android;
using UnityEngine.UI;
using System.Text;

public class TestBehaviour : MonoBehaviour
{
    [SerializeField]
    private Text resultInfo;

    private StringBuilder sb = new StringBuilder(256);

    private BleJavaWrapper bleJavaWrapper;
    private string addr = "";

    // Start is called before the first frame update
    IEnumerator Start()
    {
        string permission = Permission.FineLocation;
        if (!Permission.HasUserAuthorizedPermission(permission))
        {
            Permission.RequestUserPermission(permission);
        }
        yield return null;
        yield return null;
        if (Permission.HasUserAuthorizedPermission(permission))
        {
            bleJavaWrapper = new BleJavaWrapper();
            bleJavaWrapper.Initialize();
        }
        else
        {
            Debug.LogError("No Permission");
        }
    }

    // Update is called once per frame
    void Update()
    {
        sb.Clear();
        if (bleJavaWrapper != null)
        {
            bleJavaWrapper.UpdateScannerResult();
            var devices = bleJavaWrapper.GetScannedDevices();
            foreach (var device in devices)
            {
                sb.Append(device.address).Append(" ").Append(device.rssi).Append("\n");
            }
        }
        this.resultInfo.text = sb.ToString();
    }
    public void OnScan()
    {
        if (bleJavaWrapper != null)
        {
            Debug.Log("Start Scan!!!");
            bleJavaWrapper.StartScan("10B20100-5B3B-4571-9508-CF3EFCD7BBAE");
        }
    }

    public void OnConnect()
    {
        if (bleJavaWrapper != null)
        {
            var devices = bleJavaWrapper.GetScannedDevices();
            if(devices.Count > 0)
            {
                this.addr = devices[0].address;
                bleJavaWrapper.ConnectRequest(devices[0].address);
                bleJavaWrapper.StopScan();
            }
        }
    }


    public void OnExec()
    {
        if(bleJavaWrapper != null && !string.IsNullOrEmpty( this.addr))
        {
            byte[] data = new byte[7];
            data[0] = 0x01;
            data[1] = 0x01;
            data[2] = 0x01;
            data[3] = 0x10;

            data[4] = 0x02;
            data[5] = 0x02;
            data[6] = 0x10;
            //            
            var characteristic = "10b20102-5b3b-4571-9508-cf3efcd7bbae";
            bleJavaWrapper.WriteCharacteristic(this.addr, characteristic, data,
                data.Length, false);
        }
    }
}
