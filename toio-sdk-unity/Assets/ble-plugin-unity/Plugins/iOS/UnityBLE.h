//
// Copyright (c) 2020-present, Sony Interactive Entertainment Inc.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

#import <Foundation/Foundation.h>

@interface BleModule : NSObject

- (void)createClient;

- (void)destroyClient;

- (void)startDeviceScan:(NSArray *)filteredUUIDs options:(NSDictionary *)options;

- (void)stopDeviceScan;

- (void)connectedDevices:(NSArray<NSString *> *)serviceUUIDs
                resolver:(void (^)(NSArray<NSDictionary *> *peripherals))resolve
                rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)connectToDevice:(NSString *)deviceIdentifier
                options:(NSDictionary *)options
               resolver:(void (^)(NSDictionary *peripheral))resolve
               rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)cancelDeviceConnection:(NSString *)deviceIdentifier
                      resolver:(void (^)(NSDictionary *peripheral))resolve
                      rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)discoverAllServicesAndCharacteristicsForDevice:(NSString *)deviceIdentifier
                                         transactionId:(NSString *)transactionId
                                              resolver:(void (^)(NSDictionary *peripheral))resolve
                                              rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)servicesForDevice:(NSString *)deviceIdentifier
                 resolver:(void (^)(NSArray<NSDictionary *> *services))resolve
                 rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)characteristicsForDevice:(NSString *)deviceIdentifier
                     serviceUUID:(NSString *)serviceUUID
                        resolver:(void (^)(NSArray<NSDictionary *> *characteristics))resolve
                        rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)characteristicsForService:(nonnull NSNumber *)serviceIdentifier
                         resolver:(void (^)(NSArray<NSDictionary *> *characteristics))resolve
                         rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)readCharacteristicForDevice:(NSString *)deviceIdentifier
                        serviceUUID:(NSString *)serviceUUID
                 characteristicUUID:(NSString *)characteristicUUID
                      transactionId:(NSString *)transactionId
                           resolver:(void (^)(NSDictionary *characteristic))resolve
                           rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)readCharacteristicForService:(nonnull NSNumber *)serviceIdentifier
                  characteristicUUID:(NSString *)characteristicUUID
                       transactionId:(NSString *)transactionId
                            resolver:(void (^)(NSDictionary *characteristic))resolve
                            rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)readCharacteristic:(nonnull NSNumber *)characteristicIdentifier
             transactionId:(NSString *)transactionId
                  resolver:(void (^)(NSDictionary *characteristic))resolve
                  rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)writeCharacteristicForDevice:(NSString *)deviceIdentifier
                         serviceUUID:(NSString *)serviceUUID
                  characteristicUUID:(NSString *)characteristicUUID
                         valueBase64:(NSString *)valueBase64
                        withResponse:(BOOL)response
                       transactionId:(NSString *)transactionId
                            resolver:(void (^)(NSDictionary *characteristic))resolve
                            rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)writeCharacteristicForService:(nonnull NSNumber *)serviceIdentifier
                   characteristicUUID:(NSString *)characteristicUUID
                          valueBase64:(NSString *)valueBase64
                         withResponse:(BOOL)response
                        transactionId:(NSString *)transactionId
                             resolver:(void (^)(NSDictionary *characteristic))resolve
                             rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)writeCharacteristic:(nonnull NSNumber *)characteristicIdentifier
                valueBase64:(NSString *)valueBase64
               withResponse:(BOOL)response
              transactionId:(NSString *)transactionId
                   resolver:(void (^)(NSDictionary *characteristic))resolve
                   rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)monitorCharacteristicForDevice:(NSString *)deviceIdentifier
                           serviceUUID:(NSString *)serviceUUID
                    characteristicUUID:(NSString *)characteristicUUID
                         transactionID:(NSString *)transactionId
                              resolver:(void (^)(id))resolve
                              rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)monitorCharacteristicForService:(nonnull NSNumber *)serviceIdentifier
                     characteristicUUID:(NSString *)characteristicUUID
                          transactionID:(NSString *)transactionId
                               resolver:(void (^)(id))resolve
                               rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)monitorCharacteristic:(nonnull NSNumber *)characteristicIdentifier
                transactionID:(NSString *)transactionId
                     resolver:(void (^)(id))resolve
                     rejecter:(void (^)(NSString *code, NSString *message, NSError *error))reject;

- (void)cancelTransaction:(NSString *)transactionId;

@end
