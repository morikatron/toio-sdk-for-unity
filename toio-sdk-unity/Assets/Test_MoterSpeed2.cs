using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;

public class Test_MoterSpeed2 : MonoBehaviour
{
    CubeManager cubeManager;
    Cube cube;

    // Start is called before the first frame update
    async void Start()
    {
        cubeManager = new CubeManager();
        cube = await cubeManager.SingleConnect();
        cube.motorSpeedCallback.AddListener("test", null);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnChange(Cube c)
    {
        Debug.Log(cube.leftSpeed);
    }
}
