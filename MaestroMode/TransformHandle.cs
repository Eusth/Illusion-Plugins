using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MaestroMode
{
    public class TransformConstrainer : MonoBehaviour
    {
       

    
    }
    public class TransformHandle : IKHandle
    {
        public Transform target;

        public bool usePosition = false;
        public bool useRotation = false;

        protected override void OnReset()
        {
            usePosition = false;
            useRotation = false;
        }

        protected override void CopyTransform()
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }

        public override void Activate(IKHandle.DragMode mode)
        {
            if (mode == DragMode.Move) usePosition = true;
            if (mode == DragMode.Rotate) useRotation = true;
        }

        protected override void ChangeWeight(IKHandle.DragMode mode, float amount)
        {

        }


        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (usePosition) target.position = transform.position;
            if (useRotation) target.rotation = transform.rotation;
        }

        public override bool IsValid
        {
            get { return transform; }
        }

        public override bool Rotatable
        {
            get { return true; }
        }

        public static TransformHandle Create(Transform target, IllusionCamera camera)
        {
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<TransformHandle>();
            handle.transform.localScale *= 0.05f;

            handle.camera = camera;
            handle.target = target;

            return handle;
        }

    }
}
