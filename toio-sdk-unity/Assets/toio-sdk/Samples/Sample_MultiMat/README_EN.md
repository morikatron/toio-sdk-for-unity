## Sample_MultiMat

> The web app sample on this page is [[here](https://morikatron.github.io/t4u/sample/multi_mat)].

<div align="center">
<img src="/docs/res/samples/multimat.gif">
</div>

<br>

This is a sample where multiple mats are lined up and treated as one large mat.

By overriding the Update method of CubeHandle and judging that the mat has been moved when the coordinates change from one end to the other, multiple mats can be handled as one large mat.

> Cube must first be placed in the upper left mat (Mat00 in Simulator).
> If you place it on a mat other than the top left, it will not work correctly.