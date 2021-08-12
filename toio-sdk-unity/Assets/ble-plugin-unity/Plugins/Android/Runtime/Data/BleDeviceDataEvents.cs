
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



#if UNITY_ANDROID_RUNTIME
namespace toio.Android.Data {
    public class BleDeviceDataEvents
    {
        Action<string, string, byte[]> readChracteristicAct;
        Action<string, string> writeCharacteristicAct;
        Action<string, string, byte[]> notifiedCharacteristicAct;
        Action<string> unsubscribeAct;

        public void SetReadAct(Action<string, string, byte[]> act)
        {
            this.readChracteristicAct = act;
        }
        public void CallRead(string service,string characteristic,byte[] data)
        {
            if( this.readChracteristicAct != null)
            {
                readChracteristicAct(service, characteristic, data);
            }
            readChracteristicAct = null;
        }

        public void SetWriteAct(Action<string, string> act)
        {
            this.writeCharacteristicAct = act;
        }
        public void CallWrite(string service,string characteristic)
        {
            if (this.writeCharacteristicAct != null)
            {
                this.writeCharacteristicAct(service, characteristic);
            }
            this.writeCharacteristicAct = null;
        }

        public void SetNotifyAct(Action<string, string, byte[]> act)
        {
            this.notifiedCharacteristicAct = act;
        }
        public void RemoveNotifyAct()
        {
            this.notifiedCharacteristicAct = null;
        }

        public void CallNotify(string service,string characteristic,byte [] data)
        {
            if (this.notifiedCharacteristicAct != null)
            {
                this.notifiedCharacteristicAct(service, characteristic, data);
            }
        }

        public void SetUnsubscribeAct(Action<string> act)
        {
            this.unsubscribeAct = act;
        }

        public void CallUnscrive(string str)
        {
            if(this.unsubscribeAct != null)
            {
                this.unsubscribeAct(str);
                this.unsubscribeAct = null;
            }
        }

    }

}
#endif