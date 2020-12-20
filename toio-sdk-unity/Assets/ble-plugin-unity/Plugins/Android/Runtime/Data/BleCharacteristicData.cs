
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif


#if UNITY_ANDROID_RUNTIME
namespace toio.Android.Data
{
    public struct BleCharacteristicData
    {
        public string deviceAddr;
        public string characteristic;
        public byte[] data;
        public int length;
        public bool isNotify;

        public BleCharacteristicData(string addr,
            string ch,sbyte[] sbytes,bool isNot)
        {
            this.deviceAddr = addr;
            this.characteristic = ch;
            this.isNotify = isNot;
            this.length = sbytes.Length;
            this.data = new byte[sbytes.Length];
            for( int i = 0; i < length; ++i)
            {
                data[i] = unchecked((byte)sbytes[i]);
            }
        }
    }
}
#endif