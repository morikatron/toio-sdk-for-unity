using System;
using System.Collections.Generic;
using UnityEngine;

namespace toio
{
    public class CubeReal_ver2_0_0 : CubeReal
    {
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      内部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        protected Vector2 _pos = Vector2.zero;
        protected Vector2 _sensorPos = Vector2.zero;
        protected CallbackProvider _buttonCallback = new CallbackProvider();
        protected CallbackProvider _slopeCallback = new CallbackProvider();
        protected CallbackProvider _collisionCallback = new CallbackProvider();
        protected CallbackProvider _idCallback = new CallbackProvider();
        protected CallbackProvider _standardIdCallback = new CallbackProvider();
        protected CallbackProvider _idMissedCallback = new CallbackProvider();
        protected CallbackProvider _standardIdMissedCallback = new CallbackProvider();

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        public override string version { get { return "2.0.0"; } }
        public override string id { get; protected set; }
        public override string addr { get { return this.peripheral.device_address; } }
        public override int battery { get; protected set; }
        public override int x { get; protected set; }
        public override int y { get; protected set; }
        public override Vector2 pos { get { return this._pos; } }
        public override int angle { get; protected set; }
        public override Vector2 sensorPos { get { return this._sensorPos; } }
        public override int sensorAngle { get; protected set; }
        public override uint standardId { get; protected set; }
        public override bool isPressed { get; protected set; }
        public override bool isSloped { get; protected set; }
        public override bool isCollisionDetected { get; protected set; }
        public override bool isGrounded { get; protected set; }
        public override int maxSpd { get { return 100; } }

        // ボタンコールバック
        public override CallbackProvider buttonCallback { get { return this._buttonCallback; } }
        // 傾きコールバック
        public override CallbackProvider slopeCallback { get { return this._slopeCallback; } }
        // 衝突コールバック
        public override CallbackProvider collisionCallback { get { return this._collisionCallback; } }
        // 座標角度コールバック
        public override CallbackProvider idCallback { get { return this._idCallback; } }
        // StandardIDコールバック
        public override CallbackProvider standardIdCallback { get { return this._standardIdCallback; } }
        // ID Missedコールバック
        public override CallbackProvider idMissedCallback { get { return this._idMissedCallback; } }
        // StandardID Missedコールバック
        public override CallbackProvider standardIdMissedCallback { get { return this._standardIdMissedCallback; } }

        public CubeReal_ver2_0_0(BLEPeripheralInterface peripheral, Dictionary<string, BLECharacteristicInterface> characteristicTable)
        : base(peripheral, characteristicTable)
        {
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < send >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        /// <summary>
        /// キューブのモーターを制御します
        /// https://toio.github.io/toio-spec/docs/ble_motor#時間指定付きモーター制御
        /// </summary>
        /// <param name="left">左モーター速度</param>
        /// <param name="right">右モーター速度</param>
        /// <param name="durationMs">持続時間(ミリ秒)</param>
        /// <param name="order">命令の優先度</param>
        public override void Move(int left, int right, int durationMs, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
#if !RELEASE
            if (2550 < durationMs)
            {
                Debug.LogErrorFormat("[Cube.move]最大ミリ秒を超えました. durationMs={0}", durationMs);
            }
#endif
            if (!this.isConnected) { return; }

            var lSign = left > 0 ? 1 : -1;
            var rSign = right > 0 ? 1 : -1;
            var lDirection = left > 0 ? 1 : 2;
            var rDirection = right > 0 ? 1 : 2;
            var lPower = Mathf.Min(Mathf.Abs(left), maxSpd);
            var rPower = Mathf.Min(Mathf.Abs(right), maxSpd);
            var duration = Mathf.Clamp(durationMs / 10, 0, 255);

            byte[] buff = new byte[8];
            buff[0] = 2;
            buff[1] = 1;
            buff[2] = BitConverter.GetBytes(lDirection)[0];
            buff[3] = BitConverter.GetBytes(lPower)[0];
            buff[4] = 2;
            buff[5] = BitConverter.GetBytes(rDirection)[0];
            buff[6] = BitConverter.GetBytes(rPower)[0];
            buff[7] = BitConverter.GetBytes(duration)[0];

            this.Request(CHARACTERISTIC_MOTOR, buff, false, order, "move", left, right, durationMs);
        }

        /// <summary>
        /// キューブ底面についている LED を制御します
        /// https://toio.github.io/toio-spec/docs/ble_light#点灯-消灯
        /// </summary>
        /// <param name="red">赤色の強さ</param>
        /// <param name="green">緑色の強さ</param>
        /// <param name="blue">青色の強さ</param>
        /// <param name="durationMs">持続時間(ミリ秒)</param>
        /// <param name="order">命令の優先度</param>
        public override void TurnLedOn(int red, int green, int blue, int durationMs, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
#if !RELEASE
            if (2550 < durationMs)
            {
                Debug.LogErrorFormat("[Cube.turnLedOn]最大ミリ秒を超えました. duration={0}", durationMs);
            }
#endif
            if (!this.isConnected) { return; }

            byte[] buff = new byte[7];
            buff[0] = 3;
            buff[1] = BitConverter.GetBytes(Mathf.Clamp(durationMs / 10, 0, 255))[0];
            buff[2] = 1;
            buff[3] = 1;
            buff[4] = BitConverter.GetBytes(Mathf.Clamp(red, 0, 255))[0];
            buff[5] = BitConverter.GetBytes(Mathf.Clamp(green, 0, 255))[0];
            buff[6] = BitConverter.GetBytes(Mathf.Clamp(blue, 0, 255))[0];

            this.Request(CHARACTERISTIC_LIGHT, buff, true, order, "turnLedOn", red, green, blue, durationMs);
        }

        /// <summary>
        /// キューブ底面についている LED を連続的に制御します
        /// https://toio.github.io/toio-spec/docs/ble_light#連続的な点灯-消灯
        /// </summary>
        /// <param name="repeatCount">繰り返し回数</param>
        /// <param name="operations">命令配列</param>
        /// <param name="order">命令の優先度</param>
        public override void TurnOnLightWithScenario(int repeatCount, LightOperation[] operations, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
#if !RELEASE
            if (59 < operations.Length)
            {
                Debug.LogErrorFormat("[Cube.TurnOnLightWithScenario]最大発光数を超えました. operations.Length={0}", operations.Length);
            }
#endif
            if (!this.isConnected) { return; }

            repeatCount = Mathf.Clamp(repeatCount, 0, 255);
            var operation_length = Mathf.Clamp(operations.Length, 0, 59);

            byte[] buff = new byte[3 + operation_length * 6];
            buff[0] = 4;
            buff[1] = BitConverter.GetBytes(repeatCount)[0];
            buff[2] = BitConverter.GetBytes(operation_length)[0];

            for (int i = 0; i < operation_length; i++)
            {
                buff[3 + 6 * i] = BitConverter.GetBytes(Mathf.Clamp(operations[i].durationMs / 10, 1, 255))[0];
                buff[4 + 6 * i] = 1;
                buff[5 + 6 * i] = 1;
                buff[6 + 6 * i] = BitConverter.GetBytes(Mathf.Clamp(operations[i].red, 0, 255))[0];
                buff[7 + 6 * i] = BitConverter.GetBytes(Mathf.Clamp(operations[i].green, 0, 255))[0];
                buff[8 + 6 * i] = BitConverter.GetBytes(Mathf.Clamp(operations[i].blue, 0, 255))[0];
            }

            this.Request(CHARACTERISTIC_LIGHT, buff, true, order, "turnOnLightWithScenario", repeatCount);
        }

        /// <summary>
        /// キューブ底面についている LED を消灯させます
        /// https://toio.github.io/toio-spec/docs/ble_light#全てのランプを消灯
        /// </summary>
        /// <param name="order">命令の優先度</param>
        public override void TurnLedOff(ORDER_TYPE order = ORDER_TYPE.Weak)
        {
            if (!this.isConnected) { return; }

            byte[] buff = new byte[1];
            buff[0] = 1;

            this.Request(CHARACTERISTIC_LIGHT, buff, true, order, "turnLedOff");
        }

        /// <summary>
        /// キューブからあらかじめ用意された効果音を再生します
        /// https://toio.github.io/toio-spec/docs/ble_sound#効果音の再生
        /// </summary>
        /// <param name="soundId">サウンドID</param>
        /// <param name="volume">音量</param>
        /// <param name="order">命令の優先度</param>
        public override void PlayPresetSound(int soundId, int volume = 255, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
            if (!this.isConnected) { return; }

            byte[] buff = new byte[3];
            buff[0] = 2;
            buff[1] = BitConverter.GetBytes(soundId)[0];
            buff[2] = BitConverter.GetBytes(volume)[0];

            this.Request(CHARACTERISTIC_SOUND, buff, true, order, "playPresetSound", soundId);
        }

        /// <summary>
        /// キューブから任意の音を再生します
        /// https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生
        /// </summary>
        /// <param name="repeatCount">繰り返し回数</param>
        /// <param name="operations">命令配列</param>
        /// <param name="order">命令の優先度</param>
        public override void PlaySound(int repeatCount, SoundOperation[] operations, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
#if !RELEASE
            if (29 < operations.Length)
            {
                Debug.LogErrorFormat("[Cube.playSound]最大メロディ数を超えました. operations.Length={0}", operations.Length);
            }
#endif
            if (!this.isConnected) { return; }

            repeatCount = Mathf.Clamp(repeatCount, 0, 255);
            var operation_length = Mathf.Clamp(operations.Length, 0, 29);

            byte[] buff = new byte[3 + operation_length * 3];
            buff[0] = 3;
            buff[1] = BitConverter.GetBytes(repeatCount)[0];
            buff[2] = BitConverter.GetBytes(operation_length)[0];

            for (int i = 0; i < operation_length; i++)
            {
                buff[3 + 3 * i] = BitConverter.GetBytes(Mathf.Clamp(operations[i].durationMs / 10, 1, 255))[0];
                buff[4 + 3 * i] = BitConverter.GetBytes(operations[i].note_number)[0];
                buff[5 + 3 * i] = BitConverter.GetBytes(operations[i].volume)[0];
            }

            this.Request(CHARACTERISTIC_SOUND, buff, true, order, "playSound", repeatCount, operations);
        }

        /// <summary>
        /// キューブから任意の音を再生します
        /// https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生
        /// </summary>
        /// <param name="buff">命令プロトコル</param>
        /// <param name="order">命令の優先度</param>
        public override void PlaySound(byte[] buff, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
            if (!this.isConnected) { return; }

            this.Request(CHARACTERISTIC_SOUND, buff, true, order, "playSound");
        }

        /// <summary>
        /// キューブの音再生を停止します
        /// https://toio.github.io/toio-spec/docs/ble_sound#再生の停止
        /// </summary>
        /// <param name="order">命令の優先度</param>
        public override void StopSound(ORDER_TYPE order = ORDER_TYPE.Weak)
        {
            if (!this.isConnected) { return; }

            byte[] buff = new byte[1];
            buff[0] = 1;

            this.Request(CHARACTERISTIC_SOUND, buff, true, order, "stopSound");
        }

        /// <summary>
        /// キューブの水平検出のしきい値を設定します
        /// https://toio.github.io/toio-spec/docs/ble_configuration#水平検出のしきい値設定
        /// </summary>
        /// <param name="angle">傾き検知の閾値</param>
        /// <param name="order">命令の優先度</param>
        public override void ConfigSlopeThreshold(int _angle, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            if (!this.isConnected) { return; }

            _angle = Mathf.Clamp(_angle, 1, 45);

            byte[] buff = new byte[3];
            buff[0] = 5;
            buff[1] = 0;
            buff[2] = BitConverter.GetBytes(_angle)[0];

            this.Request(CHARACTERISTIC_CONFIG, buff, true, order, "configSlopeThreshold", _angle);
        }

        /// <summary>
        /// キューブの衝突検出のしきい値を設定します
        /// https://toio.github.io/toio-spec/docs/ble_configuration#衝突検出のしきい値設定
        /// </summary>
        /// <param name="level">衝突検知の閾値</param>
        /// <param name="order">命令の優先度</param>
        public override void ConfigCollisionThreshold(int level, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            if (!this.isConnected) { return; }

            level = Mathf.Clamp(level, 1, 10);

            byte[] buff = new byte[3];
            buff[0] = 6;
            buff[1] = 0;
            buff[2] = BitConverter.GetBytes(level)[0];

            this.Request(CHARACTERISTIC_CONFIG, buff, true, order, "configCollisionThreshold", level);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < recv >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected override void Recv_battery(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/2.0.0/ble_battery
            this.battery = data[0];
        }

        protected override void Recv_Id(byte[] data)
        {
            int type = data[0];

            // position id
            // https://toio.github.io/toio-spec/docs/2.0.0/ble_id#position-id
            if (1 == type)
            {
                this.isGrounded = true;
                this._pos.x = this.x = BitConverter.ToInt16(data, 1);
                this._pos.y = this.y = BitConverter.ToInt16(data, 3);
                this.angle = BitConverter.ToInt16(data, 5);
                this._sensorPos.x = BitConverter.ToInt16(data, 7);
                this._sensorPos.y = BitConverter.ToInt16(data, 9);
                this.sensorAngle = BitConverter.ToInt16(data, 11);
                this.idCallback.Notify(this);
            }
            // standard id
            // https://toio.github.io/toio-spec/docs/2.0.0/ble_id#standard-id
            else if(2 == type)
            {
                this.isGrounded = true;
                this.standardId = BitConverter.ToUInt32(data, 1);
                this.angle = BitConverter.ToUInt16(data, 5);
                this.standardIdCallback.Notify(this);
            }
            // position id missed
            // https://toio.github.io/toio-spec/docs/2.0.0/ble_id#position-id-missed
            else if(3 == type)
            {
                this.isGrounded = false;
                this.idMissedCallback.Notify(this);
            }
            // standard id missed
            // https://toio.github.io/toio-spec/docs/2.0.0/ble_id#standard-id-missed
            else if (4 == type)
            {
                this.isGrounded = false;
                this.standardIdMissedCallback.Notify(this);
            }
        }

        protected override void Recv_button(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/2.0.0/ble_button
            int type = data[0];
            if (1 == type)
            {
                this.isPressed = data[1] == 0 ? false : true;
                this.buttonCallback.Notify(this);
            }
        }

        protected override void Recv_sensor(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/2.0.0/ble_sensor
            int type = data[0];
            if (1 == type)
            {
                var _isSloped = data[1] == 0 ? true : false;
                var _isCollisionDetected = data[2] == 1 ? true : false;

                if (_isSloped != this.isSloped)
                {
                    this.isSloped = _isSloped;
                    this.slopeCallback.Notify(this);
                }

                if (_isCollisionDetected != this.isCollisionDetected)
                {
                    this.isCollisionDetected = _isCollisionDetected;
                    this.collisionCallback.Notify(this);
                }
            }
        }
    }
}