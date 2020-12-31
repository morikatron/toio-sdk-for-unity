package com.toio.ble;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCallback;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattDescriptor;
import android.bluetooth.BluetoothGattService;
import android.bluetooth.BluetoothProfile;
import android.content.Context;
import android.util.Log;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;

public class BleDeviceObj extends BluetoothGattCallback {

    private class CharastricsKey{
        private String serviceUuid;
        private String charastericUuid;

        public CharastricsKey(String service,String charasteric){
            this.serviceUuid = service.toLowerCase();
            this.charastericUuid = charasteric.toLowerCase();
        }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (o == null || getClass() != o.getClass()) return false;
            CharastricsKey that = (CharastricsKey) o;
            return Objects.equals(serviceUuid, that.serviceUuid) &&
                    Objects.equals(charastericUuid, that.charastericUuid);
        }
        public String getServiceUuid(){
            return serviceUuid;
        }
        public String getCharastericUuid(){
            return charastericUuid;
        }

        @Override
        public int hashCode() {
            return Objects.hash(serviceUuid, charastericUuid);
        }
    }
    private BluetoothDevice bluetoothDevice;
    private BluetoothGatt bluetoothGatt;
    private HashMap<CharastricsKey,BluetoothGattCharacteristic> charastricsKeyHashMap;
    private HashMap<CharastricsKey,BluetoothGattCharacteristic> pubCharastricsKeyHashMap;

    private List<CharastricsKey> charastricsKeys;
    private boolean isAvailable = false;
    private boolean disconnected = false;
    private String address;

    private class ReadData{
        public String serviceUuid;
        public String characteristic;
        public byte[] data;
        public boolean isNotification;

        public ReadData(BluetoothGattCharacteristic characteristic,boolean notify){
            this.serviceUuid = characteristic.getService().getUuid().toString();
            this.characteristic = characteristic.getUuid().toString();
            byte[] origin = characteristic.getValue();
            // 念のためコピー
            if( origin != null) {
                this.data = new byte[origin.length];
                for(int i = 0 ;i< data.length;++i){
                    this.data[i] = origin[i];
                }
            }
            this.isNotification = notify;
        }
    }

    private ArrayList<ReadData> readDataBuffer = new ArrayList<ReadData>(32);
    private ArrayList<ReadData> pubDataBuffer = new ArrayList<ReadData>(32);

    public void disconnect(){
        if(bluetoothGatt != null){
            bluetoothGatt.close();
            bluetoothGatt = null;
        }
        charastricsKeys.clear();
        this.charastricsKeyHashMap.clear();
        this.isAvailable = false;
        this.disconnected = true;
    }
    public boolean isDisconnected(){
        return this.disconnected;
    }

    public void blit(){
        pubDataBuffer.clear();
        synchronized (this){
            for(ReadData data :readDataBuffer){
                pubDataBuffer.add(data);
            }
            readDataBuffer.clear();
        }
    }
    public void blitChara(){
        pubCharastricsKeyHashMap.clear();
        this.charastricsKeys.clear();
        synchronized (this) {
            for (Map.Entry<CharastricsKey, BluetoothGattCharacteristic> data : charastricsKeyHashMap.entrySet()) {
                this.charastricsKeys.add((data.getKey()));
                this.pubCharastricsKeyHashMap.put(data.getKey(), data.getValue());
            }
        }
    }
    public int getKeysNum(){
        return this.charastricsKeys.size();
    }
    public String getCharastricUuidFromKeys(int idx){
        return this.charastricsKeys.get(idx).getCharastericUuid();
    }
    public String getServiceUuidFromKeys(int idx){
        return this.charastricsKeys.get(idx).getServiceUuid();
    }

    public String getAddress(){
        return this.address;
    }

    public int getReadNum(){
        return this.pubDataBuffer.size();
    }

    public String getServiceUuidFromReadData(int idx){
        if( idx < 0 || idx>=this.pubDataBuffer.size() ){
            return null;
        }
        ReadData data = this.pubDataBuffer.get(idx);
        return data.serviceUuid;
    }

    public String getCharacteristicFromReadData(int idx){
        if( idx < 0 || idx>=this.pubDataBuffer.size() ){
            return null;
        }
        ReadData data = this.pubDataBuffer.get(idx);
        return data.characteristic;
    }

    public boolean isNotifyReadData(int idx){
        if( idx < 0 || idx>=this.pubDataBuffer.size() ){
            return false;
        }
        ReadData data = this.pubDataBuffer.get(idx);
        return data.isNotification;
    }
    public byte[] getDataFromReadData(int idx){
        if( idx < 0 || idx>=this.pubDataBuffer.size() ){
            return null;
        }
        ReadData data = this.pubDataBuffer.get(idx);
        return data.data;
    }

    public BleDeviceObj(BluetoothDevice device, Context cxt) {
        this.bluetoothDevice = device;
        this.address = device.getAddress();
        this.charastricsKeyHashMap = new HashMap<CharastricsKey,BluetoothGattCharacteristic>();
        this.pubCharastricsKeyHashMap = new HashMap<CharastricsKey,BluetoothGattCharacteristic>();
        this.charastricsKeys = new ArrayList<CharastricsKey>(32);
        device.connectGatt(cxt,true,this);
    }


    public void writeData(String serviceUuid,
            String characteristicUuid ,byte[] data,boolean writeBack){
        if(this.bluetoothGatt == null){
            return;
        }
        CharastricsKey key = new CharastricsKey(serviceUuid,characteristicUuid);
        BluetoothGattCharacteristic characteristic = this.charastricsKeyHashMap.get(key);
        if(characteristic != null ) {
            characteristic.setValue(data);
            bluetoothGatt.writeCharacteristic(characteristic);
        }else{
            Log.e("BlePlugin","NotFound writeCharacteristic "+ serviceUuid +"::" + characteristicUuid);
        }
    }
    public void setNotification(String serviceUuid,String characteristicUuid,boolean flag){
        if(this.bluetoothGatt == null){
            return;
        }
        CharastricsKey key = new CharastricsKey(serviceUuid,characteristicUuid);
        BluetoothGattCharacteristic characteristic = this.charastricsKeyHashMap.get(key);
        if(characteristic != null ) {
            this.bluetoothGatt.setCharacteristicNotification(characteristic,flag);
            for(BluetoothGattDescriptor desc : characteristic.getDescriptors()){
                if(flag) {
                    desc.setValue(BluetoothGattDescriptor.ENABLE_NOTIFICATION_VALUE);
                }else{
                    desc.setValue(BluetoothGattDescriptor.DISABLE_NOTIFICATION_VALUE);
                }
                bluetoothGatt.writeDescriptor(desc);
            }
        }
    }

    public void readRequest(String serviceUuid,String characteristicUuid){
        if(this.bluetoothGatt == null){
            return;
        }
        CharastricsKey key = new CharastricsKey(serviceUuid,characteristicUuid);
        BluetoothGattCharacteristic characteristic = this.charastricsKeyHashMap.get(key);
        if(characteristic != null ) {
            this.bluetoothGatt.readCharacteristic(characteristic);
        }else{
            Log.e("BlePlugin","NotFound readRequest "+ serviceUuid +"::" + characteristicUuid);
        }
    }


    @Override
    public void onConnectionStateChange(BluetoothGatt gatt, int status,
                                        int newState) {
        bluetoothGatt = gatt;
        if(newState == BluetoothProfile.STATE_CONNECTED){
            gatt.discoverServices();
        }
        if(newState == BluetoothProfile.STATE_DISCONNECTED){
            this.disconnect();
        }
    }


    @Override
    // New services discovered
    public void onServicesDiscovered(BluetoothGatt gatt, int status) {
        for( BluetoothGattService service : gatt.getServices() ) {
            String serviceUuid = service.getUuid().toString();
            List<BluetoothGattCharacteristic> characteristicList = service.getCharacteristics();
            for(BluetoothGattCharacteristic ch : characteristicList){
                String charaUuid = ch.getUuid().toString();
                CharastricsKey key = new CharastricsKey(serviceUuid,charaUuid);
                synchronized (this) {
                    this.charastricsKeyHashMap.put(key, ch);
                }
            }
        }
        this.isAvailable = true;
    }

    @Override
    // Result of a characteristic read operation
    public void onCharacteristicRead(BluetoothGatt gatt,
                                     BluetoothGattCharacteristic characteristic,
                                     int status) {
        ReadData data = new ReadData(characteristic,false);
        synchronized (this) {
            this.readDataBuffer.add(data);
        }
    }

    @Override
    public void onCharacteristicChanged(BluetoothGatt gatt,
                                        BluetoothGattCharacteristic characteristic){
        ReadData data = new ReadData(characteristic,true);
        synchronized (this) {
            this.readDataBuffer.add(data);
        }
    }

    @Override
    public void onCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, int status) {
        super.onCharacteristicWrite(gatt, characteristic, status);
    }
}
