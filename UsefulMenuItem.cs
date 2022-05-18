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
                Debug.Log("\"" + Selection.activeGameObject.name + "\" add rigid done!");
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

        [MenuItem("GameObject/Add Human Component/Add Trigger", false, 2000)]
        public static void AddTrigger()
        {
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                Collider c = go.GetComponent<Collider>();
                if (c)
                    c.isTrigger = true;
                else
                    go.AddComponent<BoxCollider>().isTrigger = true;
                go.layer = 10;
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr)
                    mr.enabled = false;
                go.AddComponent<TriggerVolume>();
                Debug.Log("\"" + Selection.activeGameObject.name + "\" add trigger done!");
            }
        }

        [MenuItem("GameObject/Add Human Component/Add Label Trigger", false, 2000)]
        public static void AddLabelTrigger()
        {
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                Collider c = go.GetComponent<Collider>();
                if (c)
                    c.isTrigger = true;
                else
                    go.AddComponent<BoxCollider>().isTrigger = true;
                go.layer = 10;
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr)
                    mr.enabled = false;
                go.AddComponent<ColliderLabelTriggerVolume>();
                Debug.Log("\"" + Selection.activeGameObject.name + "\" add label trigger done!");
            }
        }

        [MenuItem("GameObject/Add Human Component/Add Linear Motor", false, 2000)]
        public static void AddLinearMotor()
        {
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                GameObject axis = new GameObject("motor");
                axis.transform.parent = go.transform;
                axis.transform.localPosition = Vector3.zero;
                LinearJoint lj = axis.AddComponent<LinearJoint>();
                lj.minValue = 0;
                lj.maxValue = 5;
                lj.body = go.transform;
                lj.axis = axis.transform;
                ServoMotor sm = axis.AddComponent<ServoMotor>();
                sm.joint = lj;
                sm.power.initialValue = 1;
                if (!go.GetComponent<Rigidbody>())
                    go.AddComponent<Rigidbody>().isKinematic = true;
                else
                    go.GetComponent<Rigidbody>().isKinematic = true;
                if (!go.GetComponent<NetBody>())
                    go.AddComponent<NetBody>();
                Debug.Log("\"" + Selection.activeGameObject.name + "\" add linear motor done!");
            }
        }

        [MenuItem("GameObject/Add Human Component/Add Angular Motor", false, 2000)]
        public static void AddAngularMotor()
        {
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                GameObject axis = new GameObject("motor");
                axis.transform.parent = go.transform;
                axis.transform.localPosition = Vector3.zero;
                AngularJoint lj = axis.AddComponent<AngularJoint>();
                lj.minValue = 0;
                lj.maxValue = 90;
                lj.body = go.transform;
                lj.axis = axis.transform;
                ServoMotor sm = axis.AddComponent<ServoMotor>();
                sm.joint = lj;
                sm.power.initialValue = 1;
                if (!go.GetComponent<Rigidbody>())
                    go.AddComponent<Rigidbody>().isKinematic = true;
                else
                    go.GetComponent<Rigidbody>().isKinematic = true;
                if (!go.GetComponent<NetBody>())
                    go.AddComponent<NetBody>();
                Debug.Log("\"" + Selection.activeGameObject.name + "\" add angular motor done!");
            }
        }
        [MenuItem("GameObject/Add Human Component/Add Fall Trigger", false, 2000)]
        public static void AddFallTrigger()
        {
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                Collider c = go.GetComponent<Collider>();
                if (c)
                    c.isTrigger = true;
                else
                    go.AddComponent<BoxCollider>().isTrigger = true;
                go.layer = 10;
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr)
                    mr.enabled = false;
                go.AddComponent<FallTrigger>();
                Debug.Log("\"" + Selection.activeGameObject.name + "\" add fall trigger done!");
            }
        }
        [MenuItem("GameObject/Add Human Component/Add Checkpoint", false, 2000)]
        public static void AddCheckpoint()
        {
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                Collider c = go.GetComponent<Collider>();
                if (c)
                    c.isTrigger = true;
                else
                    go.AddComponent<BoxCollider>().isTrigger = true;
                go.layer = 10;
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr)
                    mr.enabled = false;
                Checkpoint[] cks = FindObjectsOfType<Checkpoint>();
                List<int> num = new List<int>();
                foreach (var item in cks)
                    num.Add(item.number);
                num.Sort();//从小到大
                go.AddComponent<Checkpoint>().number = num[num.Count - 1] + 1;
                Debug.Log("\"" + Selection.activeGameObject.name + "\" add checkpoint done!");
            }
        }
        [MenuItem("GameObject/Snap to Grid %q", false, -1)]
        public static void Snap2Grid()
        {
            GameObject go = Selection.activeGameObject;
            if (go != null)
            {
                float x = Mathf.Round(go.transform.position.x);
                float y = Mathf.Round(go.transform.position.y);
                float z = Mathf.Round(go.transform.position.z);
                go.transform.position = new Vector3(x, y, z);
            }
        }
    }
}