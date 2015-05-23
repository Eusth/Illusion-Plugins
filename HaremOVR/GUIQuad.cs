using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HaremOVR
{
    public class GUIQuad : MonoBehaviour
    {
        public RenderTexture texture;
        public RenderTexture guiTexture;

        List<Camera> cameras = new List<Camera>();

        private bool dirtyFlag = false;
        //public float heightFactor = 1.4f;
        //public float angleSpan = 170;
        private RenderTexture prevRT = null;
        public float curviness = 1;

        void Awake()
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
        }

        public void RebuildPlane()
        {
            var plane = GetComponent<ProceduralPlane>();
            plane.height = (Settings.Instance.Angle / 100);
            plane.angleSpan = Settings.Instance.Angle;
            plane.curviness = curviness;
            plane.Rebuild();

            //var mesh = plane.GetComponent<MeshFilter>().mesh;
            //var verts = mesh.vertices;

            //for (int i = 0; i < verts.Length; i++)
            //{
            //    verts[i] = verts[i].normalized;
            //}

            //mesh.vertices = verts;

            //mesh.RecalculateBounds();
            //mesh.RecalculateNormals();

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

        void Start()
        {

            var mesh = GetComponent<MeshFilter>().mesh;
            //var localCameraPos = new Vector3(0, 0, -1);
            var vertices = mesh.vertices;

            //for (int i = 0; i < vertices.Length; i++)
            //{
            //    float y = vertices[i].y;
            //    var cameraPosition = new Vector3(0, vertices[i].y, -distance);
            //    vertices[i] = cameraPosition + (vertices[i] - cameraPosition).normalized * distance;
            //    //Console.WriteLine("{0} -> {1}", prevZ, vertices[i].y);
            //}

            //mesh.vertices = vertices;
            //mesh.RecalculateBounds();

            dirtyFlag = true;

        }

        bool IsFading
        {
            get
            {
                return Manager.Scene.Instance.sceneFade.IsFadeNow();
            }
        }

        void Update()
        {
            if (renderer.enabled && !dirtyFlag)
            {
                int i = 0;
                foreach (var cam in cameras)
                {
                    if (cam.gameObject.activeInHierarchy && cam.gameObject != null)
                    {
                        if (i++ == 0)
                        {
                            cam.clearFlags = CameraClearFlags.SolidColor;
                            cam.backgroundColor = new Color(1, 1, 1, 0);

                        }
                        else
                        {
                            cam.clearFlags = CameraClearFlags.Depth;
                        }

                        cam.enabled = true;
                        cam.targetTexture = texture;

                        cam.Render();

                        cam.targetTexture = null;
                        cam.enabled = false;
                    }
                }

            }
        }

        void LateUpdate()
        {
            if (dirtyFlag)
            {
                RenewCameras();
                dirtyFlag = false;
            }
        }

        public void UpdateGUI(bool transparent, bool renderGUI)
        {
            if (transparent)
            {
                if (renderGUI)
                {
                    renderer.material = Helper.GetTransparentMaterial2();
                    renderer.material.SetTexture("_MainTex", guiTexture);
                    renderer.material.SetTexture("_SubTex", texture);
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

        private void RenewCameras()
        {
            cameras.Clear();

            try
            {
                foreach (var cam in GameObject.FindObjectsOfType<Camera>().OrderBy(c => c.depth))
                {
                    if (cam.name.Contains("Anchor") || cam.name.Contains("Prefab") || cam.name == "BackCamera" || cam.name == "CapSprite" || cam.name == "RTCamera") continue;

                    cameras.Add(cam);
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

        void OnLevelWasLoaded(int level)
        {
            //Console.WriteLine("LOAD");
            dirtyFlag = true;
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
