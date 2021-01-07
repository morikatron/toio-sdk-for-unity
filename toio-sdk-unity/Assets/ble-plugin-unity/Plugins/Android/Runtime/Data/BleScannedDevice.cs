
#if UNITY_ANDROID && !UNITY_EDITOR
#define UNITY_ANDROID_RUNTIME
#endif


#if UNITY_ANDROID_RUNTIME
namespace toio.Android.Data
{
    public struct BleScannedDevice
    {
        public string address;
        public string name;
        public int rssi;

        public BleScannedDevice(string addr,string n,int r)
        {
            this.address = addr;
            this.name = n;
            this.rssi = r;
        }
        public void setRssi(int r)
        {
            this.rssi = r;
        }
    }
}
#endif