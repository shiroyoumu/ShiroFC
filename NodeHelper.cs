using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HumanAPI;
using UnityEditor;
using System;
using Multiplayer;
using UnityEngine.Profiling;

namespace RuntimeFC
{
	[AddComponentMenu("ShiroFC/Node Helper")]
	public class NodeHelper : Node
	{
		void Update()
		{
			ScanDebugNode();
		}

		void ScanDebugNode()
		{
			for (int i = 0; i < debugNode.Length; i++)
			{
				if (debugNode[i].triggerMode)
				{
					if (Input.GetKeyDown(Key2Keycode(debugNode[i].triggerKey)))
						debugNode[i].curToggleState = true;
					if (Input.GetKeyUp(Key2Keycode(debugNode[i].triggerKey)))
						debugNode[i].curToggleState = false;
					float output = (debugNode[i].highOutput - debugNode[i].lowOutput) * debugNode[i].intensity;
					debugNode[i].outnode.SetValue(debugNode[i].curToggleState ? debugNode[i].lowOutput + output : debugNode[i].lowOutput);
				}
				else
				{
					if (Input.GetKeyDown(Key2Keycode(debugNode[i].triggerKey)))
					{
						debugNode[i].curToggleState = !debugNode[i].curToggleState;
					}
					float output = (debugNode[i].highOutput - debugNode[i].lowOutput) * debugNode[i].intensity;
					debugNode[i].outnode.SetValue(debugNode[i].curToggleState ? debugNode[i].lowOutput + output : debugNode[i].lowOutput);
				}
			}
			for (int i = 0; i < debugNode2.Length; i++)
			{
				if (debugNode2[i].triggerMode)
				{
					if (Input.GetKeyDown(Key2Keycode(debugNode2[i].triggerKeyPos)))
						debugNode2[i].curToggleState = debugNode2[i].curToggleState + debugNode2[i].highOutput;
					if (Input.GetKeyUp(Key2Keycode(debugNode2[i].triggerKeyPos)))
						debugNode2[i].curToggleState = debugNode2[i].curToggleState - debugNode2[i].highOutput;
					if (Input.GetKeyDown(Key2Keycode(debugNode2[i].triggerKeyNeg)))
						debugNode2[i].curToggleState = debugNode2[i].curToggleState - debugNode2[i].highOutput;
					if (Input.GetKeyUp(Key2Keycode(debugNode2[i].triggerKeyNeg)))
						debugNode2[i].curToggleState = debugNode2[i].curToggleState + debugNode2[i].highOutput;
					debugNode2[i].outnode.SetValue(debugNode2[i].curToggleState);
				}
				else
				{
					if (Input.GetKeyDown(Key2Keycode(debugNode2[i].triggerKeyPos)))
					{
						
						debugNode2[i].curToggleState += debugNode2[i].highOutput;
						if (debugNode2[i].curToggleState > debugNode2[i].highOutput)
							debugNode2[i].curToggleState = debugNode2[i].highOutput;
					}
					if (Input.GetKeyDown(Key2Keycode(debugNode2[i].triggerKeyNeg)))
					{

						debugNode2[i].curToggleState -= debugNode2[i].highOutput;
						if (debugNode2[i].curToggleState < -debugNode2[i].highOutput)
							debugNode2[i].curToggleState = -debugNode2[i].highOutput;
					}
					debugNode2[i].outnode.SetValue(debugNode2[i].curToggleState);
				}
			}
		}

		public override void Process()
		{
			for (int i = 0; i < debugNode.Length; i++)
				debugNode[i].outnode.SetValue(debugNode[i].lowOutput);
			for (int i = 0; i < debugNode2.Length; i++)
				debugNode2[i].outnode.SetValue(0);
		}

		protected override void CollectAllSockets(List<NodeSocket> sockets)
		{
			for (int i = 0; i < debugNode.Length; i++)
			{
				debugNode[i].outnode.node = this;
				debugNode[i].outnode.name = string.Format("({1}) out{0}", i, debugNode[i].triggerKey.ToString());//"out " + i.ToString();
				sockets.Add(debugNode[i].outnode);
			}
			for (int i = 0; i < debugNode2.Length; i++)
			{
				debugNode2[i].outnode.node = this;
				debugNode2[i].outnode.name = string.Format("({1},{2}) out{0}", i, debugNode2[i].triggerKeyPos.ToString(), debugNode2[i].triggerKeyNeg.ToString());//"out " + i.ToString();
				sockets.Add(debugNode2[i].outnode);
			}
		}

		KeyCode Key2Keycode(Key key)
		{
			switch (key)
			{
				case Key.None: return KeyCode.None;
				case Key.F2: return KeyCode.F2;
				case Key.F3: return KeyCode.F3;
				case Key.F4: return KeyCode.F4;
				case Key.F5: return KeyCode.F5;
				case Key.F7: return KeyCode.F7;
				case Key.F10: return KeyCode.F10;
				case Key.F11: return KeyCode.F11;
				case Key.F12: return KeyCode.F12;
				case Key.Num1: return KeyCode.Alpha1;
				case Key.Num2: return KeyCode.Alpha2;
				case Key.Num3: return KeyCode.Alpha3;
				case Key.Num4: return KeyCode.Alpha4;
				case Key.Num5: return KeyCode.Alpha5;
				case Key.Num6: return KeyCode.Alpha6;
				case Key.Num7: return KeyCode.Alpha7;
				case Key.Num8: return KeyCode.Alpha8;
				case Key.Num9: return KeyCode.Alpha9;
				case Key.Num0: return KeyCode.Alpha0;
				case Key.Pad1: return KeyCode.Keypad1;
				case Key.Pad2: return KeyCode.Keypad2;
				case Key.Pad3: return KeyCode.Keypad3;
				case Key.Pad4: return KeyCode.Keypad4;
				case Key.Pad5: return KeyCode.Keypad5;
				case Key.Pad6: return KeyCode.Keypad6;
				case Key.Pad7: return KeyCode.Keypad7;
				case Key.Pad8: return KeyCode.Keypad8;
				case Key.Pad9: return KeyCode.Keypad9;
				case Key.Pad0: return KeyCode.Keypad0;
				case Key.Tab: return KeyCode.Tab;
				case Key.Q: return KeyCode.Q;
				case Key.R: return KeyCode.R;
				case Key.T: return KeyCode.T;
				case Key.U: return KeyCode.U;
				case Key.I: return KeyCode.I;
				case Key.O: return KeyCode.O;
				case Key.P: return KeyCode.P;
				case Key.F: return KeyCode.F;
				case Key.G: return KeyCode.G;
				case Key.H: return KeyCode.H;
				case Key.J: return KeyCode.J;
				case Key.K: return KeyCode.K;
				case Key.L: return KeyCode.L;
				case Key.Z: return KeyCode.Z;
				case Key.X: return KeyCode.X;
				case Key.C: return KeyCode.C;
				case Key.V: return KeyCode.V;
				case Key.B: return KeyCode.B;
				case Key.N: return KeyCode.N;
				case Key.M: return KeyCode.M;
				default: return KeyCode.None;
			}
		}

		public oneNode[] debugNode = {
		new oneNode(Key.Num1,true),
		new oneNode(Key.Num2,true)
		};

		public oneLeverNode[] debugNode2 = {
			new oneLeverNode(Key.Num9,Key.Num9,true)
		};

		public enum Key
		{
			None = KeyCode.None,
			F2 = KeyCode.F2,
			F3 = KeyCode.F3,
			F4 = KeyCode.F4,
			F5 = KeyCode.F5,
			F7 = KeyCode.F7,
			F10 = KeyCode.F10,
			F11 = KeyCode.F11,
			F12 = KeyCode.F12,
			Num1 = KeyCode.Alpha1,
			Num2 = KeyCode.Alpha2,
			Num3 = KeyCode.Alpha3,
			Num4 = KeyCode.Alpha4,
			Num5 = KeyCode.Alpha5,
			Num6 = KeyCode.Alpha6,
			Num7 = KeyCode.Alpha7,
			Num8 = KeyCode.Alpha8,
			Num9 = KeyCode.Alpha9,
			Num0 = KeyCode.Alpha0,
			Pad1 = KeyCode.Keypad1,
			Pad2 = KeyCode.Keypad2,
			Pad3 = KeyCode.Keypad3,
			Pad4 = KeyCode.Keypad4,
			Pad5 = KeyCode.Keypad5,
			Pad6 = KeyCode.Keypad6,
			Pad7 = KeyCode.Keypad7,
			Pad8 = KeyCode.Keypad8,
			Pad9 = KeyCode.Keypad9,
			Pad0 = KeyCode.Keypad0,
			Tab = KeyCode.Tab,
			Q = KeyCode.Q,
			R = KeyCode.R,
			T = KeyCode.T,
			U = KeyCode.U,
			I = KeyCode.I,
			O = KeyCode.O,
			P = KeyCode.P,
			F = KeyCode.F,
			G = KeyCode.G,
			H = KeyCode.H,
			J = KeyCode.J,
			K = KeyCode.K,
			L = KeyCode.L,
			Z = KeyCode.Z,
			X = KeyCode.X,
			C = KeyCode.C,
			V = KeyCode.V,
			B = KeyCode.B,
			N = KeyCode.N,
			M = KeyCode.M,
		}
		////////////////////////////////////////////////////////////////////////
		[System.Serializable]
		public struct oneNode
		{
			public NodeOutput outnode;      //节点
			[Tooltip("触发快捷键")]
			public Key triggerKey;      //触发快捷键
			[Tooltip("输出最大值")]
			public float highOutput;
			[Tooltip("输出最小值")]
			public float lowOutput;
			[Tooltip("输出力度")]
			[Range(0, 1)]
			public float intensity;     //力度
			[Tooltip("触发模式：T：按钮模式，F：开关模式")]
			public bool triggerMode;    //触发模式,T：按钮，F：开关
			[HideInInspector]
			public bool curToggleState;     //F模式下当前状态

			public oneNode(Key tk, bool tm)
			{
				this.outnode = new NodeOutput();
				this.triggerKey = tk;
				this.highOutput = 1f;
				this.lowOutput = 0;
				this.intensity = 1f;
				this.triggerMode = tm;
				this.curToggleState = false;
			}
		}

		[System.Serializable]
		public struct oneLeverNode
		{
			public NodeOutput outnode;      //节点
			[Tooltip("触发快捷键")]
			public Key triggerKeyPos;      //触发快捷键+
			[Tooltip("触发快捷键")]
			public Key triggerKeyNeg;      //触发快捷键-
			[Tooltip("输出最大值")]
			public float highOutput;
			[Tooltip("触发模式：T：按钮模式，F：开关模式")]
			public bool triggerMode;    //触发模式,T：按钮，F：开关
			[HideInInspector]
			public float curToggleState;     //F模式下当前状态

			public oneLeverNode(Key tkPos, Key tkNeg, bool tm)
			{
				this.outnode = new NodeOutput();
				this.triggerKeyPos = tkPos; 
				this.triggerKeyNeg = tkNeg;
				this.highOutput = 1f;
				this.triggerMode = true;
				this.curToggleState = 0f;
			}
		}
	}
}
