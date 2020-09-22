# Sample_WebGL

このサンプルは、WebGLビルド向けのサンプル集です。<br>
プラットフォームをWebGLに指定してビルドを行うと、自動的にBLEモジュールがWebGL向けモジュールに変更され、ブラウザページ上のアプリケーションとキューブで通信が出来ます。

* [Sample_Bluetooth](../Sample_Bluetooth/README.md)で使用している低レベルの[BLEインタフェース](/docs/sys_ble.md)を利用して、WebGL向けのBLEモジュールを実装しています
* BLEモジュール内部では、[web-bluetooth](https://github.com/WebBluetoothCG/web-bluetooth)を利用してキューブと通信を行います
* <b>web-bluetoothでは、ユーザーの操作イベントからデバイス1台ずつとの接続をする必要があります。</b>そのため、サンプルでは接続ボタンを用意して1台ずつキューブと接続します

<br>

### サンプル集

- [Sample_UI](./Sample_UI/)

このサンプルは、ユーザーがUIを操作して直接キューブを動かすサンプルです。

- [Sample_Circling](./Sample_Circling/)

このサンプルは CubeNavigator のモードの違いによって、多数台のキューブの挙動がどのように変わるか確認するサンプルです。

- [Sample_WebBluetooth_LowLevel](./Sample_WebBluetooth_LowLevel/)

このサンプルは、低レベルモジュールであるBLEインタフェースを直接利用してキューブと通信するサンプルです。

- [Sample_WebPlugin_VeryLowLevel](./Sample_WebPlugin_VeryLowLevel/)

このサンプルは、最低レベルモジュールであるWebBLEプラグインを直接利用してキューブと通信するサンプルです。
