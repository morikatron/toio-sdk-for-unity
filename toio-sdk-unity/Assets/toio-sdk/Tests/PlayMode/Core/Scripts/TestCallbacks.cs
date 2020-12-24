using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Api;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestRunner;
using UnityEditor;
using Cysharp.Threading.Tasks;
using toio;


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