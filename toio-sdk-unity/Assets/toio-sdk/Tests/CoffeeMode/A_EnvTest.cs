using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using toio;
using toio.Navigation;
using toio.MathUtils;

namespace toio.Tests
{
    // ref1 UniTaskでレッツ非同期！( http://softmedia.sakura.ne.jp/advent-calendar/2019/12-1.html )
    // ref2 UniTaskの使い方2020 ( https://speakerdeck.com/torisoup/unitask2020 )
    // ref3 Cysharp/UniTask ( https://github.com/Cysharp/UniTask )
    // ref4 API UniTask ( https://cysharp.github.io/UniTask/api/Cysharp.Threading.Tasks.UniTask.html )
    // ref5 API UniTaskAsyncEnumerable ( https://cysharp.github.io/UniTask/api/Cysharp.Threading.Tasks.Linq.UniTaskAsyncEnumerable.html )
    public class A_EnvTest : CoffeeTestCase
    {
        //[CoffeeTest]
        public async UniTask HelloTest(CancellationToken token)
        {
            Debug.Log("HelloTest!!!");
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        //[CoffeeTest]
        public async UniTask Updateテスト_時間指定(CancellationToken token)
        {
            Debug.Log("start");
            await UpdateForSeconds(3, () =>
            {
                Debug.Log("Update() " + Time.frameCount);
            }, token);
            Debug.Log("end");
        }

        //[CoffeeTest]
        public async UniTask Updateテスト_while(CancellationToken token)
        {
            Debug.Log("start");
            int cnt = 0;
            bool flg = true;
            await UpdateWhile((() => flg), () =>
            {
                flg = (cnt++ < 100);
                Debug.Log("Update() " + Time.frameCount);
            }, token);
            Debug.Log("end");
        }

        //[CoffeeTest]
        public async UniTask Delayテスト(CancellationToken token)
        {
            Debug.Log("start");
            Debug.LogFormat("Time.frameCount = {0}", Time.frameCount);
            await UniTask.Delay(5000);
            Debug.LogFormat("Time.frameCount = {0}", Time.frameCount);
            await UniTask.Delay(1000);
            Debug.LogFormat("Time.frameCount = {0}", Time.frameCount);
            Debug.Log("end");
        }

        //[CoffeeTest]
        public async UniTask Delayテスト_for文(CancellationToken token)
        {
            Debug.Log("start");
            for(int i = 0; i < 3; i++)
            {
                Debug.LogFormat("i = {0}, Time.frameCount = {1}", i, Time.frameCount);
                await UniTask.Delay(2000);
                Debug.LogFormat("i = {0}, Time.frameCount = {1}", i, Time.frameCount);
                await UniTask.Delay(1000);
                Debug.LogFormat("i = {0}, Time.frameCount = {1}", i, Time.frameCount);
            }
            Debug.Log("end");
        }

        //[CoffeeTest]
        public async UniTask Test1(CancellationToken token)
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
                await UniTask.NextFrame();
            }
        }
    }
}