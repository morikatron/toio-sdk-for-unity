//
// Copyright (c) 2020-present, Sony Interactive Entertainment Inc.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

#import "UnityBLE.h"
#import "UnityFramework/UnityFramework-Swift.h"

BleModule *_bleModule = nil;
NSMutableArray *uuids = nil;  // filtered service uuid
int uniqueId = 0;

typedef void (*ErrorActionCallback) (const char *errorCode, const char *errorMessage, const char *errorDescription);
typedef void (*InitializedActionCallback) (void);
typedef void (*FinalizedActionCallback) (void);
typedef void (*DiscoveredActionCallback) (const char*, const char*, const char*, const char*);
typedef void (*ConnectedPeripheralActionCallback) (const char*);
typedef void (*DiscoveredServiceActionCallback) (const char*, const char*);
typedef void (*DiscoveredCharacteristicActionCallback) (const char*, const char*, const char*);
typedef void (*PendingDisconnectedPeripheralActionCallback) (const char*);
typedef void (*DisconnectedPeripheralActionCallback) (const char*);
typedef void (*DidReadCharacteristicActionCallback) (const char*, const char*, const char*);
typedef void (*DidWriteCharacteristicActionCallback) (const char*, const char*);
typedef void (*NotifiedCharacteristicActionCallback) (const char*, const char*, const char*);

ErrorActionCallback errorActionCallback = nil;
InitializedActionCallback initializedActionCallback = nil;
DiscoveredActionCallback discoveredActionCallback = nil;
PendingDisconnectedPeripheralActionCallback pendingDisconnectedPeripheralActionCallback = nil;
NotifiedCharacteristicActionCallback notifiedCharacteristicActionCallback = nil;

typedef void (^Rejection) (NSString *errorCode, NSString *errorMessage, NSError *error);

Rejection rejection = ^(NSString *errorCode, NSString *errorMessage, NSError *error) {
#ifdef DEBUG
    NSLog(@"rejection: errorCode = %@", errorCode);
    NSLog(@"rejection: errorMessage = %@", errorMessage);
    NSLog(@"rejection: error = %@", error);
#endif
    if (errorActionCallback != nil) {
        errorActionCallback([errorCode UTF8String], [errorMessage UTF8String], [error.description UTF8String]);
    }
};


void dlog(NSString *message) {
    if (errorActionCallback != nil) {
        errorActionCallback(nil, nil, [message UTF8String]);
    }
}

// TODO: move this to csharp layer to use transactionId
NSString* nextUniqueId() {
    uniqueId += 1;
    return [NSString stringWithFormat:@"%d", uniqueId];
}

void _uiOSCreateClient(InitializedActionCallback initializedCallback, ErrorActionCallback errorCallback) {
    initializedActionCallback = initializedCallback;
    errorActionCallback = errorCallback;

    _bleModule = [BleModule new];
    [_bleModule createClient];
    if (uuids != nil) {
        [uuids removeAllObjects];
    }
    uniqueId = 0;
}

void _uiOSDestroyClient(FinalizedActionCallback finalizedCallback) {
    if (_bleModule != nil) {
        [_bleModule destroyClient];
        _bleModule = nil;

        if (finalizedCallback != nil) {
            finalizedCallback();
        }
    }
}

void _uiOSStartDeviceScan(const char** filteredUUIDs, DiscoveredActionCallback discoveredCallback, BOOL allowDuplicates) {
    if (_bleModule != nil) {
        discoveredActionCallback = discoveredCallback;

        if (uuids == nil) {
            uuids = [[NSMutableArray alloc] init];
        }
        for (const char **p = filteredUUIDs; *p != NULL; p++) {
            [uuids addObject:[NSString stringWithCString:*p encoding:NSUTF8StringEncoding]];
        }
        NSDictionary *options = [NSDictionary dictionaryWithObject:[NSNumber numberWithBool:allowDuplicates] forKey:@"allowDuplicates"];
        [_bleModule startDeviceScan:uuids options:options];
    }
}

void _uiOSStopDeviceScan() {
    if (_bleModule != nil) {
        [_bleModule stopDeviceScan];
    }
}

void _uiOSConnectToDevice(const char* identifier, ConnectedPeripheralActionCallback connectedPeripheralCallback, DiscoveredServiceActionCallback discoveredServiceCallback, DiscoveredCharacteristicActionCallback discoveredCharacteristicCallback, PendingDisconnectedPeripheralActionCallback pendingDisconnectedPeripheralCallback) {
    if (_bleModule != nil) {
        pendingDisconnectedPeripheralActionCallback = pendingDisconnectedPeripheralCallback;

        [_bleModule connectToDevice:[NSString stringWithFormat:@"%s", identifier] options:nil resolver:^(NSDictionary *peripheral) {
            NSString *identifier = [peripheral valueForKey:@"id"];
            if (connectedPeripheralCallback != nil) {
                connectedPeripheralCallback([identifier UTF8String]);
            }

            // retrieve services and characteristics
            [_bleModule discoverAllServicesAndCharacteristicsForDevice:identifier transactionId:nextUniqueId() resolver:^(NSDictionary *peripheral) {
                [_bleModule servicesForDevice:identifier resolver:^(NSArray<NSDictionary *> *services) {
                    [services enumerateObjectsUsingBlock:^(NSDictionary * _Nonnull obj, NSUInteger idx, BOOL * _Nonnull stop) {
                        // device id and service uuid
                        if (discoveredServiceCallback != nil) {
                            discoveredServiceCallback([[obj valueForKey:@"deviceID"] UTF8String], [[obj valueForKey:@"uuid"] UTF8String]);
                        }

                        [_bleModule characteristicsForService:[obj valueForKey:@"id"] resolver:^(NSArray<NSDictionary *> *characteristics) {
                            [characteristics enumerateObjectsUsingBlock:^(NSDictionary * _Nonnull obj, NSUInteger idx, BOOL * _Nonnull stop) {
                                if (discoveredCharacteristicCallback != nil) {
                                    discoveredCharacteristicCallback([[obj valueForKey:@"deviceID"] UTF8String], [[obj valueForKey:@"serviceUUID"] UTF8String], [[obj valueForKey:@"uuid"] UTF8String]);
                                }
                            }];
                        } rejecter:rejection];
                    }];
                } rejecter:rejection];
            } rejecter:rejection];
        } rejecter:rejection];
    }
}

void _uiOSCancelDeviceConnection(const char* identifier, DisconnectedPeripheralActionCallback disconnectedPeripheralCallback) {
    if (_bleModule != nil) {
        [_bleModule cancelDeviceConnection:[NSString stringWithFormat:@"%s", identifier] resolver:^(NSDictionary *peripheral) {
            const char *identifier = [[peripheral valueForKey:@"id"] UTF8String];
            if (pendingDisconnectedPeripheralActionCallback != nil) {
                pendingDisconnectedPeripheralActionCallback(identifier);
            }
            if (disconnectedPeripheralCallback != nil) {
                disconnectedPeripheralCallback(identifier);
            }
        } rejecter:rejection];
    }
}

void _uiOSCancelDeviceConnectionAll() {
    if (_bleModule != nil) {
        [_bleModule connectedDevices:uuids resolver:^(NSArray<NSDictionary *> *peripherals) {
            [peripherals enumerateObjectsUsingBlock:^(NSDictionary * _Nonnull obj, NSUInteger idx, BOOL * _Nonnull stop) {
                const char *identifier = [[obj valueForKey:@"id"] UTF8String];
                _uiOSCancelDeviceConnection(identifier, nil);
            }];
        } rejecter:rejection];
    }
}

void _uiOSReadCharacteristicForDevice(const char* identifier, const char* serviceUUID, const char* characteristicUUID, DidReadCharacteristicActionCallback didReadCharacteristicCallback) {
    if (_bleModule != nil) {
        [_bleModule readCharacteristicForDevice:[NSString stringWithUTF8String:identifier] serviceUUID:[NSString stringWithUTF8String:serviceUUID] characteristicUUID:[NSString stringWithUTF8String:characteristicUUID] transactionId:nextUniqueId() resolver:^(NSDictionary *characteristic) {
            if (didReadCharacteristicCallback != nil) {
                didReadCharacteristicCallback([[characteristic valueForKey:@"deviceID"] UTF8String], [[characteristic valueForKey:@"uuid"] UTF8String], [[characteristic valueForKey:@"value"] UTF8String]);
            }
        } rejecter:rejection];
    }
}

void _uiOSWriteCharacteristicForDevice(const char* identifier, const char* serviceUUID, const char* characteristicUUID, const char* data, int length, BOOL withResponse, DidWriteCharacteristicActionCallback didWriteCharacteristicCallback) {
    if (_bleModule != nil) {
        [_bleModule writeCharacteristicForDevice:[NSString stringWithUTF8String:identifier] serviceUUID:[NSString stringWithUTF8String:serviceUUID] characteristicUUID:[NSString stringWithUTF8String:characteristicUUID] valueBase64:[NSString stringWithUTF8String:data] withResponse:withResponse transactionId:nextUniqueId() resolver:^(NSDictionary *characteristic) {
            if (didWriteCharacteristicCallback != nil) {
                didWriteCharacteristicCallback([[characteristic valueForKey:@"deviceID"] UTF8String], [[characteristic valueForKey:@"uuid"] UTF8String]);
            }
        } rejecter:rejection];
    }
}

void _uiOSMonitorCharacteristicForDevice(const char* identifier, const char* serviceUUID, const char* characteristicUUID, NotifiedCharacteristicActionCallback notifiedCharacteristicCallback) {
    if (_bleModule != nil) {
        notifiedCharacteristicActionCallback = notifiedCharacteristicCallback;

        [_bleModule monitorCharacteristicForDevice:[NSString stringWithUTF8String:identifier] serviceUUID:[NSString stringWithUTF8String:serviceUUID] characteristicUUID:[NSString stringWithUTF8String:characteristicUUID] transactionID:nextUniqueId() resolver:^(id value) {
            // no op
        } rejecter:rejection];
    }
}

void _uiOSUnMonitorCharacteristicForDevice(const char* identifier, const char* serviceUUID, const char* characteristicUUID) {
    // no op
}



@interface BleModule () <BleClientManagerDelegate>
@property(nonatomic) BleClientManager* manager;
@end

@implementation BleModule
{
    bool hasListeners;
}

- (void)dispatchEvent:(NSString * _Nonnull)name value:(id _Nonnull)value {
#ifdef DEBUG
    NSLog(@"[dispatchEvent]");
    NSLog(@"  name : %@", name);
    NSLog(@"  value: %@", value);
#endif

    if ([name isEqualToString:@"StateChangeEvent"] && [value isEqualToString:@"PoweredOn"]) {
        if (initializedActionCallback != nil) {
            initializedActionCallback();
        }
        return;
    }

    if ([name isEqualToString:@"ScanEvent"]) {
        if ([value isKindOfClass:[NSArray class]]) {
            NSObject *item = [value objectAtIndex:1];

            const char *identifier = [[NSString stringWithFormat:@"%@", [item valueForKey:@"id"]] UTF8String];
            const char *name = [[NSString stringWithFormat:@"%@", [item valueForKey:@"name"]] UTF8String];
            const char *rssi = [[NSString stringWithFormat:@"%@", [item valueForKey:@"rssi"]] UTF8String];
            const char *manufacturerData = [[NSString stringWithFormat:@"%@", [item valueForKey:@"manufacturerData"]] UTF8String];

            if (discoveredActionCallback != nil) {
                discoveredActionCallback(identifier, name, rssi, manufacturerData);
            }
        }
        return;
    }


    if ([name isEqualToString:@"RestoreStateEvent"]) {
        return;
    }

    if ([name isEqualToString:@"ConnectingEvent"]) {
        return;
    }

    if ([name isEqualToString:@"ConnectedEvent"]) {
        return;
    }

    if ([name isEqualToString:@"DisconnectionEvent"]) {
        NSObject *item = [value objectAtIndex:1];
        const char *identifier = [[item valueForKey:@"id"] UTF8String];

        if (pendingDisconnectedPeripheralActionCallback != nil) {
            pendingDisconnectedPeripheralActionCallback(identifier);
        }
        return;
    }

    if ([name isEqualToString:@"ReadEvent"]) {
        if (notifiedCharacteristicActionCallback != nil) {
            if ([value isKindOfClass:[NSArray class]]) {
                NSObject *characteristic = [value objectAtIndex:1];
                if ([characteristic isEqual:[NSNull null]]) {
                    return;
                }

                const char *identifier = [[characteristic valueForKey:@"deviceID"] UTF8String];
                const char *characteristicUUID = [[characteristic valueForKey:@"uuid"] UTF8String];
                const char *value = [[characteristic valueForKey:@"value"] UTF8String];

                if (notifiedCharacteristicActionCallback != nil) {
                    notifiedCharacteristicActionCallback(identifier, characteristicUUID, value);
                }
            }
        }
        return;
    }

}

- (void)startObserving {
    hasListeners = YES;
}

- (void)stopObserving {
    hasListeners = NO;
}

- (NSArray<NSString *> *)supportedEvents {
    return BleEvent.events;
}

- (NSDictionary<NSString *,id> *)constantsToExport {
    NSMutableDictionary* consts = [NSMutableDictionary new];
    for (NSString* event in BleEvent.events) {
        [consts setValue:event forKey:event];
    }
    return consts;
}

+ (BOOL)requiresMainQueueSetup {
    return YES;
}

- (void)createClient {
    _manager = [[BleClientManager alloc] initWithQueue:dispatch_get_main_queue()
                                  restoreIdentifierKey:nil];
    _manager.delegate = self;
}

- (void)destroyClient {
    [_manager invalidate];
    _manager = nil;
}

- (void)invalidate {
    [self destroyClient];
}


// Mark: Scanning ------------------------------------------------------------------------------------------------------

- (void)startDeviceScan:(NSArray*)filteredUUIDs
                options:(NSDictionary*)options {
     [_manager startDeviceScan:filteredUUIDs options:options];
 }

- (void)stopDeviceScan {
    [_manager stopDeviceScan];
}

// Mark: Device management ---------------------------------------------------------------------------------------------

- (void)connectedDevices:(NSArray<NSString*>*)serviceUUIDs
                resolver:(void (^)(NSArray<NSDictionary*>* peripherals))resolve
                rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager connectedDevices:serviceUUIDs
                       resolve:resolve
                        reject:reject];
}

// Mark: Connection management -----------------------------------------------------------------------------------------

- (void)connectToDevice:(NSString*)deviceIdentifier
                options:(NSDictionary*)options
               resolver:(void (^)(NSDictionary* peripheral))resolve
               rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager connectToDevice:deviceIdentifier
                      options:options
                      resolve:resolve
                       reject:reject];
}

- (void)cancelDeviceConnection:(NSString*)deviceIdentifier
                      resolver:(void (^)(NSDictionary* peripheral))resolve
                      rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager cancelDeviceConnection:deviceIdentifier
                             resolve:resolve
                              reject:reject];
}

// Mark: Discovery -----------------------------------------------------------------------------------------------------

- (void)discoverAllServicesAndCharacteristicsForDevice:(NSString*)deviceIdentifier
                                         transactionId:(NSString*)transactionId
                                              resolver:(void (^)(NSDictionary* peripheral))resolve
                                              rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager discoverAllServicesAndCharacteristicsForDevice:deviceIdentifier
                                               transactionId:transactionId
                                                     resolve:resolve
                                                      reject:reject];
}


// Mark: Service and characteristic getters ----------------------------------------------------------------------------

- (void)servicesForDevice:(NSString*)deviceIdentifier
                 resolver:(void (^)(NSArray<NSDictionary*>* services))resolve
                 rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager servicesForDevice:deviceIdentifier
                        resolve:resolve
                         reject:reject];
}

- (void)characteristicsForDevice:(NSString*)deviceIdentifier
                     serviceUUID:(NSString*)serviceUUID
                        resolver:(void (^)(NSArray<NSDictionary*>* characteristics))resolve
                        rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager characteristicsForDevice:deviceIdentifier
                           serviceUUID:serviceUUID
                               resolve:resolve
                                reject:reject];
}

- (void)characteristicsForService:(nonnull NSNumber*)serviceIdentifier
                         resolver:(void (^)(NSArray<NSDictionary*>* characteristics))resolve
                         rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager characteristicsForService:serviceIdentifier.doubleValue
                                resolve:resolve
                                 reject:reject];
}

// Mark: Characteristics operations ------------------------------------------------------------------------------------

- (void)readCharacteristicForDevice:(NSString*)deviceIdentifier
                        serviceUUID:(NSString*)serviceUUID
                 characteristicUUID:(NSString*)characteristicUUID
                      transactionId:(NSString*)transactionId
                           resolver:(void (^)(NSDictionary* characteristic))resolve
                           rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager readCharacteristicForDevice:deviceIdentifier
                              serviceUUID:serviceUUID
                       characteristicUUID:characteristicUUID
                            transactionId:transactionId
                                  resolve:resolve
                                   reject:reject];
}

- (void)readCharacteristicForService:(nonnull NSNumber*)serviceIdentifier
                  characteristicUUID:(NSString*)characteristicUUID
                       transactionId:(NSString*)transactionId
                            resolver:(void (^)(NSDictionary* characteristic))resolve
                            rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager readCharacteristicForService:serviceIdentifier.doubleValue
                        characteristicUUID:characteristicUUID
                             transactionId:transactionId
                                   resolve:resolve
                                    reject:reject];
}

- (void)readCharacteristic:(nonnull NSNumber*)characteristicIdentifier
             transactionId:(NSString*)transactionId
                  resolver:(void (^)(NSDictionary* characteristic))resolve
                  rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager readCharacteristic:characteristicIdentifier.doubleValue
                   transactionId:transactionId
                         resolve:resolve
                          reject:reject];
}

- (void)writeCharacteristicForDevice:(NSString*)deviceIdentifier
                         serviceUUID:(NSString*)serviceUUID
                  characteristicUUID:(NSString*)characteristicUUID
                         valueBase64:(NSString*)valueBase64
                        withResponse:(BOOL)response
                       transactionId:(NSString*)transactionId
                            resolver:(void (^)(NSDictionary* characteristic))resolve
                            rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager writeCharacteristicForDevice:deviceIdentifier
                               serviceUUID:serviceUUID
                        characteristicUUID:characteristicUUID
                               valueBase64:valueBase64
                                  response:response
                             transactionId:transactionId
                                   resolve:resolve
                                    reject:reject];
}

- (void)writeCharacteristicForService:(nonnull NSNumber*)serviceIdentifier
                   characteristicUUID:(NSString*)characteristicUUID
                          valueBase64:(NSString*)valueBase64
                         withResponse:(BOOL)response
                        transactionId:(NSString*)transactionId
                             resolver:(void (^)(NSDictionary* characteristic))resolve
                             rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager writeCharacteristicForService:serviceIdentifier.doubleValue
                         characteristicUUID:characteristicUUID
                                valueBase64:valueBase64
                                   response:response
                              transactionId:transactionId
                                    resolve:resolve
                                     reject:reject];
}

- (void)writeCharacteristic:(nonnull NSNumber*)characteristicIdentifier
                valueBase64:(NSString*)valueBase64
               withResponse:(BOOL)response
              transactionId:(NSString*)transactionId
                   resolver:(void (^)(NSDictionary* characteristic))resolve
                   rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager writeCharacteristic:characteristicIdentifier.doubleValue
                      valueBase64:valueBase64
                         response:response
                    transactionId:transactionId
                          resolve:resolve
                           reject:reject];
}

- (void)monitorCharacteristicForDevice:(NSString*)deviceIdentifier
                           serviceUUID:(NSString*)serviceUUID
                    characteristicUUID:(NSString*)characteristicUUID
                         transactionID:(NSString*)transactionId
                              resolver:(void (^)(id))resolve
                              rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager monitorCharacteristicForDevice:deviceIdentifier
                                 serviceUUID:serviceUUID
                          characteristicUUID:characteristicUUID
                               transactionId:transactionId
                                     resolve:resolve
                                      reject:reject];
}

- (void)monitorCharacteristicForService:(nonnull NSNumber*)serviceIdentifier
                     characteristicUUID:(NSString*)characteristicUUID
                          transactionID:(NSString*)transactionId
                               resolver:(void (^)(id))resolve
                               rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager monitorCharacteristicForService:serviceIdentifier.doubleValue
                           characteristicUUID:characteristicUUID
                                transactionId:transactionId
                                      resolve:resolve
                                       reject:reject];
}

- (void)monitorCharacteristic:(nonnull NSNumber*)characteristicIdentifier
                transactionID:(NSString*)transactionId
                     resolver:(void (^)(id))resolve
                     rejecter:(void (^)(NSString* code, NSString* message, NSError* error))reject {
    [_manager monitorCharacteristic:characteristicIdentifier.doubleValue
                      transactionId:transactionId
                            resolve:resolve
                             reject:reject];
}

- (void)cancelTransaction:(NSString*)transactionId {
    [_manager cancelTransaction:transactionId];
}

@end
