using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;
using toio.Simulator;

public class GhostBinder
{
    public enum Method {
        Direct,
        AddForce,
    }

    public CubeSimulator ghost;
    public Mat mat;
    public Cube cube;
    public Method mappingMethod = Method.AddForce;


    /// <summary>
    /// Call this in FixedUpdate().
    /// </summary>
    public void FixedUpdate()
    {
        if (!this.Check()) return;
        var rb = this.ghost.GetComponent<Rigidbody>();
        var pos = this.mat.MatCoord2UnityCoord(this.cube.x, this.cube.y);
        var deg = this.mat.MatDeg2UnityDeg(this.cube.angle);

        if (this.mappingMethod == Method.Direct) {
            rb.MovePosition(pos);
            rb.MoveRotation(Quaternion.Euler(0, deg, 0));
        }
        else if (this.mappingMethod == Method.AddForce) {
            var dpos = pos - this.ghost.transform.position;
            var ddeg = (deg - this.ghost.transform.eulerAngles.y + 540) % 360 - 180;

            rb.AddForce(dpos / Time.fixedDeltaTime * 4e-3f, ForceMode.Impulse);
            rb.AddTorque(0, ddeg / Time.fixedDeltaTime * 2e-8f, 0, ForceMode.Impulse);
        }
    }

    bool Check() {
        if (this.cube == null || !this.cube.isConnected) return false;
        if (this.mat == null || this.ghost == null) return false;

        if (!this.mat.IsUnityCoordInside(this.cube.x, this.cube.y)) return false;
        return true;
    }
}
