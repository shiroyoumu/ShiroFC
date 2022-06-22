using HumanAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodeGraphEx))]
public class NodeGraphExEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        if (GUILayout.Button("Show Graph Ex"))
        {
            NodeWindowEx.Init((target as NodeGraphEx).GetComponent<NodeGraph>());
        }
        GUILayout.Space(10);
    }
}
