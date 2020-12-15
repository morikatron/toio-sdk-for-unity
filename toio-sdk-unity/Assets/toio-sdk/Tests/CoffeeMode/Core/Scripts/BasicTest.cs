using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Reflection;

namespace toio.Tests
{
    internal abstract class CoffeeTestCase
    {
        public static GameObject gameObject;
        public static MonoBehaviour script;
    }

    internal class BasicTest : MonoBehaviour
    {
        public List<CoffeeTestInterface> tests;

        // Start is called before the first frame update
        async void Start()
        {
            CoffeeTestFramework framework = new CoffeeTestFramework();
            await UniTask.Yield();

            framework.oneTimeSetUp = this.OneTimeSetUp;
            framework.oneTimeTearDown = this.OneTimeTearDown;
            framework.setUp = this.SetUp;
            framework.tearDown = this.TearDown;

            //await framework.Start(tests);

            var memlist = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Log(memlist.Length);
            foreach(var mem in memlist)
            {
                if (mem.IsDefined(typeof(CoffeeTest)))
                {
                    Debug.Log(" " + mem.DeclaringType + " " + mem.Name);
                }
            }
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