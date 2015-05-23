using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HaremOVR
{
    enum HaremMateGameLevels
    {
        Logo = 2,
        Title = 3,
        CharaMaker = 6,
        CostumeSelection = 14,
        HaremSelection = 15
    }

    class OVRPlugin : IPlugin
    {
        private OVRCamera ovrCamera;

        private bool firstRecenter = false;
        public bool Debug
        {
            get { return false; }
        }

        public void OnApplicationStart() {
            ovrCamera = OVRCamera.Instance;
            var shortcuts = ovrCamera.gameObject.AddComponent<Shortcuts>();
            shortcuts.cursor = GameObject.CreatePrimitive(PrimitiveType.Quad).AddComponent<CursorBlocker>();

           // OVRManager.display.mirrorMode = false;
            //Console.WriteLine(Application.unityVersion);
            //for (int i = 0; i <= 31; i++) //user defined layers start with layer 8 and unity supports 31 layers
            //{
            //    var layerN = LayerMask.LayerToName(i); //get the name of the layer
            //    if (layerN.Length > 0) //only add the layer if it has been named (comment this line out if you want every layer)
            //        Console.WriteLine(layerN);
            //}
        }


        public void OnLevelWasLoaded(int level)
        {
            // Menu
            if (level == 3 && !firstRecenter)
            {
                firstRecenter = true;
                OVRManager.capiHmd.RecenterPose();
            }
        }

        private bool IsMainCamera(GameObject cam)
        {
            if (cam == null) return false;
            if (!cam.CompareTag("MainCamera")) return false;
            if (!cam.name.Contains("Prefab")) return false;
            return true;

        }

        public void OnLevelWasInitialized(int level)
        {
            Console.WriteLine(level);
            var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

            if (IsMainCamera(mainCamera) && (level != (int)HaremMateGameLevels.CharaMaker && level != (int)HaremMateGameLevels.CostumeSelection ))
            {
                ovrCamera.Mimic(mainCamera);

            }
            else
            {
                ovrCamera.Mimic(null);

                if (IsMainCamera(mainCamera))
                {
                    mainCamera.name = "Old_Camera";
                    mainCamera.camera.cullingMask = LayerMask.GetMask("Chara");
                }

                UnityEngine.Object obj = global::Helper.Loader.Load(string.Concat("map/", "base01", ".unity3d"), string.Concat("Prefabs/map/", "p_m01_02"), typeof(GameObject));
                var map = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
                map.transform.position = new Vector3(0, 0, 0);
                ovrCamera.transform.position = new Vector3(0, OVRManager.profile.eyeHeight, 0);

                var light = new GameObject();
                var lightComponent = light.AddComponent<Light>();
                lightComponent.type = LightType.Point;

                foreach(var child in map.Children()) child.layer = LayerMask.NameToLayer(OVRCamera.QUAD_LAYER);
                

                //var mapManager = Singleton<Manager.Map>.Instance;
                //mapManager.Load("mapdef", true);
                //mapManager.ChangeMap(0);
            }
        }


        #region Method Stubs

        public void OnApplicationQuit() {
        }

        #endregion


        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {

        }

        public string Name
        {
            get { return "HaremOVR"; }
        }

        public string Version
        {
            get { return "0.9.2"; }
        }
    }
}
