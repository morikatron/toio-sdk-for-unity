using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


namespace toio.Simulator
{

    public class Stage : MonoBehaviour
    {
        private Transform targetPole;
        private GameObject mainLightObj;
        private GameObject sideLightObj;
        private GameObject backLightObj;
        public Mat mat { get; private set; }
        public Transform focusTarget = null;

        void Start()
        {
            this.targetPole = transform.Find("TargetPole");
            this.mat = transform.Find("Mat").GetComponent<Mat>();
            this.mainLightObj = transform.Find("Spot Light Main").gameObject;
            this.sideLightObj = transform.Find("Spot Light Side").gameObject;
            this.backLightObj = transform.Find("Spot Light Back").gameObject;

            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        private bool isDragging = false;

        void Update()
        {
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame && CubeInteraction.GetSCA(false,true,false))
                {
                    if (CubeInteraction.current==null)
                        OnLeftDown();
                }
                else if (Mouse.current.rightButton.wasPressedThisFrame && CubeInteraction.GetSCA(false,true,false))
                {
                    if (CubeInteraction.current==null)
                    {
                        OnRightDown();
                        CubeInteraction.current = this;
                    }
                }
                else if (Mouse.current.rightButton.wasReleasedThisFrame)
                {
                    if (CubeInteraction.current==this)
                    {
                        OnRightUp();
                        CubeInteraction.current = null;
                    }
                }
            }

            // ターゲットポールを移動
            // Moving TargetPole
            if (isDragging)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (Physics.Raycast(ray, out hit) && targetPole != null) {
                    targetPole.position = new Vector3(hit.point.x, targetPole.position.y, hit.point.z);
                }
            }

            // ターゲットを追従
            // Keep focusing on focusTarget
            if (focusTarget!=null){
                var tar = new Vector3(0, 0.01f, 0) + focusTarget.position;
                mainLightObj.GetComponent<Light>().transform.LookAt(tar);
                sideLightObj.GetComponent<Light>().transform.LookAt(tar);
            }
        }
        void OnSceneUnloaded(Scene scene)
        {
            if (this!=null && CubeInteraction.current!=null)
            {
                if (CubeInteraction.current==this)
                {
                    if (isDragging)
                    {
                        isDragging = false;
                    }
                }
            }
        }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CubeInteraction.current = null;
            isDragging = false;
        }

        void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }


        // ====== Event Callbacks ======

        private void OnRightDown()
        {
            var camera = Camera.main;
            Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit)) {
                isDragging = true;
                targetPole.gameObject.SetActive(true);
            }
            else targetPole.gameObject.SetActive(false);
        }
        private void OnRightUp()
        {
            isDragging = false;
        }
        private void OnLeftDown()
        {
            var camera = Camera.main;
            Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit)) {
                if (hit.transform.gameObject.CompareTag("t4u_Cube"))
                    SetFocus(hit.transform);
                else SetNoFocus();
            }
            else SetNoFocus();
        }


        /// <summary>
        /// Make Lights focus on input transfrom, and the transform can be retrieved by property "focusTarget".
        /// </summary>
        public void SetFocus(Transform transform){
            mainLightObj.GetComponent<Light>().spotAngle = 6;
            sideLightObj.GetComponent<Light>().spotAngle = 6;
            focusTarget = transform;
        }

        /// <summary>
        /// Cancel focus.
        /// </summary>
        public void SetNoFocus(){
            mainLightObj.GetComponent<Light>().spotAngle = 110;
            sideLightObj.GetComponent<Light>().spotAngle = 110;
            focusTarget = null;
        }

        /// <summary>
        /// Get name of currently focused game object.
        /// </summary>
        public string focusName { get{
            if (focusTarget == null) return null;
            else return focusTarget.gameObject.name;
        }}

        /// <summary>
        /// Get wether targetPole active
        /// </summary>
        public bool targetPoleActive { get{
            return targetPole!=null && targetPole.gameObject.activeSelf;
        }}

        /// <summary>
        /// Get coord on mat of targetPole.
        /// </summary>
        public Vector2Int targetPoleCoord { get{
            if (targetPole != null)
                return this.mat.UnityCoord2MatCoord(targetPole.position);
            return new Vector2Int(250,250);
        }}
    }

}