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
        //Debug.Log("RunStarted");
        toio.Tests.CubeTestCase.impl = new toio.Tests.SimulatorImpl();
        toio.Tests.CubeTestCase.impl.RunStarted(testsToRun);
    }

    public void RunFinished(ITestResult testResults)
    {
        toio.Tests.CubeTestCase.impl.RunFinished(testResults);
        //Debug.Log("RunFinished");
    }

    public void TestStarted(ITest test)
    {
        //Debug.Log("TestStarted");
    }

    public void TestFinished(ITestResult result)
    {
        //Debug.Log("TestFinished");

        if (result.Test.IsSuite)
        {
            //Debug.Log("is suieeet");
        }

        if (!result.Test.IsSuite)
        {
            //Debug.Log($"Result of {result.Name}: {result.ResultState.Status}");
        }
    }
}

namespace toio.Tests
{
    public class CubeTestCase
    {
        public interface TestCaseInterface
        {
            void RunStarted(ITest test);
            void RunFinished(ITestResult testResults);
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