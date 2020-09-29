## Sample_MultiMat

> ※ このページのウェブアプリサンプルは[【コチラ】](https://morikatron.github.io/t4u/sample/multi_mat)です。

<div align="center">
<img src="/docs/res/samples/multimat.gif">
</div>

<br>

このサンプルは、複数のマットを並べて一枚の大きなマットとして扱うサンプルです。

CubeHandle の Update メソッドをオーバーライドし、座標が端から反対側の端に変化した際にマットを移動した判定とすることで複数のマットを一つの大きなマットとして扱えるようにしています。

> キューブは最初に左上のマット (シミュレータだと Mat00) に配置しておく必要があります。
> 左上以外のマットに配置した場合には正しく動作しません。