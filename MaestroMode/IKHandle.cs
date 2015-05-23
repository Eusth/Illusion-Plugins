using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MaestroMode
{
    public abstract class IKHandle : MonoBehaviour
    {
        public enum DragMode { None, Move, Rotate }

        public IllusionCamera camera;

        private bool _changed = false;
        private bool _dragging = false;

        private Vector3 _iCameraFocus;
        private float _iCameraDistance;
        private Vector3 _iCameraOrientation;
        private Quaternion _cameraOrientation;
        private Vector3 _cameraPosition;
        

        private Vector3 _prevPosition;
        private Vector3 _prevMousePos;

        private float _distanceToCamera;
        private DragMode _mode = DragMode.None;
        private Material _material;

        private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        private Color activeColor = new Color(1f, 0.5f, 0.5f, 0.8f);
        private Color changedColor = new Color(0.5f, 0.5f, 1f, 0.8f);

        private void Awake()
        {
            //_material = GetComponent<MeshRenderer>().material;
            _material = GetComponent<MeshRenderer>().material = Helper.GetColorMaterial();
            //_material.renderQueue = 100000;
            _material.color = inactiveColor;

            gameObject.layer = LayerMask.NameToLayer("Chara");
        }

        public void Reset()
        {
            _changed = false;
            _material.color = inactiveColor;

            OnReset();
        }

        protected abstract void OnReset();
        protected abstract void CopyTransform();
        protected abstract void Activate(DragMode mode);
        protected abstract void ChangeWeight(DragMode mode, float amount);

        public abstract bool IsValid
        {
            get;
        }

        private void Update()
        {
            if (!_changed)
            {
                CopyTransform();
            }
            if (_dragging)
            {
                camera.Set(_iCameraFocus, _iCameraOrientation, _iCameraDistance);

                if (_mode == DragMode.Rotate && Input.GetMouseButtonUp(1))
                {
                    OnMouseUp(1);
                }
                else if (_mode == DragMode.Move && Input.GetMouseButtonUp(0))
                {
                    OnMouseUp(0);
                }
                else
                {
                    ChangeWeight(_mode, Input.GetAxis("Mouse ScrollWheel"));
                }

            }
        }

        private void LateUpdate()
        {
            if (_dragging)
            {
                GameCursor.isLock = false;
                camera.transform.position = _cameraPosition;
                camera.transform.rotation = _cameraOrientation;

                if (_mode == DragMode.Rotate)
                {
                    OnDragRotate();
                }
                else if (_mode == DragMode.Move)
                {
                    OnDragMove();
                }
            }
        }

        private void OnDragMove()
        {
            var pos = MouseToWorld();
            var diff = pos - _prevPosition;
            _prevPosition = pos;

            if(Input.GetMouseButton(1)) {
                var amountUp = Vector3.Dot(diff, Vector3.up);
                diff -= camera.transform.up * amountUp;
                diff += camera.transform.forward * amountUp;
            }

            transform.position += diff;
        }

        private void OnDragRotate()
        {
            var pos = Input.mousePosition;
            var delta = pos - _prevMousePos;
            _prevMousePos = pos;

            transform.rotation = Quaternion.AngleAxis(delta.y, camera.transform.right) * Quaternion.AngleAxis(-delta.x, camera.transform.up) * transform.rotation;
        }

        public void OnMouseDown(int mouseButton)
        {
            if (!Rotatable && mouseButton == 1) return;
            if (_dragging) return;

            _dragging = true;
            _material.color = activeColor;

            _iCameraFocus = camera.Focus;
            _iCameraOrientation = camera.Rotation;
            _iCameraDistance = camera.Distance;
            _cameraPosition = camera.transform.position;
            _cameraOrientation = camera.transform.rotation;
            _distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);

            _prevMousePos = Input.mousePosition;
            _prevPosition = MouseToWorld();

            if (mouseButton == 0) _mode = DragMode.Move;
            if (mouseButton == 1) _mode = DragMode.Rotate;

            if(!_changed)
                Activate(_mode);

            _changed = true;
        }

        private Vector3 MouseToWorld()
        {
            return Camera.main.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y, _distanceToCamera) );
        }

        private void OnMouseUp(int button)
        {
            _dragging = false;
            _mode = DragMode.None;
            _material.color = changedColor;
        }

        public void SetVisible(bool visible)
        {
            GetComponentInChildren<Renderer>().enabled = visible;
            GetComponentInChildren<Collider>().enabled = visible;

            _dragging = false;
            _mode = DragMode.None;
            _material.color = _changed ? changedColor : inactiveColor;
        }

        public abstract bool Rotatable
        {
            get;
        }

    }
}
