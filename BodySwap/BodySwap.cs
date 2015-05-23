using IllusionPlugin;
using RootMotion;
using RootMotion.FinalIK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;

namespace BodySwap
{
    public class SyncedBone : MonoBehaviour
    {
        public Transform referenceBone;
        public bool fullSync = true;
        public void Start()
        {
           
        }

        public void Update()
        {
            if (referenceBone)
            {
                if (fullSync)
                    transform.position = referenceBone.position;
                transform.rotation = referenceBone.rotation;
                //transform.localScale = referenceBone.localScale;
            }
        }
    }

    public class SyncedAnimator : MonoBehaviour
    {
        public Animator referenceAnimator;
        private Animator m_animator;
        public void Start()
        {
            m_animator = gameObject.AddComponent<Animator>();
        }
        public void Update()
        {
            if (referenceAnimator)
            {
                m_animator.GetCopyOf(referenceAnimator);
            }
        }

    }

    public class BodySwap : IPlugin
    {
        private bool m_active = false;
        private bool m_swapped = false;
        private Type[] allowedTypes = new Type[] {
            typeof(Transform),
            typeof(Renderer),
            typeof(Collider),
            typeof(MeshFilter),
            typeof(BetweenTransform),
            typeof(DynamicBoneCollider_Custom),
            typeof(DynamicBoneCollider),
            typeof(DynamicBone),
            typeof(DynamicBone_Custom),
            typeof(Human),
            typeof(MeshBlend),
            typeof(LimbIK),
            typeof(SyncBoneWeight),

            //typeof(Animator)
        };

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

        private class MyBipedIK : FullBodyBipedIK
        {
            protected override void UpdateSolver()
            {
                try
                {
                    base.UpdateSolver();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        private FullBodyBipedIK SetUpBiped(Transform root)
        {
            var ik = root.gameObject.AddComponent<MyBipedIK>();
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

            Console.WriteLine("1: " + ik.references.head);
            Console.WriteLine("2: " + ik.references.leftCalf);
            Console.WriteLine("3: " + ik.references.leftFoot);
            Console.WriteLine("4: " + ik.references.leftForearm);
            Console.WriteLine("5: " + ik.references.leftHand);
            Console.WriteLine("6: " + ik.references.leftThigh);
            Console.WriteLine("7: " + ik.references.leftUpperArm);
            Console.WriteLine("8: " + ik.references.pelvis);
            Console.WriteLine("9: " + ik.references.rightCalf);
            Console.WriteLine("10: " + ik.references.rightFoot);
            Console.WriteLine("11: " + ik.references.rightForearm);
            Console.WriteLine("12: " + ik.references.rightHand);
            Console.WriteLine("13: " + ik.references.rightThigh);
            Console.WriteLine("14: " + ik.references.rightUpperArm);

            ik.solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(ik.references);
            ik.solver.SetToReferences(ik.references, ik.solver.rootNode);

            return ik;
        }

        private const string danBone = "cm_J_dan_top";

        private GameObject chinkoPrefab;

        public void OnLevelWasInitialized(int level)
        {
            m_active = level == 3;   
            m_swapped = false;
            //if (m_active)
            //{

            //    foreach (var moz in GameObject.FindObjectsOfType<MozUV>())
            //    {
            //        GameObject.DestroyImmediate(moz);
            //       }

            //    var chinko = GameObject.Instantiate(chinkoPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            //    var dankon = GameObject.Find("cm_O_dan00").GetComponentInChildren<SkinnedMeshRenderer>();
            //    var dankon2 = GameObject.Find("cm_O_dan_f").GetComponent<Renderer>();
            //    var go = dankon.gameObject;
            //    var root = dankon.rootBone;

            //    GameObject.DestroyImmediate(dankon);
            //    GameObject.DestroyImmediate(dankon2);

            //    chinko.GetComponentInChildren<SkinnedMeshRenderer>().rootBone = root;
            //    //dankon = go.AddComponent(chinko.GetComponentInChildren<SkinnedMeshRenderer>());
            //    //dankon.rootBone = root;
            //    //dankon.sharedMaterial = chinko.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
            //    //dankon.transform.localPosition += new Vector3(0, 0, 1);
            //}
        }


        public void OnUpdate()
        {
            if (m_active && !m_swapped)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    m_swapped = true;

                    var scene = GameObject.FindObjectOfType<H_Scene>();

                    // SWAP!!
                    var male = scene.Members.First(m => m.sex == Human.SEX.MALE);
                    var female = scene.Members.First(m => m.sex == Human.SEX.FEMALE);


                    var ukeMale = Replace(female, male, false, false);
                    var semeFemale = Replace(male, female, false, true);

                    foreach (var renderer in female.GetComponentsInChildren<Renderer>())
                        renderer.enabled = false;
                    foreach (var renderer in male.GetComponentsInChildren<Renderer>())
                        renderer.enabled = false;

                    //chinko.transform.localPosition = new Vector3(0, 0.08f, -0.71f);
                    //chinko.GetComponentInChildren<SkinnedMeshRenderer>().rootBone = GameObject.Find("N_cm_dan").transform;
                    //chinko.transform.localRotation = 
                    //chinko.name = danBone;

                    //// Hide male
                    //foreach (var transform in male.GetComponentsInChildren<Renderer>())
                    //{
                    //    transform.enabled = false;
                    //    //transform.gameObject.layer = LayerMask.NameToLayer("");
                    //}

                    //// Duplicate female
                    //var futanari = GameObject.Instantiate(female.gameObject) as GameObject;
                    
                    //// Make  chinko
                    //var kokan = futanari.GetComponentsInChildren<Transform>().First(transform => transform.name == "cf_J_Kokan");
                    //var chinko = GameObject.Instantiate(chinkoPrefab, Vector3.zero, Quaternion.identity) as GameObject;

                    //chinko.transform.SetParent(kokan, false);
                    ////chinko.transform.localPosition = new Vector3(0, 0.08f, -0.71f);
                    ////chinko.GetComponentInChildren<SkinnedMeshRenderer>().rootBone = GameObject.Find("N_cm_dan").transform;
                    ////chinko.transform.localRotation = 
                    ////chinko.name = danBone;

                   
                    //Console.WriteLine("YAY");
                    //foreach (var component in futanari.GetComponentsInChildren<Component>())
                    //{
                    //    bool allowed = false;
                    //    foreach (var type in allowedTypes)
                    //    {
                    //        if (component.GetType() == type || component.GetType().IsSubclassOf(type) || component.GetType().Name == "BustSizeMover")
                    //        {
                    //            if (component.GetType().Name == "BustSizeMover") Console.WriteLine("YATTTA");
                    //            allowed = true;
                    //            break;
                    //        }
                    //    }

                    //    //if (component.name == "cf_body_01") allowed = true;

                    //    if (!allowed)
                    //        GameObject.DestroyImmediate(component);
                    //}
                    //Console.WriteLine("YAY2");


                    //var offset = male.transform.GetChild(0).position - futanari.transform.GetChild(0).position;

                    //foreach (var bone in futanari.GetComponentsInChildren<Transform>()
                    //        .Where(transform => transform.name.StartsWith("cf_") || transform.name.StartsWith("cm_"))
                    //        .Where(transform => !transform.name.Contains("Mune") && !transform.name.Contains("mune")))
                    //{
                    //    var maleBone = male.GetComponentsInChildren<Transform>().FirstOrDefault(transform => transform.name == bone.name.Replace("cf_", "cm_"));

                    //    if (maleBone)
                    //    {
                    //        //Console.WriteLine("FOUND: {0}", maleBone.name);
                    //        var syncedBone = bone.gameObject.AddComponent<SyncedBone>();
                    //        syncedBone.fullSync = bone.name.Contains("Leg")
                    //                            || bone.name == "cf_J_Hips"
                    //                            || bone.name == "cf_J_Kokan"
                    //                            || bone.name.StartsWith("cm_")
                    //                            || bone.name == danBone;

                    //        syncedBone.referenceBone = maleBone;
                    //    }
                    //}

                    //var rootBone = futanari.AddComponent<SyncedBone>();
                    //rootBone.referenceBone = male.transform;
                    //rootBone.fullSync = true;

                    ////var anim = futanari.transform.GetChild(0).gameObject.AddComponent<SyncedAnimator>();
                    ////anim.referenceAnimator = female.animator;
                }
            }
        }

        public string Name
        {
            get { return "BodySwap"; }
        }

        public string Version
        {
            get { return "0.0.0"; }
        }
        public void OnApplicationStart()
        {
            var assetBundle = AssetBundle.CreateFromMemoryImmediate(Resources.chinko2);
            chinkoPrefab = assetBundle.mainAsset as GameObject;
            assetBundle.Unload(false);
        }

        public GameObject Replace(Human target, Human source, bool autoHide = true, bool attachChinko = false)
        {
            // Hide target
            if (autoHide)
            {
                foreach (var transform in target.GetComponentsInChildren<Renderer>())
                {
                    transform.enabled = false;
                    //transform.gameObject.layer = LayerMask.NameToLayer("");
                }
            }

            // Duplicate source
            var doppelganger = GameObject.Instantiate(source.gameObject) as GameObject;

            // Make  chinko
            /*var kokan = doppelganger.GetComponentsInChildren<Transform>().First(transform => transform.name == "cf_J_Kokan");
            var chinko = GameObject.Instantiate(chinkoPrefab, Vector3.zero, Quaternion.identity) as GameObject;

            chinko.transform.SetParent(kokan, false);*/

            //chinko.transform.localPosition = new Vector3(0, 0.08f, -0.71f);
            //chinko.GetComponentInChildren<SkinnedMeshRenderer>().rootBone = GameObject.Find("N_cm_dan").transform;
            //chinko.transform.localRotation = 
            //chinko.name = danBone;

            // Make  chinko
            if (attachChinko)
            {
                var kokan = doppelganger.GetComponentsInChildren<Transform>().First(transform => Regex.IsMatch(transform.name, "^c[fm]_J_Kokan$"));
                var chinko = GameObject.Instantiate(chinkoPrefab, Vector3.zero, Quaternion.identity) as GameObject;

                chinko.transform.SetParent(kokan, false);
            }


            foreach (var component in doppelganger.GetComponentsInChildren<Component>())
            {
                bool allowed = false;
                foreach (var type in allowedTypes)
                {
                    if (component.GetType() == type || component.GetType().IsSubclassOf(type) || component.GetType().Name == "BustSizeMover")
                    {
                        allowed = true;
                        break;
                    }
                }

                //if (component.name == "cf_body_01") allowed = true;

                if (!allowed)
                    GameObject.DestroyImmediate(component);
            }


            var ik = SetUpBiped(doppelganger.transform.GetChild(0));

            //{ HumanBodyBones.Head, "Head" },
            //{ HumanBodyBones.LeftUpperLeg, "LegUp00_L" },
            //{ HumanBodyBones.LeftLowerLeg, "LegLow01_L"},
            //{ HumanBodyBones.LeftFoot, "Foot01_L"},
            //{ HumanBodyBones.RightUpperLeg, "LegUp00_R" },
            //{ HumanBodyBones.RightLowerLeg, "LegLow01_R"},
            //{ HumanBodyBones.RightFoot, "Foot01_R"},
            //{ HumanBodyBones.LeftLowerArm, "ArmLow01_L"},
            //{ HumanBodyBones.RightLowerArm, "ArmLow01_R"},
            //{ HumanBodyBones.LeftHand, "Hand_L"},
            //{ HumanBodyBones.RightHand, "Hand_R"},
            //{ HumanBodyBones.LeftUpperArm, "ArmUp00_L"},
            //{ HumanBodyBones.RightUpperArm, "ArmUp00_R"},
            //{ HumanBodyBones.Hips, "Hips" }


            Dictionary<string, FullBodyBipedEffector> bones = new Dictionary<string, FullBodyBipedEffector>() { 
                { "Hand_L", FullBodyBipedEffector.LeftHand },
                { "Hand_R", FullBodyBipedEffector.RightHand }, 
                { "Foot01_L", FullBodyBipedEffector.LeftFoot }, 
                { "Foot01_R", FullBodyBipedEffector.RightFoot },
                { "LegUp00_L", FullBodyBipedEffector.LeftThigh }, 
                { "LegUp00_R", FullBodyBipedEffector.RightThigh },
                { "ArmUp00_R", FullBodyBipedEffector.RightShoulder},
                { "ArmUp00_L", FullBodyBipedEffector.LeftShoulder },
                //{ "Spine01", FullBodyBipedEffector.Body }

            };

            foreach (var bone in target.transform.GetChild(0).GetComponentsInChildren<Transform>().Where(t => t.name.Length > 5 && bones.Keys.Contains(t.name.Substring(5))))
            {
                var effectorName = bones[bone.name.Substring(5)];
                var effector = ik.solver.GetEffector(effectorName);
                
                Console.WriteLine("Effector {0}: {1}", bone.name, effector.bone);
                //var syncedBone = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<SyncedBone>();
                //syncedBone.fullSync = true;
                //syncedBone.referenceBone = bone;

                //effector.target = syncedBone.transform;
                effector.target = bone;
                effector.positionWeight = 1f;
                effector.rotationWeight = 1f;
                effector.maintainRelativePositionWeight = 1;
                //effector.SetToTarget();
            }



            foreach (var bone in doppelganger.GetComponentsInChildren<Transform>()
                    .Where(transform => Regex.IsMatch(transform.name, @"^c[fm]_")  )
                    .Where(transform => !transform.name.Contains("Mune") && !transform.name.Contains("mune")))
            {
                var targetBone = target.GetComponentsInChildren<Transform>().Where(transform => transform.name.Length > 2).FirstOrDefault(transform => transform.name.Substring(2) == bone.name.Substring(2));

                if (targetBone)
                {
                    Debug.Log("OK");
                    //Console.WriteLine("FOUND: {0}", maleBone.name);
                    var syncedBone = bone.gameObject.AddComponent<SyncedBone>();
                    syncedBone.fullSync = //bone.name.Contains("Leg")
                                        Regex.IsMatch(bone.name, "^c[fm]_J_Hip")
                                        || Regex.IsMatch(bone.name, "^c[fm]_J_Kokan")
                                        || bone.name.StartsWith("cm_J_dan")
                        //|| bone.name.StartsWith("cm_")
                                        || bone.name == danBone;

                    syncedBone.referenceBone = targetBone;

                    if (targetBone.name.EndsWith("ArmLow01_R"))
                    {
                        var constraint = ik.solver.GetBendConstraint(FullBodyBipedChain.RightArm);
                        constraint.bendGoal = targetBone;
                        constraint.weight = 0.8f;
                    }

                    if (targetBone.name.EndsWith("ArmLow01_L"))
                    {
                        var constraint = ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm);
                        constraint.bendGoal = targetBone;
                        constraint.weight = 0.8f;
                    }
                    if (targetBone.name.EndsWith("LegLow01_L"))
                    {
                        var constraint = ik.solver.GetBendConstraint(FullBodyBipedChain.LeftLeg);
                        constraint.bendGoal = targetBone;
                        constraint.weight = 0.8f;
                    }
                    if (targetBone.name.EndsWith("LegLow01_R"))
                    {
                        var constraint = ik.solver.GetBendConstraint(FullBodyBipedChain.RightLeg);
                        constraint.bendGoal = targetBone;
                        constraint.weight = 0.8f;
                    }
                }
            }

            var rootBone = doppelganger.AddComponent<SyncedBone>();
            rootBone.referenceBone = target.transform;
            rootBone.fullSync = true;

            return doppelganger;
        }

        #region Stubs



        public void OnApplicationQuit()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }
        public void OnFixedUpdate()
        {
        }

        #endregion

    }
}
