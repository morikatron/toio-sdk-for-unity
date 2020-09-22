using UnityEngine;

namespace toio
{
    // see http://waken.hatenablog.com/entry/2016/03/05/102928
    public class GenericSingleton<T> where T : class, new()
    {
        // 万一、外からコンストラクタを呼ばれたときに、ここで引っ掛ける
        protected GenericSingleton()
        {
            Debug.Assert(null == _instance);
        }
        private static readonly T _instance = new T();

        public static T Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}