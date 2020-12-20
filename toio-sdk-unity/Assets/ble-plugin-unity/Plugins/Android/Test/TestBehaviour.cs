
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif

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

#if UNITY_ANDROID_RUNTIME
    private StringBuilder sb = new StringBuilder(256);
    private BleJavaWrapper bleJavaWrapper;
    private string addr = "";
#endif

    // Start is called before the first frame update
    IEnumerator Start()
    {
#if UNITY_ANDROID_RUNTIME
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
#else
        yield return null;
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID_RUNTIME
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
#endif
    }
    public void OnScan()
    {
#if UNITY_ANDROID_RUNTIME
        if (bleJavaWrapper != null)
        {
            Debug.Log("Start Scan!!!");
            bleJavaWrapper.StartScan("10B20100-5B3B-4571-9508-CF3EFCD7BBAE");
        }
#endif
    }

    public void OnConnect()
    {
#if UNITY_ANDROID_RUNTIME
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
#endif
    }


    public void OnExec()
    {
#if UNITY_ANDROID_RUNTIME
        if (bleJavaWrapper != null && !string.IsNullOrEmpty( this.addr))
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
            var service = "10B20100-5B3B-4571-9508-CF3EFCD7BBAE";
            var characteristic = "10b20102-5b3b-4571-9508-cf3efcd7bbae";
            bleJavaWrapper.WriteCharacteristic(this.addr, service, characteristic, data,
                data.Length, false);
        }
#endif
    }
}
