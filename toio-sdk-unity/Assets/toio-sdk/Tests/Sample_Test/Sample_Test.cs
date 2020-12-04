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

    public void test_reset() {
        this.textRes.text =　"";
            cube.TargetMove(250,250,270,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);}

    public void test_11() {
        this.textRes.text =　"";
        int[] xl = new int[]{250,200};
        int[] yl = new int[]{200,250};
        int[] al = new int[]{200,300};
        Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};

        cube.MultiTargetMove(xl,yl,al,tl,0,20,
                            Cube.TargetMoveType.RotatingMove,30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.MultiWriteType.Write,
                            Cube.ORDER_TYPE.Strong);}

    public void test_add() {
        this.textRes.text =　"";
        int[] xl = new int[]{300,300};
        int[] yl = new int[]{350,250};
        int[] al = new int[]{80,120};
        Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};

        cube.MultiTargetMove(xl,yl,al,tl,0,20,
                            Cube.TargetMoveType.RotatingMove,30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.MultiWriteType.Add,
                            Cube.ORDER_TYPE.Strong);}

    public void test_Nosuppot() {
        this.textRes.text =　"";
        int[] xl = new int[]{300,-1};
        int[] yl = new int[]{350,-1};
        int[] al = new int[]{80,120};
        Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.Original};

        cube.MultiTargetMove(xl,yl,al,tl,0,20,
                            Cube.TargetMoveType.RotatingMove,30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.MultiWriteType.Write,
                            Cube.ORDER_TYPE.Strong);}


    public void test_Acc_1() {cube.AccelerationMove(50,2,50,Cube.AccPriorityType.Translation,5,Cube.ORDER_TYPE.Strong);}
    public void test_Acc_2() {cube.AccelerationMove(-50,2,-50,Cube.AccPriorityType.Translation,5,Cube.ORDER_TYPE.Strong);}

    public void test_Acc_3() {cube.AccelerationMove(50,2,50,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);}
    public void test_Acc_4() {cube.AccelerationMove(50,2,50,Cube.AccPriorityType.Rotation,0,Cube.ORDER_TYPE.Strong);}

    public void test_Acc_5() {cube.AccelerationMove(0,2,100,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);}
    public void test_Acc_6() {cube.AccelerationMove(0,2,50,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);}

    public void test_Acc_7() {cube.AccelerationMove(0,2,100,Cube.AccPriorityType.Rotation,0,Cube.ORDER_TYPE.Strong);}
    public void test_Acc_8() {cube.AccelerationMove(0,2,50,Cube.AccPriorityType.Rotation,0,Cube.ORDER_TYPE.Strong);}

    public void test_Acc_9() {cube.AccelerationMove(20,2,100,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);}
    public void test_Acc_10() {cube.AccelerationMove(20,2,50,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);}

    public void test_Acc_11() {cube.AccelerationMove(20,2,100,Cube.AccPriorityType.Rotation,0,Cube.ORDER_TYPE.Strong);}
    public void test_Acc_12() {cube.AccelerationMove(20,2,50,Cube.AccPriorityType.Rotation,0,Cube.ORDER_TYPE.Strong);}


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
