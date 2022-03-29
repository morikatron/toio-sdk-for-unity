using System;

namespace toio.VisualScript
{
    public class VisualScriptCubeConfigration
    {
        public bool IsComplete { set; get; }

        public async void ConfigMotorRead(Cube cube, bool valid, float timeOutSec = 0.5f, Action<bool, Cube> callback = null, Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Strong)
        {
            IsComplete = false;
            await cube.ConfigMotorRead(valid, timeOutSec, callback, order);
            IsComplete = true;
        }
        public async void ConfigIDNotification(Cube cube, int intervalMs, Cube.IDNotificationType notificationType = Cube.IDNotificationType.Balanced,
           float timeOutSec = 0.5f, Action<bool, Cube> callback = null, Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Strong)
        {
            IsComplete = false;
            await cube.ConfigIDNotification(intervalMs, notificationType, timeOutSec, callback, order);
            IsComplete = true;
        }
        public async void ConfigIDMissedNotification(Cube cube, int sensitivityMs,
            float timeOutSec = 0.5f, Action<bool, Cube> callback = null, Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Strong)
        {
            IsComplete = false;
            await cube.ConfigIDMissedNotification(sensitivityMs, timeOutSec, callback, order);
            IsComplete = true;
        }
        public async void ConfigMagneticSensor(Cube cube, Cube.MagneticMode mode, float timeOutSec = 0.5f,
            Action<bool, Cube> callback = null, Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Strong)
        {
            IsComplete = false;
            await cube.ConfigMagneticSensor(mode, timeOutSec, callback, order);
            IsComplete = true;
        }
        public async void ConfigMagneticSensor(Cube cube, Cube.MagneticMode mode, int intervalMs, Cube.MagneticNotificationType notificationType,
            float timeOutSec = 0.5f, Action<bool, Cube> callback = null, Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Strong)
        {
            IsComplete = false;
            await cube.ConfigMagneticSensor(mode, intervalMs, notificationType, timeOutSec, callback, order);
            IsComplete = true;
        }
        public async void ConfigAttitudeSensor(Cube cube, Cube.AttitudeFormat format, int intervalMs, Cube.AttitudeNotificationType notificationType,
            float timeOutSec = 0.5f, Action<bool, Cube> callback = null, Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Strong)
        {
            IsComplete = false;
            await cube.ConfigAttitudeSensor(format, intervalMs, notificationType, timeOutSec, callback, order);
            IsComplete = true;
        }
    }
}
