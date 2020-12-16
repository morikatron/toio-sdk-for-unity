using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio.Tests
{
    public abstract class CoffeeTestCase
    {
        public static GameObject gameObject;
        public static MonoBehaviour script;
    }

    public partial class BasicTest : MonoBehaviour
    {
        public List<CoffeeTestInterface> tests = new List<CoffeeTestInterface>();

        // Start is called before the first frame update
        async void Start()
        {
            await UniTask.Yield();
            CoffeeTestFramework framework = new CoffeeTestFramework();

            framework.oneTimeSetUp = this.OneTimeSetUp;
            framework.oneTimeTearDown = this.OneTimeTearDown;
            framework.setUp = this.SetUp;
            framework.tearDown = this.TearDown;

            await framework.Start(tests);
        }

        async UniTask OneTimeSetUp()
        {
            await UniTask.Yield();
        }

        async UniTask OneTimeTearDown()
        {
            await UniTask.Yield();
        }

        async UniTask SetUp()
        {
            await UniTask.Yield();
        }

        async UniTask TearDown()
        {
            await UniTask.Yield();
        }
    }
}