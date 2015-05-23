using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CameraModifications
{
    /// <summary>
    /// Represents a target that can be looked at.
    /// </summary>
    class EyeTarget : MonoBehaviour
    {
        private static readonly float defaultOffset = ModPrefs.GetFloat("Camera", "fEyeTargetOffset", 0.5f, true);
        /// <summary>
        /// Offset in meters from the camera.
        /// </summary>
        public float offset = defaultOffset;

        /// <summary>
        /// Origin of the gaze.
        /// </summary>
        public Transform rootNode;

        private Transform camera;
        //private GameObject sphere;
        void Start()
        {
            try
            {
                var eyeCamera = GameObject.Find("CenterEyeAnchor");
                if (eyeCamera) camera = eyeCamera.transform;
                else camera = Camera.main.transform;

                //sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //sphere.transform.localScale *= 0.1f;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        void Update()
        {
            if (rootNode != null)
            {
                //sphere.transform.position = rootNode.position;
                var dir = (camera.position - rootNode.position).normalized;

                transform.position = camera.position + dir * offset;
            }


        }
    }
}
