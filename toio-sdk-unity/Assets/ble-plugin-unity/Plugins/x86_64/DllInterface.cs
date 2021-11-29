using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
namespace toio.Windows
{
    public struct UuidHandler
    {
        public IntPtr ptr;
        public UuidHandler(IntPtr p)
        {
            ptr = p;
        }
    }
    public struct BleDeviceHandler
    {
        public IntPtr ptr;
        public BleDeviceHandler(IntPtr p)
        {
            ptr = p;
        }
    }
    public struct ReadRequestHandler
    {
        public IntPtr ptr;
        public ReadRequestHandler(IntPtr p)
        {
            ptr = p;
        }
    }
    public struct WriteRequestHandler
    {
        public IntPtr ptr;
        public WriteRequestHandler(IntPtr p)
        {
            ptr = p;
        }
    }
    public struct UuidData
    {
        public uint data1;
        public uint data2;
        public uint data3;
        public uint data4;

        public override string ToString()
        {
            return string.Format("{0:X8}-{1:X4}-{2:X4}-{3:X4}-{4:X4}{5:X8}", data1,
                (data2>>16) &0xffff,
                (data2 & 0xffff),
                (data3 >> 16) & 0xffff,
                (data3 & 0xffff),data4 );
        }
    }
    internal unsafe struct UuidBuffer
    {
        public fixed uint fixedBuffer[4];
    }
    internal unsafe struct CharastricsBuffer
    {
        public const int BufferSize = 22;
        public fixed byte fixedBuffer[BufferSize];
    }

    public class DllInterface
    {
        const string pluginName = "BlePluginWinows";

        public enum EBluetoothStatus : int
        {
            None = -1,
            Fine = 0,
            NotSupportBle = 1,
            BluetoothDisable = 2,
            UnknownError = 99
        };

        [DllImport(pluginName)]
        private static extern void _BlePluginBleAdapterStatusRequest();
        public static void BleAdapterStatusRequest() {
            _BlePluginBleAdapterStatusRequest();
        }

        [DllImport(pluginName)]
        private static extern int _BlePluginBleAdapterUpdate();
        public static EBluetoothStatus BleAdapterUpdate()
        {
            int val = _BlePluginBleAdapterUpdate();
            EBluetoothStatus status = (EBluetoothStatus)val;
            return status;
        }

        [DllImport(pluginName)]
        private static extern void _BlePluginFinalize();
        public static void FinalizePlugin()
        {
            _BlePluginFinalize();
        }


        [DllImport(pluginName)]
        private static extern IntPtr _BlePluginGetOrCreateUuidObject(uint d1, uint d2, uint d3, uint d4);
        public static UuidHandler GetOrCreateUuidObject(uint d1, uint d2, uint d3, uint d4)
        {
            var ptr = _BlePluginGetOrCreateUuidObject(d1, d2, d3, d4);
            return new UuidHandler(ptr);
        }


        [DllImport(pluginName)]
        private static extern void _BlePluginConvertUuidUint128(IntPtr ptr,IntPtr outData);
        public static unsafe UuidData ConvertUuidData(UuidHandler handler)
        {
            UuidData data = new UuidData();
            UuidBuffer uuidBuffer = new UuidBuffer();
            void* ptr = &uuidBuffer.fixedBuffer[0];
            _BlePluginConvertUuidUint128(handler.ptr, new IntPtr(ptr));

            data.data1 = uuidBuffer.fixedBuffer[0];
            data.data2 = uuidBuffer.fixedBuffer[1];
            data.data3 = uuidBuffer.fixedBuffer[2];
            data.data4 = uuidBuffer.fixedBuffer[3];
            return data;
        }

        [DllImport(pluginName)]
        private static extern void _BlePluginUpdateWatcher();
        [DllImport(pluginName)]
        private static extern void _BlePluginUpdateDevicdeManger();

        public static void UpdateFromMainThread()
        {
            _BlePluginUpdateWatcher();
            _BlePluginUpdateDevicdeManger();
        }

        [DllImport(pluginName)]
        public static extern void _BlePluginAddScanServiceUuid(IntPtr ptr);
        public static void AddScanServiceUuid(UuidHandler uuid)
        {
            _BlePluginAddScanServiceUuid(uuid.ptr);
        }

        [DllImport(pluginName)]
        private static extern void _BlePluginStartScan();
        public static void StartScan()
        {
            _BlePluginStartScan();
        }

        [DllImport(pluginName)]
        private static extern void _BlePluginStopScan();
        public static void StopScan()
        {
            _BlePluginStopScan();
        }

        [DllImport(pluginName)]
        private static extern void _BlePluginClearScanFilter();
        public static void ClearScanFilter()
        {
            _BlePluginClearScanFilter();
        }

        // Scan Data
        [DllImport(pluginName)]
        private static extern int _BlePluginScanGetDeviceLength();
        public static int ScanGetDeviceLength()
        {
            return _BlePluginScanGetDeviceLength();
        }

        [DllImport(pluginName)]
        private static extern ulong _BlePluginScanGetDeviceAddr(int idx);
        public static ulong ScanGetDeviceAddr(int idx)
        {
            return _BlePluginScanGetDeviceAddr(idx);
        }

        [DllImport(pluginName)]
        private static extern string _BlePluginScanGetDeviceName(int idx);
        public static string ScanGetDeviceName(int idx)
        {
            return _BlePluginScanGetDeviceName(idx);
        }
        [DllImport(pluginName)]
        private static extern int _BlePluginScanGetDeviceRssi(int idx);
        public static int ScanGetDeviceRssi(int idx)
        {
            return _BlePluginScanGetDeviceRssi(idx);
        }

        // Connect Dissconnect
        [DllImport(pluginName)]
        private static extern IntPtr _BlePluginConnectDevice(ulong addr);
        public static void ConnectDevice(ulong addr)
        {
            _BlePluginConnectDevice(addr);
        }

        [DllImport(pluginName)]
        private static extern void _BlePluginDisconnectDevice(ulong addr);
        public static void DisconnectDevice(ulong addr)
        {
            _BlePluginDisconnectDevice(addr);
        }
        [DllImport(pluginName)]
        private static extern void _BlePluginDisconnectAllDevice();
        public static void DisconnectAllDevice()
        {
            _BlePluginDisconnectAllDevice();
        }

        [DllImport(pluginName)]
        private static extern bool _BlePluginIsDeviceConnectedByAddr(ulong addr);
        public static bool IsDeviceConnected(ulong addr)
        {
            return _BlePluginIsDeviceConnectedByAddr(addr);
        }

        [DllImport(pluginName)]
        private static extern bool _BlePluginIsDeviceConnected(IntPtr devicePtr);
        public static bool IsDeviceConnected(BleDeviceHandler handle)
        {
            return _BlePluginIsDeviceConnected(handle.ptr);
        }
        [DllImport(pluginName)]
        private static extern ulong _BlePluginDeviceGetAddr(IntPtr devicePtr);
        public ulong GetDeviceAddr(BleDeviceHandler handle) {
            return _BlePluginDeviceGetAddr(handle.ptr);
        }

        // GetDevice
        [DllImport(pluginName)]
        private static extern int _BlePluginGetConectDeviceNum();
        public static int GetConnectDeviceNum()
        {
            return _BlePluginGetConectDeviceNum();
        }

        [DllImport(pluginName)]
        private static extern ulong _BlePluginGetConectDevicAddr(int idx);
        public static ulong GetConnectDeviceAddr(int idx)
        {
            return _BlePluginGetConectDevicAddr(idx);
        }

        [DllImport(pluginName)]
        private static extern IntPtr _BlePluginGetConnectDevicePtr(int idx);
        public static BleDeviceHandler GetConnectDevicePtr(int idx)
        {
            var ptr = _BlePluginGetConnectDevicePtr(idx);
            var handle =new BleDeviceHandler(ptr);
            return handle;
        }

        [DllImport(pluginName)]
        private static extern IntPtr _BlePluginGetDevicePtrByAddr(ulong addr);
        public static BleDeviceHandler GetDeviceHandleByAddr(ulong addr)
        {
            var ptr = _BlePluginGetDevicePtrByAddr(addr);
            var handle = new BleDeviceHandler(ptr);
            return handle;
        }

        [DllImport(pluginName)]
        private static extern int _BlePluginDeviceCharastricsNum(IntPtr devicePtr);
        public static int GetDeviceCharastricsNum(BleDeviceHandler handle)
        {
            return _BlePluginDeviceCharastricsNum(handle.ptr);
        }

        [DllImport(pluginName)]
        private static extern IntPtr _BlePluginDeviceCharastricUuid(IntPtr devicePtr, int idx);
        public static UuidHandler GetDeviceCharastricUuid(BleDeviceHandler handle,int idx)
        {
            var ptr = _BlePluginDeviceCharastricUuid(handle.ptr, idx);
            var uuid = new UuidHandler(ptr);
            return uuid;
        }

        [DllImport(pluginName)]
        private static extern IntPtr _BlePluginDeviceCharastricServiceUuid(IntPtr devicePtr, int idx);
        public static UuidHandler GetDeviceCharastricServiceUuid(BleDeviceHandler handle,int idx)
        {
            var ptr = _BlePluginDeviceCharastricServiceUuid(handle.ptr, idx);
            var uuid = new UuidHandler(ptr);
            return uuid;

        }


        // Read/Write Request
        [DllImport(pluginName)]
        private static extern IntPtr _BlePluginReadCharacteristicRequest(ulong addr, IntPtr serviceUuid, IntPtr charaUuid);
        public static ReadRequestHandler ReadCharastristicRequest(ulong addr,UuidHandler serviceUuid,UuidHandler charaUuid)
        {
            var ptr = _BlePluginReadCharacteristicRequest(addr, serviceUuid.ptr, charaUuid.ptr);
            var handle = new ReadRequestHandler(ptr);
            return handle;
        }
        [DllImport(pluginName)]
        private static extern IntPtr _BlePluginWriteCharacteristicRequest(ulong addr, IntPtr serviceUuid, IntPtr charaUuid, IntPtr data, int size);
        public static unsafe WriteRequestHandler WriteCharastristicRequest(ulong addr,UuidHandler serviceUuid,UuidHandler charaUuid, byte[] data, int idx,int size)
        {
            IntPtr resultPtr;
            fixed (void* ptr = &data[idx])
            {
                var dataPtr = new IntPtr(ptr);
                resultPtr = _BlePluginWriteCharacteristicRequest(addr, serviceUuid.ptr, charaUuid.ptr, dataPtr, size);
            }
            var handle = new WriteRequestHandler(resultPtr);
            return handle;
        }
        public static WriteRequestHandler WriteCharastristicRequest(ulong addr, UuidHandler serviceUuid, UuidHandler charaUuid, byte[] data)
        {
            return WriteCharastristicRequest(addr, serviceUuid, charaUuid, data, 0, data.Length);
        }

        [DllImport(pluginName)]
        private static extern bool _BlePluginIsReadRequestComplete(IntPtr ptr);
        public static bool IsReadRequestComplete(ReadRequestHandler handle)
        {
            return _BlePluginIsReadRequestComplete(handle.ptr);
        }

        [DllImport(pluginName)]
        private static extern bool _BlePluginIsReadRequestError(IntPtr ptr);
        public static bool IsReadRequestError(ReadRequestHandler handle)
        {
            return _BlePluginIsReadRequestError(handle.ptr);
        }

        [DllImport(pluginName)]
        private static extern int _BlePluginCopyReadRequestData(IntPtr ptr, IntPtr data, int maxSize);
        public static unsafe byte[] GetReadRequestData(ReadRequestHandler handle,int maxSize)
        {
            byte[] retData = null;
            var buffer = new CharastricsBuffer();
            void* ptr = &buffer.fixedBuffer[0];
            var writePtr = new IntPtr(ptr);

            int size = _BlePluginCopyReadRequestData(handle.ptr, writePtr,
                CharastricsBuffer.BufferSize);

            retData = new byte[size];
            for(int i = 0; i < size; ++i)
            {
                retData[i] = buffer.fixedBuffer[i];
            }
            return retData;
        }

        [DllImport(pluginName)]
        private static extern void _BlePluginReleaseReadRequest(ulong deviceaddr, IntPtr ptr);
        public static void ReleaseReadRequest(ulong deviceAddr,ReadRequestHandler handle)
        {
            _BlePluginReleaseReadRequest(deviceAddr, handle.ptr);
        }

        [DllImport(pluginName)]
        private static extern bool _BlePluginIsWriteRequestComplete(IntPtr ptr);
        public static bool IsWriteRequestComplete(WriteRequestHandler handle)
        {
            return _BlePluginIsWriteRequestComplete(handle.ptr);
        }

        [DllImport(pluginName)]
        private static extern bool _BlePluginIsWriteRequestError(IntPtr ptr);
        public static bool IsWriteRequestError(WriteRequestHandler handle)
        {
            return _BlePluginIsWriteRequestError(handle.ptr);
        }

        [DllImport(pluginName)]
        private static extern void _BlePluginReleaseWriteRequest(ulong deviceaddr, IntPtr ptr);

        public static void ReleaseWriteRequest(ulong deviceAddr, WriteRequestHandler handle)
        {
            _BlePluginReleaseWriteRequest(deviceAddr, handle.ptr);
        }

        // Notificate
        [DllImport(pluginName)]
        private static extern void _BlePluginSetNotificateRequest(ulong addr, IntPtr serviceUuid, IntPtr charaUuid, bool enable);
        public static void SetNotificationRequest(ulong addr,UuidHandler serviceUuid,UuidHandler charaUuid,bool flag)
        {
            _BlePluginSetNotificateRequest(addr, serviceUuid.ptr, charaUuid.ptr, flag);
        }


        [DllImport(pluginName)]
        private static extern int _BlePluginGetDeviceNotificateNum(ulong addr);
        public static int GetDeviceNotificateNum(ulong addr)
        {
            return _BlePluginGetDeviceNotificateNum(addr);
        }

        [DllImport(pluginName)]
        private static extern int _BlePluginCopyDeviceNotificateData(ulong addr, int idx, IntPtr ptr, int maxSize);

        public static unsafe byte[] GetDeviceNotificateData(ulong addr, int idx)
        {
            byte[] retData = null;
            var buffer = new CharastricsBuffer();
            void* ptr = &buffer.fixedBuffer[0];
            var writePtr = new IntPtr(ptr);
            int size =_BlePluginCopyDeviceNotificateData(addr, idx, writePtr, CharastricsBuffer.BufferSize);
            retData = new byte[size];
            for(int i = 0; i< size; ++i)
            {
                retData[i] = buffer.fixedBuffer[i];
            }

            return retData;
        }

        [DllImport(pluginName)]
        private static extern IntPtr _BlePluginGetDeviceNotificateServiceUuid(ulong addr, int idx);
        public static UuidHandler GetDeviceNotificateServiceUuid(ulong addr,int idx)
        {
            var ptr = _BlePluginGetDeviceNotificateServiceUuid(addr,idx);
            return new UuidHandler(ptr);
        }

        [DllImport(pluginName)]
        private static extern IntPtr _BlePluginGetDeviceNotificateCharastricsUuid(ulong addr, int idx);
        public static UuidHandler GetDeviceNotificateCharastricsUuid(ulong addr, int idx)
        {
            var ptr = _BlePluginGetDeviceNotificateCharastricsUuid(addr, idx);
            return new UuidHandler(ptr);
        }


    }
}
#endif
