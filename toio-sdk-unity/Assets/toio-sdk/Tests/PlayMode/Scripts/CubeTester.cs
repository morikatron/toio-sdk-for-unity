using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using toio.Simulator;
using toio.Navigation;

namespace toio.Tests
{
    public class CubeTester : MonoBehaviour, IMonoBehaviourTest
    {
        public static Action init;
        public static Func<bool> update;

        public static void Reset()
        {
            Clear();
            isInit = true;
            isCubeInited = false;
        }
        public static void Clear()
        {
            init = (() => { });
            update = (() => { return true; });
            cubeList.Clear();
            cubeUList.Clear();
            CubeNavigator.ClearGNavigators();
        }

        public static Cube CreateCube()
        {
            return CreateCube(Vector3.zero, Vector3.zero, null);
        }
        public static Cube CreateCube(int posX, int posY, int angle=0, Mat mat=null)
        {
            return CreateCube(new Vector2(posX, posY), new Vector3(0,angle,0), mat);
        }
        public static Cube CreateCube(Vector2 pos, Mat mat=null)
        {
            return CreateCube(pos, Vector3.zero, mat);
        }
        public static Cube CreateCube(Vector2 matPos, Vector3 euler, Mat mat=null)
        {
            if (null == res_cube)
            {
                res_cube = (GameObject)Resources.Load("Cube");
            }

            // マット座標 to Unity座標
            var unipos = Mat.MatCoord2UnityCoord(matPos.x, matPos.y, mat);
            euler.y = Mat.MatDeg2UnityDeg(euler.y, mat);

            var obj = GameObject.Instantiate(res_cube, unipos, Quaternion.Euler(euler));
            obj.name = "Cube" + cubeList.Count.ToString();
            cubeList.Add(obj);
            var cube = new CubeUnity(obj);
            cubeUList.Add(cube);
            return cube;
        }

        private static List<GameObject> cubeList = new List<GameObject>();
        private static List<CubeUnity> cubeUList = new List<CubeUnity>();
        private static GameObject res_cube = null;
        public bool IsTestFinished { get; protected set; }
        // メンバ変数だと初期化が2フレーム遅れるため静的変数に変更
        private static bool isInit = true;
        private static bool isCubeInited = false;


        private void Start()
        {
            isInit = true;
        }

        private void Update()
        {
            if (!isCubeInited)
            {
                isCubeInited = true;
                foreach (var cube in cubeUList)
                {
                    isCubeInited = isCubeInited && cube.Init();
                }
            }
            else
            {
                if (isInit)
                {
                    isInit = false;
                    init();
                }
                if (update())
                {
                    foreach (var obj in cubeList)
                    {
                        GameObject.DestroyImmediate(obj);
                    }
                    Clear();
                    StartCoroutine(DelayFinish());
                }
            }
        }

        private IEnumerator DelayFinish()
        {
            yield return new WaitForSeconds(1.0f);
            this.IsTestFinished = true;
        }
    }
}