package com.toio.ble;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.le.BluetoothLeScanner;
import android.bluetooth.le.ScanFilter;
import android.bluetooth.le.ScanResult;
import android.bluetooth.le.ScanSettings;
import android.content.Context;
import android.os.ParcelUuid;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.UUID;

import android.bluetooth.le.ScanCallback;
import android.util.Log;

public class BleScannerObj extends ScanCallback{
    private BluetoothLeScanner bleScanner;
    private Context context;

    private HashMap<String,BluetoothDevice> bleDevices = new HashMap<String,BluetoothDevice>();
    private HashMap<String,Integer> bleRssi = new HashMap<String,Integer>();

    private HashMap<String,BluetoothDevice> pubBleDevices = new HashMap<String,BluetoothDevice>();
    private HashMap<String,Integer> pubBleRssi = new HashMap<String,Integer>();
    private HashMap<String,BluetoothDevice> foundDevicesInScan = new HashMap<String,BluetoothDevice>();

    private List<String> pubAddresses = new ArrayList<String>();
    private List<ScanFilter> scanFilters = new ArrayList<>(8);
    private boolean isScanning = false;



    public BleScannerObj(BluetoothLeScanner scanner,Context cxt){
        this.context = cxt;
        this.bleScanner = scanner;
    }

    public void blit(){
        this.pubAddresses.clear();
        this.pubBleDevices.clear();
        synchronized (this) {
            for (Map.Entry<String, BluetoothDevice> entry : bleDevices.entrySet()) {
                String addr = entry.getKey();
                this.pubAddresses.add(addr);
                this.pubBleDevices.put(addr, entry.getValue());
                this.pubBleRssi.put(addr, bleRssi.get(addr));
                this.foundDevicesInScan.put(addr,entry.getValue());
            }
            this.bleDevices.clear();
        }
    }

    public int getDeviceNum(){
        return pubAddresses.size();
    }
    public String getDeviceAddr(int idx){
        if(idx < 0 || idx >= pubAddresses.size() ){
            return null;
        }
        return pubAddresses.get(idx);
    }
    public String getDeviceNameByAddr(String addr){
        BluetoothDevice d = pubBleDevices.get(addr);
        if(d == null){return null;}
        return d.getName();
    }

    public BluetoothDevice getDeviceByAddr(String addr){
        return pubBleDevices.get(addr);
    }

    public BluetoothDevice getFoundDeviceByAddr(String addr){
        return this.foundDevicesInScan.get(addr);
    }

    public int getRssiByAddr(String addr){
        return bleRssi.get(addr);
    }

    public void startScan(String uuid){
        this.clearScanFilter();
        this.addScanFilter(uuid);
        this.startScan();
    }
    public void startScan(){
        if(this.isScanning) {
            return;
        }
        this.foundDevicesInScan.clear();

        ScanSettings.Builder scanBuilder = new ScanSettings.Builder();
        ScanSettings settings = scanBuilder.build();
        bleScanner.startScan(this.scanFilters,settings, this );
        this.isScanning = true;
    }
    public void addScanFilter(String uuid){
        UUID uuidObj = UUID.fromString(uuid);
        ScanFilter scanFilter =
                new ScanFilter.Builder().setServiceUuid( new ParcelUuid(uuidObj)).build();
        this.scanFilters.add(scanFilter);
    }
    public void clearScanFilter(){
        scanFilters.clear();
    }

    public void stopScan(){
        bleScanner.stopScan(this);
        synchronized (this) {
            this.bleRssi.clear();
            this.bleDevices.clear();
        }
        this.isScanning = false;
    }

    public void onScanResult(int callbackType, ScanResult result) {
        super.onScanResult(callbackType,result);
        BluetoothDevice bluetoothDevice = result.getDevice();
        // result.getRssi();

        String addr =  bluetoothDevice.getAddress();
        synchronized (this) {
            this.bleDevices.put(addr, bluetoothDevice);
            this.bleRssi.put(addr, result.getRssi());
        }
    }

    @Override
    public void onBatchScanResults(List<ScanResult> results) {
        super.onBatchScanResults(results);
        if(results == null){
            return;
        }
        synchronized (this) {
            for (ScanResult result : results) {
                BluetoothDevice bluetoothDevice = result.getDevice();
                String addr = bluetoothDevice.getAddress();
                this.bleDevices.put(addr, bluetoothDevice);
                this.bleRssi.put(addr, result.getRssi());
            }
        }
    }

    @Override
    public void onScanFailed(int errorCode) {
        super.onScanFailed(errorCode);
    }

}
