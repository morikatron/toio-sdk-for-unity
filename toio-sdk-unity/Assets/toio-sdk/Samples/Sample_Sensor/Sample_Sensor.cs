using UnityEngine;
using UnityEngine.UI;
using toio;
using Cysharp.Threading.Tasks;

public class Sample_Sensor : MonoBehaviour
{
    Cube cube;

    Text textBattery;
    Text textFlat;
    Text textCollision;
    Text textButton;
    Text textPositionID;
    Text textStandardID;
    Text textAngle;
    Text textDoubleTap;
    Text textPose;
    Text textShake;
    Text textSpeed;
    Text textMag;
    Text textAttitude;

    async void Start()
    {
        // UI の取得
        this.textBattery = GameObject.Find("TextBattery").GetComponent<Text>();
        this.textCollision = GameObject.Find("TextCollision").GetComponent<Text>();
        this.textFlat = GameObject.Find("TextFlat").GetComponent<Text>();
        this.textPositionID = GameObject.Find("TextPositionID").GetComponent<Text>();
        this.textStandardID = GameObject.Find("TextStandardID").GetComponent<Text>();
        this.textButton = GameObject.Find("TextButton").GetComponent<Text>();
        this.textAngle = GameObject.Find("TextAngle").GetComponent<Text>();
        this.textDoubleTap = GameObject.Find("TextDoubleTap").GetComponent<Text>();
        this.textPose = GameObject.Find("TextPose").GetComponent<Text>();
        this.textShake = GameObject.Find("TextShake").GetComponent<Text>();
        this.textSpeed = GameObject.Find("TextSpeed").GetComponent<Text>();
        this.textMag = GameObject.Find("TextMag").GetComponent<Text>();
        this.textAttitude = GameObject.Find("TextAttitude").GetComponent<Text>();
        await UniTask.Delay(0); // Avoid warning

#if UNITY_EDITOR || !UNITY_WEBGL
        var btn = GameObject.Find("ButtonConnect").GetComponent<Button>();
        btn.gameObject.SetActive(false);
        await Connect();
#endif
    }

    private async UniTask Connect()
    {
        // Cube の接続
        var peripheral = await new NearestScanner().Scan();
        cube = await new CubeConnecter().Connect(peripheral);
        // モーター速度の読み取りをオンにする
        await cube.ConfigMotorRead(true);

        // コールバック登録
        cube.collisionCallback.AddListener("Sample_Sensor", OnCollision);          // 衝突イベント
        cube.slopeCallback.AddListener("Sample_Sensor", OnSlope);                  // 傾きイベント
        cube.buttonCallback.AddListener("Sample_Sensor", OnPressButton);           // ボタンイベント
        cube.idCallback.AddListener("Sample_Sensor", OnUpdateID);                  // 座標角度イベント
        cube.standardIdCallback.AddListener("Sample_Sensor", OnUpdateStandardId);  // standardIdイベント
        cube.idMissedCallback.AddListener("Sample_Sensor", OnMissedID);            // 座標角度 missedイベント
        cube.standardIdMissedCallback.AddListener("Sample_Sensor", OnMissedStandardID);    // standardId missedイベント
        cube.poseCallback.AddListener("Sample_Sensor", OnPose);                    // 姿勢イベント
        cube.doubleTapCallback.AddListener("Sample_Sensor", OnDoubleTap);          // ダブルタップイベント
        cube.shakeCallback.AddListener("Sample_Sensor", OnShake);                  // Shake
        cube.motorSpeedCallback.AddListener("Sample_Sensor", OnSpeed);             // Motor Speed
        cube.magnetStateCallback.AddListener("Sample_Sensor", OnMagnetState);      // Magnet State
        cube.magneticForceCallback.AddListener("Sample_Sensor", OnMagForce);       // Magnetic Force
        cube.attitudeCallback.AddListener("Sample_Sensor", OnAttitude);            // Attitude

        await cube.ConfigIDNotification(500);       // 精度10ms
        await cube.ConfigIDMissedNotification(500); // 精度10ms
    }

    public async void OnBtnConnect() { await Connect(); }

    public void Forward() { cube.Move(60, 60, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
    public void Backward() { cube.Move(-40, -40, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
    public void TurnRight() { cube.Move(60, 30, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
    public void TurnLeft() { cube.Move(30, 60, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
    public void Stop() { cube.Move(0, 0, durationMs:0, order:Cube.ORDER_TYPE.Strong); }

    Cube.MagneticMode magMode = Cube.MagneticMode.Off;
    public async void OnSwitchMag()
    {
        this.magMode = (Cube.MagneticMode)(((int)this.magMode + 1) % 3);
        await cube.ConfigMagneticSensor(
            this.magMode,
            intervalMs: 500,    // 精度20msなの注意
            notificationType: Cube.MagneticNotificationType.OnChanged
        );
        if (this.magMode == Cube.MagneticMode.Off)
            this.textMag.text = "MagneticSensor Off";
        else
            cube.RequestMagneticSensor();
    }

    int attitudeMode = 0;
    public async void OnSwitchAttitude()
    {
        this.attitudeMode = (((int)this.attitudeMode + 1) % 3);
        if (attitudeMode == 0)
        {
            // The only way to Disable attitude notifications is to set interval to 0
            await cube.ConfigAttitudeSensor(
                Cube.AttitudeFormat.Eulers, intervalMs: 0,
                notificationType: Cube.AttitudeNotificationType.OnChanged
            );
            this.textAttitude.text = "AttitudeSensor Off";
        }
        else if (attitudeMode == 1)
        {
            await cube.ConfigAttitudeSensor(
                Cube.AttitudeFormat.Eulers, intervalMs: 500,                // 精度10ms
                notificationType: Cube.AttitudeNotificationType.OnChanged
            );
            cube.RequestAttitudeSensor(Cube.AttitudeFormat.Eulers);
        }
        else if (attitudeMode == 2)
        {
            await cube.ConfigAttitudeSensor(
                Cube.AttitudeFormat.Quaternion, intervalMs: 500,            // 精度10ms
                notificationType: Cube.AttitudeNotificationType.OnChanged
            );
            cube.RequestAttitudeSensor(Cube.AttitudeFormat.Quaternion);
        }
    }

    public void Update()
    {
        if (cube != null)
        {
            if (cube.isConnected)
            {
                this.textBattery.text = "Battery: " +cube.battery.ToString()+"%";
            }
        }
    }

    public void OnPose(Cube c)
    {
        switch (cube.pose)
        {
            case Cube.PoseType.Up:
                this.textPose.text = "Pose: Up";
                break;
            case Cube.PoseType.Down:
                this.textPose.text = "Pose: Down";
                break;
            case Cube.PoseType.Front:
                this.textPose.text = "Pose: Front";
                break;
            case Cube.PoseType.Back:
                this.textPose.text = "Pose: Back";
                break;
            case Cube.PoseType.Right:
                this.textPose.text = "Pose: Right";
                break;
            case Cube.PoseType.Left:
                this.textPose.text = "Pose: Left";
                break;
            default:
                this.textPose.text = "Pose: Up";
                break;
        }
    }

    public void OnDoubleTap(Cube c)
    {
        if (c.isDoubleTap)
        {
            this.textDoubleTap.text = "DoubleTap: True";
        }
        else
        {
            this.textDoubleTap.text = "DoubleTap: False";
        }
    }

    public void OnCollision(Cube c)
    {
        if (c.isCollisionDetected)
        {
            this.textCollision.text = "Collision: True";
        }
        else
        {
            this.textCollision.text = "Collision: False";
        }
    }

    public void OnSlope(Cube c)
    {
        if (c.isSloped)
        {
            this.textFlat.text = "Flat: False";
        }
        else
        {
            this.textFlat.text = "Flat: True";
        }

    }

    public void OnPressButton(Cube c)
    {
        if (c.isPressed)
        {
            this.textButton.text = "Button: True";
        }
        else
        {
            this.textButton.text = "Button: False";
        }

    }

    public void OnUpdateID(Cube c)
    {
        this.textPositionID.text = "PosID:" + " X=" + c.pos.x.ToString() + " Y=" + c.pos.y.ToString();
        this.textAngle.text = " Angle: " + c.angle.ToString();
    }

    public void OnUpdateStandardId(Cube c)
    {
        this.textStandardID.text =  "StandardID: " + c.standardId.ToString();
        this.textAngle.text = " Angle: " + c.angle.ToString();
    }

    public void OnMissedID(Cube c)
    {
        this.textPositionID.text = "PositionID Missed";
        this.textAngle.text = "Angle Missed";
    }
    public void OnMissedStandardID(Cube c)
    {
        this.textStandardID.text = "StandardID Missed";
        this.textAngle.text = "Angle Missed";
    }

    public void OnSpeed(Cube c)
    {
        this.textSpeed.text = "Speed:" + " L=" + c.leftSpeed.ToString() + " R=" + c.rightSpeed.ToString();
    }

    public void OnShake(Cube c)
    {
        this.textShake.text = "Shake: " + c.shakeLevel.ToString();
    }

    public void OnMagnetState(Cube c)
    {
        this.textMag.text = "MagnetState: " + c.magnetState.ToString();
    }

    public void OnMagForce(Cube c)
    {
        this.textMag.text = "MagForce=" + c.magneticForce.ToString("F0");
    }

    public void OnAttitude(Cube c)
    {
        var eulers = c.eulers;
        var q = c.quaternion;
        if (this.attitudeMode == 0)
        {
            this.textAttitude.text = "AttitudeSensor Off";
        }
        else if (this.attitudeMode == 1)
        {
            this.textAttitude.text = "Eulers=" + eulers.ToString("F0");
        }
        else
        {
            this.textAttitude.text = "Quat=" + q.ToString("F2");
        }
    }

}
