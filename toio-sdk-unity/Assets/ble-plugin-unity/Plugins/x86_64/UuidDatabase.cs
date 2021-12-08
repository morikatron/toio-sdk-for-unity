using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
namespace toio.Windows
{
    public class UuidDatabase
    {
        private static Dictionary<string, UuidHandler> uuidDictByStr;
        private static Dictionary<UuidHandler,string> uuidDictByHandle;

        static UuidDatabase()
        {
            uuidDictByStr = new Dictionary<string, UuidHandler>();
            uuidDictByHandle = new Dictionary<UuidHandler, string>();
        }

        public static UuidHandler GetUuid(string str)
        {
            uint d1, d2, d3, d4;
            str = str.ToUpper();
            var handle = new UuidHandler();
            if(uuidDictByStr.TryGetValue(str,out handle))
            {
                return handle;
            }
            ParseUuid(str, out d1, out d2, out d3, out d4);
            handle = DllInterface.GetOrCreateUuidObject(d1, d2, d3, d4);
            uuidDictByStr.Add(str, handle);
            uuidDictByHandle.Add(handle,str);
            return handle;
        }

        public static string GetUuidStr(UuidHandler handle)
        {
            string str;
            if (uuidDictByHandle.TryGetValue(handle, out str))
            {
                return str;
            }
            var data = DllInterface.ConvertUuidData(handle);
            str = data.ToString();
            uuidDictByStr.Add(str, handle);
            uuidDictByHandle.Add(handle, str);
            return str;

        }

        private static void ParseUuid(string str,         
            out uint d1, out uint d2, out uint d3, out uint d4)
        {
            d1 = d2 = d3 = d4 = 0;
            int idx = 0;
            idx = GetUintFromString(str, idx, out d1);
            idx = GetUintFromString(str, idx, out d2);
            idx = GetUintFromString(str, idx, out d3);
            idx = GetUintFromString(str, idx, out d4);
        }

        private static int GetUintFromString(string str, int idx, out uint data)
        {
            data = 0;
            int length = str.Length;
            int currentExecChar = 0;
            for (; idx < length; ++idx)
            {
                char ch = str[idx];
                int val = 0;
                if ('0' <= ch && ch <= '9')
                {
                    val = ch - '0';
                }
                else if ('A' <= ch && ch <= 'F')
                {
                    val = (ch - 'A') + 10;
                }
                else if ('a' <= ch && ch <= 'f')
                {
                    val = (ch - 'a') + 10;
                }
                else
                {
                    continue;
                }
                data = data << 4;
                data |= (uint)val;

                ++currentExecChar;
                if (currentExecChar >= 8)
                {
                    break;
                }
            }
            return idx + 1;
        }
    }
}
#endif