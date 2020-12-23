using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Api;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.TestRunner;
using Cysharp.Threading.Tasks;
using toio;
using toio.Simulator;
using toio.Navigation;


namespace toio.Tests
{
    public class SimulatorImpl : CubeTestCase.TestCaseInterface
    {
        private static bool firstTime = true;
        private static ITest testRoot;
        public void RunStarted(ITest _testRoot) { testRoot = _testRoot; }
        public void RunFinished(ITestResult testResults) { }
        public void OneTimeSetUp() { }
        public void OneTimeTearDown() { }
        public IEnumerator UnitySetUp() => UniTask.ToCoroutine(async () =>
        {
            if (firstTime)
            {
                firstTime = false;

                // ステージオブジェクトを配置
                await EnvUtl.CreateSimStageObject();
                // キューブオブジェクトを配置
                for (int x = 0; x < 4; x++)
                {
                    await EnvUtl.CreateSimCubeObject("Cube", new Vector2(100+100*x, 70), new Vector3(0, 90, 0));
                    await EnvUtl.CreateSimCubeObject("Cube", new Vector2(100+100*x, 500-70), new Vector3(0, 270, 0));
                }
                // キューブオブジェクトに接続
                await CubeTestCase.cubeManager.MultiConnect(8);

                // UIオブジェクトを生成
                var selectView = await EnvUtl.CreateTestUI(testRoot);

                // 選択モードの場合は、終了するまでテストを待機
                // 実行モードの場合は、ボタンから選択モードを終了
                await UniTask.WaitUntil(() => selectView.IsFinished);
            }
            await EnvUtl.Move2Home(CubeTestCase.cubeManager);
            EnvUtl.ResetCubeManager(CubeTestCase.cubeManager);
        });
        public IEnumerator UnityTearDown() { yield return null; }
    }

    public class MobileRealImpl : CubeTestCase.TestCaseInterface
    {
        private static bool firstTime = true;
        public void RunStarted(ITest test) { }
        public void RunFinished(ITestResult testResults) { }
        public void OneTimeSetUp() { }
        public void OneTimeTearDown() { }
        public IEnumerator UnitySetUp() => UniTask.ToCoroutine(async () =>
        {
            if (firstTime)
            {
                firstTime = false;

                await CubeTestCase.cubeManager.MultiConnect(8);
            }
            await EnvUtl.Move2Home(CubeTestCase.cubeManager);
            EnvUtl.ResetCubeManager(CubeTestCase.cubeManager);
        });
        public IEnumerator UnityTearDown() { yield return null; }
    }

    public class WebGLRealImpl : CubeTestCase.TestCaseInterface
    {
        private static bool firstTime = true;
        public void RunStarted(ITest test) { }
        public void RunFinished(ITestResult testResults) { }
        public void OneTimeSetUp() { }
        public void OneTimeTearDown() { }
        public IEnumerator UnitySetUp() => UniTask.ToCoroutine(async () =>
        {
            if (firstTime)
            {
                firstTime = false;

                // ボタンを配置して押下待機
                var button = await EnvUtl.CreateButton(new Vector3(50, 50, 0), new Vector2(200, 200));
                try
                {
                    for(int i = 0; i < 8; i++)
                    {
                        await CubeTestCase.cubeManager.SingleConnect();
                        bool down = false;
                        button.onClick.AddListener(() => {down = true; });
                        await UniTask.WaitUntil(() => true == down );
                    }
                }
                catch {}
                GameObject.DestroyImmediate(button.gameObject);
            }
            await EnvUtl.Move2Home(CubeTestCase.cubeManager);
            EnvUtl.ResetCubeManager(CubeTestCase.cubeManager);
        });
        public IEnumerator UnityTearDown() { yield return null; }
    }

    public static class EnvUtl
    {
        private static GameObject res_stage = null;
        private static GameObject res_cube = null;
        private static GameObject res_buttonCanvas = null;
        private static GameObject res_testUI = null;

        public static async UniTask<GameObject> CreateSimStageObject()
        {
            if (null == res_stage)
            {
                res_stage = await Resources.LoadAsync<GameObject>("Stage") as GameObject;
            }
            var stage = GameObject.Find("Stage");
            if (null == stage)
            {
                stage = GameObject.Instantiate(res_stage, Vector3.zero, Quaternion.identity);
                stage.name = "Stage";
                await UniTask.WaitForFixedUpdate();
            }
            return stage;
        }

        public static async UniTask<GameObject> CreateSimCubeObject(string name, Vector2 matPos, Vector3 euler, Mat mat=null)
        {
            if (null == res_cube)
            {
                res_cube = await Resources.LoadAsync<GameObject>("Cube") as GameObject;
            }
            // マット座標 to Unity座標
            var unipos = Mat.MatCoord2UnityCoord(matPos.x, matPos.y, mat);
            euler.y = Mat.MatDeg2UnityDeg(euler.y, mat);

            var obj = GameObject.Instantiate(res_cube, unipos, Quaternion.Euler(euler));
            obj.name = name;
            await UniTask.WaitForFixedUpdate();
            return obj;
        }

        public static void ResetCubeManager(CubeManager cubeManager)
        {
            CubeNavigator.ClearGNavigators();

            cubeManager.handles.Clear();
            cubeManager.navigators.Clear();
            foreach(var cube in cubeManager.cubes)
            {
                // note 良くない
                cube.buttonCallback.ClearListener();
                cube.slopeCallback.ClearListener();
                cube.collisionCallback.ClearListener();
                cube.idCallback.ClearListener();
                cube.standardIdCallback.ClearListener();
                cube.idMissedCallback.ClearListener();
                cube.standardIdMissedCallback.ClearListener();
                cube.doubleTapCallback.ClearListener();
                cube.poseCallback.ClearListener();
                cube.targetMoveCallback.ClearListener();
                cube.shakeCallback.ClearListener();
                cube.motorSpeedCallback.ClearListener();
                var handle = new CubeHandle(cube);
                cubeManager.handles.Add(handle);
                cubeManager.navigators.Add(new CubeNavigator(handle));
            }
        }

        public static async UniTask Move2Home(CubeManager cubeManager)
        {
            var list = Get_HomePosXY_and_AngleY_List();
            var cubes = new List<Cube>(cubeManager.cubes);
            foreach(var posAngle in list)
            {
                var homePos = new Vector2(posAngle.x, posAngle.y);
                Cube minCube = null;
                float minDistance = float.MaxValue;
                foreach(var cube in cubes)
                {
                    var distance = Vector2.Distance(homePos, cube.pos);
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        minCube = cube;
                    }
                }
                cubes.Remove(minCube);
                // 移動
                bool reach = false;
                minCube.targetMoveCallback.AddListener("__tmp", ((cube, id, result) => { reach = true; }));
                minCube.TargetMove((int)homePos.x, (int)homePos.y, (int)posAngle.z, 0, 0, Cube.TargetMoveType.RoundBeforeMove);
                await UniTask.WaitUntil(() => reach == true);
                minCube.targetMoveCallback.RemoveListener("__tmp");
            }
            await UniTask.Yield();
        }

        // Vec3(posX, posY, angleY)
        public static List<Vector3> Get_HomePosXY_and_AngleY_List()
        {
            var poslist = new List<Vector3>();
            for (int i = 0; i < 4; i++)
            {
                poslist.Add(new Vector3(100+100*i, 70, 90));
            }
            for (int i = 0; i < 4; i++)
            {
                poslist.Add(new Vector3(100+100*i, 500-70, 270));
            }
            return poslist;
        }

        public static async UniTask<UnityEngine.UI.Button> CreateButton(Vector3 pos, Vector2 size)
        {
            if (null == res_buttonCanvas)
            {
                res_buttonCanvas = await Resources.LoadAsync<GameObject>("ButtonCanvas") as GameObject;
            }

            var obj = GameObject.Instantiate<GameObject>(res_buttonCanvas);
            var button = obj.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>();
            return button;
        }

        public static async UniTask<toio.Tests.TestSelectView> CreateTestUI(ITest testRoot, GameObject parent=null)
        {
            // テスト配列を取得
            List<ITest> testList = new List<ITest>();
            CollectTestsRecusively(testList, testRoot);

            // テスト選択UIを生成
            if (null == res_testUI)
            {
                res_testUI = await Resources.LoadAsync<GameObject>("TestUI") as GameObject;
            }

            var obj = GameObject.Instantiate<GameObject>(res_testUI);
            var view = obj.GetComponentInChildren<toio.Tests.TestSelectView>(true);
            view.testRoot = testRoot;
            view.tests = testList;
            if (null != parent)
            {
                obj.transform.SetParent(parent.transform);
            }
            return view;
        }

        public static void CollectTestsRecusively(List<ITest> list, ITest test)
        {
            if (!test.HasChildren) { list.Add(test); }
            foreach(var t in test.Tests) { CollectTestsRecusively(list, t); }
        }
    }
}
