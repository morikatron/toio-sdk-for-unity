using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace toio
{
    public abstract class CubeReal : Cube
    {
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      プロパティ
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        public override bool isConnected { get { return this.peripheral.isConnected; } }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      純粋仮想関数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        protected abstract void Recv_battery(byte[] data);
        protected abstract void Recv_Id(byte[] data);
        protected abstract void Recv_button(byte[] data);
        protected abstract void Recv_sensor(byte[] data);

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      定数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        public const string SERVICE_ID = "10B20100-5B3B-4571-9508-CF3EFCD7BBAE";
        public const string CHARACTERISTIC_CONFIG = "10B201FF-5B3B-4571-9508-CF3EFCD7BBAE";
        public const string CHARACTERISTIC_ID = "10B20101-5B3B-4571-9508-CF3EFCD7BBAE";
        public const string CHARACTERISTIC_SENSOR = "10B20106-5B3B-4571-9508-CF3EFCD7BBAE";
        public const string CHARACTERISTIC_BUTTON = "10B20107-5B3B-4571-9508-CF3EFCD7BBAE";
        public const string CHARACTERISTIC_BATTERY = "10B20108-5B3B-4571-9508-CF3EFCD7BBAE";
        public const string CHARACTERISTIC_MOTOR = "10B20102-5B3B-4571-9508-CF3EFCD7BBAE";
        public const string CHARACTERISTIC_LIGHT = "10B20103-5B3B-4571-9508-CF3EFCD7BBAE";
        public const string CHARACTERISTIC_SOUND = "10B20104-5B3B-4571-9508-CF3EFCD7BBAE";

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      内部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        public BLEPeripheralInterface peripheral { get; private set; }
        public Dictionary<string, BLECharacteristicInterface> characteristicTable { get; private set; }

        public CubeReal(BLEPeripheralInterface peripheral, Dictionary<string, BLECharacteristicInterface> characteristicTable)
        {
            this.peripheral = peripheral;
            this.characteristicTable = characteristicTable;
            this.isPressed = false; // 初期値:非押下
            this.isSloped = false; // 初期値:水平
            this.isCollisionDetected = false; // 初期値:非衝突
            this.battery = 100;

            // idが無効となったため、addrで代用
            this.id = addr;
        }

        /// <summary>
        /// 自動通知機能の購読を開始する
        /// </summary>
        public async UniTask StartNotifications()
        {
            characteristicTable[CHARACTERISTIC_BATTERY].StartNotifications(this.Recv_battery);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#else
            await UniTask.Delay(1);
#endif
            characteristicTable[CHARACTERISTIC_ID].StartNotifications(this.Recv_Id);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#endif
            this.characteristicTable[CHARACTERISTIC_BUTTON].StartNotifications(this.Recv_button);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#endif
            this.characteristicTable[CHARACTERISTIC_SENSOR].StartNotifications(this.Recv_sensor);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#endif
        }

        protected void Request(string characteristicName, byte[] buff, bool withResponse, Cube.ORDER_TYPE order, string DEBUG_name, params object[] DEBUG_plist)
        {
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => this.characteristicTable[characteristicName].WriteValue(buff, withResponse), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrder(this, () => this.characteristicTable[characteristicName].WriteValue(buff, withResponse), order, DEBUG_name, DEBUG_plist);
#endif
        }
    }
}