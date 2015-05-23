using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HaremOVR;
using System.Collections;
using System;
using System.Linq;
using System.Reflection;

namespace StudioVR
{
    class OVRCursor : MonoBehaviour
    {
        Vector3 mousePosition = Vector3.zero;
        public float rayDistance = 100f;
        private bool isHold;

        public HoverState hover_state = HoverState.NONE;
        private RaycastHit hitInfo = new RaycastHit();
        private RaycastHit hitHold = new RaycastHit();
        public GameObject hoveredGO;

        private LineRenderer lineRenderer;

        private Vector2 sensitivity = new Vector2(0.01f, 0.01f);
        private bool _enabled = false;
        private Vector3 savedMousePosition;
        private StudioUIController uiController;
        private List<Collider> colliders = new List<Collider>();
        private FieldInfo objControllerEnableFlag;

        void Awake()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.SetVertexCount(2);
            lineRenderer.SetWidth(0.01f, 0.01f);
            lineRenderer.material = HaremOVR.Helper.GetColorMaterial();
            lineRenderer.material.color = Color.red;
            lineRenderer.renderer.enabled = false;
        }

        void Start()
        {
            uiController = GameObject.FindObjectOfType<StudioUIController>();
            objControllerEnableFlag = typeof(StudioUIController).GetField("ObjControllerEnableFlag", BindingFlags.NonPublic | BindingFlags.Instance);
            
            UpdateVisibility();
        }

        Ray GetRay()
        {
            var camera = OVRCamera.Instance;
            var origin = camera.transform.position + camera.transform.up * 0.2f;
            //Console.WriteLine(-camera.transform.forward);
            float r = 10;
            float p = mousePosition.y;
            float h = mousePosition.x;

            var point = new Vector3(
                r * Mathf.Cos(p) * Mathf.Sin(h),
                r * Mathf.Sin(p),
                r * Mathf.Cos(p) * Mathf.Cos(h)
            );

            point = Quaternion.LookRotation(camera.transform.forward, camera.transform.up) * point;

            //Console.WriteLine(mousePosition);
            //Console.WriteLine(point);
            return new Ray(
                origin,
                (point - origin).normalized
            );
        }

        bool IsVisible
        {
            get
            {
                return _enabled && colliders.Count > 0 && !Input.GetMouseButton(0) && !Input.GetMouseButton(1);
            }
        }


        float accum;
        int frames;
        void UpdateFPS()
        {
            frames++;
            accum += Time.timeScale / Time.deltaTime;

            Console.WriteLine("{0}fps", accum / frames);
        }

        void FixedUpdate()
        {
           
        }
        void Update()
        {
            //UpdateFPS();
            if (Input.GetKeyDown(KeyCode.Delete) /*&& (bool)objControllerEnableFlag.GetValue(uiController)*/)
            {
                SetEnabled(!_enabled);
            }

            if (IsVisible || isHold)
            {
                // Update mouse position
                UpdateMouse();
            }

            UpdateVisibility();

            if (_enabled)
            {
                UpdateLogic();

                var ray = GetRay();

                //Console.WriteLine(hover_state);

                var p1 = ray.origin + ray.direction * 0.4f;
                var p2 = ray.origin + ray.direction * 5f;
                if (hover_state == HoverState.HOVER)
                {
                    var startPoint = hitInfo.point - ray.direction * 0.1f;
                    p1 = Vector3.Distance(p1, ray.origin) > Vector3.Distance(hitInfo.point, ray.origin) ? startPoint : p1;
                    p2 = hitInfo.point;

                    lineRenderer.material.color = new Color(0,1,0,0.5f);
                }
                else
                {
                    lineRenderer.material.color = new Color(1,0,0, 0.5f);
                }
                lineRenderer.SetPosition(0, p1);
                lineRenderer.SetPosition(1, p2);
            }
        }

        void UpdateMouse()
        {
            mousePosition += new Vector3(Input.GetAxis("Mouse X") * sensitivity.x, Input.GetAxis("Mouse Y") * sensitivity.y, 0);
        }

        private void UpdateLogic()
        {
            colliders.ForEach(c => c.enabled = true);
            var ray = GetRay();
            //bool raycastResult = Physics.Raycast(ray, out this.hitInfo, this.rayDistance, -1);
            bool raycastResult = Physics.Raycast(ray, out this.hitInfo, this.rayDistance, 1 << LayerMask.NameToLayer("OnMouseObj"));
            //Console.WriteLine(raycastResult);
            //Console.WriteLine(hitInfo.ToString());
            if (!this.isHold && !Input.GetMouseButtonDown(0))
            {
                if (raycastResult)
                {

                    if (this.hover_state == HoverState.NONE)
                    {
                        this.hitInfo.collider.SendMessage("ObjOnMouseEnter", SendMessageOptions.DontRequireReceiver);
                        this.hoveredGO = this.hitInfo.collider.gameObject;
                    }
                    this.hover_state = HoverState.HOVER;
                }
                else
                {

                    if (this.hover_state == HoverState.HOVER && (bool)((UnityEngine.Object)this.hoveredGO))
                        this.hoveredGO.SendMessage("ObjOnMouseExit", SendMessageOptions.DontRequireReceiver);
                    this.hover_state = HoverState.NONE;
                }
            }
            if (this.isHold)
            {
                if (Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0))
                {
                    this.hitHold.collider.SendMessage("ObjOnMouseUp", SendMessageOptions.DontRequireReceiver);
                    this.isHold = false;
                }
                else
                    this.hitHold.collider.SendMessage("ObjOnMouseDrag", SendMessageOptions.DontRequireReceiver);
            }
            if (this.hover_state != HoverState.HOVER)
                return;
            if (!this.isHold)
            {
                if (!Input.GetMouseButtonDown(0))
                    return;
                this.hitInfo.collider.SendMessage("ObjOnMouseDown", SendMessageOptions.DontRequireReceiver);
                this.isHold = true;
                this.hitHold = this.hitInfo;
            }
            else
            {
                if (!Input.GetMouseButtonUp(0))
                    return;
                this.hitHold.collider.SendMessage("ObjOnMouseUp", SendMessageOptions.DontRequireReceiver);
                this.isHold = false;
            }

        }

        public void SetEnabled(bool enabled)
        {

            if (enabled && !_enabled)
            {
                savedMousePosition = Input.mousePosition;

                UpdateColliders();
                // Show handles
                StartCoroutine(ShowHandlesDelayed());
                
            }
            else if(!enabled && _enabled)
            {
                lineRenderer.enabled = false;
                Screen.lockCursor = false;
            }

            _enabled = enabled;
        }

        private void UpdateVisibility()
        {
            if (_enabled)
            {
                if (IsVisible)
                {
                    lineRenderer.enabled = true;
                    Screen.lockCursor = true;
                }
                else
                {
                    lineRenderer.enabled = false;
                    Screen.lockCursor = false;
                }
            }
        }

        private void UpdateColliders()
        {
            colliders.Clear();
            foreach (var ctrl in GameObject.FindObjectsOfType<Collider>())
                if (ctrl.gameObject.activeInHierarchy)
                    colliders.Add(ctrl.collider);
            //Console.WriteLine(colliders.Count);

        }
        private IEnumerator ShowHandlesDelayed()
        {


            yield return new WaitForFixedUpdate();

            foreach (var collider in colliders) collider.gameObject.SetActive(true);

         //   yield return new WaitForEndOfFrame();
            //yield return 0;

            //uiController.stdioMain.ChangeAllControllerObjActiveFlag(_enabled);

            //foreach (var ctrl in GameObject.FindObjectsOfType<ObjControllerManager>()) ctrl.ChangeEnable(_enabled);
            
        


            //using (List<ObjControllerManager>.Enumerator enumerator = uiController.stdioMain.ObjControllerList.GetEnumerator())
            //{
            //    while (enumerator.MoveNext())
            //    {
            //        enumerator.Current.ChangeEnable(_enabled);
            //    }
            //}
        }
        
        public enum HoverState
        {
            HOVER,
            NONE,
        }
    }
}
