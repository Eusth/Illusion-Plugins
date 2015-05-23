using HaremOVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace StudioVR
{
    class ShisenCorrecter : MonoBehaviour
    {
        StudioUIController uiController = null;
        private FieldInfo eyeLookCtrlInfo;
        private FieldInfo neckLookCtrlInfo;

        void Start()
        {
            uiController = GameObject.FindObjectOfType<StudioUIController>();

            eyeLookCtrlInfo = typeof(CharFemale).GetField("eyeLookCtrl", BindingFlags.NonPublic | BindingFlags.Instance);
            neckLookCtrlInfo = typeof(CharFemale).GetField("neckLookCtrl", BindingFlags.NonPublic | BindingFlags.Instance);
        }


        void Update()
        {
            foreach (var female in uiController.stdioMain.FemaleList)
            {
                CheckFemale(female);
            }

       //     Console.WriteLine(GameObject.FindObjectsOfType<EyeLookController>().Where(ctrl => !(ctrl is OVREyeLookController)).Count());
        }

        void CheckFemale(CharFemale female)
        {
            // Get controllers
            var eyeLookCtrl = eyeLookCtrlInfo.GetValue(female) as EyeLookController;
            var targetCtrl = female.GetBodyObj().GetComponent<EyeTargetController>();

            // Make a new EyeTargetController if required
            if (targetCtrl == null)
            {
                targetCtrl = female.GetBodyObj().AddComponent<EyeTargetController>();
                targetCtrl.rootNode = eyeLookCtrl.eyeLookScript.rootNode;
            }

            // Replace camera targets if required
            if (eyeLookCtrl.target.camera == Camera.main)
            {
                eyeLookCtrl.target = targetCtrl.Target;
            }

            var neckLookCtrl = neckLookCtrlInfo.GetValue(female) as NeckLookController;
            if (neckLookCtrl.target.camera == Camera.main)
            {
                neckLookCtrl.target = targetCtrl.Target;
            }

        }
    }
}
