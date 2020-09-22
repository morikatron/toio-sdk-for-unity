## Sample_Bluetooth

<div align="center">
<img height=280 src="/docs/res/samples/real.gif" title="キューブの動作" alt="キューブの動作">
</div>

<br>

このサンプルは、低レベルモジュールであるBLEインタフェースを直接利用してキューブと通信するサンプルです。<br>
スマートフォンビルドで実行すると、クルクルとキューブが回転します。

> このサンプルは Bluetoothモジュールのみ使用しているため、シミュレータでは動作しません。

* 全ての通信モジュールはBLEインタフェースを継承して実装されており、新たに継承クラスを独自開発する事でBLE以外の通信モジュールに差し替える事も可能です
* シングルトンクラスであるBLEServiceの`BLEService.Instance.SetImplement()`関数を呼び出す事で、通信モジュールの内部実装を注入出来ます
* サンプルでは`BLEService.Instance.SetImplement(new BLEMobileService())`によってモバイルBLEの内部実装を注入しています
* このサンプルはBLEインタフェースのみに依存しており、独自開発した通信モジュールの動作確認に使う事も出来ます
* 詳しくは[BLE機能説明](/docs/sys_ble.md)をご参照下さい。