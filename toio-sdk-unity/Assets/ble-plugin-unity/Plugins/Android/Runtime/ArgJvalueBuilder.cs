
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif


#if UNITY_ANDROID_RUNTIME

using System.Collections.Generic;
using UnityEngine;

namespace toio.Android
{
    public class ArgJvalueBuilder
    {
        jvalue[][] argPat1;
        List<jvalue> buffer = new List<jvalue>(8);

        public ArgJvalueBuilder()
        {
            argPat1 = new jvalue[5][];
        }

        public ArgJvalueBuilder Clear()
        {
            buffer.Clear();
            return this;
        }
        public ArgJvalueBuilder Append(jvalue val)
        {
            buffer.Add(val);
            return this;
        }
        public jvalue[] Build()
        {
            int cnt = buffer.Count;
            if(cnt == 0) { return null; }
            if(cnt > argPat1.Length)
            {
                return buffer.ToArray();
            }
            int idx = cnt - 1;
            if(argPat1[idx] == null)
            {
                argPat1[idx] = new jvalue[cnt]; 
            }
            for(int i = 0; i < cnt; ++i)
            {
                argPat1[idx][i] = buffer[i];
            }

            return argPat1[idx];
        }

        public static jvalue GenerateJvalue(System.IntPtr ptr)
        {
            jvalue arg = new jvalue();
            arg.l = ptr;
            return arg;
        }
        public static jvalue GenerateJvalue(byte[] bin,int length)
        {
            jvalue arg = new jvalue();
            var ptr = AndroidJNI.NewSByteArray(length);
            for (int i = 0; i < length; ++i) {
                AndroidJNI.SetSByteArrayElement(ptr, i, unchecked((sbyte)bin[i]) );
            }
            arg.l = ptr;
            return arg;
        }

        public static jvalue GenerateJvalue(string str)
        {
            jvalue arg = new jvalue();
            arg.l = AndroidJNI.NewStringUTF(str);
            return arg;
        }
        public static jvalue GenerateJvalue(int param)
        {
            jvalue arg = new jvalue();
            arg.i = param;
            return arg;
        }
        public static jvalue GenerateJvalue(bool param)
        {
            jvalue arg = new jvalue();
            arg.z = param;
            return arg;
        }

    }
}
#endif
