using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace toio
{
    public class RequestInfo
    {
        public Cube cube;
        public float deadline;
        public Action<bool, Cube> callback;
        public Action request;
        public bool isRequesting = false;
        public bool hasConfigResponse = false;
        public bool isConfigResponseSucceeded = false;
        // public bool wasTimeOut = false;
        public bool hasReceivedData = false;

        public RequestInfo(Cube cube)
        {
            this.cube = cube;
        }

        public async UniTask<bool> GetAccess(float deadline, Action<bool, Cube> callback)
        {
            while (this.isRequesting)
            {
                if (deadline < Time.time)
                {
                    callback?.Invoke(false, this.cube);
                    return false;
                }
                await UniTask.Delay(50);
            }

            // Get Access
            this.isRequesting = true;
            this.hasConfigResponse = false;
            this.isConfigResponseSucceeded = false;
            this.hasReceivedData = false;
            this.deadline = deadline;
            this.callback = callback;
            return true;
        }

        public async UniTask Run()
        {
            this.isRequesting = true;
            bool responded = true;
            while (!this.hasConfigResponse)
            {
                if (deadline < Time.time)
                {
                    responded = false;
                    break;
                }

                if (!this.cube.isConnected)
                {
                    await UniTask.Delay(50);
                    continue;
                }

                this.request?.Invoke();
                await UniTask.Delay(200);
            }

            this.isRequesting = false;
            callback?.Invoke(responded && this.isConfigResponseSucceeded, this.cube);
        }
    }

}