using UnityEngine;
using Cysharp.Threading.Tasks;
using toio;
using toio.Navigation;
using toio.MathUtils;

namespace toio.Tests
{
    public class A_EnvTest : CoffeeTestCase
    {
        [CoffeeTest]
        public async UniTask Test1()
        {
            Debug.Log("Test1");

            var cm = new CubeManager();
            await cm.SingleConnect();

            foreach (var navi in cm.navigators)
            {
                navi.usePred = true;
                navi.mode = Navigator.Mode.BOIDS;
            }

            while(true)
            {
                var tar = Vector.fromRadMag(Time.time/1, 160) + new Vector(250, 250);
                if (cm.synced){

                    for (int i=0; i<cm.navigators.Count; i++)
                    {
                        var navi = cm.navigators[i];
                        navi.mode = Navigator.Mode.BOIDS;
                        var mv = navi.Navi2Target(tar, maxSpd:60).Exec();
                    }
                }
                await UniTask.Yield();
            }
        }
        [CoffeeTest]
        public async UniTask Test2()
        {
            Debug.Log("Test2");
        }
    }
}