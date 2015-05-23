using RootMotion.FinalIK;
using System;
using UnityEngine;

namespace MaestroMode
{
    public class EffectorHandle : IKHandle
    {
        public IKEffector effector;

        protected override void OnReset()
        {
            effector.target = transform;
            effector.positionWeight = 0;
            effector.rotationWeight = 0;
        }

        protected override void CopyTransform()
        {
            transform.position = effector.bone.position;
            transform.rotation = effector.bone.rotation;
        }

        protected override void Activate(IKHandle.DragMode mode)
        {
            effector.target = transform;
            effector.positionWeight = 1;
            effector.rotationWeight = 1;

        }

        public static EffectorHandle Create(IKEffector effector, IllusionCamera camera)
        {
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<EffectorHandle>();
            handle.transform.localScale *= 0.1f;

            handle.camera = camera;
            handle.effector = effector;
            effector.maintainRelativePositionWeight = 1;

            return handle;
        }


        public override bool IsValid
        {
            get { return effector.bone; }
        }

        protected override void ChangeWeight(IKHandle.DragMode mode, float amount)
        {
            if (mode == DragMode.Move)
            {
                effector.positionWeight = Mathf.Clamp01(effector.positionWeight + amount);
            }
            else if(mode == DragMode.Rotate)
            {
                effector.rotationWeight = Mathf.Clamp01(effector.rotationWeight + amount);
            }
        }

        public override bool Rotatable
        {
            get { return effector.isEndEffector; }
        }
    }

}
