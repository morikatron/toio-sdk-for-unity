// device
var deviceCount = 0;
var deviceTable = {};
var deviceEventTable = {};
var deviceIdTable = {};
// server
var serverCount = 0;
var serverTable = {};
var serverIdTable = {};
// service
var serviceCount = 0;
var serviceTable = {};
var serviceIdTable = {};
// characteristic
var characteristicCount = 0;
var characteristicTable = {};
var characteristicIdTable = {};
var characteristicNotificationTable = {};

// callback(int deviceID, string deviceUUID, string deviceName)
function bluetooth_requestDevice(SERVICE_UUID, callback, errorCallback)
{
    let options = {};
    options.filters = [ {services: [SERVICE_UUID]}, ];
    navigator.bluetooth.requestDevice(options)
    .then(device => {
        let id = deviceCount;
        if (device.id in deviceIdTable)
        {
            id = deviceIdTable[device.id];
        }
        else
        {
            deviceIdTable[device.id] = id;
            deviceCount++;
        }
        deviceTable[id] = device;
        callback(id, device.id, device.name);
    })
    .catch(error => {
        errorCallback(error.message);
    });
}

// callback(int deviceID, int serverID, int serviceID, string serviceUUID)
// disconnectCallback(int deviceID)
function server_connect(deviceID, SERVICE_UUID, callback, disconnectCallback)
{
    deviceTable[deviceID].gatt.connect()
    .then(server => {
        let ondisconnected = () => {
            deviceTable[deviceID].removeEventListener('gattserverdisconnected', deviceEventTable[deviceID]);
            delete deviceEventTable[deviceID];
            disconnectCallback(deviceID);
        }
        deviceEventTable[deviceID] = ondisconnected;
        deviceTable[deviceID].addEventListener('gattserverdisconnected', ondisconnected);
        let id = serverCount;
        if (server.device.id in serverIdTable)
        {
            id = serverIdTable[server.device.id];
        }
        else
        {
            serverIdTable[server.device.id] = id;
            serverCount++;
        }
        serverTable[id] = server;
        server_getPrimaryService(id, SERVICE_UUID, (serviceID, serviceUUID) => { callback(deviceID, id, serviceID, serviceUUID); });
    });
}

// callback(int serviceID, string serviceUUID)
function server_getPrimaryService(serverID, SERVICE_UUID, callback)
{
    serverTable[serverID].getPrimaryService(SERVICE_UUID)
    .then(service => {
        let id = serviceCount;
        let uuid = create_serviceUUID(serverID, service.uuid);
        if (uuid in serviceIdTable)
        {
            id = serviceIdTable[uuid];
        }
        else
        {
            serviceIdTable[uuid] = id;
            serviceCount++;
        }
        serviceTable[id] = service;
        callback(id, service.uuid);
    });
}

function server_disconnect(serverID)
{
    serverTable[serverID].disconnect();
}

function service_getCharacteristic(serviceID, characteristicUUID, callback)
{
    serviceTable[serviceID].getCharacteristic(characteristicUUID)
    .then(chara => {
        let id = characteristicCount;
        let uuid = create_characteristicUUID(serviceID, chara.uuid)
        if (uuid in characteristicIdTable)
        {
            id = characteristicIdTable[uuid];
        }
        else
        {
            characteristicIdTable[uuid] = id;
            characteristicCount++;
        }
        characteristicTable[id] = chara;
        callback(serviceID, id, characteristicUUID);
    });
}

function service_getCharacteristics(serviceID, callback)
{
    serviceTable[serviceID].getCharacteristics()
    .then(charas => {

        for(let i = 0; i < charas.length; i++)
        {
            let id = characteristicCount;
            let uuid = create_characteristicUUID(serviceID, charas[i].uuid)
            if (uuid in characteristicIdTable)
            {
                id = characteristicIdTable[uuid];
            }
            else
            {
                characteristicIdTable[uuid] = id;
                characteristicCount++;
            }

            characteristicTable[id] = charas[i];
            callback(serviceID, charas.length, i, id, charas[i].uuid);
        }
    });
}

function characteristic_writeValue(characteristicID, bytes)
{
    characteristicTable[characteristicID].writeValue(bytes);
}

function characteristic_readValue(characteristicID, callback)
{
    characteristicTable[characteristicID].readValue()
    .then(response => {
        callback(characteristicID, response.buffer);
    });
}

// callback(int characteristicID, byte[] data)
function characteristic_startNotifications(characteristicID, callback)
{
    characteristicTable[characteristicID].startNotifications().then(char => {
        console.log('notifications started');
        let onchanged = (event) => {
            callback(characteristicID, event.target.value.buffer);
        };
        characteristicNotificationTable[characteristicID] = onchanged;
        char.addEventListener('characteristicvaluechanged', onchanged);
    });
}

function characteristic_stopNotifications(characteristicID)
{
    characteristicTable[characteristicID].stopNotifications().then(char => {
        char.removeEventListener('characteristicvaluechanged', characteristicNotificationTable[characteristicID]);
        delete characteristicNotificationTable[characteristicID];
    });
}

function ab2str(buf)
{
    return String.fromCharCode.apply(null, new Uint8Array(buf));
}

function create_serviceUUID(serverID, serviceUUID)
{
    return serverID + serviceUUID;
}

function create_characteristicUUID(serviceID, characteristicUUID)
{
    return serviceID + characteristicUUID;
}