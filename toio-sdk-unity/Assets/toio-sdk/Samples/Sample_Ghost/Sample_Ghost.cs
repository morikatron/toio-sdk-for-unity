using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;
using toio.Simulator;

public class Sample_Ghost : MonoBehaviour
{

    public CubeSimulator simulator;
    public Mat mat;
    [Tooltip("Mapping method from real to simulator. Direct: directly set position and rotation. AddForce: apply force and torque to Rigidbody.")]
    public GhostBinder.Method mappingMethod = GhostBinder.Method.AddForce;

    private Cube cube;
    private GhostBinder ghostBinder = new GhostBinder();

    async void Start()
    {
        var peri = await new CubeScanner(ConnectType.Real).NearestScan();
        this.cube = await new CubeConnecter(ConnectType.Real).Connect(peri);

        // Disable simulator interaction
        this.simulator.GetComponent<CubeInteraction>().enabled = false;

        // Bind cube and simulator
        ghostBinder.cube = this.cube;
        ghostBinder.mat = this.mat;
        ghostBinder.ghost = this.simulator;
    }

    void Update()
    {
        // Update fields
        this.ghostBinder.ghost = this.simulator;
        this.ghostBinder.mat = this.mat;
        this.ghostBinder.mappingMethod = this.mappingMethod;
    }

    void FixedUpdate()
    {
        this.ghostBinder.FixedUpdate();
    }
}
