

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
namespace toio.Windows.Data {
    internal struct BleWriteRequestData
    {
        public BleCharastericsKeyInfo charastericsInfo;
        public WriteRequestHandler handle;
        public Action<string, string> didWriteCharacteristicAction;

        public BleWriteRequestData(BleCharastericsKeyInfo chInfo,WriteRequestHandler h, Action<string, string> act)
        {
            this.charastericsInfo = chInfo;
            this.handle = h;
            this.didWriteCharacteristicAction = act;
        }
    }
    internal struct BleReadRequestData
    {
        public BleCharastericsKeyInfo charastericsInfo;
        public ReadRequestHandler handle;
        public Action<string, string, byte[]> didReadChracteristicAction;

        public BleReadRequestData(BleCharastericsKeyInfo chInfo, ReadRequestHandler h, Action<string, string, byte[]> act)
        {
            this.charastericsInfo = chInfo;
            this.handle = h;
            this.didReadChracteristicAction = act;
        }
    }
    internal struct BleNotifyData
    {
        public Action<string, string, byte[]> notifiedCharacteristicAction;
        public BleNotifyData(Action<string, string, byte[]> act)
        {
            this.notifiedCharacteristicAction = act;
        }
    }
}
#endif