using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class CubeReal_ver2_2_0 : CubeReal_ver2_1_0
    {
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      内部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected CallbackProvider _shakeCallback = new CallbackProvider();
        protected EventCallbackProvider _motorSpeedCallback = new EventCallbackProvider();
        private bool isInitialized = false;
        private bool needMotorSpeed = false;
        private bool isEnablingMotorSpeed = false;
        private bool isEnabledMotorSpeed = false;
        private int _leftSpeed = 0;
        private int _rightSpeed = 0;

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        public override bool isShake { get; protected set; }
        public override string version { get { return "2.2.0"; } }
        public override int leftSpeed
        { 
            get
            {
                this.needMotorSpeed = true;
                if (this.isEnabledMotorSpeed)
                    return this._leftSpeed;
                else if (this.isInitialized && !this.isEnablingMotorSpeed)
                    this.EnableMotorRead(true);                    
                return 0;
            }
            protected set { this._leftSpeed = value; }
        }
        public override int rightSpeed
        {
            get
            {
                this.needMotorSpeed = true;
                if (this.isEnabledMotorSpeed)
                    return this._rightSpeed;
                else if (this.isInitialized && !this.isEnablingMotorSpeed)
                    this.EnableMotorRead(true);                    
                return 0;
            }
            protected set { this._rightSpeed = value; }
        }

        // シェイクコールバック
        public override CallbackProviderInterface shakeCallback { get { return this._shakeCallback; } }
        public override CallbackProviderInterface motorSpeedCallback { get { return this._motorSpeedCallback; } }

        public CubeReal_ver2_2_0(BLEPeripheralInterface peripheral, Dictionary<string, BLECharacteristicInterface> characteristicTable)
        : base(peripheral, characteristicTable)
        {
            this._motorSpeedCallback.AddEventListener(
                "enableMotorSpeed", 
                EventCallbackProvider.EventType.ADD,
                (() => { this.needMotorSpeed = true; })
            );
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < send >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        // キューブのモーター速度情報の取得を有効化します
        private void EnableMotorRead(bool valid)
        {
            if (!this.isConnected) { return; }
            this.isEnablingMotorSpeed = true;
            byte[] buff = new byte[3];
            buff[0] = 0x1c;
            buff[1] = 0;
            buff[2] = BitConverter.GetBytes(valid)[0];
            this.Request(CHARACTERISTIC_CONFIG, buff, true, ORDER_TYPE.Strong, "EnableMotorRead", valid);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < subscribe >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        /// <summary>
        /// 自動通知機能の購読を開始する
        /// </summary>
        public override async UniTask Initialize()
        {
            await base.Initialize();
            this.characteristicTable[CHARACTERISTIC_MOTOR].StartNotifications(this.Recv_motor);
            this.characteristicTable[CHARACTERISTIC_CONFIG].StartNotifications(this.Recv_config);
#if !UNITY_EDITOR
            await UniTask.Delay(500);
#endif
            if (this.needMotorSpeed)
            {
                this.EnableMotorRead(true);
            }
            this.isInitialized = true;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < recv >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected override void Recv_sensor(byte[] data)
        {
            base.Recv_sensor(data);

            // https://toio.github.io/toio-spec/docs/ble_sensor
            int type = data[0];
            if (1 == type)
            {
                var _isShake = data[5] == 1 ? true : false;

                if (_isShake != this.isShake)
                {
                    this.isShake = _isShake;
                    this.shakeCallback.Notify(this);
                }
            }
        }

        //キューブのモーター速度情報を取得
        protected virtual void Recv_motor(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/ble_motor
            int type = data[0];
            if (0xe0 == type)
            {
                this.leftSpeed = data[1];
                this.rightSpeed = data[2];
                this.motorSpeedCallback.Notify(this);
            }
        }

        protected virtual void Recv_config(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/ble_configuration
            int type = data[0];
            if (0x9c == type)
            {
                this.isEnablingMotorSpeed = false;
                this.isEnabledMotorSpeed = (0x01 == data[2]);
            }
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      内部クラス
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected class EventCallbackProvider : CallbackProviderInterface
        {
            public enum EventType : int
            {
                ADD,
                REMOVE,
                CLEAR
            }

            private class Listener
            {
                public readonly string key;
                public readonly EventType type;
                public readonly Action action;
                public Listener(string _key, EventType _type, Action _action)
                {
                    this.key = _key;
                    this.type = _type;
                    this.action = _action;
                }
            }

            private CallbackProvider callbackProvider = new CallbackProvider();
            private Dictionary<string, Listener> listenerTable = new Dictionary<string, Listener>();
            private Dictionary<EventType, List<Listener>> listenerList = new Dictionary<EventType, List<Listener>>();

            public EventCallbackProvider()
            {
                this.listenerList.Add(EventType.ADD, new List<Listener>());
                this.listenerList.Add(EventType.REMOVE, new List<Listener>());
                this.listenerList.Add(EventType.CLEAR, new List<Listener>());
            }

            //
            public void AddEventListener(string key, EventType type, Action listener)
            {
                var l = new Listener(key, type, listener);
                this.listenerTable[key] = l;
                this.listenerList[type].Add(l);
            }
            public void RemoveEventListener(string key)
            {
                if (this.listenerTable.ContainsKey(key))
                {
                    var l = this.listenerTable[key];
                    this.listenerList[l.type].Remove(l);
                    this.listenerTable.Remove(key);
                }
            }
            public void ClearEventListener()
            {
                this.listenerTable.Clear();
                this.listenerList[EventType.ADD].Clear();
                this.listenerList[EventType.REMOVE].Clear();
                this.listenerList[EventType.CLEAR].Clear();
            }
            private void NotifyEvent(EventType type)
            {
                var list = this.listenerList[type];
                foreach(var l in list)
                {
                    l.action();
                }
            }

            //
            public void AddListener(string key, Action<Cube> listener)
            {
                this.NotifyEvent(EventType.ADD);
                this.callbackProvider.AddListener(key, listener);
            }
            public void RemoveListener(string key)
            {
                this.NotifyEvent(EventType.REMOVE);
                this.callbackProvider.RemoveListener(key);
            }
            public void ClearListener()
            {
                this.NotifyEvent(EventType.CLEAR);
                this.callbackProvider.ClearListener();
            }
            public void Notify(Cube target)
            {
                this.callbackProvider.Notify(target);
            }
        }
    }
}