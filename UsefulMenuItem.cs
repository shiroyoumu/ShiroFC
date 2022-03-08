using HumanAPI;
using Multiplayer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace EditorFC
{
    public class UsefulMenuItem : Editor
    {
        [MenuItem("GameObject/Add Human Component/Add Rigid", false, -1)]
        public static void AddRigid()
        {
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                if (!go.GetComponent<Rigidbody>())
                    go.AddComponent<Rigidbody>();
                if (!go.GetComponent<NetBody>())
                    go.AddComponent<NetBody>();
                if (!go.GetComponent<CollisionAudioSensor>())
                    go.AddComponent<CollisionAudioSensor>();
                Debug.Log("\"" + Selection.activeGameObject.name + "\" rigid ready!");
            }
        }

        [MenuItem("Human/Up Hierarchy %&u", false, 2000)]
        public static void MoveUpHier()
        {
            GameObject go = Selection.activeGameObject;
            if (go != null && go.transform.parent.transform.parent)
            {
                go.transform.parent = go.transform.parent.transform.parent;
            }
        }

    }
}