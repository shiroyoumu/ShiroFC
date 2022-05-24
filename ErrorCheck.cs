using HumanAPI;
using Multiplayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		/// 遍历搜索一个NodeGraph输出节点前向路径是否有Net Signal
		/// </summary>
		/// <param name="node">头节点</param>
		void FindNetSignalInGraph(Node node)
		{
			if (node)
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
				if (LIn.Count == 0 || node.GetType() == typeof(NodeGraph))
				{ isLackNetS = true; nRecursion--; return; }
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
								FindNetSignalInGraph(i.connectedNode);
							}
							else    //超过说明没有net signal
							{ isLackNetS = true; nRecursion--; return; }
						}
					}
				}
				nRecursion--;//递归返回前深度 -1
			}
			else
			{ isLackNetS = true; nRecursion--; return; }
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
				isLackNetS = false;//清除标志位
				nRecursion = 0;
				FindNetSignal(item);
				if (isLackNetS)
				{
					list.Add(item);
				}
			}
			return list;
		}

		/// <summary>
		/// 遍历搜索节点图内部的Net Signal
		/// </summary>
		/// <param name="ngs">要遍历的节点列表</param>
		/// <returns>缺少Net Signal的节点列表</returns>
		List<NodeGraph> FindNetSignalInList(NodeGraph[] ngs)
		{
			List<NodeGraph> list = new List<NodeGraph>();
			foreach (var i in ngs)
			{
				foreach (var j in i.outputs)
				{
					isLackNetS = false;
					nRecursion = 0;
					FindNetSignalInGraph(j.outputSocket.connectedNode);
					if (isLackNetS)
					{
						list.Add(i);
						break;
					}
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
			SignalUnityEvent[] gos2 = GameObject.FindObjectsOfType<SignalUnityEvent>();
			LerpEmission[] gos3 = GameObject.FindObjectsOfType<LerpEmission>();
			LerpLightIntensity[] gos4 = GameObject.FindObjectsOfType<LerpLightIntensity>();
			LerpParticles[] gos5 = GameObject.FindObjectsOfType<LerpParticles>();
			LerpPitch[] gos6 = GameObject.FindObjectsOfType<LerpPitch>();
			SignalLerpVolume[] gos7 = GameObject.FindObjectsOfType<SignalLerpVolume>();
			List<Node> list = new List<Node>();
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
			foreach (var item in instance.FindNetSignalInList(gos7))
				list.Add(item);
			return list;
		}

		/// <summary>
		/// 在节点图中检查缺少Net Signal
		/// </summary>
		/// <returns>缺少Net Signal的节点图</returns>
		public static List<NodeGraph> CheckNetSignalInGraph()
		{
			NodeGraph[] ngs = GameObject.FindObjectsOfType<NodeGraph>();
			return instance.FindNetSignalInList(ngs);
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
		/// 相同组件对
		/// </summary>
		public struct ComponentPair
		{
			public Component c1;
			public Component c2;
		};

		static ErrorCheck instance = new ErrorCheck();
		int nRecursion = 0;//递归计数器
		bool isLackNetS = false;//是否缺少Net Signal
	}
}