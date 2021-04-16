## Sample_VisualizeNavigator

> ※ UnityEngine.Debug モジュールの可視化が利用できないため、WebGL 版はなしです。

<div align="center">
<img src="/docs/res/samples/visualize_navigator.gif">
</div>

<br>

このサンプルは CubeNavigator の HLAvoid 計算結果と定義されたすべての Wall を可視化するサンプルです。
- 黒い線は Wall の位置を表しています。マージンは表現されてません。
- 緑の線は HAvoid が計算した最適なウェイポイントとキューブとを繋げています。
- 赤い線は 衝突状況によって、除外されたウェイポイントとキューブとを繋げています。
- 青い線は その他の候補ウェイポイントとキューブとを繋げています。

