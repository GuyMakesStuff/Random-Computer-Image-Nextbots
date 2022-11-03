using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Experiments.Global.Visuals
{
    public class Rotator : MonoBehaviour
    {
        public Vector3 Eulers;
        public float Speed;

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(Eulers, Speed * 360 * Time.deltaTime);
        }
    }
}