using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


namespace toio.Simulator
{
    public class CubeInteraction : MonoBehaviour,
        IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        // === Global ===
        public static Object current { get; set; }　// currently object under interaction

        [SerializeField]
        public Camera targetCamera;

        CubeSimulator cube;
        Rigidbody rb;

        //  Drag and Drop
        public static bool isDragging {get; protected set;} = false;
        static bool droppable = false;
        static bool maskClickOnDrop = false;
        static RaycastHit dropHit;
        static Transform _cubeIndicator;
        static Transform cubeIndicator{get{
            if (_cubeIndicator==null)
            {
                _cubeIndicator = Instantiate((GameObject)Resources.Load("CubeIndicator")).transform;
                _cubeIndicator.gameObject.SetActive(false);
            }
            return _cubeIndicator;
        }}

        //  Pull
        public static bool isPulling {get; protected set;} = false;
        static Vector3 pullInitialLocalPos;
        static float pullInitialY;
        static LineRenderer _pullIndicator;
        static LineRenderer pullIndicator{get{
            if (_pullIndicator==null)
            {
                _pullIndicator = Instantiate((GameObject)Resources.Load("Line")).GetComponent<LineRenderer>();
                _pullIndicator.startColor = Color.black;
                _pullIndicator.endColor = Color.black;
                _pullIndicator.gameObject.SetActive(false);
            }
            return _pullIndicator;
        }}

        //  Button
        bool isPressing = false;


        void Start()
        {
            cube = gameObject.GetComponent<CubeSimulator>();
            rb = gameObject.GetComponent<Rigidbody>();

            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void Update()
        {
            if (current==this)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    if (isDragging)
                    {
                        DragCubeEnd();
                        current = null;
                        isDragging = false;
                    }
                }

                if (isDragging) DragCube();
                if (isPulling) PullCube();
            }
        }

        void OnSceneUnloaded(Scene scene)
        {
            if (this!=null && current!=null)
            {
                if (current==this)
                {
                    if (isDragging)
                    {
                        DragCubeEnd();
                        isDragging = false;
                    }
                    if (isPulling)
                    {
                        PullCubeEnd();
                        isPulling = false;
                    }
                    if (isPressing)
                    {
                        cube.button = false;
                        isPressing = false;
                    }
                }
            }
        }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            current = null;
            Cursor.visible = true;
            isDragging = false;
            isPulling = false;
            isPressing = false;
        }

        void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected static bool isShift {get{
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);}}
        protected static bool isCtrl {get{
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);}}
        protected static bool isAlt {get{
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);}}
        public static bool GetSCA(bool shift, bool ctrl, bool alt)
        {
            return shift==isShift && ctrl==isCtrl && alt==isAlt;
        }


        // ====== Events ======
        public void OnPointerClick(PointerEventData data)
        {
            if (data.button==PointerEventData.InputButton.Right
                && GetSCA(false,false,false))
            {
                if (current==null && !maskClickOnDrop)
                {
                    DragCubeStart();
                    current = this;
                    isDragging = true;
                }
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (data.button==PointerEventData.InputButton.Right
                && GetSCA(false,false,false))
            {
                maskClickOnDrop = current==this && isDragging;
            }
            else if (data.button==PointerEventData.InputButton.Left
                && GetSCA(false,false,false))
            {
                if (current==null)
                {
                    cube.button = true;
                    current = this;
                    isPressing = true;
                }
            }
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (data.button==PointerEventData.InputButton.Left
                && GetSCA(false,false,false))
            {
                if (current==this && isPressing)
                {
                    cube.button = false;
                    current = null;
                    isPressing = false;
                }
            }
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (data.button==PointerEventData.InputButton.Right
                && GetSCA(false,false,false))
            {
                if (current==null)
                {
                    var downPos = data.pointerPressRaycast.worldPosition;
                    pullInitialY = downPos.y;
                    pullInitialLocalPos = transform.InverseTransformPoint(downPos);
                    PullCubeStart();
                    current = this;
                    isPulling = true;
                }
            }
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (data.button==PointerEventData.InputButton.Right
                && GetSCA(false,false,false))
            {
                if (current==this && isPulling)
                {
                    PullCubeEnd();
                    current = null;
                    isPulling = false;
                }
            }
        }

        public void OnDrag(PointerEventData data)
        {
        }


        // =================== Implementation of Operations =========================

        void DragCubeStart()
        {
            // Stop the cube
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Lift up the cube
            transform.position = transform.position
                + new Vector3(0, 0.05f, 0);

            transform.eulerAngles = new Vector3(
                0, cube.transform.eulerAngles.y, 0
            );

            // Enable indicator
            cubeIndicator.gameObject.SetActive(true);

            // Disable cube casting shadow
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Cursor
            Cursor.visible = false;
        }

        void DragCube()
        {
            // Mouse Wheel to rotate cube
            transform.eulerAngles = new Vector3(
                cube.transform.eulerAngles.x,
                cube.transform.eulerAngles.y + Input.mouseScrollDelta.y*20,
                cube.transform.eulerAngles.z);
            cubeIndicator.eulerAngles = cube.transform.eulerAngles;

            // Following mouse
            var camera = targetCamera != null ? targetCamera : Camera.main;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastAll(ray);
            System.Array.Sort(hits, (x,y) => x.distance.CompareTo(y.distance));

            foreach (var hit in hits)
            {
                if (hit.transform!=cube.transform && !hit.transform.IsChildOf(cubeIndicator))
                {
                    dropHit = hit;
                    // Move cube
                    transform.position = new Vector3(
                        hit.point.x,
                        hit.point.y + 0.1f,
                        hit.point.z);
                    // Move indicator
                    cubeIndicator.position = new Vector3(
                        hit.point.x,
                        hit.point.y + 0.001f,
                        hit.point.z);
                    break;
                }
            }

            // Able to drop?
            var dropColliders = Physics.OverlapBox(
                dropHit.point + new Vector3(0,0.011f,-0.0038f),
                new Vector3(0.015f, 0.01f, 0.015f),
                cubeIndicator.rotation);
            droppable = dropColliders.Length == 0;
            droppable &= Vector3.Dot(dropHit.normal, new Vector3(0,1,0)) > 0.9f;
            var renderers = cubeIndicator.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (droppable) {
                    renderer.material.color = new Color(0, 1, 0, 0.2f);
                    renderer.material.SetColor("_EmissionColor", new Color(0.1f, 0.3f, 0.1f));
                }
                else
                {
                    renderer.material.color = new Color(1, 0, 0, 0.2f);
                    renderer.material.SetColor("_EmissionColor", new Color(0.5f, 0.15f, 0.15f, 0.1f));
                }
            }

            // Draw line
            var line = cubeIndicator.gameObject.GetComponentInChildren<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, dropHit.point);
            if (droppable) line.startColor = new Color(0, 1, 0, 0.2f);
            else line.startColor = new Color(1, 0, 0, 0.2f);

            // Change Light Color
            var light = cubeIndicator.GetComponentInChildren<Light>();
            if (droppable) { light.color = Color.green; light.intensity=1; }
            else { light.color = Color.red; light.intensity = 2; }

        }

        void DragCubeEnd()
        {
            // Recover physics
            rb.useGravity = true;

            // Drop the cube
            transform.position = dropHit.point + new Vector3(
                0, 0.005f, 0);

            // Disable indicator
            if (_cubeIndicator!=null)
                _cubeIndicator.gameObject.SetActive(false);

            // Enable cube casting shadow
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            // Cursor
            Cursor.visible = true;
        }


        void PullCubeStart()
        {
            pullIndicator.gameObject.SetActive(true);
            Cursor.visible = false;
        }

        void PullCube()
        {
            var camera = targetCamera != null ? targetCamera : Camera.main;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            var t = (pullInitialY - ray.origin.y) / ray.direction.y;
            Vector3 dragEndPos = new Vector3(
                ray.origin.x + ray.direction.x * t,
                pullInitialY,
                ray.origin.z + ray.direction.z * t);
            Vector3 dragStartPos = transform.TransformPoint(pullInitialLocalPos);
            Vector3 dragDPos = dragEndPos-dragStartPos;

            // Draw Arrow
            pullIndicator.positionCount = 2;
            pullIndicator.SetPosition(0, dragStartPos);
            pullIndicator.SetPosition(1, dragEndPos);

            // Add Force
            var force = dragDPos * 1;
            var forcePos = new Vector3(
                dragStartPos.x,
                transform.TransformPoint(new Vector3(0,0.01f,-0.003f)).y,   // Cube の重心位置のｙ値を取り、トルクのないように力を
                dragStartPos.z);
            rb.AddForceAtPosition(force, forcePos);
        }

        void PullCubeEnd()
        {
            if (_pullIndicator!=null)
                _pullIndicator.gameObject.SetActive(false);
            Cursor.visible = true;
        }


    }
}