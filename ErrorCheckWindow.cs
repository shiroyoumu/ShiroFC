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
		[MenuItem("Human/Check Net Error %&x", false, 2003)]
		/// <summary>
		/// 打开窗口
		/// </summary>
		static void ShowWindow()
		{
			EditorWindow win = GetWindow<ErrorCheckWindow>();
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
			if (listNetBody.Count > 0)
			{
				foldBool[0].target = EditorGUILayout.Foldout(foldBool[0].target, $"{listNetBody.Count}个：该分类下物体具有RigidBody但是缺少Net Body");
				if (EditorGUILayout.BeginFadeGroup(foldBool[0].faded))
				{
					foreach (var item in listNetBody)
					{
						EditorGUILayout.ObjectField(item, typeof(GameObject), true);
						GUILayout.Space(3);
					}
				}
				EditorGUILayout.EndFadeGroup();
			}			
			if (listNetSignal.Count > 0)
			{
				foldBool[1].target = EditorGUILayout.Foldout(foldBool[1].target, $"{listNetSignal.Count}个：该分类下的组件其输入端路径上缺少Net Signal");
				if (EditorGUILayout.BeginFadeGroup(foldBool[1].faded))
				{
					foreach (var item in listNetSignal)
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
			if (listLevelParts.Count > 0)
			{
				foldBool[2].target = EditorGUILayout.Foldout(foldBool[2].target, $"{listLevelParts.Count}个：该分类下的组件其序号或ID重复");
				if (EditorGUILayout.BeginFadeGroup(foldBool[2].faded))
				{
					foreach (var item in listLevelParts)
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
			//报错项目毕
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(240));
			ifCkRigid = EditorGUILayout.Toggle("检查刚体Net body", ifCkRigid);
			ifCkSignal = EditorGUILayout.Toggle("检查事件前Net Signal", ifCkSignal);
			ifCkCkeckpoint = EditorGUILayout.Toggle("检查Checkpoint和Net Scene", ifCkCkeckpoint);
			if (GUILayout.Button("检    查") && level)
				Check();
			GUILayout.Space(10);
			if (GUILayout.Button("为所有刚体添加Net Body", GUILayout.Width(240)))
				ErrorCheck.AddAllNetBody();

			GUILayout.Space(10);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUILayout.TextArea(log, new GUILayoutOption[] { GUILayout.Height(30) });
		}

		/// <summary>
		/// 检查问题
		/// </summary>
		void Check()
		{
			listNetBody.Clear();
			listNetSignal.Clear();
			listLevelParts.Clear();
			if (ifCkRigid)
			{
				listNetBody = ErrorCheck.CheckNetbody(level);
			}
			if (ifCkSignal)
			{
				listNetSignal = ErrorCheck.CheckNetSignal();
			}
			if (ifCkCkeckpoint)
			{
				listLevelParts = ErrorCheck.CheckLevelParts();
			}
			int n = listNetBody.Count + listNetSignal.Count + listLevelParts.Count;
			if (n == 0)
				log = "没有发现问题";
			else
				log = $"一共存在 {n} 个问题";
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
		List<GameObject> listNetBody = new List<GameObject>();
		List<Node> listNetSignal = new List<Node>();
		List<ErrorCheck.ComponentPair> listLevelParts = new List<ErrorCheck.ComponentPair>();
		Vector2 scrollPos;
		public static string log;
	}
}