using HumanAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodeGraph))]
public class NodeGraphEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.DrawDefaultInspector();
		if (GUILayout.Button("Show Graph", new GUILayoutOption[0]))
		{
			NodeWindow.Init(base.target as NodeGraph);
		}
		if (GUILayout.Button("Show Graph Ex", new GUILayoutOption[0]))
		{
			NodeWindowEx.Init(base.target as NodeGraph);
		}
	}
}