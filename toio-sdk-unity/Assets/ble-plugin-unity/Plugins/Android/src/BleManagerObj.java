package com.utj.ble;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothManager;
import android.bluetooth.le.BluetoothLeScanner;
import android.content.Context;

import java.util.ArrayList;
import java.util.HashMap;

public class BleManagerObj {
    private BluetoothAdapter bluetoothAdapter;

    private Context context;

    private BleScannerObj scanObj;
    private HashMap<String, BleDeviceObj> deviceObjHashMap;
    private ArrayList<BleDeviceObj> bluetoothDeviceObjs;

    private static BleManagerObj instance;

    private BleManagerObj() {
    }
    public static BleManagerObj getInstance(){
        if(instance == null){
            instance = new BleManagerObj();
        }
        return instance;
    }

    public void initialize(Context ctx) {
        final BluetoothManager bluetoothManager = (BluetoothManager) ctx.getSystemService(Context.BLUETOOTH_SERVICE);
        bluetoothAdapter = bluetoothManager.getAdapter();
        BluetoothLeScanner scanner = bluetoothAdapter.getBluetoothLeScanner();
        scanObj = new BleScannerObj(scanner,ctx);
        this.deviceObjHashMap = new HashMap<String, BleDeviceObj>(32);
        this.bluetoothDeviceObjs = new ArrayList<BleDeviceObj>(32);
    }

    public BleScannerObj getScanner(){
        return scanObj;
    }

    public BleDeviceObj connect(String addr){
        BluetoothDevice device = scanObj.getFoundDeviceByAddr(addr);
        if(device == null){
            return null;
        }
        BleDeviceObj deviceObj = new BleDeviceObj(device,this.context);
        this.deviceObjHashMap.put(addr,deviceObj);
        this.bluetoothDeviceObjs.add(deviceObj);
        return  deviceObj;
    }

    public BleDeviceObj getDeviceByAddr(String addr){
        return this.deviceObjHashMap.get(addr);
    }

    public int getConnectedDeviceNum(){
        return this.bluetoothDeviceObjs.size();
    }
    public String getConnectedDeviceAddr(int idx){
        return this.bluetoothDeviceObjs.get(idx).getAddress();
    }
    public BleDeviceObj getConnectedDevice(int idx){
        return this.bluetoothDeviceObjs.get(idx);
    }

    public void disconnect(String addr){
        BleDeviceObj obj = this.deviceObjHashMap.get(addr);
        this.bluetoothDeviceObjs.remove(obj );
        this.deviceObjHashMap.remove(addr);
    }

}
