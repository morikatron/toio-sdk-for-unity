
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif


#if UNITY_ANDROID_RUNTIME
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace toio.Android
{
    public class BleBehaviour : MonoBehaviour
    {
        struct ExecuteInfo
        {
            public IEnumerator enumerator;
            public Action onEndFunc;
        }
        private List<ExecuteInfo> executeInfos = new List<ExecuteInfo>();
        private List<Action> updateActions = new List<Action>();

        public static BleBehaviour Create()
        {
            var gmo = new GameObject();
            gmo.name = "BleBehaviour";
            GameObject.DontDestroyOnLoad(gmo);
            var obj = gmo.AddComponent<BleBehaviour>();
            return obj;
        }
        public void DeleteObject()
        {
            GameObject.Destroy(this.gameObject);
        }

        public void AddExecute( IEnumerator e,Action onEnd)
        {
            ExecuteInfo executeInfo = new ExecuteInfo()
            {
                enumerator = e,
                onEndFunc = onEnd
            };
            executeInfos.Add(executeInfo);
        }
        public void AddUpdateAction(Action act)
        {
            updateActions.Add(act);
        }
        public void RemoveUpdateAction(Action act)
        {
            updateActions.Remove(act);
        }

        private void Update()
        {            
            for(int i = 0; i < this.executeInfos.Count; ++i)
            {
                var executeInfo = this.executeInfos[i];
                bool next = executeInfo.enumerator.MoveNext();
                if (!next)
                {
                    if(executeInfo.onEndFunc != null)
                    {
                        executeInfo.onEndFunc();
                    }
                    this.executeInfos.RemoveAt(i);
                    --i;
                }
            }
            // update actions 
            foreach( var act in this.updateActions)
            {
                act();
            }
        }
    }
}
#endif