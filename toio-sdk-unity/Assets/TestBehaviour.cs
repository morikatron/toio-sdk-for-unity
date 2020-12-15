using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;
using toio.Android;
using UnityEngine.Android;

public class TestBehaviour : MonoBehaviour
{
    BleJavaWrapper bleJavaWrapper;

    // Start is called before the first frame update
    void Start()
    {
        bleJavaWrapper = new BleJavaWrapper();
        bleJavaWrapper.Initialize();

       // Permission.RequestUserPermission(Permission.CoarseLocation);
    }

    // Update is called once per frame
    void Update()
    {
        bleJavaWrapper.UpdateScanResult();
    }
    public void OnScan()
    {
        bleJavaWrapper.StartScan("10B20100-5B3B-4571-9508-CF3EFCD7BBAE");
    }
}
