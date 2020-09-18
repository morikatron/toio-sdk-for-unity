using System;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class BLEService : GenericSingleton<BLEService>
    {
        public bool hasImplement { get; private set; }

        public BLEService()
        {
            hasImplement = false;
        }

        private BLEServiceInterface impl;

        // 実装インスタンスを外部から設定
        public void SetImplement(BLEServiceInterface impl)
        {
            if (this.hasImplement) { return; }

            this.impl = impl;
            this.hasImplement = true;
        }

        // 動作端末のBLE機能変数を取得
        public void RequestDevice(Action<BLEDeviceInterface> action)
        {
            this.impl.RequestDevice(action);
        }
        // BLE機能をオン/オフ
        public async UniTask Enable(bool enable, Action action)
        {
            await this.impl.Enable(enable, action);
        }
        public void DisconnectAll()
        {
            if (null != this.impl) { this.impl.DisconnectAll(); }
        }
    }
}