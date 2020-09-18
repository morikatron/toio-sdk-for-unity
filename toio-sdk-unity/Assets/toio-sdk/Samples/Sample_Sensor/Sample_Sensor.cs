using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Collections.Generic;

public class Sample_Sensor : MonoBehaviour

{
    Cube cube;

    GameObject DoubleTap;
    GameObject Pose;
    GameObject Battery;
    GameObject Flat;
    GameObject Collision;
    GameObject Button;
    GameObject PositionID;
    GameObject StandardID;
    GameObject Angle;

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

        this.Battery = GameObject.Find("Battery");
        this.Collision = GameObject.Find("Collision");
        this.Flat = GameObject.Find("Flat");
        this.PositionID = GameObject.Find("PositionID");
        this.StandardID = GameObject.Find("StandardID");
        this.Button = GameObject.Find("Button");
        this.Angle = GameObject.Find("Angle");
    }

    public void FixedUpdate()
    {
        if(cube != null)
        {
            if (cube.isConnected)
            {
                this.Battery.GetComponent<Text>().text = "Battery:" +cube.battery.ToString()+"%";
            }
        }
    }

    public void OnCollision(Cube c)
    {
        if (c.isCollisionDetected)
        {
            this.Collision.GetComponent<Text>().text = "Collision:ON";
        }
        else
        {
            this.Collision.GetComponent<Text>().text = "Collision:OFF";
        }
    }

    public void OnSlope(Cube c)
    {
        if (c.isSloped)
        {
            this.Flat.GetComponent<Text>().text = "Flat:OFF";
        }
        else
        {
            this.Flat.GetComponent<Text>().text = "Flat:ON";
        }

    }

    public void OnPressButton(Cube c)
    {
        if (c.isPressed)
        {
            this.Button.GetComponent<Text>().text = "Button:ON";
        }
        else
        {
            this.Button.GetComponent<Text>().text = "Button:OFF";
        }

    }

    public void OnUpdateID(Cube c)
    {
        if (c.isGrounded)
        {
            this.StandardID.GetComponent<Text>().text = "X:" + c.pos.x.ToString() + " Y:" + c.pos.y.ToString();
            this.Angle.GetComponent<Text>().text = " Angle:" + c.angle.ToString();
        }
        else
        {
            this.StandardID.GetComponent<Text>().text = "Standard";
            this.Angle.GetComponent<Text>().text = " Angle";
        }

    }

    public void OnUpdateStandardId(Cube c)
    {
        if (c.isGrounded)
        {
            this.PositionID.GetComponent<Text>().text = c.standardId.ToString();
        }
        else
        {
            this.PositionID.GetComponent<Text>().text = "PositionID";
        }

    }
}
