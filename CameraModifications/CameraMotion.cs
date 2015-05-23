using IllusionPlugin;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CameraModifications
{
    internal class CameraMotion
    {
        private static readonly float MIN_DURATION = ModPrefs.GetFloat("CameraMotions", "fMinDuration", 3f, true);
        private static readonly float MAX_DURATION = ModPrefs.GetFloat("CameraMotions", "fMaxDuration", 6f, true);
        private static readonly float MIN_SPEED = ModPrefs.GetFloat("CameraMotions", "fMinDrawSpeed", -0.05f, true);
        private static readonly float MAX_SPEED = ModPrefs.GetFloat("CameraMotions", "fMaxDrawSpeed", 0.3f, true);
        private static readonly float MAX_SWING_SPEED = ModPrefs.GetFloat("CameraMotions", "fMaxSwingSpeed", 5f, true);
        private static readonly float MIN_DISTANCE = ModPrefs.GetFloat("CameraMotions", "fMinDistance", 1f, true);
        private static readonly float MAX_DISTANCE = ModPrefs.GetFloat("CameraMotions", "fMaxDistance", 2f, true);


        public Vector3 Focus = Vector3.zero;
        public float Distance = 1f;
        public Vector3 Rotation = Vector3.zero;

        public Vector3 CameraSpeed = Vector3.zero;
        public float Speed = 0f;

        public float Duration = 5f;

        public CameraMotion(Human human)
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    Init(human.genitalsPos.position, -human.genitalsPos.forward);
                    break;
                case 1:
                    Init(human.headPos.position, -human.headPos.forward);
                    break;
                case 2:
                    Init((human.genitalsPos.position + (human.headPos.position - human.genitalsPos.position) * Random.value), -human.transform.forward);
                    break;
            }
        }

        public CameraMotion(Vector3 focus)
        {
            Init(focus, Vector3.forward);
        }

        private void Init(Vector3 focus, Vector3 favoredDirection)
        {
            Focus = focus;
            Rotation = (Quaternion.LookRotation(favoredDirection) * Quaternion.Euler(Random.Range(0, 3) < 3 ? Random.insideUnitSphere * 30 : Random.insideUnitSphere * 180)).eulerAngles;
            Distance = Random.Range(MIN_DISTANCE, MAX_DISTANCE);

            CameraSpeed = Random.insideUnitSphere * MAX_SWING_SPEED;
            Speed = Random.Range(MIN_SPEED, MAX_SPEED);

            Duration = Random.Range(MIN_DURATION, MAX_DURATION);
        }
    }
}
