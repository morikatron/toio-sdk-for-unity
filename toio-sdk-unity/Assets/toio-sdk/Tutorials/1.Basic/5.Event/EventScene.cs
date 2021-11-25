using UnityEngine;
using System.Collections;

namespace toio.tutorial
{
    public class EventScene : MonoBehaviour
    {
        Cube cube;
        bool showId = false;

        async void Start()
        {
            var peripheral = await new NearestScanner().Scan();
            cube = await new CubeConnecter().Connect(peripheral);
            // コールバック登録
            cube.buttonCallback.AddListener("EventScene", OnPressButton);
            cube.slopeCallback.AddListener("EventScene", OnSlope);
            cube.collisionCallback.AddListener("EventScene", OnCollision);
            cube.idCallback.AddListener("EventScene", OnUpdateID);
            cube.standardIdCallback.AddListener("EventScene", OnUpdateStandardID);
            cube.idMissedCallback.AddListener("EventScene", OnMissedID);
            cube.standardIdMissedCallback.AddListener("EventScene", OnMissedStandardID);

            // 2.1.0
            cube.poseCallback.AddListener("EventScene", OnPose);
            cube.doubleTapCallback.AddListener("EventScene", OnDoubleTap);

            // 2.2.0
            cube.shakeCallback.AddListener("EventScene", OnShake);
            cube.motorSpeedCallback.AddListener("EventScene", OnMotorSpeed);
            cube.magnetStateCallback.AddListener("EventScene", OnMagnetState);

            // 2.3.0
            cube.magneticForceCallback.AddListener("EventScene", OnMagneticForce);
            cube.attitudeCallback.AddListener("EventScene", OnAttitude);

            // Enable Sensors
            await cube.ConfigMotorRead(true);
            await cube.ConfigAttitudeSensor(Cube.AttitudeFormat.Eulers, 100, Cube.AttitudeNotificationType.OnChanged);
            await cube.ConfigMagneticSensor(Cube.MagneticMode.MagnetState);
        }

        void OnCollision(Cube c)
        {
            cube.PlayPresetSound(2);
            StartCoroutine(DelayedRequestSensor(cube));
        }

        private IEnumerator DelayedRequestSensor(Cube cube)
        {
            yield return new WaitForSecondsRealtime(0.05f);
            cube.RequestMotionSensor();
        }

        void OnSlope(Cube c)
        {
            cube.PlayPresetSound(1);
        }

        void OnPressButton(Cube c)
        {
            if (c.isPressed)
            {
                showId = !showId;
            }
            cube.PlayPresetSound(0);
        }

        void OnUpdateID(Cube c)
        {
            if (showId)
            {
                Debug.LogFormat("pos=(x:{0}, y:{1}), angle={2}", c.pos.x, c.pos.y, c.angle);
            }
        }

        void OnUpdateStandardID(Cube c)
        {
            if (showId)
            {
                Debug.LogFormat("standardId:{0}, angle={1}", c.standardId, c.angle);
            }
        }

        void OnMissedID(Cube cube)
        {
            Debug.LogFormat("Postion ID Missed.");
        }

        void OnMissedStandardID(Cube c)
        {
            Debug.LogFormat("Standard ID Missed.");
        }

        void OnPose(Cube c)
        {
            Debug.Log($"pose = {c.pose.ToString()}");
        }

        void OnDoubleTap(Cube c)
        {
            c.PlayPresetSound(3);
        }

        void OnShake(Cube c)
        {
            if (c.shakeLevel > 5)
                c.PlayPresetSound(4);
        }

        void OnMotorSpeed(Cube c)
        {
            Debug.Log($"motor speed: left={c.leftSpeed}, right={c.rightSpeed}");
        }

        void OnMagnetState(Cube c)
        {
            Debug.Log($"magnet state: {c.magnetState.ToString()}");
        }

        void OnMagneticForce(Cube c)
        {
            Debug.Log($"magnetic force = {c.magneticForce}");
        }

        void OnAttitude(Cube c)
        {
            Debug.Log($"attitude = {c.eulers}");
        }
    }
}