using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HaremOVR
{

    public static class GameObjectExt {

        public static IEnumerable<GameObject> Children(this GameObject gameObject)
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            queue.Enqueue(gameObject);

            while (queue.Count > 0)
            {
                var obj = queue.Dequeue();

                yield return obj;

                // Enqueue children
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    queue.Enqueue(obj.transform.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        /// Makes a breadth-first search for a gameObject with a tag.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static IEnumerable<GameObject> FindGameObjectsByTag(this GameObject gameObject, string tag)
        {
            return gameObject.Children().Where(child => child.CompareTag(tag));
        }

        public static GameObject FIndGameObjectByTag(this GameObject gameObject, string tag)
        {
            return gameObject.FindGameObjectsByTag(tag).FirstOrDefault();
        }
    }
}
