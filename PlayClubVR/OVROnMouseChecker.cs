using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


//namespace HaremOVR
//{
//    public class OVROnMouseChecker : MonoBehaviour
//    {
//        public OnMouseChecker.HoverState hover_state = OnMouseChecker.HoverState.NONE;
//        public float rayDistance = 100f;
//        private RaycastHit hitInfo = new RaycastHit();
//        private RaycastHit hitHold = new RaycastHit();
//        private Ray ray = new Ray();
//        public GameObject hoveredGO;
//        private bool isHold;
//        private Camera camera;
//        public StudioUIController stUICtrl;

//        private LineRenderer lineRenderer;

//        private void Start()
//        {

//            lineRenderer = gameObject.AddComponent<LineRenderer>();
//            lineRenderer.SetVertexCount(2);
//            lineRenderer.SetColors(Color.red, Color.red);
//            lineRenderer.SetWidth(0.01f, 0.01f);

//            if ((bool)((Object)Camera.main))
//                this.camera = Camera.main;
//            if (!((Object)this.stUICtrl == (Object)null))
//                return;
//            this.stUICtrl = this.GetComponent<StudioUIController>();
//        }

//        private void Update()
//        {
//            var camera = OVRCamera.Instance.CenterEye;
//            lineRenderer.SetPosition(0, camera.transform.position + camera.transform.forward * 2);
//            lineRenderer.SetPosition(1, camera.transform.position + camera.transform.forward * rayDistance);

//            this.ray = new Ray(camera.transform.position, camera.transform.forward);


//            if (!this.isHold && !this.stUICtrl.IsGUICheckFlag && Input.GetMouseButtonDown(0))
//            {
//                if (Physics.Raycast(this.ray, out this.hitInfo, this.rayDistance, 1 << LayerMask.NameToLayer("OnMouseObj")))
//                {
//                    if (this.hover_state == OnMouseChecker.HoverState.NONE)
//                    {
//                        this.hitInfo.collider.SendMessage("ObjOnMouseEnter", SendMessageOptions.DontRequireReceiver);
//                        this.hoveredGO = this.hitInfo.collider.gameObject;
//                    }
//                    this.hover_state = OnMouseChecker.HoverState.HOVER;
//                }
//                else
//                {
//                    if (this.hover_state == OnMouseChecker.HoverState.HOVER && (bool)((Object)this.hoveredGO))
//                        this.hoveredGO.SendMessage("ObjOnMouseExit", SendMessageOptions.DontRequireReceiver);
//                    this.hover_state = OnMouseChecker.HoverState.NONE;
//                }
//            }
//            if (this.isHold)
//            {
//                if (Input.GetMouseButtonUp(0) || !Input.GetMouseButton(0) || this.stUICtrl.IsGUICheckFlag)
//                {
//                    this.hitHold.collider.SendMessage("ObjOnMouseUp", SendMessageOptions.DontRequireReceiver);
//                    this.isHold = false;
//                }
//                else
//                    this.hitHold.collider.SendMessage("ObjOnMouseDrag", SendMessageOptions.DontRequireReceiver);
//            }
//            if (this.hover_state != OnMouseChecker.HoverState.HOVER)
//                return;
//            if (!this.isHold)
//            {
//                if (!Input.GetMouseButtonDown(0))
//                    return;
//                this.hitInfo.collider.SendMessage("ObjOnMouseDown", SendMessageOptions.DontRequireReceiver);
//                this.isHold = true;
//                this.hitHold = this.hitInfo;
//            }
//            else
//            {
//                if (!Input.GetMouseButtonUp(0))
//                    return;
//                this.hitHold.collider.SendMessage("ObjOnMouseUp", SendMessageOptions.DontRequireReceiver);
//                this.isHold = false;
//            }

//        }

//        public enum HoverState
//        {
//            HOVER,
//            NONE,
//        }
//    }

//}
