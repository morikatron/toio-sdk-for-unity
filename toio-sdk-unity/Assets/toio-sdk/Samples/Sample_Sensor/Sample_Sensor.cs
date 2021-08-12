using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Collections.Generic;

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
    Text textMagnet;
    Text textMagForce;

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
        this.textMagnet = GameObject.Find("TextMagnet").GetComponent<Text>();
        this.textMagForce = GameObject.Find("TextMagForce").GetComponent<Text>();

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

        await cube.ConfigIDNotification(100);
        await cube.ConfigIDMissedNotification(100);
        await cube.ConfigMagneticSensor(
            Cube.MagneticSensorMode.MagneticForce,
            interval:1,
            notificationType:Cube.MagneticSensorNotificationType.OnChanged
        );
    }

    public void Forward() { cube.Move(60, 60, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
    public void Backward() { cube.Move(-40, -40, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
    public void TurnRight() { cube.Move(60, 30, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
    public void TurnLeft() { cube.Move(30, 60, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
    public void Stop() { cube.Move(0, 0, durationMs:0, order:Cube.ORDER_TYPE.Strong); }

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
        Debug.LogWarning("pos = " + c.pos.x.ToString() + ", " + c.pos.y.ToString());
    }

    public void OnUpdateStandardId(Cube c)
    {
        this.textStandardID.text =  "StandardID: " + c.standardId.ToString();
        this.textAngle.text = " Angle: " + c.angle.ToString();
        Debug.LogWarning("stdid = " + c.standardId.ToString());
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
        Debug.LogWarning("Speed = " + " L=" + c.leftSpeed.ToString() + " R=" + c.rightSpeed.ToString());
        this.textSpeed.text = "Speed:" + " L=" + c.leftSpeed.ToString() + " R=" + c.rightSpeed.ToString();
    }

    public void OnShake(Cube c)
    {
        this.textShake.text = "Shake: " + c.shakeLevel.ToString();
    }

    public void OnMagnetState(Cube c)
    {
        this.textMagnet.text = "Magnet: " + c.magnetState.ToString();
    }

    public void OnMagForce(Cube c)
    {
        int x = (int) c.magneticForce.x;
        int y = (int) c.magneticForce.y;
        int z = (int) c.magneticForce.z;
        this.textMagForce.text = "MagForce=(" + x + ", " + y + ", " + z + ")";
    }

}
