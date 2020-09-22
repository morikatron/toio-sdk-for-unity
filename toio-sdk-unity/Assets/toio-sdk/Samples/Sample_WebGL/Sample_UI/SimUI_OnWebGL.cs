using UnityEngine;

public class SimUI_OnWebGL : MonoBehaviour
{
    #if UNITY_EDITOR
    private int updateCnt = 0;

    void Update()
    {
        if (updateCnt < 3) updateCnt++;
        if (updateCnt == 2){
            // キャンバスを下に移動
            var canvasObj = GameObject.Find("Canvas");
            var simCanvasObj = GameObject.Find("SimCanvas");
            canvasObj.transform.SetParent(simCanvasObj.transform);
            canvasObj.transform.localScale *= 0.7f;
            canvasObj.transform.position = new Vector3(canvasObj.transform.position.x, 780/2 * canvasObj.transform.localScale.y * 0.8f, canvasObj.transform.position.z);
        }
    }
    #endif
}
