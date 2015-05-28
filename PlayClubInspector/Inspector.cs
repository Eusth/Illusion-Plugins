using IllusionPlugin;
using RootMotion;
using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlayClubInspector
{

    internal class Handle : MonoBehaviour
    {
        public static Handle Create(Transform at)
        {
            var handle = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<Handle>();
            handle.transform.position = at.position;
            handle.transform.rotation = at.rotation;
            handle.transform.localScale *= 0.1f;

            return handle;
        }

        public void OnMouseDown()
        {

        }

        public void OnMouseDrag()
        {

        }

        public void OnMouseUp()
        {

        }
    }
    public class Inspector : IPlugin
    {
        private Texture2D m_nudeTexture;
        private H_Scene scene;
        private FieldInfo nakedRenderersInfo = typeof(Human).GetField("nakedRenderers", BindingFlags.NonPublic | BindingFlags.Instance);

        Component CopyComponent(Component original, GameObject destination)
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy;
        }

        public class WatchDog : MonoBehaviour
        {
            public void Start()
            {

                GameObject.DontDestroyOnLoad(gameObject);
            }
            public void Update()
            {
                foreach (var slider in GameObject.FindObjectsOfType<Slider>().Where(slider => slider.GetComponent<ChangedSlider>() == null))
                {
                    Console.WriteLine("Add slider");
                    slider.gameObject.AddComponent<ChangedSlider>();
                }
            }
        }
        public class ChangedSlider : MonoBehaviour
        {
            Slider slider;
            float initialValue;
            public void Start()
            {
                slider = GetComponent<Slider>();
                initialValue = slider.maxValue;
            }

            public void Update() {
                Console.WriteLine("{0}->{1}", slider.maxValue, initialValue * 5);
                slider.maxValue = initialValue * 5;
            }
        }

        //private class LoadAssetBundle : MonoBehaviour
        //{
        //    string path = @"D:\Novels\illusion\PlayClub\cm3d.unity3d";
        //    Animator marioAnim;
        //    H_Scene scene;


        //    public IEnumerator Start()
        //    {

        //        scene = GameObject.FindObjectOfType<H_Scene>();
        //        Console.WriteLine("Connecting..");
        //        WWW www = new WWW(new Uri(path).AbsoluteUri);
        //        yield return www;
        //        try
        //        {
        //            Console.WriteLine("Opening..");

        //            var assetBundle = www.assetBundle;
        //            Console.WriteLine("opened");


        //            var mario = assetBundle.mainAsset;

        //            Console.WriteLine("Playing.. {0}", mario);

        //            var marioObj = GameObject.Instantiate(mario, Vector3.zero, Quaternion.identity) as GameObject;
        //            foreach (var transform in marioObj.GetComponentsInChildren<Transform>())
        //            {
        //                transform.gameObject.layer = LayerMask.NameToLayer("Chara");
        //            }

        //            assetBundle.Unload(false);

        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //        finally
        //        {
        //        }
                
        //    }

        //    public void Update()
        //    {
        //        if (Input.GetKeyUp(KeyCode.Space))
        //        {
        //            Console.WriteLine("EXPORT");
        //            //ExportToCollada(scene.Members.FirstOrDefault().animator.transform);
        //        }
        //    }
        //    public void LateUpdate()
        //    {
        //    }
        //}

        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public void OnApplicationStart()
        {
            //new GameObject().AddComponent<WatchDog>();
            // Get textures to swap
            if (Directory.Exists("Textures"))
            {
                foreach (var texturePath in Directory.GetFiles("Textures").Where(file => file.EndsWith(".jpg") || file.EndsWith(".png")))
                {
                    var texture = new Texture2D(2, 2);
                    texture.LoadImage(File.ReadAllBytes(texturePath));

                    textures.Add(Path.GetFileNameWithoutExtension(texturePath), texture);

                    Console.WriteLine("Loaded {0}", Path.GetFileNameWithoutExtension(texturePath));
                }
            }

        }


        public void OnApplicationQuit()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }


        public void OnLevelWasInitialized(int level)
        {
            scene = GameObject.FindObjectOfType<H_Scene>();

            // Remove mosaic
            //foreach (var mosaic in GameObject.FindObjectsOfType<MozUV>())
            //{
            //    mosaic.enabled = false;

            //    if (mosaic.name.StartsWith("cf_"))
            //        mosaic.GetComponent<Renderer>().enabled = false;
            //}


            //foreach (var renderer in GameObject.FindObjectsOfType<Renderer>().Where(renderer => renderer.sharedMaterial.mainTexture != null))
            //{
            //    if (textures.ContainsKey(renderer.sharedMaterial.mainTexture.name))
            //    {
            //        renderer.sharedMaterial.mainTexture = textures[renderer.sharedMaterial.mainTexture.name];
            //    }
            //    //Console.WriteLine("OWRAP: {0}", renderer.sharedMaterial.mainTexture.wrapMode);
            //    //Console.WriteLine("NWRAP: {0}", m_nudeTexture.wrapMode);

            //    // renderer.sharedMaterial.SetTexture("_MainTex", m_nudeTexture);
            //    //renderer.sharedMaterial.mainTexture = m_nudeTexture;
            //}

            if (level == 3)
            {
                // Make bipeds!

                //var target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //target.transform.localScale = Vector3.one * 0.1f;

                //var scene = GameObject.FindObjectOfType<H_Scene>();
                //foreach (var human in scene.Members)
                //{
                //    var biped = human.animator.gameObject.AddComponent<FullBodyBipedIK>();

                //    // Auto detect
                //    biped.references = new BipedReferences();
                //    BipedReferences.AutoDetectReferences(ref biped.references, biped.transform, new BipedReferences.AutoDetectParams(false, false));
                //    biped.solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(biped.references);
                //    biped.solver.SetToReferences(biped.references, biped.solver.rootNode);

                //    var leftHandEffector = biped.solver.GetEffector(FullBodyBipedEffector.LeftHand);
                //    var rightHandEffector = biped.solver.GetEffector(FullBodyBipedEffector.RightHand);

                //    leftHandEffector.positionWeight = 1;
                //    //leftHandEffector.rotationWeight = 1;

                //    Handle.Create(leftHandEffector.bone);
                //}
            }

        }

        public bool Debug
        {
            get { return true; }
        }


        public void OnUpdate()
        {

            if (Input.GetKeyUp(KeyCode.Alpha0))
            {
                Camera.main.GetComponent<IllusionCamera>().enabled = !Camera.main.GetComponent<IllusionCamera>().enabled;
            }
            //if (Application.loadedLevel == 0)
            {
                //for (int i = (int)KeyCode.Keypad1; i <= (int)KeyCode.Keypad9; i++)
                //{
                //    if (Input.GetKeyDown((KeyCode)i))
                //    {
                //        Application.LoadLevel(i - (int)KeyCode.Keypad0);
                //        break;
                //    }
                //}
            }

            //if (Application.loadedLevel == 3)
            //{
            //    if (Input.GetMouseButtonDown(0))
            //    {
            //        Console.WriteLine("SHOOT");

            //        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //        float minDistance = float.MaxValue;
            //        Human selectedHuman = null;
            //        foreach (var member in scene.Members)
            //        {
            //            foreach (var renderer in nakedRenderersInfo.GetValue(member) as List<Renderer>)
            //            {
            //                float distance;
                            
            //                if (renderer.bounds.IntersectRay(ray, out distance))
            //                {
            //                    Console.WriteLine("{0} at {1}", member.name, distance);

            //                    if (distance < minDistance)
            //                    {
            //                        minDistance = distance;
            //                        selectedHuman = member;
            //                    }
            //                }
            //            }
            //        }

            //        if (selectedHuman)
            //        {
            //            Console.WriteLine(selectedHuman.name);
            //        }

            //        //RaycastHit hit;
            //        //if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10, LayerMask.GetMask("Chara", "ToLiquidCollision"))) {
            //        //    Console.WriteLine("HIT: {0}", hit.collider.name);
            //        //}
            //    }
            //}
        }

        public void OnFixedUpdate()
        {

        }

        public string Name
        {
            get { return "TextureSwapper"; }
        }

        public string Version
        {
            get { return "0.0.0"; }
        }
    }
}
