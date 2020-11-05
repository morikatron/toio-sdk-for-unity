using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Collections.Generic;

public class Sample_Sensor : MonoBehaviour
{
    Cube cube;

    UnityEngine.UI.Text textBattery;
    UnityEngine.UI.Text textFlat;
    UnityEngine.UI.Text textCollision;
    UnityEngine.UI.Text textButton;
    UnityEngine.UI.Text textPositionID;
    UnityEngine.UI.Text textStandardID;
    UnityEngine.UI.Text textAngle;

    async void Start()
    {
        var peripheral = await new NearestScanner().Scan();
        cube = await new CubeConnecter().Connect(peripheral);
        // コールバック登録
        cube.collisionCallback.AddListener("Sample_Sensor", OnCollision);          // 衝突イベント
        cube.slopeCallback.AddListener("Sample_Sensor", OnSlope);                  // 傾きイベント
        cube.buttonCallback.AddListener("Sample_Sensor", OnPressButton);           // ボタンイベント
        cube.idCallback.AddListener("Sample_Sensor", OnUpdateID);                  // 座標角度イベント
        cube.standardIdCallback.AddListener("Sample_Sensor", OnUpdateStandardId);  // standardIdイベント
        cube.idMissedCallback.AddListener("Sample_Sensor", OnMissedID);            // 座標角度 missedイベント
        cube.standardIdMissedCallback.AddListener("Sample_Sensor", OnMissedID);    // standardId missedイベント

        this.textBattery = GameObject.Find("TextBattery").GetComponent<Text>();
        this.textCollision = GameObject.Find("TextCollision").GetComponent<Text>();
        this.textFlat = GameObject.Find("TextFlat").GetComponent<Text>();
        this.textPositionID = GameObject.Find("TextPositionID").GetComponent<Text>();
        this.textStandardID = GameObject.Find("TextStandardID").GetComponent<Text>();
        this.textButton = GameObject.Find("TextButton").GetComponent<Text>();
        this.textAngle = GameObject.Find("TextAngle").GetComponent<Text>();
    }

    public void FixedUpdate()
    {
        if(cube != null)
        {
            if (cube.isConnected)
            {
                this.textBattery.text = "Battery: " +cube.battery.ToString()+"%";
            }
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
        this.textPositionID.text = "PositionID:" + " X=" + c.pos.x.ToString() + " Y=" + c.pos.y.ToString();
        this.textAngle.text = " Angle: " + c.angle.ToString();
    }

    public void OnUpdateStandardId(Cube c)
    {
        this.textStandardID.text =  "StandardID: " + c.standardId.ToString();
        this.textAngle.text = " Angle: " + c.angle.ToString();
    }

    public void OnMissedID(Cube c)
    {
        this.textPositionID.text = "PositionID";
        this.textStandardID.text = "StandardID";
        this.textAngle.text = " Angle";
    }

}
