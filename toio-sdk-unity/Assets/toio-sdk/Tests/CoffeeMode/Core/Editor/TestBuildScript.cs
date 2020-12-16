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
        private const string KEY_COMPILE_STATUS = "KEY_COMPILE_STATUS";
        private const string KEY_GENSCRIPT_PATH = "KEY_GENSCRIPT_PATH";
        private const string KEY_GENSCENE_PATH = "KEY_GENSCENE_PATH";
        private const string VAL_BEFORE_COMPILE = "VAL_BEFORE_COMPILE";
        private const string VAL_AFTER_COMPILE = "VAL_AFTER_COMPILE";

        private const string OriginalTestScenePath = "Assets/toio-sdk/Tests/CoffeeMode/Core/Scene/TestScene.unity";
        private const string DestDir = "Assets/";

        [MenuItem ("toio/run test")]
        public static void MenuItem()
        {
            EditorPrefs.SetString(KEY_COMPILE_STATUS, VAL_BEFORE_COMPILE);
            BuildTest();
        }

        [DidReloadScripts]
        // ref https://answers.unity.com/questions/1007004/generate-script-in-editor-script-and-gettype-retur.html
        public static async void BuildTest ()
        {
            if (!EditorPrefs.HasKey(KEY_COMPILE_STATUS)) { return; }

            var status = EditorPrefs.GetString(KEY_COMPILE_STATUS);

            try
            {
                if (VAL_BEFORE_COMPILE == status)
                {
                    // 1. スクリプト生成
                    var scriptPath = CreateScript();
                    EditorPrefs.SetString(KEY_COMPILE_STATUS, VAL_AFTER_COMPILE);
                    EditorPrefs.SetString(KEY_GENSCRIPT_PATH, scriptPath);
                    // スクリプト生成処理完了後に明示的にアセット読み込み
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                }
                else if (VAL_AFTER_COMPILE == status)
                {
                    // 1.アセンブリロードコールバック時ではシーンが開けないため、Yieldを利用してコンテクストを変更
                    await Task.Yield();
                    // 2.テスト用シーンを生成
                    var scriptPath = EditorPrefs.GetString(KEY_GENSCRIPT_PATH);
                    var scenePath = CreateTestScene(scriptPath);
                    EditorPrefs.SetString(KEY_GENSCENE_PATH, scenePath);
                    // 3.ビルド
                    Build(scenePath);

                    //
                    Reset();
                }
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
                Reset();
            }
        }

        static void Reset()
        {
            Debug.Log("Reset");
            if (EditorPrefs.HasKey(KEY_GENSCRIPT_PATH)) { AssetDatabase.DeleteAsset(EditorPrefs.GetString(KEY_GENSCRIPT_PATH)); }
            if (EditorPrefs.HasKey(KEY_GENSCENE_PATH)) { AssetDatabase.DeleteAsset(EditorPrefs.GetString(KEY_GENSCENE_PATH)); }

            EditorPrefs.DeleteKey(KEY_COMPILE_STATUS);
            EditorPrefs.DeleteKey(KEY_GENSCRIPT_PATH);
            EditorPrefs.DeleteKey(KEY_GENSCENE_PATH);
        }

        static string CreateScript()
        {
            // ---- 1.テストスクリプトを生成 ----  //
            string testScriptName = "CLASS" + Guid.NewGuid().ToString("N").Substring(0, 10);
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

            // 2.書き込み範囲(##CODE##)に書き込む文字列を生成
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
            return GenerateScript(Path.Combine(DestDir, testScriptName + ".cs"), code);
        }

        static string CreateTestScene(string testScriptPath)
        {
            string randomFileName = "_" + Guid.NewGuid().ToString("N").Substring(0, 10);
            var scenePath = Path.Combine(DestDir, randomFileName + ".unity");

            // 1.一時シーンを生成
            if (!CopyScene(OriginalTestScenePath, scenePath)) { Debug.LogError("一時テストシーンの生成に失敗しました"); }

            // 2.一時シーンを読み込み
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // 3.テスト用オブジェクトをシーンに追加
            var obj = new GameObject("_script");
            var classType = GetTypeByClassName(Path.GetFileNameWithoutExtension(testScriptPath));
            obj.AddComponent(classType);

            return scenePath;
        }

        static void Build(string scenePath)
        {
            var scenes = new[] { scenePath };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = "iOSBuild";
            buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
            buildPlayerOptions.options = BuildOptions.Development;

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
            // スクリプトの場合はコンパイルが発生してエディタスクリプトが停止するのでコメントアウト
            //AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            return assetPath;
        }

        static UnityEngine.SceneManagement.Scene GenerateScene(string filepath)
        {
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(filepath);
            var scene = EditorSceneManager.CreateScene(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            return scene;
        }

        static bool CopyScene(string filepath, string newfilepath)
        {
            var ret = AssetDatabase.CopyAsset(filepath, newfilepath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            return ret;
        }


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
                        if (!mem.IsPublic) { Debug.LogErrorFormat("public 関数にして下さい. 関数= {0}.{1}", mem.DeclaringType, mem.Name); EditorApplication.Exit(0); }
                        if (!mem.ReturnType.Equals(typeof(UniTask))) { Debug.LogErrorFormat("戻り値を UniTask にして下さい. 関数= {0}.{1}", mem.DeclaringType, mem.Name); EditorApplication.Exit(0); } 
                        methods.Add(mem);
                    }
                }
            }
            return methods;
        }

        // ref http://bochituku.jugem.jp/?eid=4
        static Type GetTypeByClassName(string className)
        {
            /*
            var assembly = Assembly.Load("Assembly-CSharp");

            foreach(Type type in assembly.GetTypes())
            {
                if(type.Name == className)
                {
                    return type;
                }
            }
            */

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