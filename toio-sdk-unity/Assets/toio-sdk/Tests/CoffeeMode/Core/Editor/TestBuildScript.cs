#if UNITY_EDITOR
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor.Callbacks;

namespace toio.Tests
{
    public class TestBuildScript
    {
        //
        private const string KEY_TEST_STATUS = "KEY_COMPILE_STATUS";
        private const string VAL_TEST_STATUS_CREATE_SCRIPT = "VAL_TEST_STATUS_CREATESCRIPT";
        private const string VAL_TEST_STATUS_CREATE_SCENE = "VAL_TEST_STATUS_CREATESCENE";
        private const string VAL_TEST_STATUS_RUN_EDITOR = "VAL_TEST_STATUS_RUN_EDITOR";
        //
        private const string KEY_GENSCRIPT_PATH = "KEY_GENSCRIPT_PATH";
        //
        private const string KEY_GENSCENE_PATH = "KEY_GENSCENE_PATH";
        //
        private const string KEY_TESTENV = "KEY_TEST_ENV";
        private const string VAL_TESTENV_EDITOR = "VAL_TESTENV_EDITOR";
        private const string VAL_TESTENV_APP = "VAL_TESTENV_APP";
        //
        private const string KEY_HOME_SCENE = "KEY_HOME_SCENE";

        static TestBuildScript()
        {
            EditorApplication.playModeStateChanged += async (stateChange) =>
            {
                if (PlayModeStateChange.ExitingPlayMode == stateChange) { await Reset(); }
            };
        }

        [MenuItem ("toio/run test/Editor")]
        public static void MenuItem_Editor()
        {
            EditorPrefs.SetString(KEY_TEST_STATUS, VAL_TEST_STATUS_CREATE_SCRIPT);
            EditorPrefs.SetString(KEY_TESTENV, VAL_TESTENV_EDITOR);
            BuildTest();
        }
        [MenuItem ("toio/run test/App")]
        public static void MenuItem_App()
        {
            EditorPrefs.SetString(KEY_TEST_STATUS, VAL_TEST_STATUS_CREATE_SCRIPT);
            EditorPrefs.SetString(KEY_TESTENV, VAL_TESTENV_APP);
            BuildTest();
        }

        [DidReloadScripts]
        public static async void BuildTest()
        {
            Debug.Log("---on reload---");
            // 無効の場合はreturn
            if (!EditorPrefs.HasKey(KEY_TEST_STATUS)) { return; }

            // 基本的にはコンパイル発生毎に分岐する
            // ref https://answers.unity.com/questions/1007004/generate-script-in-editor-script-and-gettype-retur.html
            var status = EditorPrefs.GetString(KEY_TEST_STATUS);
            try
            {
                if (VAL_TEST_STATUS_CREATE_SCRIPT == status)
                {
                    Debug.Log("VAL_TEST_STATUS_CREATE_SCRIPT start");
                    // 1.スクリプト生成
                    var scriptPath = CreateScript("Assets/");
                    EditorPrefs.SetString(KEY_TEST_STATUS, VAL_TEST_STATUS_CREATE_SCENE);
                    EditorPrefs.SetString(KEY_GENSCRIPT_PATH, scriptPath);
                    // 2.スクリプト生成処理完了後に明示的にアセット読み込み
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                    Debug.Log("VAL_TEST_STATUS_CREATE_SCRIPT end");
                }
                else if (VAL_TEST_STATUS_CREATE_SCENE == status)
                {
                    Debug.Log("VAL_TEST_STATUS_CREATE_SCENE start");
                    // 1.アセンブリロードコールバック時ではシーンが開けないため、Yieldを利用してコンテクストを変更
                    await Task.Yield();
                    // 2.テスト用シーンを生成
                    var scriptPath = EditorPrefs.GetString(KEY_GENSCRIPT_PATH);
                    var scenePath = CreateTestScene(scriptPath, "Assets/");
                    EditorPrefs.SetString(KEY_GENSCENE_PATH, scenePath);
                    // 3.テストを実行
                    var env = EditorPrefs.GetString(KEY_TESTENV);
                    if (VAL_TESTENV_EDITOR == env)
                    {
                        EditorApplication.EnterPlaymode();
                        EditorPrefs.SetString(KEY_TEST_STATUS, VAL_TEST_STATUS_RUN_EDITOR);
                        // エディターの場合はプレイモード終了時にリセット
                    }
                    else if (VAL_TESTENV_APP == env)
                    {
                        BuildTestApp(scenePath, env);
                        await Reset();
                    }
                    Debug.Log("VAL_TEST_STATUS_CREATE_SCENE end");
                }
                else if (VAL_TEST_STATUS_RUN_EDITOR == status)
                {
                    // 何もしない
                    /* note
                       Unity2019.3以前 ではプレイモード遷移事にコンパイルが入るため、条件分岐しておく
                       Unity2019.3以後 ではプレイモード遷移時にコンパイルが入らない場合があるため、プレイモード終了時のリセット処理は playModeStateChanged でイベント実行する
                    */
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                await Reset();
            }
        }

        static string CreateScript(string targetDir)
        {
            // 0.変数定義
            string testScriptName = "_" + Guid.NewGuid().ToString("N").Substring(0, 10);
            const string CodeTemplate = @"
                using UnityEngine;
                using toio.Tests;
                public class %%CLASS%% : MonoBehaviour
                {
                    void Awake()
                    {
                        var test = this.gameObject.AddComponent<BasicTest>();
                        %%CODE%%
                    }
                }";

            // 1.テスト関数を取得
            var testMethods = GetTestMethods(typeof(CoffeeTestCase), typeof(CoffeeTest));

            // 2.書き込み範囲(%%CODE%%)に書き込む文字列を生成
            var classHash = new HashSet<Type>();
            var sb = new StringBuilder();
            foreach(var m in testMethods)
            {
                var className = m.DeclaringType.ToString();
                var varName = m.DeclaringType.Name.ToLower();
                var methodName = m.Name;
                if (!classHash.Contains(m.DeclaringType))
                {
                    classHash.Add(m.DeclaringType);
                    sb = sb.AppendLine($"var {varName} = new {className}();");
                }
                sb = sb.AppendLine($"test.tests.Add( new {nameof(BasicCoffeeTest)}( {varName}.{methodName} ) );");
            }

            // 3.クラス名(%%CLASS%%) と 書き込み範囲(%%CODE%%) を置換
            var code = CodeTemplate.Replace("%%CLASS%%", testScriptName).Replace("%%CODE%%", sb.ToString());

            // 4.スクリプトを生成
            return GenerateScript(Path.Combine(targetDir, testScriptName + ".cs"), code);
        }

        static async Task Reset()
        {
            Debug.Log("Reset");

            // note シーン実行直後に別のシーンを開こうとするとエラーになるため、とりあえず一定時間待機
            while(EditorApplication.isPlaying) { await Task.Delay(500); }
            if (EditorPrefs.HasKey(KEY_HOME_SCENE) && 0 < EditorPrefs.GetString(KEY_HOME_SCENE).Length)
            {
                EditorSceneManager.OpenScene(EditorPrefs.GetString(KEY_HOME_SCENE));
            }
            else
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            }

            if (EditorPrefs.HasKey(KEY_GENSCRIPT_PATH)) { AssetDatabase.DeleteAsset(EditorPrefs.GetString(KEY_GENSCRIPT_PATH)); }
            if (EditorPrefs.HasKey(KEY_GENSCENE_PATH)) { AssetDatabase.DeleteAsset(EditorPrefs.GetString(KEY_GENSCENE_PATH)); }

            EditorPrefs.DeleteKey(KEY_TEST_STATUS);
            EditorPrefs.DeleteKey(KEY_GENSCRIPT_PATH);
            EditorPrefs.DeleteKey(KEY_GENSCENE_PATH);
            EditorPrefs.DeleteKey(KEY_TESTENV);
            EditorPrefs.DeleteKey(KEY_HOME_SCENE);
        }

        static string CreateTestScene(string testScriptPath, string targetDir)
        {
            // 0.変数定義
            string randomFileName = "_" + Guid.NewGuid().ToString("N").Substring(0, 10);
            const string OriginalTestScenePath = "Assets/toio-sdk/Tests/CoffeeMode/Core/Scene/TestScene.unity";
            var scenePath = Path.Combine(targetDir, randomFileName + ".unity");

            // 1.一時シーンを生成
            if (!CopyScene(OriginalTestScenePath, scenePath)) { Debug.LogError("一時テストシーンの生成に失敗しました"); }

            // 2.一時シーンを読み込み
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorPrefs.SetString(KEY_HOME_SCENE, EditorSceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // 3.テスト用オブジェクトをシーンに追加
            var obj = new GameObject("_script");
            var classType = GetTypeByClassName(Path.GetFileNameWithoutExtension(testScriptPath));
            obj.AddComponent(classType);

            return scenePath;
        }

        static void BuildTestApp(string scenePath, string env)
        {
            if (BuildTarget.iOS == EditorUserBuildSettings.activeBuildTarget)
            {
                Build(scenePath, "iOSBuild", BuildOptions.Development);
            }
            else if (BuildTarget.WebGL == EditorUserBuildSettings.activeBuildTarget)
            {
                Build(scenePath, "WebGLBuild", BuildOptions.AutoRunPlayer);
            }
            else
            {

            }
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      便利関数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        // classType テスト関数を列挙するクラス
        // attributeType テスト関数用属性
        static List<MethodInfo> GetTestMethods(Type classType, Type attributeType)
        {
            List<MethodInfo> methods = new List<MethodInfo>();

            var types = Assembly.GetAssembly(classType).GetTypes().Where(t=>{ return t.IsSubclassOf(classType) && !t.IsAbstract; });
            foreach(var type in types)
            {
                var memlist = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach(var mem in memlist)
                {
                    if (mem.IsDefined(attributeType))
                    {
                        if (!mem.IsPublic) { throw new Exception(String.Format("public 関数にして下さい. 関数= {0}.{1}", mem.DeclaringType, mem.Name)); }
                        if (!mem.ReturnType.Equals(typeof(UniTask))) { throw new Exception(String.Format("戻り値を UniTask にして下さい. 関数= {0}.{1}", mem.DeclaringType, mem.Name)); }
                        methods.Add(mem);
                    }
                }
            }
            return methods;
        }

        static void Build(string scenePath, string appPath, BuildOptions options)
        {
            var scenes = new[] { scenePath };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = appPath;
            buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
            buildPlayerOptions.options = options;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed");
            }
        }

        // out パスが重複していた場合は自動で数字を付与してファイル生成する
        // ref https://www.hanachiru-blog.com/entry/2019/12/20/221633
        static string GenerateScript(string filepath, string code)
        {
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(filepath);
            File.WriteAllText(assetPath, code);
            // note スクリプトの場合はコンパイルが発生してエディタスクリプトが停止するためSaveAssets()やRefresh()は勝手に実行しない
            return assetPath;
        }

        static UnityEngine.SceneManagement.Scene GenerateScene(string filepath)
        {
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(filepath);
            var scene = EditorSceneManager.CreateScene(assetPath);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            return scene;
        }

        static bool CopyScene(string filepath, string newfilepath)
        {
            var ret = AssetDatabase.CopyAsset(filepath, newfilepath);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            return ret;
        }

        // ref http://bochituku.jugem.jp/?eid=4
        static Type GetTypeByClassName(string className)
        {
            foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach(Type type in assembly.GetTypes())
                {
                    if(type.Name == className)
                    {
                        return type;
                    }
                }
            }
            return null;
        }
    }
}
#endif