using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace toio.tutorial.Template_ConnectName_CubeHandle
{
    public class TouchOperation : MonoBehaviour
    {
        public Transform targetPole;

        void Update()
        {
            if (!targetPole) return;
            if (Input.touchCount > 0) {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Began)
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);

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
