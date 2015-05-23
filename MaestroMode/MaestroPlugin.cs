using IllusionPlugin;
using RootMotion;
using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MaestroMode
{
    public class MaestroPlugin : IPlugin
    {
        enum MaestroMode
        {
            None,
            FBBIK,
            BIK
        }

        private H_Scene _scene;
        private IllusionCamera _camera;
        private List<IKHandle> _handles = new List<IKHandle>();
        private bool _visible = false;
        private MaestroMode _mode = MaestroMode.None;

        private H_Style _currentStyle;

        private readonly KeyStroke _toggleMaestroKey = new KeyStroke(ModPrefs.GetString("Maestro Mode", "sToggleMaestroKey", "F9", true));
        private readonly KeyStroke _toggleSimpleMaestroKey = new KeyStroke(ModPrefs.GetString("Maestro Mode", "sToggleSimpleMaestroeKey", "F10", true));

        private Dictionary<HumanBodyBones, string> IK_MAPPING = new Dictionary<HumanBodyBones, string>()
        {
            { HumanBodyBones.Head, "Head" },
            { HumanBodyBones.LeftUpperLeg, "LegUp00_L" },
            { HumanBodyBones.LeftLowerLeg, "LegLow01_L"},
            { HumanBodyBones.LeftFoot, "Foot01_L"},
            { HumanBodyBones.RightUpperLeg, "LegUp00_R" },
            { HumanBodyBones.RightLowerLeg, "LegLow01_R"},
            { HumanBodyBones.RightFoot, "Foot01_R"},
            { HumanBodyBones.LeftLowerArm, "ArmLow01_L"},
            { HumanBodyBones.RightLowerArm, "ArmLow01_R"},
            { HumanBodyBones.LeftHand, "Hand_L"},
            { HumanBodyBones.RightHand, "Hand_R"},
            { HumanBodyBones.LeftUpperArm, "ArmUp00_L"},
            { HumanBodyBones.RightUpperArm, "ArmUp00_R"},
            { HumanBodyBones.Hips, "Hips" }
        };

        public string Name
        {
            get { return "Maestro Mode"; }
        }

        public string Version
        {
            get { return "0.3"; }
        }

        public void OnLevelWasLoaded(int level)
        {
            _scene = null;
            _camera = null;
            _handles.Clear();
        }

        public void OnLevelWasInitialized(int level)
        {
            if (level == 3)
            {
                _scene = GameObject.FindObjectOfType<H_Scene>();
                _camera = Camera.main.GetComponent<IllusionCamera>();
            }
        }

        private void CleanScene()
        {
            // Clean
            foreach (var handle in _handles)
            {
                GameObject.Destroy(handle.gameObject);
            }

            foreach (var biped in GameObject.FindObjectsOfType<SolverManager>())
            {
                GameObject.DestroyImmediate(biped);
            }

            _handles.Clear();
        }

        private void MakeBipeds()
        {
            // We're in a scene
            foreach (var human in _scene.Members.Where(member => member.GetComponent<SolverManager>() == null))
            {
                if (_mode == MaestroMode.FBBIK)
                {
                    var biped = SetUpBiped(human);

                    foreach (var effector in biped.solver.effectors)
                    {
                        InitHandle(EffectorHandle.Create(effector, _camera), human);
                    }

                    foreach (var constraintType in Enum.GetValues(typeof(FullBodyBipedChain)).Cast<FullBodyBipedChain>())
                    {
                        InitHandle(ConstraintHandle.Create(biped.solver.GetBendConstraint(constraintType), _camera), human);
                    }

                }
                else if (_mode == MaestroMode.BIK)
                {
                    var biped = SetUpSimpleBiped(human);
                    foreach (var limb in biped.solvers.limbs)
                    {
                        InitHandle(LimbHandle.Create(limb, _camera), human);
                        InitHandle(LimbGoalHandle.Create(limb, _camera), human);
                    }
                }
            }
        }

        public void OnUpdate()
        {
            if (_scene)
            {
                if (_scene.StyleMgr.nowStyle != _currentStyle)
                {
                    _currentStyle = _scene.StyleMgr.nowStyle;
                    CleanScene();

                    if(_mode != MaestroMode.None)
                        MakeBipeds();

                }

                if (_visible)
                {
                   
                    if (Input.GetMouseButtonDown(0))
                    {
                        var handle = GetIKHandle();
                        if (handle) handle.OnMouseDown(0);
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        var handle = GetIKHandle();
                        if (handle) handle.OnMouseDown(1);
                    }
                    else if (Input.GetMouseButtonUp(2))
                    {
                        var handle = GetIKHandle();
                        if (handle) handle.Reset();
                    }
                }

                if (_toggleMaestroKey.Check())
                {
                    if (_mode == MaestroMode.FBBIK)
                    {
                        _visible = !_visible;
                        UpdateHandleVisibility();
                    }
                    else if(_mode == MaestroMode.BIK)
                    {
                        CleanScene();

                        _mode = MaestroMode.FBBIK;
                        _visible = true;

                        MakeBipeds();

                    }
                    else if (_mode == MaestroMode.None)
                    {
                        _visible = true;
                        _mode = MaestroMode.FBBIK;
                        MakeBipeds();
                    }
                }

                if (_toggleSimpleMaestroKey.Check())
                {
                    if (_mode == MaestroMode.BIK)
                    {
                        _visible = !_visible;
                        UpdateHandleVisibility();
                    }
                    else if (_mode == MaestroMode.FBBIK)
                    {
                        CleanScene();

                        _mode = MaestroMode.BIK;
                        _visible = true;

                        MakeBipeds();
                    }
                    else if (_mode == MaestroMode.None)
                    {
                        _visible = true;
                        _mode = MaestroMode.BIK;
                        MakeBipeds();
                    }
                }
            }
        }

        private void UpdateHandleVisibility()
        {
            foreach (var handle in _handles)
            {
                handle.SetVisible(_visible);
            }
        }

        private void InitHandle(IKHandle handle, Human human)
        {
            handle.transform.SetParent(human.transform, false);
            handle.SetVisible(_visible);

            _handles.Add(handle);
        }

        private IKHandle GetIKHandle()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f, LayerMask.GetMask("Chara")))
            {
                var handle = hit.collider.gameObject.GetComponent<IKHandle>();
               
                return handle;
            }
            return null;
        }

        private IKSolverLimb InitLimb(IKSolverLimb limb, IKSolverLimb.BendModifier bendModifier)
        {
            limb.IKPositionWeight = 0;
            limb.IKRotationWeight = 0;
            limb.bendModifier = bendModifier;
           
            return limb;
        }

        private BipedIK SetUpSimpleBiped(Human member)
        {
            var ik = member.gameObject.AddComponent<BipedIK>();
            var root = member.transform.FindChild("cm_body_01");
            if (root == null) root = member.transform.FindChild("cf_body_01");
            if (root == null)
            {
                Console.WriteLine("No entry point found: {0}", member.name);
            }

            ik.references = new BipedReferences();
            ik.references.root = root;

            var spines = new List<Transform>();
            foreach (var transform in root.GetComponentsInChildren<Transform>())
            {
                string name = transform.name;
                if (name.Length > 5)
                {
                    name = name.Substring(5);
                    if (name == IK_MAPPING[HumanBodyBones.Head])
                        ik.references.head = transform;

                    // LEGS
                    else if (name == IK_MAPPING[HumanBodyBones.LeftUpperLeg])
                        ik.references.leftThigh = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.LeftLowerLeg])
                        ik.references.leftCalf = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.LeftFoot])
                        ik.references.leftFoot = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightUpperLeg])
                        ik.references.rightThigh = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightLowerLeg])
                        ik.references.rightCalf = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightFoot])
                        ik.references.rightFoot = transform;

                    // ARMS
                    else if (name == IK_MAPPING[HumanBodyBones.LeftUpperArm])
                        ik.references.leftUpperArm = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.LeftLowerArm])
                        ik.references.leftForearm = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.LeftHand])
                        ik.references.leftHand = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightUpperArm])
                        ik.references.rightUpperArm = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightLowerArm])
                        ik.references.rightForearm = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightHand])
                        ik.references.rightHand = transform;

                    // HIPS
                    else if (name == IK_MAPPING[HumanBodyBones.Hips])
                        ik.references.pelvis = transform;

                    else if (name == "Spine01" || name == "Spine02" || name == "Spine03")
                        spines.Add(transform);

                }
            }

            ik.references.spine = spines.ToArray();

            //Console.WriteLine("1: " + ik.references.head);
            //Console.WriteLine("2: " + ik.references.leftCalf);
            //Console.WriteLine("3: " + ik.references.leftFoot);
            //Console.WriteLine("4: " + ik.references.leftForearm);
            //Console.WriteLine("5: " + ik.references.leftHand);
            //Console.WriteLine("6: " + ik.references.leftThigh);
            //Console.WriteLine("7: " + ik.references.leftUpperArm);
            //Console.WriteLine("8: " + ik.references.pelvis);
            //Console.WriteLine("9: " + ik.references.rightCalf);
            //Console.WriteLine("10: " + ik.references.rightFoot);
            //Console.WriteLine("11: " + ik.references.rightForearm);
            //Console.WriteLine("12: " + ik.references.rightHand);
            //Console.WriteLine("13: " + ik.references.rightThigh);
            //Console.WriteLine("14: " + ik.references.rightUpperArm);

            ik.SetToDefaults();

            foreach (var limb in ik.solvers.limbs)
                InitLimb(limb, IKSolverLimb.BendModifier.Goal);

            return ik;
        }

        private FullBodyBipedIK SetUpBiped(Human member)
        {
            var ik = member.gameObject.AddComponent<FullBodyBipedIK>();
            var root = member.transform.FindChild("cm_body_01");
            if (root == null) root = member.transform.FindChild("cf_body_01");
            if (root == null)
            {
                Console.WriteLine("No entry point found: {0}", member.name);
            }

            ik.references = new BipedReferences();
            ik.references.root = root;

            var spines = new List<Transform>();
            foreach (var transform in root.GetComponentsInChildren<Transform>())
            {
                string name = transform.name;
                if (name.Length > 5)
                {
                    name = name.Substring(5);
                    if (name == IK_MAPPING[HumanBodyBones.Head])
                        ik.references.head = transform;

                    // LEGS
                    else if (name == IK_MAPPING[HumanBodyBones.LeftUpperLeg])
                        ik.references.leftThigh = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.LeftLowerLeg])
                        ik.references.leftCalf = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.LeftFoot])
                        ik.references.leftFoot = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightUpperLeg])
                        ik.references.rightThigh = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightLowerLeg])
                        ik.references.rightCalf = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightFoot])
                        ik.references.rightFoot = transform;

                    // ARMS
                    else if (name == IK_MAPPING[HumanBodyBones.LeftUpperArm])
                        ik.references.leftUpperArm = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.LeftLowerArm])
                        ik.references.leftForearm = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.LeftHand])
                        ik.references.leftHand = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightUpperArm])
                        ik.references.rightUpperArm = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightLowerArm])
                        ik.references.rightForearm = transform;
                    else if (name == IK_MAPPING[HumanBodyBones.RightHand])
                        ik.references.rightHand = transform;

                    // HIPS
                    else if (name == IK_MAPPING[HumanBodyBones.Hips])
                        ik.references.pelvis = transform;

                    else if (name == "Spine01" || name == "Spine02" || name == "Spine03")
                        spines.Add(transform);

                }
            }

            ik.references.spine = spines.ToArray();

            //Console.WriteLine("1: " + ik.references.head);
            //Console.WriteLine("2: " + ik.references.leftCalf);
            //Console.WriteLine("3: " + ik.references.leftFoot);
            //Console.WriteLine("4: " + ik.references.leftForearm);
            //Console.WriteLine("5: " + ik.references.leftHand);
            //Console.WriteLine("6: " + ik.references.leftThigh);
            //Console.WriteLine("7: " + ik.references.leftUpperArm);
            //Console.WriteLine("8: " + ik.references.pelvis);
            //Console.WriteLine("9: " + ik.references.rightCalf);
            //Console.WriteLine("10: " + ik.references.rightFoot);
            //Console.WriteLine("11: " + ik.references.rightForearm);
            //Console.WriteLine("12: " + ik.references.rightHand);
            //Console.WriteLine("13: " + ik.references.rightThigh);
            //Console.WriteLine("14: " + ik.references.rightUpperArm);

            ik.solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(ik.references);
            ik.solver.SetToReferences(ik.references, ik.solver.rootNode);

            foreach (var effector in ik.solver.effectors)
            {
                effector.positionWeight = 0;
                effector.rotationWeight = 0;
            }

            return ik;
        }

        #region Stubs

        public void OnApplicationStart()
        {
        }

        public void OnApplicationQuit()
        {
        }
        public void OnFixedUpdate()
        {
        }

        #endregion
    }
}
