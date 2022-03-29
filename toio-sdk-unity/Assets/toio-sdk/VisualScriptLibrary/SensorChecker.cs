using Unity.VisualScripting;

namespace toio.VisualScript
{
    public class SensorChecker
    {
        public SensorChecker
        (
            Cube cube,
            bool buttonCallback = true,
            bool slopeCallback = true,
            bool collisionCallback = true,
            bool idCallback = true,
            bool standardIdCallback = true,
            bool idMissedCallback = true,
            bool standardIdMissedCallback = true,
            bool poseCallback = true,
            bool doubleTapCallback = true,
            bool shakeCallback = true,
            bool motorSpeedCallback = true,
            bool magnetStateCallback = true,
            bool magneticForceCallback = true,
            bool attitudeCallback = true
        )

        {
            if (buttonCallback) cube.buttonCallback.AddListener("EventScene", _OnPressButton);
            if (slopeCallback) cube.slopeCallback.AddListener("EventScene", _OnSlope);
            if (collisionCallback) cube.collisionCallback.AddListener("EventScene", _OnCollision);
            if (idCallback) cube.idCallback.AddListener("EventScene", _OnUpdateID);
            if (standardIdCallback) cube.standardIdCallback.AddListener("EventScene", _OnUpdateStandardID);
            if (idMissedCallback) cube.idMissedCallback.AddListener("EventScene", _OnMissedID);
            if (standardIdMissedCallback) cube.standardIdMissedCallback.AddListener("EventScene", _OnMissedStandardID);

            // 2.1.0
            if (poseCallback) cube.poseCallback.AddListener("EventScene", _OnPose);
            if (doubleTapCallback) cube.doubleTapCallback.AddListener("EventScene", _OnDoubleTap);

            // 2.2.0
            if (shakeCallback) cube.shakeCallback.AddListener("EventScene", _OnShake);
            if (motorSpeedCallback) cube.motorSpeedCallback.AddListener("EventScene", _OnMotorSpeed);
            if (magnetStateCallback) cube.magnetStateCallback.AddListener("EventScene", _OnMagnetState);

            // 2.3.0
            if (magneticForceCallback) cube.magneticForceCallback.AddListener("EventScene", _OnMagneticForce);
            if (attitudeCallback) cube.attitudeCallback.AddListener("EventScene", _OnAttitude);

        }

        private void _OnCollision(Cube c)
        {
            EventBus.Trigger(EventNames.OnCollision, c);
        }

        private void _OnSlope(Cube c)
        {
            EventBus.Trigger(EventNames.OnSlope, c);
        }

        private void _OnPressButton(Cube c)
        {
            EventBus.Trigger(EventNames.OnPressButton, c);
        }

        private void _OnUpdateID(Cube c)
        {
            EventBus.Trigger(EventNames.OnUpdateID, c);
        }

        private void _OnUpdateStandardID(Cube c)
        {
            EventBus.Trigger(EventNames.OnUpdateStandardID, c);
        }

        private void _OnMissedID(Cube c)
        {
            EventBus.Trigger(EventNames.OnMissedID, c);
        }

        private void _OnMissedStandardID(Cube c)
        {
            EventBus.Trigger(EventNames.OnMissedStandardID, c);
        }

        private void _OnPose(Cube c)
        {
            EventBus.Trigger(EventNames.OnPose, c);
        }

        private void _OnDoubleTap(Cube c)
        {
            EventBus.Trigger(EventNames.OnDoubleTap, c);
        }

        private void _OnShake(Cube c)
        {
            EventBus.Trigger(EventNames.OnShake, c);
        }

        private void _OnMotorSpeed(Cube c)
        {
            EventBus.Trigger(EventNames.OnMotorSpeed, c);
        }

        private void _OnMagnetState(Cube c)
        {
            EventBus.Trigger(EventNames.OnMagnetState, c);
        }

        private void _OnMagneticForce(Cube c)
        {
            EventBus.Trigger(EventNames.OnMagneticForce, c);
        }

        private void _OnAttitude(Cube c)
        {
            EventBus.Trigger(EventNames.OnAttitude, c);
        }
    }
}
