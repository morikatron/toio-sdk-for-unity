//using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    public interface NearestScannerInterface
    {
        UniTask<BLEPeripheralInterface> Scan();
    }

    public class NearestScanner : NearestScannerInterface
    {
        private NearestScannerInterface impl;

        public NearestScanner(ConnectType type = ConnectType.Auto)
        {
            // プリセットで用意したマルチプラットフォーム内部実装(UnityEditor/Mobile/WebGL)
            this.impl = new AdapterImpl(type);
        }

        public NearestScanner(NearestScannerInterface impl)
        {
            // NearestScannerの内部実装を外部入力から変更
            this.impl = impl;
        }

        public async UniTask<BLEPeripheralInterface> Scan()
        {
            return await this.impl.Scan();
        }

        public class AdapterImpl : NearestScannerInterface
        {
            private CubeScanner scanner;
            public AdapterImpl(ConnectType type) { this.scanner = new CubeScanner(type); }
            public async UniTask<BLEPeripheralInterface> Scan() { return await this.scanner.NearestScan(); }
        }
    }
}
