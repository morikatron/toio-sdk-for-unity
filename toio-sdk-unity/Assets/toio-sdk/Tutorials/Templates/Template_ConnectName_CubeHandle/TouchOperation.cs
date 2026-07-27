using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;


namespace toio.tutorial.Template_ConnectName_CubeHandle
{
    public class TouchOperation : MonoBehaviour
    {
        public Transform targetPole;

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        void OnDisable()
        {
            // EnhancedTouchSupport is global; don't disable it here because other systems may rely on it.
        }

        void Update()
        {
            if (!targetPole) return;

            foreach (var touch in Touch.activeTouches)
            {
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Began)
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(touch.screenPosition);

                    if (Physics.Raycast(ray, out hit)) {
                        targetPole.gameObject.SetActive(true);
                        targetPole.position = new Vector3(hit.point.x, targetPole.position.y, hit.point.z);
                    }
                    else
                        targetPole.gameObject.SetActive(false);
                }
            }
        }
    }
}
