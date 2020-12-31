using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Api;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.TestRunner;
using UnityEngine.TestTools;
using UnityEditor;
using Cysharp.Threading.Tasks;
using toio;
using toio.Simulator;
using toio.Navigation;

//_/_/_/_/_/_/_/_/_/_/_/
//  基礎システム
//_/_/_/_/_/_/_/_/_/_/_/

[assembly:TestRunCallback(typeof(MyTestRunCallback))]
public class MyTestRunCallback : ITestRunCallback
{
    public void RunStarted(ITest testsToRun)
    {
#if UNITY_EDITOR
        toio.Tests.CubeTestCase.impl = new toio.Tests.SimulatorImpl();
#elif !UNITY_EDITOR && UNITY_IOS
        toio.Tests.CubeTestCase.impl = new toio.Tests.MobileRealImpl();
#elif !UNITY_EDITOR && UNITY_WEBGL
        toio.Tests.CubeTestCase.impl = new toio.Tests.WebGLRealImpl();
#endif

        toio.Tests.CubeTestCase.impl.RunStarted(testsToRun);
    }

    public void RunFinished(ITestResult testResults)
    {
        toio.Tests.CubeTestCase.impl.RunFinished(testResults);
    }

    public void TestStarted(ITest test)
    {
        toio.Tests.CubeTestCase.impl.TestStarted(test);
     }

    public void TestFinished(ITestResult result)
    {
        toio.Tests.CubeTestCase.impl.TestFinished(result);
    }
}

namespace toio.Tests
{
    public class CubeTestCase
    {
        public interface TestCaseInterface
        {
            // ITestRunCallback
            void RunStarted(ITest test);
            void RunFinished(ITestResult testResults);
            void TestStarted(ITest test);
            void TestFinished(ITestResult result);
            // callback attributes
            void OneTimeSetUp();
            void OneTimeTearDown();
            IEnumerator UnitySetUp();
            IEnumerator UnityTearDown();
        }
        public static TestCaseInterface impl = null;
        public static CubeManager cubeManager = new CubeManager();

        [OneTimeSetUp] // クラスのテストが開始される前に一度だけ実行される
        public void OneTimeSetUp()
        {
            impl?.OneTimeSetUp();
        }

        [OneTimeTearDown] // クラスの全てのテストが終了した後に実行される
        public void OneTimeTearDown()
        {
            impl?.OneTimeTearDown();
        }

        [UnitySetUp] // クラスのテストが開始される前に一度だけ実行される
        public IEnumerator UnitySetUp()
        {
            BasicTestMonoBehaviour.Reset();
            yield return impl?.UnitySetUp();
        }

        [UnityTearDown] // クラスの全てのテストが終了した後に実行される
        public IEnumerator UnityTearDown()
        {
            yield return impl?.UnityTearDown();
        }
    }
}

//_/_/_/_/_/_/_/_/_/_/_/
//  プラットフォーム別実装
//_/_/_/_/_/_/_/_/_/_/_/

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
        public void TestStarted(ITest test)
        {
            if (!firstTime && !test.HasChildren) { Debug.LogFormat("<color=green>テスト開始 {0}/{1}</color>", test.Parent.Name, test.Name); }
        }
        public void TestFinished(ITestResult result)
        {
            Debug.Log("<color=red>テスト終了</color>");
        }
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
        public IEnumerator UnityTearDown() => UniTask.ToCoroutine(async () =>
        {
            await EnvUtl.Move2Home(CubeTestCase.cubeManager);
            EnvUtl.ResetCubeManager(CubeTestCase.cubeManager);
        });
    }

    public class MobileRealImpl : CubeTestCase.TestCaseInterface
    {
        private static bool firstTime = true;
        private static ITest testRoot;
        public void RunStarted(ITest _testRoot) { testRoot = _testRoot; }
        public void RunFinished(ITestResult testResults) { }
        public void TestStarted(ITest test) { }
        public void TestFinished(ITestResult result) { }
        public void OneTimeSetUp() { }
        public void OneTimeTearDown() { }
        public IEnumerator UnitySetUp() => UniTask.ToCoroutine(async () =>
        {
            if (firstTime)
            {
                firstTime = false;

                // ステージオブジェクトを配置
                await EnvUtl.CreateSimStageObject();

                // キューブに接続
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
        public IEnumerator UnityTearDown() => UniTask.ToCoroutine(async () =>
        {
            await EnvUtl.Move2Home(CubeTestCase.cubeManager);
            EnvUtl.ResetCubeManager(CubeTestCase.cubeManager);
        });
    }

    public class WebGLRealImpl : CubeTestCase.TestCaseInterface
    {
        private static bool firstTime = true;
        private static ITest testRoot;
        public void RunStarted(ITest _testRoot) { testRoot = _testRoot; }
        public void RunFinished(ITestResult testResults) { }
        public void TestStarted(ITest test) { }
        public void TestFinished(ITestResult result) { }
        public void OneTimeSetUp() { }
        public void OneTimeTearDown() { }
        public IEnumerator UnitySetUp() => UniTask.ToCoroutine(async () =>
        {
            if (firstTime)
            {
                firstTime = false;

                // ステージオブジェクトを配置
                await EnvUtl.CreateSimStageObject();

                // ボタンを配置して押下待機
                var connectButton = await EnvUtl.CreateButton("connect", new Vector3(0, 80, 0), new Vector2(160, 100), 24);
                var startButton = await EnvUtl.CreateButton("start", new Vector3(0, -80, 0), new Vector2(160, 100), 24);
                var startFlg = false;
                startButton.onClick.AddListener(() => { startFlg = true; });
                try
                {
                    Cube cube = null;
                    for(int i = 0; i < 8; i++)
                    {
                        connectButton.onClick.AddListener(async () => { cube = await CubeTestCase.cubeManager.SingleConnect(); });
                        await UniTask.WaitUntil(() => (null != cube || startFlg) );
                        if (startFlg) { break; }
                        cube = null;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                GameObject.DestroyImmediate(connectButton.gameObject);

                // UIオブジェクトを生成
                var selectView = await EnvUtl.CreateTestUI(testRoot);

                // 選択モードの場合は、終了するまでテストを待機
                // 実行モードの場合は、ボタンから選択モードを終了
                await UniTask.WaitUntil(() => selectView.IsFinished);
            }
            await EnvUtl.Move2Home(CubeTestCase.cubeManager);
            EnvUtl.ResetCubeManager(CubeTestCase.cubeManager);
        });
        public IEnumerator UnityTearDown() => UniTask.ToCoroutine(async () =>
        {
            await EnvUtl.Move2Home(CubeTestCase.cubeManager);
            EnvUtl.ResetCubeManager(CubeTestCase.cubeManager);
        });
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
                if (0 == cubes.Count) { break; }
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

        public static async UniTask<UnityEngine.UI.Button> CreateButton(string text, Vector3 pos, Vector2 size, int fontsize=16)
        {
            if (null == res_buttonCanvas)
            {
                res_buttonCanvas = await Resources.LoadAsync<GameObject>("ButtonCanvas") as GameObject;
            }

            var obj = GameObject.Instantiate<GameObject>(res_buttonCanvas);
            var button = obj.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>();
            var txt = button.GetComponentInChildren<Text>();
            txt.text = text;
            txt.fontSize = fontsize;
            var rect = button.GetComponent<RectTransform>();
            rect.localPosition = pos;
            rect.sizeDelta = new Vector2(size.x, size.y);
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




