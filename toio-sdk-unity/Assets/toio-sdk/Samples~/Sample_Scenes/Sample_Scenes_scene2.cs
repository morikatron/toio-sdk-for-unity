using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using toio;

public class Sample_Scenes_scene2 : MonoBehaviour
{
    private CubeManager cm;

    void Start()
    {
        if (Sample_Scenes_Preload.Keep_script)
        {
            gameObject.SetActive(false);
        }

        cm = Sample_Scenes_Preload.cm;
        // await cm.MultiConnect(1);

        Invoke("ChangeScene", 2f);
    }

    void Update()
    {
        if (cm!=null && cm.synced)
        for (int i=0; i<cm.navigators.Count; i++)
        {
            cm.cubes[i].TurnLedOn(0, 0, 255, 1000);
        }
    }

    void ChangeScene()
    {
        SceneManager.LoadScene("scene1");
    }
}
