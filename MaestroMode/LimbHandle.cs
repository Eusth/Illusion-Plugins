using RootMotion.FinalIK;
using System;
using UnityEngine;

namespace MaestroMode
{
    public class LimbHandle : IKHandle
    {
        public IKSolverLimb limb;

        protected override void OnReset()
        {
            limb.target = transform;
            limb.IKPositionWeight = 0;
            limb.IKRotationWeight = 0;
        }

        protected override void CopyTransform()
        {
            transform.position = limb.bone3.transform.position;
            transform.rotation = limb.bone3.transform.rotation;
        }

        public override void Activate(IKHandle.DragMode mode)
        {
            limb.target = transform;
            limb.IKPositionWeight = 1;
            limb.IKRotationWeight = 1;
        }

        public static LimbHandle Create(IKSolverLimb solver, IllusionCamera camera)
        {
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<LimbHandle>();
            handle.transform.localScale *= 0.1f;

            handle.camera = camera;
            handle.limb = solver;

            return handle;
        }


        public override bool IsValid
        {
            get { return limb.bone1.transform; }
        }

        protected override void ChangeWeight(IKHandle.DragMode mode, float amount)
        {
            if (mode == DragMode.Move)
            {
                limb.IKPositionWeight = Mathf.Clamp01(limb.IKPositionWeight + amount);
            }
            else if(mode == DragMode.Rotate)
            {
                limb.IKRotationWeight = Mathf.Clamp01(limb.IKRotationWeight + amount);
            }
        }

        public override bool Rotatable
        {
            get { return true; }
        }
    }

}
