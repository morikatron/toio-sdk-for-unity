using Unity.VisualScripting;

namespace toio.VisualScript
{
    public static class EventNames
    {
        public static string OnPressButton = "OnPressButton";
        public static string OnSlope = "OnSlope";
        public static string OnCollision = "OnCollision";
        public static string OnUpdateID = "OnUpdateID";
        public static string OnUpdateStandardID = "OnUpdateStandardID";
        public static string OnMissedID = "OnMissedID";
        public static string OnMissedStandardID = "OnMissedStandardID";
        public static string OnPose = "OnPose";
        public static string OnDoubleTap = "OnDoubleTap";
        public static string OnShake = "OnShake";
        public static string OnMotorSpeed = "OnMotorSpeed";
        public static string OnMagnetState = "OnMagnetState";
        public static string OnMagneticForce = "OnMagneticForce";
        public static string OnAttitude = "OnAttitude";
        public static string AsyncConnected = "AsyncConnected";
    }

    /******************************************************
                        Sensor Event                        
    *******************************************************/
    public class BaseToioEvent : EventUnit<Cube>
    {
        [DoNotSerialize]// No need to serialize ports.
        public ValueOutput cube { get; private set; }
        protected override bool register => true;

        protected override void Definition()
        {
            base.Definition();
            cube = ValueOutput<Cube>(nameof(cube));
        }
        // Setting the value on our port.
        protected override void AssignArguments(Flow flow, Cube c)
        {
            flow.SetValue(cube, c);
        }
    }

    [UnitTitle("On Press Button")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnPressButton : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnPressButton);
        }
    }

    [UnitTitle("On Collision")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnCollision : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnCollision);
        }
    }

    [UnitTitle("On Slope")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnSlope : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnSlope);
        }
    }

    [UnitTitle("On Update ID")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnUpdateID : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnUpdateID);
        }
    }

    [UnitTitle("On Update Standard ID")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnUpdateStandardID : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnUpdateStandardID);
        }
    }

    [UnitTitle("On Missed ID")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnMissedID : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnMissedID);
        }
    }

    [UnitTitle("On Missed Standard ID")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnMissedStandardID : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnMissedStandardID);
        }
    }

    [UnitTitle("On Pose")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnPose : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnPose);
        }
    }

    [UnitTitle("On Double Tap")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnDoubleTap : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnDoubleTap);
        }
    }

    [UnitTitle("On Shake")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnShake : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnShake);
        }
    }

    [UnitTitle("On Mortor Speed")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnMotorSpeed : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnMotorSpeed);
        }
    }

    [UnitTitle("On Magenet State")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnMagnetState : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnMagnetState);
        }
    }

    [UnitTitle("On Magnetic Force")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnMagneticForce : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnMagneticForce);
        }
    }

    [UnitTitle("On Attitude")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnAttitude : BaseToioEvent
    {
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.OnAttitude);
        }
    }

    /******************************************************
                        Other Event                        
    *******************************************************/
    public struct OnConnectedArgs
    {
        public Cube cube {get; private set;}
        public CONNECTION_STATUS connection_status {get; private set;}
        public OnConnectedArgs(Cube cube, CONNECTION_STATUS connection_status)
        {
            this.cube = cube;
            this.connection_status = connection_status;
        }
    }

    [UnitTitle("On Async Connected")]
    [UnitCategory("Events\\t4uEvent")]
    public class OnAsyncConnected: EventUnit<OnConnectedArgs>
    {
        [DoNotSerialize]// No need to serialize ports.
        public ValueOutput cube { get; private set; }
        public ValueOutput connection_status {get; private set;}
        protected override bool register => true;

        protected override void Definition()
        {
            base.Definition();
            cube = ValueOutput<Cube>(nameof(cube));
            connection_status = ValueOutput<CONNECTION_STATUS>(nameof(connection_status));
        }
        // Setting the value on our port.
        protected override void AssignArguments(Flow flow, OnConnectedArgs args)
        {
            flow.SetValue(cube, args.cube);
            flow.SetValue(connection_status, args.connection_status);
        }
        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(EventNames.AsyncConnected);
        }
    }
}
