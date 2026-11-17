using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AD3D_EnergySolution.Runtime
{
    public class PowerWindTurbineController : GenericPowerController
    {
        public float rotationSpeed = 75f;
        public Transform BladeHead;

        public override void Start()
        {
            base.Start();

            BladeHead = gameObject.transform.Find("model/BladeHead");
        }

        void Update()
        {
            if (!IsEnabled || !HasLubricant)
                return;

            BladeHead.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
}
