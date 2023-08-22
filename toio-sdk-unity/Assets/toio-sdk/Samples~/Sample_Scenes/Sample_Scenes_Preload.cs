using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using toio;

public class Sample_Scenes_Preload : MonoBehaviour
{

    public bool keep_script = false;
    public static bool Keep_script {get; private set;}

    public static CubeManager cm;

    void Awake()
    {
    }

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // Retrieve and Save Settings
        Keep_script = keep_script;

        cm = new CubeManager();
        // await cm.MultiConnect(1);

        // Keep Cubes across scenes, On Simulator
        if (CubeScanner.actualTypeOfAuto == ConnectType.Simulator)
        {
            try
            {
                foreach (var c in GameObject.FindGameObjectsWithTag("t4u_Cube"))
                    DontDestroyOnLoad(c);
            }
            catch (UnityException){}
        }

        if (Keep_script)
        {
            DontDestroyOnLoad(this);
        }

        // Change Scene
        Invoke("ChangeScene", 0f);
    }

    void Update()
    {
        if (cm.synced)
        for (int i=0; i<cm.navigators.Count; i++)
        {
            cm.cubes[i].TurnLedOn(255, 0, 0, 1000);
        }
    }

    void ChangeScene()
    {
        SceneManager.LoadScene("scene1");
    }
}
