using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HaremOVR
{
    class InvisibleHead : MonoBehaviour
    {

        void OnEnable()
        {
            SetVisibility(false);
        }

        void OnDisable()
        {
            SetVisibility(true);
        }

        void SetVisibility(bool visible)
        {
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
            {
                renderer.enabled = visible;
            }
            foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                renderer.enabled = visible;
            }
        }
    }
}
