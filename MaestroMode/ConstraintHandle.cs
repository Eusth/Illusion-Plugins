using RootMotion.FinalIK;
using UnityEngine;

namespace MaestroMode
{
    public class ConstraintHandle : IKHandle
    {
        public IKConstraintBend constraint;

        protected override void OnReset()
        {

            constraint.bendGoal = transform;
            constraint.weight = 0;
        }

        protected override void CopyTransform()
        {
            transform.position = constraint.bone2.position;
            transform.rotation = constraint.bone2.rotation;
        }

        protected override void Activate(IKHandle.DragMode mode)
        {
            constraint.bendGoal = transform;
            if (mode == DragMode.Move)
            {
                constraint.weight = 1;
            }
        }

        public static ConstraintHandle Create(IKConstraintBend constraint, IllusionCamera camera)
        {
            var handle = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<ConstraintHandle>();
            handle.transform.localScale *= 0.1f;

            handle.camera = camera;
            handle.constraint = constraint;

            return handle;
        }


        public override bool IsValid
        {
            get { return constraint.bone1; }
        }

        protected override void ChangeWeight(IKHandle.DragMode mode, float amount)
        {
            constraint.weight = Mathf.Clamp01(constraint.weight + amount);
        }

        public override bool Rotatable
        {
            get { return false; }
        }
    }

}
