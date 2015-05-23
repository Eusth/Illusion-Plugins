using HaremOVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StudioVR
{
    /// <summary>
    /// Represents a target that can be looked at.
    /// </summary>
    class EyeTarget : MonoBehaviour
    {
        /// <summary>
        /// Offset in meters from the camera.
        /// </summary>
        public float offset = 0.5f;

        /// <summary>
        /// Origin of the gaze.
        /// </summary>
        public Transform rootNode;

        void Update()
        {
            if (rootNode != null)
            {
                var camera = OVRCamera.Instance.CenterEye.transform;
                var dir = (camera.position - rootNode.position).normalized;

                transform.position = camera.position + dir * offset;
            }


        }
    }
}
