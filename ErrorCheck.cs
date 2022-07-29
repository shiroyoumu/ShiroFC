using HumanAPI;
using Multiplayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace EditorFC
{
	public class ErrorCheck
	{
		/// <summary>
		/// 检查刚体缺少NetBody
		/// </summary>
		/// <param name="level">level物体</param>
		/// <returns>缺少NetBody的物体列表</returns>
		public static List<GameObject> CheckNetbody(GameObject level)
		{
			List<GameObject> gameObjects = new List<GameObject>();
			//收集所有组件
			Rigidbody[] gos = level.GetComponentsInChildren<Rigidbody>();
			foreach (var item in gos)
			{
				if (!item.gameObject.isStatic && !item.GetComponent<NetBody>())
				{
					//如果物体没有Netbody，添加错误条目
					gameObjects.Add(item.gameObject);
				}
			}
			return gameObjects;
		}

		/// <summary>
		/// 遍历搜索一个NodeGraph输出节点前向路径是否有Net Signal
		/// </summary>
		/// <param name="node">根节点</param>
		/// <param name="socketName">前一节点连入根节点的接口名</param>
		void FindNetSignal(Node node, string socketName)
		{
			//判断是否找到Net Signal
			if (node.GetType() == typeof(NetSignal))
			{
				isLack = false; nRecursion--; return;
			}
			//没找到的话，收集输入端		
			List<NodeInput> ns = new List<NodeInput>();//根节点所有输入端列表
													   //收集非Node Graph节点输入端
			if (node.GetType() != typeof(NodeGraph))
			{
				//获取所有接口，将输入端添加进ns
				foreach (var item in node.ListAllSockets())
				{
					if (item.GetType() == typeof(NodeInput))
					{
						ns.Add((NodeInput)item);
					}
				}
			}
			//收集Node Graph节点输入端
			else
			{
				NodeGraph ng = (NodeGraph)node;
				//根据输入端接口名来确定输入端（Node Graph边缘节点有且仅有一个唯一确定的输入端）
				foreach (var item in ng.outputs)//判断边缘输出（NodeGraphOutput）节点列表
				{
					if (item.name == socketName)
					{
						ns.Add(item.outputSocket);
					}
				}
				foreach (var item in ng.inputs)//判断边缘输入（NodeGraphInput）节点列表
				{
					if (item.name == socketName)
						ns.Add(item.input);
				}
			}
			//为0，说明找到头也没有NetSignal，缺失，返回
			if (ns.Count == 0)
			{ isLack = true; nRecursion--; return; }
			//大于等于1，说明该节点分叉了，继续遍历每个分支
			if (ns.Count >= 1)
			{
				foreach (var item in ns)
				{
					//如果没连接别的节点，视为缺失，返回
					if (item.connectedNode == null)
					{ isLack = true; nRecursion--; return; }
					else
					{
						//如果递归超过50层，说明死循环，视为缺失，返回
						if (nRecursion < 50)
						{
							nRecursion++;
							FindNetSignal(item.connectedNode, item.connectedSocket);
						}
						else
						{ isLack = true; nRecursion--; return; }
					}
				}
			}
			nRecursion--;
		}

		/// <summary>
		/// 遍历搜索节点列表中的Net Signal
		/// </summary>
		/// <param name="gos">要遍历的节点列表</param>
		/// <returns>缺少Net Signal的节点列表</returns>
		List<Node> FindNetSignalInList(Node[] gos)
		{
			List<Node> list = new List<Node>();
			foreach (var item in gos)
			{
				isLack = false;//清除标志位
				nRecursion = 0;
				FindNetSignal(item, "");
				if (isLack)
				{
					list.Add(item);
				}
			}
			return list;
		}

		/// <summary>
		/// 检查缺少Net Signal
		/// </summary>
		/// <returns>缺少Net Signal的节点列表</returns>
		public static List<Node> CheckNetSignal()
		{
			SignalUnityEvent[] gos1 = GameObject.FindObjectsOfType<SignalUnityEvent>();
			LerpEmission[] gos2 = GameObject.FindObjectsOfType<LerpEmission>();
			LerpLightIntensity[] gos3 = GameObject.FindObjectsOfType<LerpLightIntensity>();
			LerpParticles[] gos4 = GameObject.FindObjectsOfType<LerpParticles>();
			LerpPitch[] gos5 = GameObject.FindObjectsOfType<LerpPitch>();
			SignalLerpVolume[] gos6 = GameObject.FindObjectsOfType<SignalLerpVolume>();			
			List<Node> list = new List<Node>();
			foreach (var item in instance.FindNetSignalInList(gos1))
				list.Add(item);
			foreach (var item in instance.FindNetSignalInList(gos2))
				list.Add(item);
			foreach (var item in instance.FindNetSignalInList(gos3))
				list.Add(item);
			foreach (var item in instance.FindNetSignalInList(gos4))
				list.Add(item);
			foreach (var item in instance.FindNetSignalInList(gos5))
				list.Add(item);
			foreach (var item in instance.FindNetSignalInList(gos6))
				list.Add(item);
			return list;
		}

		/// <summary>
		/// 寻找相同的Checkpoint
		/// </summary>
		/// <param name="cks">比对数组</param>
		/// <returns>相同number的Checkpoint对</returns>
		List<ComponentPair> FindSameCheckpoint(Checkpoint[] cks)
		{
			List<ComponentPair> list = new List<ComponentPair>();
			for (int i = 0; i < cks.Length - 1; i++)
			{
				for (int j = i + 1; j < cks.Length; j++)
				{
					if (cks[i].number == cks[j].number)
					{
						ComponentPair pair;
						pair.c1 = cks[i];
						pair.c2 = cks[j];
						list.Add(pair);
					}
				}
			}
			return list;
		}

		/// <summary>
		/// 寻找相同的NetScene
		/// </summary>
		/// <param name="nss">比对数组</param>
		/// <returns>相同NetId的NetScene对</returns>
		List<ComponentPair> FindSameNetScene(NetScene[] nss)
		{
			List<ComponentPair> list = new List<ComponentPair>();
			for (int i = 0; i < nss.Length - 1; i++)
			{
				for (int j = i + 1; j < nss.Length; j++)
				{
					if (nss[i].netId == nss[j].netId)
					{
						ComponentPair pair;
						pair.c1 = nss[i];
						pair.c2 = nss[j];
						list.Add(pair);
					}
				}
			}
			return list;
		}

		/// <summary>
		/// 检查Checkpoint和Net Scene
		/// </summary>
		/// <returns>相同数据对列表</returns>
		public static List<ComponentPair> CheckLevelParts()
		{
			List<ComponentPair> list = new List<ComponentPair>();
			Checkpoint[] cks = GameObject.FindObjectsOfType<Checkpoint>();
			foreach (var item in instance.FindSameCheckpoint(cks))
				list.Add(item);
			NetScene[] nss = GameObject.FindObjectsOfType<NetScene>();
			foreach (var item in instance.FindSameNetScene(nss))
				list.Add(item);
			return list;
		}

		/// <summary>
		/// 为所有刚体添加NetBody
		/// </summary>
		public static void AddAllNetBody()
		{
			Rigidbody[] rbs = GameObject.FindObjectsOfType<Rigidbody>();
			int num = 0;
			foreach (var item in rbs)
			{
				if (!item.GetComponent<NetBody>())
				{
					item.gameObject.AddComponent<NetBody>();
					num++;
				}
			}
			if (num == 0)
			{
				ErrorCheckWindow.log = "没有刚体需要添加Net Body";
			}
			else
			{
				ErrorCheckWindow.log = $"已为 {num} 个刚体添加Net Body";
			}
		}

		/// <summary>
		/// 相同组件对
		/// </summary>
		public struct ComponentPair
		{
			public Component c1;
			public Component c2;
		};

		static ErrorCheck instance = new ErrorCheck();
		int nRecursion = 0;//递归计数器
		bool isLack = false;//是否缺少Net Signal
	}
}