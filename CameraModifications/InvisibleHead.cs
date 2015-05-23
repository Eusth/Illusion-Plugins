using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CameraModifications
{
    class InvisibleHead : MonoBehaviour
    {
        private List<Renderer> rendererList = new List<Renderer>();
        private bool hidden = false;
        private Transform root;

        private Renderer[] m_tongues;
        public void Awake()
        {
            root = GetComponentsInParent<Transform>().Last(t => t.name.Contains("body")).parent;

            m_tongues = root.GetComponentsInChildren<SkinnedMeshRenderer>().Where(renderer => renderer.name.StartsWith("cm_O_tang") || renderer.name == "cf_O_tang").Where(tongue => tongue.enabled).ToArray();

            Console.WriteLine("FOund {0} tongues at {1}", m_tongues.Length, root.name);
        }
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
            if (visible)
            {
                if(hidden) {
                    // enable
                    //Console.WriteLine("Enabling {0} renderers", rendererList.Count);
                    foreach (var renderer in rendererList)
                    {
                        renderer.enabled = true;
                    }
                    foreach (var renderer in m_tongues)
                    {
                        renderer.enabled = true;
                    }
                    
                }
            }
            else
            {
                if(!hidden) {
                    // disable
                    rendererList.Clear();
                    foreach (var renderer in GetComponentsInChildren<Renderer>().Where(renderer => renderer.enabled))
                    {
                        rendererList.Add(renderer);
                        renderer.enabled = false;
                    }

                    foreach (var renderer in m_tongues)
                    {
                        Console.WriteLine(renderer.name);
                        renderer.enabled = false;
                    }
                }
            }

            hidden = !visible;
        }
    }
}
