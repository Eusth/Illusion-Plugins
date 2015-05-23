using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace PlayClubVR
{
    class CursorBlocker : GuardedBehaviour
    {
        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.None, ExactSpelling = false)]
        private static extern void SetCursorPos(int X, int Y);

        [DllImport("USER32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern System.IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ClipCursor(ref RECT rcClip);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClipCursor(out RECT rcClip);
        [DllImport("user32.dll")]
        static extern int GetForegroundWindow();
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        RECT currentClippingRect;
        RECT originalClippingRect = new RECT();
 

        private bool _useHeadControls = false;
        public bool useHeadControls {
            get {
                return _useHeadControls;
            }
            set {
                _useHeadControls = value;

                this.renderer.material.mainTexture = useHeadControls ? Helper.LoadImage("crosshair_black.png") : Helper.LoadImage("cursor.png");
            }
        }
        private IntPtr windowHandle;



        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
            
            this.renderer.material = Helper.GetTransparentMaterial();

            //if(useHeadControls)
            windowHandle = GetActiveWindow();
            //this.renderer.material.shader = Shader.Find("Unlit/Transparent Colored");

            GetWindowRect(windowHandle, ref currentClippingRect);
            GetClipCursor(out originalClippingRect);

            if(Screen.fullScreen)
                ClipCursor(ref currentClippingRect);

            gameObject.layer = LayerMask.NameToLayer("UI");
            transform.localScale *= 0.1f;

            useHeadControls = _useHeadControls;
            ShowCursor(false);
        }

        void UpdateClip()
        {
            GetWindowRect(windowHandle, ref currentClippingRect);
            ClipCursor(ref currentClippingRect);
        }

        void OnApplicationQuit()
        {
            if (Screen.fullScreen)
                ClipCursor(ref originalClippingRect);
        }

        protected override void OnFixedUpdate()
        {
            //var mousePosition = Input.mousePosition;

            //if (Screen.fullScreen && Screen.a)
            //{
            //    float oldX = mousePosition.x;
            //    float oldY = mousePosition.y;
            //    mousePosition = new Vector3(
            //        Mathf.Clamp(mousePosition.x, 0, Screen.width),
            //        Mathf.Clamp(mousePosition.y, 0, Screen.height),
            //        mousePosition.z
            //    );

            //    if (mousePosition.x != oldX || mousePosition.y != oldY)
            //    {
            //        SetCursorPos(mousePosition);
            //    }
            //}
        }


        protected override void OnUpdate()
        {
            try
            {
                var mousePosition = Vector3.zero;
                if (useHeadControls)
                {
                    if (Screen.showCursor)
                    {
                        SetCursorPos(GetCursorPos());
                    }
                }
                else
                {
                    mousePosition = Input.mousePosition;
                }

                if (Screen.showCursor || (useHeadControls && mousePosition == Vector3.zero))
                {
                    if (!renderer.enabled)
                        renderer.enabled = true;

                    transform.position = (mousePosition - (new Vector3(Screen.width, Screen.height, 0) / 2) + new Vector3(25, -25, 0)) * (2f / Screen.height) - new Vector3(0, 0, 0.1f);
                }
                else
                {
                    if (renderer.enabled)
                        renderer.enabled = false;
                }


                if (Screen.fullScreen)
                    UpdateClip();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void SetCursorPos(Vector3 mousePosition)
        {
            POINT p = new POINT((int)mousePosition.x, (int)(Screen.height - mousePosition.y));
            //Console.WriteLine(mousePosition);
            //Console.WriteLine("[{0}, {1}] -> [{2}, {3}]", mousePosition.x, mousePosition.y, p.X, p.Y);
            ClientToScreen(windowHandle, ref p);

            SetCursorPos(p.X, p.Y);
        }

        Vector3 GetCursorPos()
        {
            float width = 1;//OVRCamera.Instance.GUI.size;

            float height = (float)Screen.height / Screen.width * width;

            float ratio = Screen.width / width;

            var origin = OVRCamera.Instance.CenterEye.transform;
            RaycastHit hit;
            if (Physics.Raycast(origin.position, origin.forward, out hit, 1f, LayerMask.GetMask(OVRCamera.QUAD_LAYER)))
            {
                var winCoord = hit.collider.gameObject.transform.InverseTransformPoint(hit.point) * ratio;
                
                return new Vector3(winCoord.x + Screen.width / 2, winCoord.y + Screen.height / 2, 1);
            }

            return Vector3.zero;
        }

        private void CheckTransition()
        {
            //ObjControllerManager

        }

	    public struct POINT
	    {
		    public int X;

		    public int Y;

		    public POINT(int x, int y)
		    {
			    this.X = x;
			    this.Y = y;
		    }
	    }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }


    }
}
