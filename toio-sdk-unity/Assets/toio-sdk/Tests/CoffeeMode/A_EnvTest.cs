using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio.Tests
{
    public class A_EnvTest : CoffeeTestCase
    {
        [CoffeeTest]
        public async UniTask aeee()
        {
            Debug.Log("aa");
            await UniTask.Yield();
        }
    }
}