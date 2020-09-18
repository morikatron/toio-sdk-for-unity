var WebBLEPlugin =
{
    $scriptObjectName: "",
    $unityMethods: {},

    call_setscriptObjectName: function(name)
    {
        scriptObjectName = Pointer_stringify(name);
        return 0;
    },

    InitMethods: function(return_RequestDevice, return_Connect, callback_Disconnected, return_getCharacteristic, return_getCharacteristics, return_ReadValue, callback_StartNotifications)
    {
        unityMethods.return_RequestDevice = return_RequestDevice;
        unityMethods.return_Connect = return_Connect;
        unityMethods.callback_Disconnected = callback_Disconnected;
        unityMethods.return_getCharacteristic = return_getCharacteristic;
        unityMethods.return_getCharacteristics = return_getCharacteristics;
        unityMethods.return_ReadValue = return_ReadValue;
        unityMethods.callback_StartNotifications = callback_StartNotifications;
    },

    InitErrorMethods: function(error_RequestDevice)
    {
        unityMethods.error_RequestDevice = error_RequestDevice;
    },

    call_RequestDevice: function(SERVICE_UUID)
    {
        var str = Pointer_stringify(SERVICE_UUID);
        bluetooth_requestDevice(str, return_call_RequestDevice, error_call_RequestDevice);
        return 0;
    },

    $return_call_RequestDevice: function(id, uuid, name)
    {
        // uuid
        var size = lengthBytesUTF8(uuid) + 1; // null文字終端となるため+1
        var ptr_uuid = _malloc(size);
        stringToUTF8(uuid, ptr_uuid, size); // この関数でHEAPxxxに書き込まれる
        // name
        size = lengthBytesUTF8(name) + 1; // null文字終端となるため+1
        var ptr_name = _malloc(size);
        stringToUTF8(name, ptr_name, size); // この関数でHEAPxxxに書き込まれる
        Runtime.dynCall('viii', unityMethods.return_RequestDevice, [id, ptr_uuid, ptr_name]);
    },

    $error_call_RequestDevice: function(msg)
    {
        var size = lengthBytesUTF8(msg) + 1; // null文字終端となるため+1
        var ptr = _malloc(size);
        stringToUTF8(msg, ptr, size); // この関数でHEAPxxxに書き込まれる
        Runtime.dynCall('vi', unityMethods.error_RequestDevice, [ptr]);
    },

    call_Connect: function(deviceID, SERVICE_UUID)
    {
        var str = Pointer_stringify(SERVICE_UUID);
        server_connect(deviceID, str, return_call_Connect, callback_Disconnected);
        return 0;
    },

    $return_call_Connect: function(deviceID, serverID, serviceID, serviceUUID)
    {
        var size = lengthBytesUTF8(serviceUUID) + 1; // null文字終端となるため+1
        var ptr = _malloc(size);
        stringToUTF8(serviceUUID, ptr, size); // この関数でHEAPxxxに書き込まれる
        Runtime.dynCall('viiii', unityMethods.return_Connect, [deviceID, serverID, serviceID, ptr]);
    },

    $callback_Disconnected: function(deviceID)
    {
        Runtime.dynCall('vi', unityMethods.callback_Disconnected, [deviceID]);
    },

    call_Disconnect: function(serverID)
    {
        server_disconnect(serverID);
        return 0;
    },

    call_getCharacteristic: function(serviceID, characteristicUUID)
    {
        var str = Pointer_stringify(characteristicUUID);
        service_getCharacteristic(serviceID, str, return_call_getCharacteristic);
        return 0;
    },

    $return_call_getCharacteristic: function(serviceID, id, characteristicUUID)
    {
        var size = lengthBytesUTF8(characteristicUUID) + 1; // null文字終端となるため+1
        var ptr = _malloc(size);
        stringToUTF8(characteristicUUID, ptr, size); // この関数でHEAPxxxに書き込まれる
        Runtime.dynCall('viii', unityMethods.return_getCharacteristic, [serviceID, id, ptr]);
    },

    call_getCharacteristics: function(serviceID)
    {
        service_getCharacteristics(serviceID, return_call_getCharacteristics);
        return 0;
    },

    $return_call_getCharacteristics: function(serviceID, len, idx, id, uuid)
    {
        var size = lengthBytesUTF8(uuid) + 1; // null文字終端となるため+1
        var ptr = _malloc(size);
        stringToUTF8(uuid, ptr, size); // この関数でHEAPxxxに書き込まれる
        Runtime.dynCall('viiiii', unityMethods.return_getCharacteristics, [serviceID, len, idx, id, ptr]);
    },

    // ポインターから各種数値型配列(サブ配列)を返す関数を用意
    // https://qiita.com/gtk2k/items/1c7aa7a202d5f96ebdbf
    $arrFromPtr: function(ptr, size, heap)
    {
        // 配列生成(値コピー)せず、HEAPのサブ配列を返す
        var startIndex = ptr / heap.BYTES_PER_ELEMENT; // 型のバイト数で割って開始インデックスを算出
        return heap.subarray(startIndex, startIndex + size);
    },

    call_WriteValue: function(characteristicID, pByteArr, pByteArrLen)
    {
        var byteArr = arrFromPtr(pByteArr, pByteArrLen, HEAPU8);
        characteristic_writeValue(characteristicID, byteArr);
        return 0;
    },

    call_ReadValue: function(characteristicID)
    {
        characteristic_readValue(characteristicID, return_call_ReadValue);
        return 0;
    },

    $return_call_ReadValue: function(characteristicID, resBuffer)
    {
        var ptr = _malloc(resBuffer.byteLength);
        HEAP8.set(new Uint8Array(resBuffer), ptr);
        Runtime.dynCall('viii', unityMethods.return_ReadValue, [characteristicID, ptr, resBuffer.byteLength]);
        _free(ptr);
    },

    call_startNotifications: function(characteristicID)
    {
        characteristic_startNotifications(characteristicID, callback_call_startNotifications);
        return 0;
    },

    $callback_call_startNotifications: function(characteristicID, resBuffer)
    {
        var ptr = _malloc(resBuffer.byteLength);
        HEAP8.set(new Uint8Array(resBuffer), ptr);
        Runtime.dynCall('viii', unityMethods.callback_StartNotifications, [characteristicID, ptr, resBuffer.byteLength]);
        _free(ptr);
    },

    call_stopNotifications: function(characteristicID)
    {
        characteristic_stopNotifications(characteristicID);
        return 0;
    }
}

autoAddDeps(WebBLEPlugin, '$unityMethods');
autoAddDeps(WebBLEPlugin, '$scriptObjectName');
autoAddDeps(WebBLEPlugin, '$return_call_RequestDevice');
autoAddDeps(WebBLEPlugin, '$error_call_RequestDevice');
autoAddDeps(WebBLEPlugin, '$return_call_Connect');
autoAddDeps(WebBLEPlugin, '$callback_Disconnected');
autoAddDeps(WebBLEPlugin, '$return_call_getCharacteristic');
autoAddDeps(WebBLEPlugin, '$return_call_getCharacteristics');
autoAddDeps(WebBLEPlugin, '$arrFromPtr');
autoAddDeps(WebBLEPlugin, '$return_call_ReadValue');
autoAddDeps(WebBLEPlugin, '$callback_call_startNotifications');
mergeInto(LibraryManager.library, WebBLEPlugin);