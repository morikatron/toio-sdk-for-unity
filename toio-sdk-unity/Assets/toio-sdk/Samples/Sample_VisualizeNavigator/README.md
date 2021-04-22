## Sample_VisualizeNavigator

> ※ UnityEngine.Debug モジュールの可視化が利用できないため、WebGL 版はなしです。

<div align="center">
<img src="/docs/res/samples/visualize_navigator.gif">
</div>

<br>

このサンプルは CubeNavigator の HLAvoid 計算結果と定義されたすべての Wall を可視化するサンプルです。

キューブの目標座標（ピンクの棒）は、`CTRL`を押しながら右クリックで設定できます。

可視化をオンにするには、Unity Editor の「シーン」又は「ゲーム」ウィンドウの右上にある「ギスモ」をオンにしてください。

- 黒い線は Wall の位置を表しています。マージンは表現されていません。
- 緑の線は HLAvoid が計算した最適なウェイポイントとキューブとを繋げています。
- 赤い線は 衝突状況によって、候補から除外されたウェイポイントとキューブとを繋げています。
- 青い線は その他の候補ウェイポイントとキューブとを繋げています。
