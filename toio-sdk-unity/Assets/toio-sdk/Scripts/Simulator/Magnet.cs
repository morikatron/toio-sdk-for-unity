using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace toio.Simulator
{
    [DisallowMultipleComponent]
    public class Magnet : MonoBehaviour
    {
        // permeability in vacuum
        public const float mu0 = 1.2566e-6f;

        [SerializeField, TooltipAttribute("Self's magnetic charge (1 Mx = 10e-8 Wb)")]
        public float maxwell = 1;

        [SerializeField, TooltipAttribute("relative permeability")]
        public float relativePermeability = 1;

        public float mu => mu0 * relativePermeability;


        void Start()
        {
        }

        void Update()
        {
        }

        public Vector3 GetSelfH(Vector3 pos)
        {
            var src = transform.position;
            var dpos = pos - src;
            var r = dpos.magnitude;
            return maxwell / (4 * Mathf.PI * mu * r * r * r) * dpos;
        }

        public Vector3 SumUpH(Vector3 pos)
        {
            var magnets = GetComponentsInChildren<Magnet>();
            Vector3 h = Vector3.zero;
            foreach (var magnet in magnets)
            {
                h += magnet.GetSelfH(pos);
            }
            return h;
        }
    }
}
