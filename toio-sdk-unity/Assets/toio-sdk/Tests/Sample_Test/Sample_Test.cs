using UnityEngine;
using UnityEngine.UI;
using toio;
using System.Collections.Generic;

public class Sample_Test : MonoBehaviour
{
    Cube cube;

    UnityEngine.UI.Text textNum;
    UnityEngine.UI.Text textRes;
    UnityEngine.UI.Text textPositionID;
    UnityEngine.UI.Text textAngle;
    UnityEngine.UI.Text textSpeed;

    async void Start()
    {
        // UI の取得
        this.textRes = GameObject.Find("TextRes").GetComponent<Text>();
        this.textPositionID = GameObject.Find("TextPositionID").GetComponent<Text>();
        this.textAngle = GameObject.Find("TextAngle").GetComponent<Text>();
        this.textSpeed = GameObject.Find("TextSpeed").GetComponent<Text>();
        this.textNum = GameObject.Find("TextNum").GetComponent<Text>();
        // Cube の接続
        var peripheral = await new NearestScanner().Scan();
        cube = await new CubeConnecter().Connect(peripheral);
        // モーター速度の読み取りをオンにする
        await cube.ConfigMotorRead(true);
        // コールバック登録
        cube.idCallback.AddListener("Sample_Sensor", OnUpdateID);                  // 座標角度イベント
        cube.idMissedCallback.AddListener("Sample_Sensor", OnMissedID);            // 座標角度 missedイベント
        cube.motorSpeedCallback.AddListener("Sample_Sensor", OnSpeed);             //
        cube.targetMoveCallback.AddListener("Sample_Sensor", OnTargetRespond);
        cube.multiTargetMoveCallback.AddListener("Sample_Sensor", OnTargetRespond);
    }

    private int m = 0;//3
    private int s = 0;//4
    private int r = 0;//7
    private bool logflag = false;
    private int index = 0;
    private long start_t;

    public void test_random() {

        if (m == 3)
        {
            m=0;
            s=s+1;
        }
        if (s == 4)
        {
            s=0;
            r=r+1;
        }
        if (r == 7){return;}
        this.textNum.text = index.ToString();
        Random.InitState(index);
        var x = Random.Range(100,400);
        var y = Random.Range(100,400);
        var a = Random.Range(0,720);

        var movetype = (Cube.TargetMoveType)m;
        var speedtype = (Cube.TargetSpeedType)s;
        var rotype = (Cube.TargetRotationType)r;
        index = index+1;
        Debug.Log("test"+index.ToString());
        Debug.Log(x.ToString() + " " + y.ToString() +" "+ a.ToString());
        Debug.Log(movetype.ToString());
        Debug.Log(speedtype.ToString());
        Debug.Log(rotype.ToString());
        logflag=true;
        start_t = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        cube.TargetMove(x,y,a,
                        0,                               //0~255
                        255,                             //0~255
                        movetype,//0~2
                        50,                              //10~110
                        speedtype,   //0~4
                        rotype, //0~2絶対角度 3~6
                        Cube.ORDER_TYPE.Strong);

        m=m+1;
        }

    public void test_11() {
        this.textRes.text =　"";
            cube.TargetMove(326,146,592,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);}

    public void test_12() {
        this.textRes.text =　"";
            cube.TargetMove(374,218,266,0,255,
                            Cube.TargetMoveType.RoundForwardMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);}

    public void test_21() {
        this.textRes.text =　"";
            cube.TargetMove(111,342,423,0,255,
                            Cube.TargetMoveType.RoundBeforeMove,
                            30,
                            Cube.TargetSpeedType.VariableSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);}

    public void test_22() {
        this.textRes.text =　"";
            cube.TargetMove(334,134,597,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteClockwise,
                            Cube.ORDER_TYPE.Strong);}

    public void test_31() {
        this.textRes.text =　"";
            cube.TargetMove(235,280,643,0,255,
                            Cube.TargetMoveType.RoundBeforeMove,
                            30,
                            Cube.TargetSpeedType.Acceleration,
                            Cube.TargetRotationType.AbsoluteClockwise,
                            Cube.ORDER_TYPE.Strong);}

    public void test_32() {
        this.textRes.text =　"";
            cube.TargetMove(327,317,190,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.Deceleration,
                            Cube.TargetRotationType.AbsoluteClockwise,
                            Cube.ORDER_TYPE.Strong);}

    public void test_41() {
        this.textRes.text =　"";
            cube.TargetMove(290,296,472,0,255,
                            Cube.TargetMoveType.RoundBeforeMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteCounterClockwise,
                            Cube.ORDER_TYPE.Strong);}

    public void test_42() {
        this.textRes.text =　"";
            cube.TargetMove(145,172,621,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.Acceleration,
                            Cube.TargetRotationType.AbsoluteCounterClockwise,
                            Cube.ORDER_TYPE.Strong);}

    public void test_51() {
        this.textRes.text =　"";
            cube.TargetMove(272,181,557,0,255,
                            Cube.TargetMoveType.RoundForwardMove,
                            30,
                            Cube.TargetSpeedType.Acceleration,
                            Cube.TargetRotationType.AbsoluteCounterClockwise,
                            Cube.ORDER_TYPE.Strong);}
    public void test_52() {
        this.textRes.text =　"";
            cube.TargetMove(300,199,29,0,255,
                            Cube.TargetMoveType.RoundBeforeMove,
                            30,
                            Cube.TargetSpeedType.Acceleration,
                            Cube.TargetRotationType.AbsoluteCounterClockwise,
                            Cube.ORDER_TYPE.Strong);}
    public void test_61() {
        this.textRes.text =　"";
            cube.TargetMove(140,147,440,0,255,
                            Cube.TargetMoveType.RoundForwardMove,
                            30,
                            Cube.TargetSpeedType.VariableSpeed,
                            Cube.TargetRotationType.AbsoluteCounterClockwise,
                            Cube.ORDER_TYPE.Strong);}
    public void test_62() {
        this.textRes.text =　"";
            cube.TargetMove(172,285,528,0,255,
                            Cube.TargetMoveType.RoundBeforeMove,
                            30,
                            Cube.TargetSpeedType.VariableSpeed,
                            Cube.TargetRotationType.AbsoluteCounterClockwise,
                            Cube.ORDER_TYPE.Strong);}
    public void test_71() {
        this.textRes.text =　"";
            cube.TargetMove(250,336,454,0,255,
                            Cube.TargetMoveType.RoundBeforeMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.RelativeClockwise,
                            Cube.ORDER_TYPE.Strong);}
    public void test_72() {
        this.textRes.text =　"";
            cube.TargetMove(357,291,494,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.Acceleration,
                            Cube.TargetRotationType.RelativeClockwise,
                            Cube.ORDER_TYPE.Strong);}
    public void test_81() {
        this.textRes.text =　"";
            cube.TargetMove(380,227,180,0,255,
                            Cube.TargetMoveType.RoundBeforeMove,
                            30,
                            Cube.TargetSpeedType.VariableSpeed,
                            Cube.TargetRotationType.RelativeCounterClockwise,
                            Cube.ORDER_TYPE.Strong);}
    public void test_82() {
        this.textRes.text =　"";
            cube.TargetMove(344,197,297,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.NotRotate,
                            Cube.ORDER_TYPE.Strong);}
    public void test_91() {
        this.textRes.text =　"";
            cube.TargetMove(366,373,427,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.Acceleration,
                            Cube.TargetRotationType.Original,
                            Cube.ORDER_TYPE.Strong);}
    public void test_92() {
        this.textRes.text =　"";
            cube.TargetMove(186,287,61,0,255,
                            Cube.TargetMoveType.RoundForwardMove,
                            30,
                            Cube.TargetSpeedType.Acceleration,
                            Cube.TargetRotationType.Original,
                            Cube.ORDER_TYPE.Strong);}
    /*
    public void test2() {
        this.textRes.text =　"";
        int[] xl = new int[]{(int)cube.pos.x,(int)cube.pos.x};
        int[] yl = new int[]{(int)cube.pos.y,(int)cube.pos.y};
        int[] al = new int[]{(int)cube.angle,(int)cube.angle};
        Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};

        cube.MultiTargetMove(xl,yl,al,tl,0,20,
                            Cube.TargetMoveType.RotatingMove,80, 
                            Cube.TargetSpeedType.UniformSpeed, 
                            Cube.MultiWriteType.Write, 
                            Cube.ORDER_TYPE.Strong);}

    public void test3() {
        this.textRes.text =　"";
        int[] xl = new int[]{(int)cube.pos.x,200};
        int[] yl = new int[]{(int)cube.pos.y,200};
        int[] al = new int[]{(int)cube.angle,90};
        Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};
        cube.MultiTargetMove(xl,yl,al,tl,0,20,
                            Cube.TargetMoveType.RotatingMove,80, 
                            Cube.TargetSpeedType.UniformSpeed, 
                            Cube.MultiWriteType.Write, 
                            Cube.ORDER_TYPE.Strong);}


    public void test_s0() {
        this.textRes.text =　"";
        int[] xl = new int[]{250,200};
        int[] yl = new int[]{200,250};
        int[] al = new int[]{270,90};
        Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};

        cube.MultiTargetMove(xl,yl,al,tl,0,20,
                            Cube.TargetMoveType.RotatingMove,80, 
                            Cube.TargetSpeedType.UniformSpeed, 
                            Cube.MultiWriteType.Write, 
                            Cube.ORDER_TYPE.Strong);}


    public void test_s1() {
        this.textRes.text =　"";
        int[] xl = new int[]{250,200,200};
        int[] yl = new int[]{200,250,200};
        int[] al = new int[]{270,90,180};
        Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};

        cube.MultiTargetMove(xl,yl,al,tl,0,20,
                            Cube.TargetMoveType.RotatingMove,80, 
                            Cube.TargetSpeedType.UniformSpeed, 
                            Cube.MultiWriteType.Write, 
                            Cube.ORDER_TYPE.Strong);}


    public void test_s2() {
        this.textRes.text =　"";
        int[] xl = new int[]{250,200,200,200};
        int[] yl = new int[]{200,250,200,300};
        int[] al = new int[]{270,90,180,90};
        Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};

        cube.MultiTargetMove(xl,yl,al,tl,0,20,
                            Cube.TargetMoveType.RotatingMove,80, 
                            Cube.TargetSpeedType.UniformSpeed, 
                            Cube.MultiWriteType.Write, 
                            Cube.ORDER_TYPE.Strong);}

    public void test_s3() {
        this.textRes.text =　"";
        int[] xl = new int[]{250,200,200,200,200};
        int[] yl = new int[]{200,250,200,300,300};
        int[] al = new int[]{270,90,80,70,60};
        Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};

        cube.MultiTargetMove(xl,yl,al,tl,0,20,
                            Cube.TargetMoveType.RotatingMove,80, 
                            Cube.TargetSpeedType.UniformSpeed, 
                            Cube.MultiWriteType.Write, 
                            Cube.ORDER_TYPE.Strong);}



    public void test_ss() {
        this.textRes.text =　"";
        cube.TargetMove(200,200,
                                            30,
                                            0,                               //0~255
                                            255,                             //0~255
                                            Cube.TargetMoveType.RotatingMove,//0~2
                                            80,                              //10~110
                                            Cube.TargetSpeedType.UniformSpeed,   //0~4
                                            Cube.TargetRotationType.AbsoluteClockwise, //0~2絶対角度 3~6 
                                            Cube.ORDER_TYPE.Strong);}


    public void test3_1() {cube.AccelerationMove(100,2,0,Cube.AccRotationType.Clockwise,Cube.AccMoveType.Forward,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);}
    public void test3_2() {cube.AccelerationMove(50,2,Cube.AccMoveType.Forward,0,Cube.ORDER_TYPE.Strong);}
*/

    public void Update()
    {
        if (cube != null)
        {
            if (cube.isConnected)
            {

            }
        }
    }

    public void OnTargetRespond(Cube c, int configID, Cube.TargetMoveRespondType Res)
    {
        logflag = false;
        Debug.Log(" ");
        this.textRes.text = "c:" +configID.ToString() + " r:" +Res.ToString();
    }

    public void OnUpdateID(Cube c)
    {
        if (logflag){
            long t = System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - start_t;
            Debug.Log(t.ToString()+" "+c.pos.x.ToString()+" "+c.pos.y.ToString()+" "+c.angle.ToString());
        }
        this.textPositionID.text = "PositionID:" + " X=" + c.pos.x.ToString() + " Y=" + c.pos.y.ToString();
        this.textAngle.text = " Angle: " + c.angle.ToString();
    }

    public void OnMissedID(Cube c)
    {
        this.textPositionID.text = "PositionID Missed";
        this.textAngle.text = "Angle Missed";
    }

    public void OnSpeed(Cube c)
    {
        this.textSpeed.text = "Speed:" + " L=" + c.leftSpeed.ToString() + " R=" + c.rightSpeed.ToString();
    }
}
