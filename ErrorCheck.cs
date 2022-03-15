using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HumanAPI;
using UnityEditor;
using System;
using Multiplayer;

namespace EditorFC
{
	public class ErrorCheckWindow : EditorWindow
	{
		[MenuItem("Human/Error Check Window %&x", false, 2003)]
		/// <summary>
		/// 打开窗口
		/// </summary>
		static void ShowWindow()
		{
			EditorWindow win = EditorWindow.GetWindow<ErrorCheckWindow>();
			win.titleContent = new GUIContent("检查网络同步问题");
			win.minSize = new Vector2(350, 574);
			win.maxSize = new Vector2(350, 575);
			win.Show();
		}

		void OnEnable()
		{
			getLevelObj();
		}

		void OnGUI()
		{
			GUILayout.Space(5);
			ifCkRigid = EditorGUILayout.Toggle("检查刚体Net body", ifCkRigid);
			ifCkSignal = EditorGUILayout.Toggle("检查事件前Net Signal", ifCkSignal);
			GUILayout.Space(5);
			if (GUILayout.Button("为所有刚体添加Net Body", new GUILayoutOption[0]) && level)
				AddAllNetBody();
			GUILayout.Space(5);
			if (GUILayout.Button("检    查", new GUILayoutOption[0]) && level)
				CheckNetError();
			GUILayout.Label("________________________________________________");
			GUILayout.Space(5);
			GUILayout.Label("问题报告");
			GUILayout.Space(5);
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(345), GUILayout.Height(390));
			if (ErrItemList != null)
			{
				foreach (var item in ErrItemList)
				{
					if (item.c.GetType() == typeof(Rigidbody))
						CreateLogItem((Rigidbody)item.c, item.t);
					else
						CreateLogItem((Node)item.c, item.t);
				}
			}
			EditorGUILayout.EndScrollView();
			GUILayout.Space(5);
			GUILayout.TextArea(UniLog, new GUILayoutOption[] { GUILayout.Height(30) });
		}

		/// <summary>
		/// 为所有刚体添加Net Body
		/// </summary>
		public void AddAllNetBody()
		{
			Rigidbody[] r = level.GetComponentsInChildren<Rigidbody>();
			int i = 0;
            foreach (var item in r)
            {
				if (!item.GetComponent<NetBody>())
				{ 
					item.gameObject.AddComponent<NetBody>();
					i++;
				}	
            }
			if (i == 0)
				UniLog = "没有刚体需要添加Net Body"; 
			else
				UniLog = String.Format("已为 {0} 个刚体添加Net Body", i);
		}

		/// <summary>
		/// 检查网络错误
		/// </summary>
		public void CheckNetError()
		{
			//清空数据
			ErrItemList = new List<ErrItem>();
			nRigid = nSignal = 0;
			//根据要求检错
			if (ifCkRigid)
			{
				CheckI();
			}
			if (ifCkSignal)
			{
				CheckII();
			}
			//输出总览
			if (nRigid + nSignal > 0)
			{
				if (ifCkRigid && ifCkSignal)
					UniLog = string.Format("场景包含 {0} 个刚体问题，包含 {1} 个信号同步问题", nRigid, nSignal);
				else
					if (ifCkRigid)
					UniLog = string.Format("场景包含 {0} 个刚体问题", nRigid);
				else
						if (ifCkSignal)
					UniLog = string.Format("场景包含 {0} 个信号同步问题", nSignal);
			}
			else
			{
				UniLog = "未检测到网络信号同步问题";
			}
		}

		/// <summary>
		/// 找Level物体
		/// </summary>
		void getLevelObj() 
		{
			try
			{
				ErrorCheckWindow.level = GameObject.FindGameObjectWithTag("Level");
				BuiltinLevel level = ErrorCheckWindow.level.GetComponent<BuiltinLevel>();
			}
			catch (NullReferenceException)
			{
				UniLog = "Level物体未找到！";
				return;
			}
		}

		/// <summary>
		/// 检查刚体
		/// </summary>
		void CheckI()
		{
			//收集所有组件
			Rigidbody[] gos = level.GetComponentsInChildren<Rigidbody>();
			foreach (var item in gos)
			{
				if (!item.gameObject.isStatic && !item.GetComponent<NetBody>())
				{
					//如果物体没有Netbody，添加错误条目
					ErrItemList.Add(CreateAnErrItem(item, EnumErrType.Err_1));
					nRigid++;
				}
			}
		}

		/// <summary>
		/// 遍历搜索一个节点前向路径是否有Net Signal
		/// </summary>
		/// <param name="node">头节点</param>
		void FindNetSignal(Node node)
		{
			//Debug.Log(node.GetType());
			//判断该节点是否为NetSignal，是则有NetSignal，返回
			if (node.GetType() == typeof(NetSignal))
			{ nRecursion--; return; }
			//不是的话，收集输入端
			List<NodeSocket> L = node.ListNodeSockets();
			List<NodeInput> LIn = new List<NodeInput>();
			foreach (var i in L)
			{
				if (i.GetType() == typeof(NodeInput))
				{
					LIn.Add((NodeInput)i);
				}
			}
			//为0，说明找到头也没有NetSignal，缺失，返回
			if (LIn.Count == 0) { isLackNetS = true; nRecursion--; return; }
			//大于等于1，说明该节点分叉了，继续遍历每个分支
			if (LIn.Count >= 1)
			{
				foreach (var i in LIn)
				{
					if (i.connectedNode == null)    //如果没连接别的节点，视为缺失，返回
					{ isLackNetS = true; nRecursion--; return; }
					else    //否则继续找
					{
						if (nRecursion < 50)//递归不超过50层
						{
							nRecursion++;
							FindNetSignal(i.connectedNode);
						}
						else    //超过说明没有net signal
						{ isLackNetS = true; nRecursion--; return; }
					}
				}
			}
			nRecursion--;//递归返回前深度 -1
		}

		/// <summary>
		/// 遍历搜索节点列表中的Net Signal
		/// </summary>
		/// <param name="gos">要遍历的节点列表</param>
		/// <param name="e">指定错误类型</param>
		void FindNetSignalInList(Node[] gos, EnumErrType e)
		{
			foreach (var item in gos)
			{
				isLackNetS = false;//清除标志位
				FindNetSignal(item);
				if (isLackNetS)
				{
					ErrItemList.Add(CreateAnErrItem(item, e));
					nSignal++;
				}
			}
		}

		/// <summary>
		/// 检查Net Signal
		/// </summary>
		void CheckII()
		{
			SignalUnityEvent[] gos2 = level.GetComponentsInChildren<SignalUnityEvent>();
			FindNetSignalInList(gos2, EnumErrType.Err_2);

			LerpEmission[] gos3 = level.GetComponentsInChildren<LerpEmission>();
			FindNetSignalInList(gos3, EnumErrType.Err_3);

			LerpLightIntensity[] gos4 = level.GetComponentsInChildren<LerpLightIntensity>();
			FindNetSignalInList(gos4, EnumErrType.Err_4);

			LerpParticles[] gos5 = level.GetComponentsInChildren<LerpParticles>();
			FindNetSignalInList(gos5, EnumErrType.Err_5);

			LerpPitch[] gos6 = level.GetComponentsInChildren<LerpPitch>();
			FindNetSignalInList(gos6, EnumErrType.Err_6);

			SignalLerpVolume[] gos7 = level.GetComponentsInChildren<SignalLerpVolume>();
			FindNetSignalInList(gos7, EnumErrType.Err_7);
		}

		/// <summary>
		/// 创建ErrItem结构体
		/// </summary>
		/// <param name="c">组件</param>
		/// <param name="t">错误类型</param>
		/// <returns></returns>
		public ErrItem CreateAnErrItem(Component c, EnumErrType t)
		{
			ErrItem item = new ErrItem();
			item.c = c;
			item.t = t;
			return item;
		}

		/// <summary>
		/// 创建一个NetBody条目
		/// </summary>
		/// <param name="r">物体</param>
		/// <param name="errT">错误类型</param>
		public static void CreateLogItem(Rigidbody r, EnumErrType errT)
		{
			EditorGUILayout.ObjectField(r.gameObject, typeof(GameObject), true);
			GUILayout.Label(TranslateLog(errT));
			GUILayout.Space(3);
		}

		/// <summary>
		/// 创建一个netsignal条目
		/// </summary>
		/// <param name="c">组件</param>
		/// <param name="errT">错误类型</param>
		public static void CreateLogItem(Node c, EnumErrType errT)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.ColorField(c.nodeColour, GUILayout.Width(50));
			EditorGUILayout.ObjectField(c, c.GetType(), true);
			EditorGUILayout.EndHorizontal();
			GUILayout.Label(TranslateLog(errT));
			GUILayout.Space(3);
		}

		/// <summary>
		/// 返回错误类型对应的文字描述
		/// </summary>
		/// <param name="errT">错误类型</param>
		/// <returns></returns>
		static string TranslateLog(EnumErrType errT)
		{
			switch (errT)
			{
				case EnumErrType.Err_1: return "该物体具有Rigidbody但缺少Net Body";
				case EnumErrType.Err_2: return "该物体具有Signal Unity Event但其输入端路径上缺少Net Signal";
				case EnumErrType.Err_3: return "该物体具有Lerp Emission但其输入端路径上缺少Net Signal";
				case EnumErrType.Err_4: return "该物体具有Lerp Light Intensity但其输入端路径上缺少Net Signal";
				case EnumErrType.Err_5: return "该物体具有Lerp Particles但其输入端路径上缺少Net Signal";
				case EnumErrType.Err_6: return "该物体具有Lerp Pitch但其输入端路径上缺少Net Signal";
				case EnumErrType.Err_7: return "该物体具有Signal Lerp Volume但其输入端路径上缺少Net Signal";
			}
			return "ERROR";
		}

		public string UniLog;   //总数信息
		public enum EnumErrType
		{
			Err_1 = 1,
			Err_2,
			Err_3,
			Err_4,
			Err_5,
			Err_6,
			Err_7,
		}
		public static int nRecursion = 0;//递归计数器
		public static bool isLackNetS = false;//是否缺少Net Signal
		static int nRigid;//刚体问题个数
		static int nSignal;//信号问题个数
		public static List<ErrItem> ErrItemList;//出错组件列表
		public struct ErrItem
		{
			public Component c;
			public EnumErrType t;
		}
		public static GameObject level;
		public static bool ifCkRigid = true;
		public static bool ifCkSignal = true;
		Vector2 scrollPos;
	}
}