using UnityEngine;

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
        }

        void OnCollision(Cube c)
        {
            cube.PlayPresetSound(2);
        }

        void OnSlope(Cube c)
        {
            cube.PlayPresetSound(8);
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

    }
}