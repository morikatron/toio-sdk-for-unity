using System;
using System.Collections;
//using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    /// <summary>
    /// Interface.
    /// </summary>
    public interface NearScannerInterface
    {
        int satisfiedNum { get; }
        int scanningNum { get; }
        bool isScanning { get; }

        UniTask<BLEPeripheralInterface[]> Scan(float waitSeconds = 3.0f);
        void ScanAsync(MonoBehaviour coroutineObject, Action<BLEPeripheralInterface> callback, bool autoRunning = true);
    }

    /// <summary>
    /// Abstraction in bridge pattern.
    /// </summary>
    public class NearScanner : NearScannerInterface
    {
        // --- public fields ---
        public int satisfiedNum { get { return this.impl.satisfiedNum; } }
        public int scanningNum { get { return this.impl.scanningNum; } }
        public bool isScanning { get { return this.impl.isScanning; } }

        // --- private fields ---
        private NearScannerInterface impl;

        public NearScanner(int satisfiedNum, ConnectType type = ConnectType.Auto)
        {
            // プリセットで用意したマルチプラットフォーム内部実装(UnityEditor/Mobile/WebGL)
            this.impl = new AdapterImpl(type, satisfiedNum);
        }

        // --- public methods ---
        public NearScanner(int satisfiedNum, NearScannerInterface impl)
        {
            // NearScannerの内部実装を外部入力から変更
            this.impl = impl;
        }

        public async UniTask<BLEPeripheralInterface[]> Scan(float waitSeconds = 3.0f)
        {
            return await this.impl.Scan(waitSeconds);
        }

        public void ScanAsync(MonoBehaviour coroutineObject, Action<BLEPeripheralInterface> callback, bool autoRunning=true)
        {
            this.impl.ScanAsync(coroutineObject, callback, autoRunning);
        }

        public class AdapterImpl : NearScannerInterface
        {
            public int satisfiedNum { get; }
            public int scanningNum { get { return 0; } }
            public bool isScanning { get { return this.scanner.isScanning; } }
            private CubeScanner scanner;
            public AdapterImpl(ConnectType type, int satisfiedNum) { this.scanner = new CubeScanner(type); this.satisfiedNum = satisfiedNum; }
            public async UniTask<BLEPeripheralInterface[]> Scan(float waitSeconds) { return await this.scanner.NearScan(this.satisfiedNum, waitSeconds); }
            public void ScanAsync(MonoBehaviour coroutineObject, Action<BLEPeripheralInterface> callback, bool autoRunning) { this.scanner.NearScanAsync(this.satisfiedNum, coroutineObject, callback, autoRunning); }
        }
    }
}