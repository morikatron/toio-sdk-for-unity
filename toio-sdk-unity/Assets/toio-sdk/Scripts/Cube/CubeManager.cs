using System;
using System.Collections.Generic;
using UnityEngine;
using toio.Navigation;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class CubeManager
    {
        // 接続済みCube管理
        public List<Cube> cubes;
        public Dictionary<string, Cube> cubeTable;
        public List<CubeHandle> handles;
        public List<CubeNavigator> navigators;
        // 接続機能
        protected NearestScannerInterface nearestScanner;
        protected NearScannerInterface nearScanner;
        protected CubeConnecterInterface connecter;
        protected Action<Cube, CONNECTION_STATUS> connectedAction;

        public bool synced
        { get {
            if (cubes.Count == 0) return false;
            foreach (var cube in cubes)
                if (!IsControllable(cube))
                    return false;
            // Update all navigators (Update runs only once within 15ms)
            foreach (var navigator in navigators)
                navigator.Update();
            return true;
        }}

        public List<Cube> syncCubes
        { get {
            // Return empty list if any cube is not controllable
            if (!synced) return new List<Cube>();
            return cubes;
        }}
        public List<CubeHandle> syncHandles
        { get {
            // Return empty list if any handle is not controllable
            if (!synced) return new List<CubeHandle>();
            return handles;
        }}
        public List<CubeNavigator> syncNavigators
        { get {
            // Return empty list if any navigator is not controllable
            if (!synced) return new List<CubeNavigator>();
            return navigators;
        }}


        // --- public methods ---
        public CubeManager()
        {
            this.cubes = new List<Cube>();
            this.handles = new List<CubeHandle>();
            this.navigators = new List<CubeNavigator>();
            this.cubeTable = new Dictionary<string, Cube>();
        }

        public virtual async UniTask<Cube> SingleConnect()
        {
            if (null == this.nearestScanner)
            {
                this.nearestScanner = new NearestScanner();
            }
            if (null == this.connecter)
            {
                this.connecter = new CubeConnecter();
            }
            var peripheral = await this.nearestScanner.Scan();
            if (null == peripheral) { return null; }
            var cube = await this.connecter.Connect(peripheral);
            this.AddCube(cube);
            return cube;
        }

        public virtual async UniTask<Cube[]> MultiConnect(int cubeNum)
        {
#if UNITY_WEBGL
            Debug.Log("[CubeManager.MultiConnect]MultiConnect doesn't run on the web");
#endif
            if (null == this.nearScanner)
            {
                this.nearScanner = new NearScanner(cubeNum);
            }
            if (null == this.connecter)
            {
                this.connecter = new CubeConnecter();
            }
            var peripheral = await this.nearScanner.Scan();
            var cubes = await this.connecter.Connect(peripheral);
            this.AddCube(cubes);

            return cubes;
        }

        public virtual void MultiConnectAsync(int cubeNum, MonoBehaviour coroutineObject, Action<Cube, CONNECTION_STATUS> connectedAction =null, bool autoRunning=true)
        {
            if (null == this.nearScanner)
            {
                this.nearScanner = new NearScanner(cubeNum);
            }
            this.connectedAction = connectedAction;
            this.nearScanner.ScanAsync(coroutineObject, this.OnPeripheralScanned, autoRunning);
        }

        public virtual void Disconnect(Cube cube)
        {
            this.connecter.Disconnect(cube);
        }

        /// <summary>
        /// 前回のCubeへの送信から45ミリ秒以上空いていた時にTrueが返ります.
        /// </summary>
        public bool IsControllable(Cube cube)
        {
            return (null != cube) && (cube.isConnected) && (CubeOrderBalancer.Instance.IsOrderable(cube));
        }
        public bool IsControllable(CubeHandle handle)
        {
            return IsControllable(handle.cube);
        }
        public bool IsControllable(CubeNavigator navigator)
        {
            return IsControllable(navigator.cube);
        }

        public void SetNearScanner(NearScannerInterface scanner)
        {
            this.nearScanner = scanner;
        }

        public void SetNearestScanner(NearestScannerInterface scanner)
        {
            this.nearestScanner = scanner;
        }

        public void SetCubeConnecter(CubeConnecterInterface _connecter)
        {
            this.connecter = _connecter;
        }

        // --- private methods ---
        protected async void OnPeripheralScanned(BLEPeripheralInterface peripheral)
        {
            if (null == this.connecter)
            {
                this.connecter = new CubeConnecter();
            }
            if (this.cubeTable.ContainsKey(peripheral.device_address))
            {
                var cube = this.cubeTable[peripheral.device_address];
                await this.connecter.ReConnect(cube, peripheral);
                this.connectedAction(cube, new CONNECTION_STATUS(CONNECTION_STATUS.RE_CONNECTED));
            }
            else
            {
                var cube = await this.connecter.Connect(peripheral);
                this.AddCube(cube);
                if (null != this.connectedAction)
                {
                    this.connectedAction(cube, new CONNECTION_STATUS(CONNECTION_STATUS.NEW_CONNECTED));
                }
            }
        }

        protected void AddCube(Cube cube)
        {
            if (this.cubeTable.ContainsKey(cube.id)) return;
            this.cubes.Add(cube);
            var handle = new CubeHandle(cube);
            this.handles.Add(handle);
            this.navigators.Add(new CubeNavigator(handle));
            this.cubeTable.Add(cube.id, cube);
        }

        protected void AddCube(Cube[] cubes)
        {
            foreach(var cube in cubes)
            {
               AddCube(cube);
            }
        }
    }

    public struct CONNECTION_STATUS
    {
        public const int NEW_CONNECTED = 1;
        public const int RE_CONNECTED = 2;
        private int status;

        public bool IsNewConnected { get { return NEW_CONNECTED == this.status; } }
        public bool IsReConnected { get { return RE_CONNECTED == this.status; } }
        public CONNECTION_STATUS(int status)
        {
            this.status = status;
        }
    }
}
