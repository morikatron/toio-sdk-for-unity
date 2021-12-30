using System;
using System.Collections.Generic;
using UnityEngine;

namespace toio
{
    public static class BLENetProtocol
    {
        // c2s protocol key
        public const int C_UDP_PORT = 50007;
        public const byte C2S_JOIN = 100;
        public const byte C2S_JOINS = 101;
        public const byte C2S_SUBSCRIBE = 102;
        public const byte C2S_READ_CALLBACK = 103;

        // s2c protocol key
        public const int S_PORT = 50006;
        public const byte S2C_WRITE = 150;
        public const byte S2C_READ = 151;

        // ID
        public const byte SERVICE_ID = 255;
        public const byte CHARACTERISTIC_CONFIG = 101;
        public const byte CHARACTERISTIC_ID = 102;
        public const byte CHARACTERISTIC_SENSOR = 103;
        public const byte CHARACTERISTIC_BUTTON = 104;
        public const byte CHARACTERISTIC_BATTERY = 105;
        public const byte CHARACTERISTIC_MOTOR = 106;
        public const byte CHARACTERISTIC_LIGHT = 107;
        public const byte CHARACTERISTIC_SOUND = 108;

        public static readonly byte[] PROTOCOL_TERMINAL_SYMBOL = System.Text.Encoding.UTF8.GetBytes("mrk");

        public static byte Characteristic2ShortID(string charaID)
        {
            if (charaID == CubeReal.CHARACTERISTIC_CONFIG) return CHARACTERISTIC_CONFIG;
            else if (charaID == CubeReal.CHARACTERISTIC_ID) return CHARACTERISTIC_ID;
            else if (charaID == CubeReal.CHARACTERISTIC_SENSOR) return CHARACTERISTIC_SENSOR;
            else if (charaID == CubeReal.CHARACTERISTIC_BUTTON) return CHARACTERISTIC_BUTTON;
            else if (charaID == CubeReal.CHARACTERISTIC_BATTERY) return CHARACTERISTIC_BATTERY;
            else if (charaID == CubeReal.CHARACTERISTIC_MOTOR) return CHARACTERISTIC_MOTOR;
            else if (charaID == CubeReal.CHARACTERISTIC_LIGHT) return CHARACTERISTIC_LIGHT;
            else if (charaID == CubeReal.CHARACTERISTIC_SOUND) return CHARACTERISTIC_SOUND;
            return default;
        }

        public static string Characteristic2UUID(byte charaID)
        {
            if (charaID == CHARACTERISTIC_CONFIG) return CubeReal.CHARACTERISTIC_CONFIG;
            else if (charaID == CHARACTERISTIC_ID) return CubeReal.CHARACTERISTIC_ID;
            else if (charaID == CHARACTERISTIC_SENSOR) return CubeReal.CHARACTERISTIC_SENSOR;
            else if (charaID == CHARACTERISTIC_BUTTON) return CubeReal.CHARACTERISTIC_BUTTON;
            else if (charaID == CHARACTERISTIC_BATTERY) return CubeReal.CHARACTERISTIC_BATTERY;
            else if (charaID == CHARACTERISTIC_MOTOR) return CubeReal.CHARACTERISTIC_MOTOR;
            else if (charaID == CHARACTERISTIC_LIGHT) return CubeReal.CHARACTERISTIC_LIGHT;
            else if (charaID == CHARACTERISTIC_SOUND) return CubeReal.CHARACTERISTIC_SOUND;
            return default;
        }

        ///////////////////////////////////////////////////
        //      Client To Server (c2s) Convert Functions
        ///////////////////////////////////////////////////

        // C2S_JOIN
        public static byte[] Encode_C2S_JOIN(int localCubeIndex, CubeReal cube)
        {
            var addrBytes = System.Text.Encoding.UTF8.GetBytes(cube.peripheral.device_address);
            var nameBytes = System.Text.Encoding.UTF8.GetBytes(cube.peripheral.device_name);

            int head = 3;
            int body = 4 + addrBytes.Length + nameBytes.Length + cube.characteristicTable.Count;

            byte[] buff = new byte[head+body];
    #if !RELEASE
            if (UInt16.MaxValue < buff.Length) { Debug.LogErrorFormat("最大バッファサイズを超えました. プロトコルを変更して下さい."); }
    #endif
            WriteBytesU16(buff, 0, Convert.ToUInt16(buff.Length));
            buff[2] = BLENetProtocol.C2S_JOIN;
            buff[3] = (byte)localCubeIndex;
            buff[4] = (byte)addrBytes.Length;
            buff[5] = (byte)nameBytes.Length;
            buff[6] = (byte)cube.characteristicTable.Count;
            Buffer.BlockCopy(addrBytes, 0, buff, 7, addrBytes.Length);
            Buffer.BlockCopy(nameBytes, 0, buff, 7+addrBytes.Length, nameBytes.Length);
            int offset = 7 + addrBytes.Length + nameBytes.Length;
            foreach(var chara in cube.characteristicTable)
            {
                buff[offset++] = BLENetProtocol.Characteristic2ShortID(chara.Key);
            }
            return buff;
        }
        public static (int localCubeIndex, string deviceAddr, string deviceName, string[] charaList) Decode_C2S_JOIN(byte[] data)
        {
            var localCubeIndex = data[0];
            var addr_len = data[1];
            var name_len = data[2];
            var chara_len = data[3];
            var deviceAddr = System.Text.Encoding.UTF8.GetString(data, 4, addr_len);
            var deviceName = System.Text.Encoding.UTF8.GetString(data, 4+addr_len, name_len);
            int offset;
            var charaList = new string[chara_len];
            for (int i = 0; i < chara_len; i++)
            {
                offset = 4 + addr_len + name_len + i;
                charaList[i] = BLENetProtocol.Characteristic2UUID(data[offset]);
            }
            return (localCubeIndex, deviceAddr, deviceName, charaList);
        }

        // C2S_JOINS
        public static byte[] Encode_C2S_JOINS(List<CubeReal> allCubes)
        {
            List<byte[]> addrList = new List<byte[]>();
            foreach(var c in allCubes) { addrList.Add(System.Text.Encoding.UTF8.GetBytes(c.peripheral.device_address)); }
            List<byte[]> nameList = new List<byte[]>();
            foreach(var c in allCubes) { nameList.Add(System.Text.Encoding.UTF8.GetBytes(c.peripheral.device_name)); }
            List<int> charaLenList = new List<int>();
            foreach(var c in allCubes) { charaLenList.Add(c.characteristicTable.Count); }

            // 合計バイト計算
            int head = 3;
            int body = 1;
            for (int i = 0; i < allCubes.Count; i++) { body += (4 + addrList[i].Length + nameList[i].Length + charaLenList[i]); }
            byte[] buff = new byte[head+body];

            // 入力
    #if !RELEASE
            if (UInt16.MaxValue < buff.Length) { Debug.LogErrorFormat("最大バッファサイズを超えました. プロトコルを変更して下さい."); }
    #endif
            WriteBytesU16(buff, 0, Convert.ToUInt16(buff.Length));
            buff[2] = BLENetProtocol.C2S_JOINS;
            buff[3] = (byte)allCubes.Count;
            int offset = 4;
            CubeReal _c;
            for (int i = 0; i < allCubes.Count; i++)
            {
                _c = allCubes[i];
                buff[offset] = (byte)i;
                buff[offset+1] = (byte)addrList[i].Length;
                buff[offset+2] = (byte)nameList[i].Length;
                buff[offset+3] = (byte)_c.characteristicTable.Count;
                Buffer.BlockCopy(addrList[i], 0, buff, offset+4, addrList[i].Length);
                Buffer.BlockCopy(nameList[i], 0, buff, offset+4+addrList[i].Length, nameList[i].Length);
                offset = offset + 4 + addrList[i].Length + nameList[i].Length;
                foreach(var chara in _c.characteristicTable)
                {
                    buff[offset++] = BLENetProtocol.Characteristic2ShortID(chara.Key);
                }
            }
            return buff;
        }
        public static (int localCubeIndex, string deviceAddr, string deviceName, string[] charaList)[] Decode_C2S_JOINS(byte[] data)
        {
            var peri_len = data[0];
            var results = new ValueTuple<int, string, string, string[]>[peri_len];
            int offset = 1;
            for (int i = 0; i < peri_len; i++)
            {
                var localCubeIndex = data[offset];
                var addr_len = data[offset + 1];
                var name_len = data[offset + 2];
                var chara_len = data[offset + 3];
                var deviceAddr = System.Text.Encoding.UTF8.GetString(data, offset + 4, addr_len);
                var deviceName = System.Text.Encoding.UTF8.GetString(data, offset + 4 + addr_len, name_len);
                string[] charaList = new string[chara_len];
                offset = offset + 4 + addr_len + name_len;
                for (int j = 0; j < chara_len; j++)
                {
                    charaList[j] = BLENetProtocol.Characteristic2UUID(data[offset++]);
                }
                results[i] = new ValueTuple<int, string, string, string[]>(localCubeIndex, deviceAddr, deviceName, charaList);
            }
            return results;
        }

        // C2S_SUBSCRIBE
        public static byte[] Encode_C2S_SUBSCRIBE(List<(int localCubeIndex, BLECharacteristicInterface chara, byte[] data)> recvDataList)
        {
            int head = 3;
            int body = 1;
            foreach(var d in recvDataList)
            {
                body += 3 + d.data.Length;
            }
            byte[] buff = new byte[head+body];
    #if !RELEASE
            if (UInt16.MaxValue < buff.Length) { Debug.LogErrorFormat("最大バッファサイズを超えました. プロトコルを変更して下さい."); }
    #endif
            // head
            BLENetProtocol.WriteBytesU16(buff, 0, Convert.ToUInt16(buff.Length));
            buff[2] = BLENetProtocol.C2S_SUBSCRIBE;
            // body
            buff[3] = (byte)recvDataList.Count;
            int offset = head+1;
            for (int i = 0; i < recvDataList.Count; i++)
            {
                buff[offset] = (byte)recvDataList[i].localCubeIndex;
                buff[offset+1] = BLENetProtocol.Characteristic2ShortID(recvDataList[i].chara.characteristicUUID);
                buff[offset+2] = (byte)recvDataList[i].data.Length;
                Buffer.BlockCopy(recvDataList[i].data, 0, buff, offset+3, recvDataList[i].data.Length);
                offset += 3 + recvDataList[i].data.Length;
            }
            return buff;
        }
        public static (int localCubeIndex, string charaID, byte[] buffer)[] Decode_C2S_SUBSCRIBE(byte[] data)
        {
            var chara_len = data[0];
            var results = new ValueTuple<int, string, byte[]>[chara_len];
            int offset = 1;
            for (int i = 0; i < chara_len; i++)
            {
                var localCubeIndex = data[offset];
                var charaID = BLENetProtocol.Characteristic2UUID(data[offset + 1]);
                var buffsize = data[offset + 2];
                var buff = new byte[buffsize];
                Buffer.BlockCopy(data, offset + 3, buff, 0, buffsize);
                offset += (3 + buffsize);
                results[i] = new ValueTuple<int, string, byte[]>(localCubeIndex, charaID, buff);
            }
            return results;
        }

        // C2S_READ_CALLBACK
        public static byte[] Encode_C2S_READ_CALLBACK(int localCubeIndex, string characteristicUUID, byte[] data)
        {
            // head
            var buff = new byte[6+data.Length];
            WriteBytesU16(buff, 0, Convert.ToUInt16(buff.Length));
            buff[2] = BLENetProtocol.C2S_READ_CALLBACK;
            // body
            buff[3] = (byte)localCubeIndex;
            buff[4] = BLENetProtocol.Characteristic2ShortID(characteristicUUID);
            buff[5] = (byte)data.Length;
            Buffer.BlockCopy(data, 0, buff, 6, data.Length);
            return buff;
        }
        public static (int localCubeIndex, string characteristicUUID, byte[] data) Decode_C2S_READ_CALLBACK(byte[] data)
        {
            var localCubeIndex = data[0];
            var charaID = BLENetProtocol.Characteristic2UUID(data[1]);
            var buffsize = data[2];
            var buff = new byte[buffsize];
            Buffer.BlockCopy(data, 3, buff, 0, buffsize);
            return (localCubeIndex, charaID, buff);
        }

        ///////////////////////////////////////////////////
        //      Server To Client (s2c)  Convert Functions
        ///////////////////////////////////////////////////

        // S2C_WRITE
        public static byte[] Encode_S2C_WRITE(int localCubeIndex, byte characteristicShortID, byte[] data, bool withResponse)
        {
            byte[] buff = new byte[4 + 4 + data.Length];
            var bb = BitConverter.GetBytes(buff.Length);
            buff[0] = bb[0];
            buff[1] = bb[1];
            buff[2] = BLENetProtocol.S2C_WRITE;
            buff[3] = 1;
            buff[4] = (byte)localCubeIndex;
            buff[5] = characteristicShortID;
            buff[6] = (byte)(withResponse ? 1 : 0);
            buff[7] = (byte)data.Length;
            Buffer.BlockCopy(data, 0, buff, 8, data.Length);
            return buff;
        }
        public static (int localCubeIndex, string charaID, byte[] buffer, bool withResponse)[] Decode_S2C_WRITE(byte[] data)
        {
            var len = data[0];
            var results = new ValueTuple<int, string, byte[], bool>[len];
            int offset = 1;
            for (int i = 0; i < len; i++)
            {
                var idx = data[offset];
                var charaID = BLENetProtocol.Characteristic2UUID(data[offset+1]);
                var withResponse = (1 == data[offset+2]) ? true : false;
                var buffsize = data[offset+3];
                var buff = new byte[buffsize];
                Buffer.BlockCopy(data, offset+4, buff, 0, buffsize);
                offset += (4 + buffsize);
                results[i] = new ValueTuple<int, string, byte[], bool>(idx, charaID, buff, withResponse);
            }
            return results;
        }

        // S2C_READ
        public static byte[] Encode_S2C_READ(int localCubeIndex, string characteristicUUID)
        {
            // head
            var buff = new byte[5];
            WriteBytesU16(buff, 0, Convert.ToUInt16(buff.Length));
            buff[2] = BLENetProtocol.S2C_READ;
            // body
            buff[3] = (byte)localCubeIndex;
            buff[4] = BLENetProtocol.Characteristic2ShortID(characteristicUUID);
            return buff;
        }
        public static (int localCubeIndex, string charaID) Decode_S2C_READ(byte[] data)
        {
            var localCubeIndex = data[0];
            var charaID = BLENetProtocol.Characteristic2UUID(data[1]);
            return (localCubeIndex, charaID);
        }

        ///////////////////////////////////////////////////
        //      Useful Funcitons
        ///////////////////////////////////////////////////

        public static void WriteBytesS16(byte[] buff, int offset, Int16 val)
        {
            var b = BitConverter.GetBytes(val);
            buff[offset+0] = b[0];
            buff[offset+1] = b[1];
        }

        public static void WriteBytesU16(byte[] buff, int offset, UInt16 val)
        {
            var b = BitConverter.GetBytes(val);
            buff[offset+0] = b[0];
            buff[offset+1] = b[1];
        }

        public static string Bytes2String(byte[] buff, int size)
        {
            if (0 == size) { return ""; }
            var sb = new System.Text.StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                sb.Append(buff[i] + ",");
            }
            sb = sb.Remove(sb.Length-1, 1);
            return sb.ToString();
        }
    }
}