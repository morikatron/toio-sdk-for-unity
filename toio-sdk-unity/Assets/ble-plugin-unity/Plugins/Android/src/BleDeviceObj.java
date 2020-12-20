package com.utj.ble;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCallback;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattDescriptor;
import android.bluetooth.BluetoothGattService;
import android.bluetooth.BluetoothProfile;
import android.content.Context;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class BleDeviceObj extends BluetoothGattCallback {
    private BluetoothDevice bluetoothDevice;

    private BluetoothGatt bluetoothGatt;
    private HashMap<String,BluetoothGattCharacteristic> characteristicHashMap;
    private boolean isAvailable = false;
    private String address;

    private class ReadData{
        public String characteristic;
        public byte[] data;
        public boolean isNotification;

        public ReadData(BluetoothGattCharacteristic characteristic,boolean notify){
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
            bluetoothGatt.close();;
            bluetoothGatt = null;
            this.isAvailable = false;
        }
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
    public String getAddress(){
        return this.address;
    }
    public int getReadNum(){
        return this.pubDataBuffer.size();
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
        this.characteristicHashMap = new HashMap<String,BluetoothGattCharacteristic>();
        device.connectGatt(cxt,true,this);
    }
    public String[] getCharastrics(){
        String[] characteristics  = new String[ this.characteristicHashMap.size() ];
        int idx =0;
        for(Map.Entry entry: this.characteristicHashMap.entrySet()){
            characteristics[idx] = entry.getKey().toString();
            ++idx;
        }
        return characteristics;
    }


    public void writeData(String characteristicUuid ,byte[] data,boolean writeBack){
        BluetoothGattCharacteristic characteristic = this.characteristicHashMap.get(characteristicUuid.toLowerCase());
        if(characteristic != null) {
            characteristic.setValue(data);
            bluetoothGatt.writeCharacteristic(characteristic);
        }
    }
    public void setNotification(String characteristicUuid,boolean flag){
        BluetoothGattCharacteristic characteristic = this.characteristicHashMap.get(characteristicUuid.toLowerCase());
        if(characteristic != null) {
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

    public void readRequest(String characteristicUuid){
        BluetoothGattCharacteristic characteristic = this.characteristicHashMap.get(characteristicUuid.toLowerCase());
        if(characteristic != null) {
            this.bluetoothGatt.readCharacteristic(characteristic);
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
            this.isAvailable = false;
        }
    }


    @Override
    // New services discovered
    public void onServicesDiscovered(BluetoothGatt gatt, int status) {
        for( BluetoothGattService service : gatt.getServices() ) {
            List<BluetoothGattCharacteristic> characterlistics = service.getCharacteristics();
            for(BluetoothGattCharacteristic ch : characterlistics){
                this.characteristicHashMap.put( ch.getUuid().toString().toLowerCase(),ch );
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
