using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StudioVR
{
    /**
     * Manages the eye target for a single female.
     */
    class EyeTargetController : MonoBehaviour
    {
        public Transform Target { get; private set; }
        public Transform rootNode;
        private EyeTarget eyeTarget;
        
        void Start()
        {
            Target = new GameObject().transform;

            eyeTarget = Target.gameObject.AddComponent<EyeTarget>();
            eyeTarget.rootNode = rootNode;
        }


        void OnDestroy()
        {
            // Character was destroyed, so destroy the created target!
            Destroy(Target.gameObject);
        }
    }
}
