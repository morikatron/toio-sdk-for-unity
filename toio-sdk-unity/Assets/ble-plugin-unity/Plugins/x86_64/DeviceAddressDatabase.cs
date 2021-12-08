
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace toio.Windows
{
    public class DeviceAddressDatabase
    {
        private static readonly char[] Digit = new char[] {
            '0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'
        };
        private static StringBuilder stringBuffer;
        private static Dictionary<string, ulong> addrDictByStr;
        private static Dictionary<ulong, string> strDictByAddr;

        static DeviceAddressDatabase()
        {
            addrDictByStr = new Dictionary<string, ulong>();
            strDictByAddr = new Dictionary<ulong, string>();
        }

        public static string GetAddressStr(ulong addr)
        {
            string str;
            if (strDictByAddr.TryGetValue(addr, out str))
            {
                return str;
            }
            str = ConvertToString(addr);
            addrDictByStr.Add(str, addr);
            strDictByAddr.Add(addr, str);
            return str;
        }
        public static ulong GetAddressValue(string str)
        {
            ulong addr;
            if(addrDictByStr.TryGetValue(str,out addr)){
                return addr;
            }
            addr = ConvertFromString(str);
            addrDictByStr.Add(str, addr);
            strDictByAddr.Add(addr, str);
            return addr;
        }
        private static string ConvertToString(ulong addr)
        {
            if(stringBuffer == null)
            {
                stringBuffer = new StringBuilder(16);
            }
            stringBuffer.Clear();
            for (int i = 0; i < 16; ++i)
            {
                int bitShift = (15 - i) * 4;
                int val = (int)((addr & (0x0fUL << bitShift)) >> bitShift);
                stringBuffer.Append(Digit[val]);
            }
            return stringBuffer.ToString();
        }
        private static ulong ConvertFromString(string str)
        {
            int max = str.Length;
            if( max > 16) { max = 16; }
            ulong addr = 0;
            for (int i = 0; i < max; ++i)
            {
                int bitShift = ((max-1) - i) * 4;
                int val = 0;
                char ch = str[i];
                if( ch >= 'A')
                {
                    val = (ch - 'A') + 10;
                }
                else
                {
                    val = ch - '0';
                }
                addr |= (ulong)val << bitShift;
            }
            return addr;
        }
    }
}

#endif