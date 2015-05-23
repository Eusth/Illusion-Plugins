using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SoftUncensor
{
    public class SyncedBone : MonoBehaviour
    {
        public Transform referenceBone;
        public bool fullSync = true;
        public void Start()
        {

        }

        public void LateUpdate()
        {
            if (referenceBone)
            {
                if (fullSync)
                    transform.position = referenceBone.position;
                transform.rotation = referenceBone.rotation;
                transform.localScale = referenceBone.localScale;
            }
        }
    }


    public class SoftUncensor : IPlugin
    {
        private GameObject m_chinkoPrefab;
        
        public void OnApplicationStart()
        {
            var assetBundle = AssetBundle.CreateFromMemoryImmediate(Resource1.chinko3);
            m_chinkoPrefab = assetBundle.mainAsset as GameObject;
            assetBundle.Unload(false);
        }

        public void OnApplicationQuit()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
            // Remove mosaics, BAH!
            foreach(var moz in GameObject.FindObjectsOfType<MozUV>()) {
                GameObject.DestroyImmediate(moz.gameObject);

            }

            // Add CHINKOs
            foreach (var dan in GameObject.FindObjectsOfType<Transform>().Where(transform => transform.name == "cm_J_dan100_00"))
            {
                var chinko = GameObject.Instantiate(m_chinkoPrefab) as GameObject;
                chinko.transform.SetParent(dan.transform , false);

                foreach (var bone in chinko.GetComponentsInChildren<Transform>())
                {
                    var refBone = dan.GetComponentsInChildren<Transform>().FirstOrDefault(transform => transform.name == bone.name);
                    if (refBone)
                    {
                        var synced = bone.gameObject.AddComponent<SyncedBone>();
                        synced.referenceBone = refBone;
                        synced.fullSync = true;

                    }
                }
                
            }
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }

        public string Name
        {
            get { return "Soft Uncensor"; }
        }

        public string Version
        {
            get { return "0.0.1"; }
        }
    }
}
