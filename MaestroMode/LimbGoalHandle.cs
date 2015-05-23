using RootMotion.FinalIK;
using UnityEngine;

namespace MaestroMode
{
    public class LimbGoalHandle : IKHandle
    {
        public IKSolverLimb constraint;

        protected override void OnReset()
        {

            constraint.bendGoal = transform;
            constraint.bendModifierWeight = 0;
        }

        protected override void CopyTransform()
        {
            transform.position = constraint.bone2.transform.position;
            transform.rotation = constraint.bone2.transform.rotation;
        }

        protected override void Activate(IKHandle.DragMode mode)
        {
            constraint.bendGoal = transform;
            if (mode == DragMode.Move)
            {
                constraint.bendModifierWeight = 1;
            }
        }

        public static LimbGoalHandle Create(IKSolverLimb solver, IllusionCamera camera)
        {
            var handle = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<LimbGoalHandle>();
            handle.transform.localScale *= 0.1f;

            handle.camera = camera;
            handle.constraint = solver;

            return handle;
        }


        public override bool IsValid
        {
            get { return constraint.bone1.transform; }
        }

        protected override void ChangeWeight(IKHandle.DragMode mode, float amount)
        {
            constraint.bendModifierWeight = Mathf.Clamp01(constraint.bendModifierWeight + amount);
        }

        public override bool Rotatable
        {
            get { return false; }
        }
    }

}
