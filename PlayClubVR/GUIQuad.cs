using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlayClubVR
{
    public class GUIQuad : GuardedBehaviour
    {

        public class SortingAwareGraphicRaycaster : GraphicRaycaster
        {
            private Canvas m_Canvas;
            private Canvas canvas
            {
                get
                {
                    if (m_Canvas != null)
                        return m_Canvas;

                    m_Canvas = GetComponent<Canvas>();
                    return m_Canvas;
                }
            }

            public override int priority
            {
                get
                {
                    return -canvas.sortingOrder;
                }
            }
            public override int sortOrderPriority
            {
                get
                {
                    return -canvas.sortingOrder;
                }
            }
        }

        public RenderTexture texture;
        new public RenderTexture guiTexture;

        private FieldInfo m_Graphics;

        List<Camera> cameras = new List<Camera>();

        private bool dirtyFlag = false;
        //public float heightFactor = 1.4f;
        //public float angleSpan = 170;
        private RenderTexture prevRT = null;
        public float curviness = 1;

        Camera guiCamera;
        protected override void OnAwake()
        {

            var plane = gameObject.AddComponent<ProceduralPlane>();
            plane.distance = 1;
            plane.xSegments = 100;

            texture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Default);
            texture.antiAliasing = 4;
            texture.Create();

            guiTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.Default);
            guiTexture.Create();

            transform.localPosition = Vector3.zero;// new Vector3(0, 0, distance);
            transform.localRotation = Quaternion.identity;
            gameObject.layer = LayerMask.NameToLayer(OVRCamera.QUAD_LAYER);

            DontDestroyOnLoad(gameObject);

            gameObject.AddComponent<FastGUI>();
            gameObject.AddComponent<SlowGUI>();

            RebuildPlane();
            UpdatePosition();

            // Add GUI camera
            guiCamera = new GameObject("GUICamera").AddComponent<Camera>();
            guiCamera.transform.position = Vector3.zero;
            guiCamera.transform.rotation = Quaternion.identity;
            GameObject.DontDestroyOnLoad(guiCamera);

            guiCamera.cullingMask = LayerMask.GetMask("UI");
            guiCamera.depth = 1000;
            guiCamera.nearClipPlane = 99f;
            guiCamera.farClipPlane = 10000;
            guiCamera.targetTexture = texture;
            //raycasterManager = Type.GetType("UnityEngine.UI.CanvasListPool, UnityEngine.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            m_Graphics = typeof(GraphicRegistry).GetField("m_Graphics", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void RebuildPlane()
        {
            var plane = GetComponent<ProceduralPlane>();
            plane.height = (Settings.Instance.Angle / 100);
            plane.angleSpan = Settings.Instance.Angle;
            plane.curviness = curviness;
            plane.Rebuild();

            float distance = Settings.Instance.Distance;
            transform.localScale = new Vector3(distance, distance /* (angleSpan / 100) * distance*/ /*((float)Screen.height) / Screen.width * size*/, distance);
        }



        public Vector3 GetCursorWorldPos()
        {
            var plane = GetComponent<ProceduralPlane>();
            var cursor = Input.mousePosition;
            var normalizedCursor = new Vector3(cursor.x / Screen.width, cursor.y / Screen.height, cursor.z);

            // Shift to [-0.5, 0.5]
            normalizedCursor -= new Vector3(0.5f, 0.5f, 0);
            var localPoint = new Vector3(plane.TransformX(normalizedCursor.x), normalizedCursor.y * plane.height, plane.TransformZ(normalizedCursor.x));
            return transform.TransformPoint(localPoint);
        }

        public void UpdatePosition()
        {
            transform.localEulerAngles = Vector3.zero;
            //transform.localPosition = Vector3.zero + new Vector3(0, Settings.Instance.OffsetY, 0);
        }

        protected override void OnStart()
        {
            StartCoroutine(DelayedUpdate());
        }


        protected void CatchCanvas()
        {
            //foreach (var raycaster in ((List<BaseRaycaster>)getRaycasters.Invoke(raycasterManager, null)).OfType<GraphicRaycaster>())
            //{
            //    var canvas = raycaster.GetComponent<Canvas>();
            //    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            //    {
            //        if (canvas.name.Contains("TexFade")) continue;
            //        //if(canvas.name.Contains("Modal"))
            //        //Console.WriteLine("Add {0} ({1}: {2})", canvas.name, canvas.sortingLayerName, canvas.sortingOrder);
            //        canvas.renderMode = RenderMode.ScreenSpaceCamera;
            //        canvas.worldCamera = guiCamera;
            //    }
            //}

            // Get a list of canvas. This is faster than iterating over all objects in the scene.
            var canvasList = ((m_Graphics.GetValue(GraphicRegistry.instance) as IDictionary).Keys as ICollection<Canvas>)
                            .Where(c => c != null).SelectMany(canvas => canvas.gameObject.GetComponentsInChildren<Canvas>());
            //var canvasList = GameObject.FindObjectsOfType<Canvas>();
            
            foreach (var canvas in canvasList.Where(c => c.renderMode == RenderMode.ScreenSpaceOverlay && c.worldCamera != guiCamera))
            {
                if (canvas.name.Contains("TexFade")) continue;
                Console.WriteLine("Add {0} ({1}: {2})", canvas.name, canvas.sortingLayerName, LayerMask.LayerToName(canvas.gameObject.layer));
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = guiCamera;

                var raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster)
                {
                    GameObject.DestroyImmediate(raycaster);
                    var newRaycaster = canvas.gameObject.AddComponent<SortingAwareGraphicRaycaster>();
                    newRaycaster.ignoreReversedGraphics = raycaster.ignoreReversedGraphics;
                    newRaycaster.blockingObjects = raycaster.blockingObjects;
                }
            }
        }

        protected override void OnUpdate()
        {
            if (renderer.enabled)
            {
                //var watch = System.Diagnostics.Stopwatch.StartNew();
                CatchCanvas();
                //Console.WriteLine(watch.ElapsedTicks);
            }
        }

        public void UpdateGUI(bool transparent, bool renderGUI)
        {
            //Console.WriteLine();
            //renderGUI = false;
            try
            {
                if (transparent)
                {
                    if (renderGUI)
                    {
                        renderer.material = Helper.GetTransparentMaterial2();
                        renderer.material.SetTexture("_MainTex", texture);
                        renderer.material.SetTexture("_SubTex", guiTexture);
                    }
                    else
                    {
                        renderer.material = Helper.GetTransparentMaterial();
                        renderer.material.mainTexture = texture;
                    }
                }
                else
                {
                    renderer.material = Helper.GetMaterial();
                    renderer.material.mainTexture = texture;
                }


                //renderer.materials = new Material[]{
                //    transparent ? Helper.GetTransparentMaterial() : Helper.GetMaterial(),
                //    Helper.GetTransparentMaterial()
                //};

                //renderer.materials[0].mainTexture = texture;
                //renderer.materials[1].mainTexture = guiTexture;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private IEnumerator DelayedUpdate()
        {
            yield return new WaitForSeconds(0.1f);
            RenewCameras();
        }

        private void RenewCameras()
        {
            return;
            cameras.Clear();

            try
            {
                int i = 0;

                foreach (var cam in GameObject.FindObjectsOfType<Camera>().OrderBy(c => c.depth)
                                .Where(c => !c.name.Contains("Anchor") && !c.name.Contains("Oculus")) // Ignore Oculsu cameras
                                .Where(c => (c.cullingMask & LayerMask.GetMask("UI")) != 0)) // Ignore cameras that don't render UI
                {

                    Console.WriteLine("ADD {0}: {1}", cam.name, string.Join(", ", Helper.GetLayerNames(cam.cullingMask)));
                    cameras.Add(cam);
                    cam.targetTexture = texture;
                    if (i++ == 0)
                    {
                        cam.clearFlags = CameraClearFlags.SolidColor;
                        cam.backgroundColor = new Color(1, 1, 1, 0);
                    }
                    else
                    {
                        cam.clearFlags = CameraClearFlags.Depth;
                    }

                    //cam.targetTexture = texture;

                    //cam.renderingPath = RenderingPath.Forward;
                    //if (firstCamera)
                    //{
                    //    firstCamera = false;

                    //    cam.clearFlags = CameraClearFlags.SolidColor;
                    //    cam.backgroundColor = new Color(1, 1, 1, 0);
                    //}
                    //else
                    //{
                    //    cam.clearFlags = CameraClearFlags.Depth;
                    //}
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("Error initializing the cameras: " + e);
            }
        }

        protected override void OnLevel(int level)
        {
            StartCoroutine(DelayedUpdate());
        }

        internal void OnAfterGUI()
        {
            if (Event.current.type == EventType.Repaint)
                RenderTexture.active = prevRT;
        }

        internal void OnBeforeGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                prevRT = RenderTexture.active;
                RenderTexture.active = guiTexture;

                GL.Clear(true, true, Color.clear);
            }
        }
    }

    class FastGUI : MonoBehaviour
    {
        private void OnGUI()
        {
            GUI.depth = 1000;

            if (Event.current.type == EventType.Repaint)
            {
                SendMessage("OnBeforeGUI");
            }
        }
    }

    class SlowGUI : MonoBehaviour
    {
        private void OnGUI()
        {
            GUI.depth = -1000;

            if (Event.current.type == EventType.Repaint)
            {
                SendMessage("OnAfterGUI");
            }
        }
    }

}
