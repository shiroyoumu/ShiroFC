using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HumanAPI;
using UnityEditor;
using System;
using Multiplayer;
using UnityEditor.AnimatedValues;

namespace EditorFC
{
	public class ErrorCheckWindow : ScriptableWizard
	{
		[MenuItem("Human/ErrorTTTTTEST %&x", false, 2003)]
		/// <summary>
		/// 打开窗口
		/// </summary>
		static void ShowWindow()
		{
			EditorWindow win = EditorWindow.GetWindow<ErrorCheckWindow>();
			win.titleContent = new GUIContent("检查网络同步问题");
			win.minSize = new Vector2(600, 400);
			win.maxSize = new Vector2(601, 401);
			win.Show();
		}

		void OnEnable()
		{
			getLevelObj();
			InitialFoldBool();
		}

		void OnGUI()
		{
			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(350));
			GUILayout.Label("问题报告");
			GUILayout.Space(3);
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			//报错项目开始
			nError = 0;
			if (ifCkRigid)
			{
				List<GameObject> list = ErrorCheck.CheckNetbody(level);
				if (list.Count > 0)
				{
					nError += list.Count;
					foldBool[0].target = EditorGUILayout.Foldout(foldBool[0].target, $"{list.Count}个：该分类下物体具有RigidBody但是缺少Net Body");
					if (EditorGUILayout.BeginFadeGroup(foldBool[0].faded))
					{
						foreach (var item in list)
						{
							EditorGUILayout.ObjectField(item, typeof(GameObject), true);
							GUILayout.Space(3);
						}
					}
					EditorGUILayout.EndFadeGroup();
				}
			}
			if (ifCkSignal)
			{
				List<Node> list = ErrorCheck.CheckNetSignal();
				if (list.Count > 0)
				{
					nError += list.Count;
					foldBool[1].target = EditorGUILayout.Foldout(foldBool[1].target, $"{list.Count}个：该分类下的组件其输入端路径上缺少Net Signal");
					if (EditorGUILayout.BeginFadeGroup(foldBool[1].faded))
					{
						foreach (var item in list)
						{
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.ColorField(item.nodeColour, GUILayout.Width(50));
							EditorGUILayout.ObjectField(item, item.GetType(), true);
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(3);
						}
					}
					EditorGUILayout.EndFadeGroup();
				}
				List<NodeGraph> list2 = ErrorCheck.CheckNetSignalInGraph();
				if (list2.Count > 0)
				{
					nError += list.Count;
					foldBool[2].target = EditorGUILayout.Foldout(foldBool[2].target, $"{list2.Count}个：该分类下节点图的输出端前并不是都接有Net Signal");
					if (EditorGUILayout.BeginFadeGroup(foldBool[2].faded))
					{
						foreach (var item in list2)
						{
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.ColorField(item.nodeColour, GUILayout.Width(50));
							EditorGUILayout.ObjectField(item, item.GetType(), true);
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(3);
						}
					}
					EditorGUILayout.EndFadeGroup();
				}
			}
			if (ifCkCkeckpoint)
			{
				List<ErrorCheck.ComponentPair> list = ErrorCheck.CheckLevelParts();
				if (list.Count > 0)
				{
					nError += list.Count;
					foldBool[3].target = EditorGUILayout.Foldout(foldBool[3].target, $"{list.Count}个：该分类下的组件其序号或ID重复");
					if (EditorGUILayout.BeginFadeGroup(foldBool[3].faded))
					{
						foreach (var item in list)
						{
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.ObjectField(item.c1, item.c1.GetType(), true);
							EditorGUILayout.ObjectField(item.c2, item.c2.GetType(), true);
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(3);
						}
					}
					EditorGUILayout.EndFadeGroup();
				}
			}
			//报错项目毕
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(240));
			ifCkRigid = EditorGUILayout.Toggle("检查刚体Net body", ifCkRigid);
			ifCkSignal = EditorGUILayout.Toggle("检查事件前Net Signal", ifCkSignal);
			ifCkCkeckpoint = EditorGUILayout.Toggle("检查Checkpoint和Net Scene", ifCkCkeckpoint);
			GUILayout.Space(10);
			if (GUILayout.Button("为所有刚体添加Net Body", GUILayout.Width(240)))
				ErrorCheck.AddAllNetBody();

			GUILayout.Space(10);

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			//log = $"检测到 {nError} 个问题";
			GUILayout.TextArea(log, new GUILayoutOption[] { GUILayout.Height(30) });
		}

		void InitialFoldBool()
		{
			foldBool = new AnimBool[10]
			{
			new AnimBool(true),
			new AnimBool(true),
			new AnimBool(true),
			new AnimBool(true),
			new AnimBool(true),
			new AnimBool(true),
			new AnimBool(true),
			new AnimBool(true),
			new AnimBool(true),
			new AnimBool(true),
			};
			foreach (var item in foldBool)
			{
				item.valueChanged.AddListener(Repaint);
			}
		}

		void getLevelObj()
		{
			try
			{
				ErrorCheckWindow.level = GameObject.FindObjectOfType<BuiltinLevel>().gameObject;
			}
			catch (NullReferenceException)
			{
				log = "Level物体未找到！";
				return;
			}
		}

		public static GameObject level;
		AnimBool[] foldBool;
		public bool ifCkRigid = true;
		public bool ifCkSignal = true;
		public bool ifCkCkeckpoint = true;
		bool isCheck;
		int nError = 0;
		Vector2 scrollPos;
		public static string log;
	}
}