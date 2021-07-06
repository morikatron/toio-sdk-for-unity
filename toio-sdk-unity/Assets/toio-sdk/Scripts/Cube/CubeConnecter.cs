using System;
//using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    public interface CubeConnecterInterface
    {
        UniTask<Cube> Connect(BLEPeripheralInterface peripheral);
        UniTask<Cube[]> Connect(BLEPeripheralInterface[] peripherals);
        void Disconnect(Cube cube);
        [Obsolete("Deprecated. Please use ReConnect(Cube cube) instead.", false)]
        UniTask ReConnect(Cube cube, BLEPeripheralInterface peripheral);
        UniTask ReConnect(Cube cube);
    }

    /// <summary>
    /// CoreCubeのプロトコルバージョンを参照し, バージョンに応じたCubeクラスを生成.
    /// </summary>
    public class CubeConnecter : CubeConnecterInterface
    {
        private CubeConnecterInterface impl;

        public CubeConnecter(ConnectType type = ConnectType.Auto)
        {
            if (ConnectType.Auto == type)
            {
#if (UNITY_EDITOR || UNITY_STANDALONE)
                this.impl = new SimImpl();
#elif (UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL)
                this.impl = new RealImpl();
#endif
            }
            else if (ConnectType.Simulator == type)
            {
                this.impl = new SimImpl();
            }
            else if (ConnectType.Real == type)
            {
                this.impl = new RealImpl();
            }
        }

        public async UniTask<Cube> Connect(BLEPeripheralInterface peripheral)
        {
            return await this.impl.Connect(peripheral);
        }
        public async UniTask<Cube[]> Connect(BLEPeripheralInterface[] peripherals)
        {
            return await this.impl.Connect(peripherals);
        }
        public void Disconnect(Cube cube)
        {
            this.impl.Disconnect(cube);
        }
        public async UniTask ReConnect(Cube cube, BLEPeripheralInterface peripheral)
        {
            await this.impl.ReConnect(cube);
        }
        public async UniTask ReConnect(Cube cube)
        {
            await this.impl.ReConnect(cube);
        }

        public class SimImpl : CubeConnecterInterface
        {
            public async UniTask<Cube> Connect(BLEPeripheralInterface peripheral)
            {
                var uniperipheral = peripheral as UnityPeripheral;
                var cube = new CubeUnity(uniperipheral.obj);
                while (!cube.isConnected) await UniTask.Delay(50);
                cube.Init();
                return cube as Cube;
            }

            public async UniTask<Cube[]> Connect(BLEPeripheralInterface[] peripherals)
            {
                if (peripherals == null) return default;
                var uniperipherals = peripherals as UnityPeripheral[];
                var cubes = Array.ConvertAll<UnityPeripheral, CubeUnity>(uniperipherals, peri => new CubeUnity(peri.obj));
                foreach (var cube in cubes)
                {
                    while (!cube.isConnected)
                        await UniTask.Delay(50);
                    cube.Init();
                }
                return cubes as Cube[];
            }
            public void Disconnect(Cube cube)
            {
            }
            public UniTask ReConnect(Cube cube, BLEPeripheralInterface peripheral)
            {
                return default;
            }
            public UniTask ReConnect(Cube cube)
            {
                return default;
            }
        }


        public class RealImpl : CubeConnecterInterface
        {
            private bool isConnecting = false;

            public async UniTask<Cube> Connect(BLEPeripheralInterface peripheral)
            {
                try
                {
                    while(this.isConnecting) { await UniTask.Delay(100); }

                    this.isConnecting = true;
                    var characteristicTable = await this.ConnectCharacteristics(peripheral);
                    var version = await this.GetProtocolVersion(characteristicTable[CubeReal.CHARACTERISTIC_CONFIG]);
                    CubeReal cube = null;
                    switch(version)
                    {
                        case "2.0.0":
                            cube = new CubeReal_ver2_0_0(peripheral, characteristicTable);
                            break;
                        case "2.1.0":
                            cube = new CubeReal_ver2_1_0(peripheral, characteristicTable);
                            break;
                        case "2.2.0":
                            cube = new CubeReal_ver2_2_0(peripheral, characteristicTable);
                            break;
                        default:
                            // Basically, BLE protocol version has backward compatibility,
                            // so consider unknown version as the latest version.
                            //
                            // TODO:
                            // - patch(build) number can be ignored (should be?)
                            // - major number should be checked
                            cube = new CubeReal_ver2_2_0(peripheral, characteristicTable);
                            break;
                    }
                    await cube.Initialize();
                    this.isConnecting = false;
                    return cube;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                    this.isConnecting = false;
                    return null;
                }
            }

            public async UniTask<Cube[]> Connect(BLEPeripheralInterface[] peripherals)
            {
                Cube[] cubes = new Cube[peripherals.Length];
                for (int i = 0; i < peripherals.Length; i++)
                {
                    cubes[i] = await this.Connect(peripherals[i]);
                }
                return cubes;
            }

            public void Disconnect(Cube cube)
            {
                (cube as CubeReal).peripheral.Disconnect();
            }

            public async UniTask ReConnect(Cube cube, BLEPeripheralInterface peripheral)
            {
                await ReConnect(cube);
            }

            public async UniTask ReConnect(Cube cube)
            {
                try
                {
                    var peripheral = (cube as CubeReal).peripheral;
                    while(this.isConnecting) { await UniTask.Delay(100); }

                    this.isConnecting = true;
                    var characteristicTable = await this.ConnectCharacteristics(peripheral);
                    (cube as CubeReal).SetCharacteristicTable(characteristicTable);
                    await (cube as CubeReal).Initialize();
                    this.isConnecting = false;
                }
                catch (System.Exception)
                {
                    this.isConnecting = false;
                }
            }

            /// <summary>
            /// CoreCubeの操作に必要な全ての機能と接続.
            /// </summary>
            protected virtual async UniTask<Dictionary<string, BLECharacteristicInterface>> ConnectCharacteristics(BLEPeripheralInterface peripheral, float waitSeconds = 10.0f)
            {
                float startTime = Time.time;

                var characteristicTable = new Dictionary<string, BLECharacteristicInterface>();
                characteristicTable.Add(CubeReal.CHARACTERISTIC_CONFIG, null);
                characteristicTable.Add(CubeReal.CHARACTERISTIC_ID, null);
                characteristicTable.Add(CubeReal.CHARACTERISTIC_SENSOR, null);
                characteristicTable.Add(CubeReal.CHARACTERISTIC_BUTTON, null);
                characteristicTable.Add(CubeReal.CHARACTERISTIC_BATTERY, null);
                characteristicTable.Add(CubeReal.CHARACTERISTIC_MOTOR, null);
                characteristicTable.Add(CubeReal.CHARACTERISTIC_LIGHT, null);
                characteristicTable.Add(CubeReal.CHARACTERISTIC_SOUND, null);

                peripheral.Connect((chara) =>
                {
                    if (chara.serviceUUID == CubeReal.SERVICE_ID)
                        characteristicTable[chara.characteristicUUID] = chara;
                });

                bool isBreak = true;
                while (true)
                {
                    await UniTask.Delay(100);
                    isBreak = true;
                    foreach (var chara in characteristicTable.Values)
                    {
                        if (null == chara)
                        {
                            isBreak = false;
                            break;
                        }
                    }
                    if (isBreak)
                    {
                        break;
                    }

                    if (waitSeconds < (Time.time - startTime))
                    {
                        return null;
                    }
                }

                return characteristicTable;
            }

            /// <summary>
            /// CoreCubeのプロトコルバージョンを取得.
            /// 必要な遅延処理は機種によって異なります, デフォルトでは安全性優先で長めの遅延時間となっています.
            /// 遅延時間を変更したい場合はoverrideを推奨.
            /// </summary>
            protected virtual async UniTask<string> GetProtocolVersion(BLECharacteristicInterface config)
            {
                await UniTask.Delay(500);

                var start_time = Time.time;
                string version = null;
                while(null == version)
                {
                    byte[] buff = new byte[2];
                    buff[0] = 1;
                    buff[1] = 0;
                    config.WriteValue(buff, true);

                    await UniTask.Delay(500);

                    config.ReadValue(((chara, resultBuff) =>
                    {
                        version = System.Text.Encoding.UTF8.GetString(resultBuff, 2, resultBuff.Length-2);
                    }));

                    await UniTask.Delay(500);
                }

                return version;
            }
        }
    }
}
