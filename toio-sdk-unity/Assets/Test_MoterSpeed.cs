using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;

public class Test_MoterSpeed : MonoBehaviour
{
    CubeManager cubeManager;
    Cube cube;

    // Start is called before the first frame update
    async void Start()
    {
        cubeManager = new CubeManager();
        cube = await cubeManager.SingleConnect();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(cube?.leftSpeed);
    }
}
