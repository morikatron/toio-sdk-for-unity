using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;
using Cysharp.Threading.Tasks;
using System.Linq;

public class Sample_DigitalTwin : MonoBehaviour
{
    public DigitalTwinConnector connector;

    void Update()
    {
        foreach (var cube in this.connector.cubes)
        {
            // Write your codes here
        }
    }

}