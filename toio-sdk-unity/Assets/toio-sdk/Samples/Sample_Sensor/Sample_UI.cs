using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample_UI : MonoBehaviour
{
    #if UNITY_EDITOR
    private int updateCnt = 0;

    void Update()
    {
        if (updateCnt < 3) updateCnt++;
        if (updateCnt == 2){
            // ステージを右に移動
            var localPos = Camera.main.transform.localPosition;
            localPos.x = -0.15f;
            Camera.main.transform.localPosition = localPos;
            // キャンバスを左に移動
            var canvasObj = GameObject.Find("Canvas");
            var simCanvasObj = GameObject.Find("SimCanvas");

            canvasObj.transform.SetParent(simCanvasObj.transform);
            canvasObj.transform.position = new Vector3(860/2 * canvasObj.transform.localScale.x * 0.8f,
                canvasObj.transform.position.y, canvasObj.transform.position.z);

        }
    }
    #endif
}
