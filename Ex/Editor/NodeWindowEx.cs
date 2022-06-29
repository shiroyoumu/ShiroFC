using HumanAPI;
using Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

public class NodeWindowEx : EditorWindow
{
	public static NodeWindowEx Init(NodeGraph graph)
	{
		NodeWindowEx win = (NodeWindowEx)EditorWindow.GetWindow(typeof(NodeWindowEx));
		win.pendingGraph = graph;
		win.OnEnable();
		return win;
	}
	void OnEnable()
	{
		circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
		square = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
		Init();
		try//读取按钮状态
		{			
			isLiveUpdate = Convert.ToBoolean(EditorUserSettings.GetConfigValue("isLiveUpdate"));
			isTrackSelection = Convert.ToBoolean(EditorUserSettings.GetConfigValue("isTrackSelection"));
			isMap = Convert.ToBoolean(EditorUserSettings.GetConfigValue("isMap"));
		} catch (Exception) { }
	}
	void OnLostFocus()
	{
		//失焦保存按钮状态
		EditorUserSettings.SetConfigValue("isLiveUpdate", isLiveUpdate.ToString());
		EditorUserSettings.SetConfigValue("isTrackSelection", isTrackSelection.ToString());
		EditorUserSettings.SetConfigValue("isMap", isMap.ToString());
	}
	void OnFocus()
	{
		try//得焦读取按钮状态
		{			
			isLiveUpdate = Convert.ToBoolean(EditorUserSettings.GetConfigValue("isLiveUpdate"));
			isTrackSelection = Convert.ToBoolean(EditorUserSettings.GetConfigValue("isTrackSelection"));
			isMap = Convert.ToBoolean(EditorUserSettings.GetConfigValue("isMap"));
		} catch (Exception) { }
	}
	void OnSelectionChange()
	{
		isUpdateSelection = true;
		Repaint();
	}
	void Update()
	{ 
		if(!EditorApplication.isPlaying || (EditorApplication.isPlaying && isLiveUpdate))
			Repaint();
	}
	void OnGUI()
	{
		if (pendingGraph == null)
		{
			if (Selection.activeGameObject != null)
			{
				pendingGraph = Selection.activeGameObject.GetComponentInParent<NodeGraph>();
			}
		}
		GUI.skin.button.richText = true;
		GUI.skin.label.richText = true;
		using (new EditorGUILayout.HorizontalScope())
		{
			using (new EditorGUILayout.VerticalScope())
			{ 
				DrawLine(new Rect(0, 0, 46, position.height), new Color(0.5f, 0.5f, 0.5f));
				GUILayout.Space(10);
				SetColorByBool(isLiveUpdate, new Color(0.75f, 1, 0.5f), Color.white);
				if (GUILayout.Button(new GUIContent() { text = "Live", tooltip = "实时更新" }, GUILayout.Width(40), GUILayout.Height(40)))
				{
					isLiveUpdate = !isLiveUpdate;
				}
				SetColorByBool(isTrackSelection, new Color(0.75f, 1, 0.5f), Color.white);
				if (GUILayout.Button(new GUIContent() { text = "Track", tooltip = "追踪选中节点" }, GUILayout.Width(40), GUILayout.Height(40)))
				{
					isTrackSelection = !isTrackSelection;
				}
				GUILayout.Space(15);
				GUI.color = Color.white;
				Event e = Event.current;
				EditorGUI.BeginDisabledGroup(!enAll);
				if (GUILayout.Button(new GUIContent() { text = "Re.", tooltip = "刷新节点图。快捷键：" + k_refresh.ToString() }, GUILayout.Width(40), GUILayout.Height(40)) ||
					(e.type == EventType.KeyDown && e.keyCode == k_refresh))
				{
					DoRefresh();
				}
				EditorGUI.BeginDisabledGroup(!enUp);
				if (GUILayout.Button(new GUIContent() { text = "▲Up", tooltip = "跳转至父级" }, GUILayout.Width(40), GUILayout.Height(40)))
				{
					DoUp();
				}
				EditorGUI.EndDisabledGroup();
				if (GUILayout.Button(new GUIContent() { text = "<size=20>//</size>", tooltip = "添加注释" }, GUILayout.Width(40), GUILayout.Height(40)))
				{
					DoAddComment();
				}
				if (GUILayout.Button(new GUIContent() { text = "<size=40>▦</size>", tooltip = "对齐至格点。快捷键：" + k_snap.ToString() }, GUILayout.Width(40), GUILayout.Height(40)) ||
					(e.type == EventType.KeyDown && e.keyCode == k_snap))
				{
					DoSnap();
				}
				EditorGUI.EndDisabledGroup();
				//if (GUILayout.Button(new GUIContent() { text = "Auto", tooltip = "自动连线" }, GUILayout.Width(40), GUILayout.Height(40)))
				//{
				//    DoAutoConnect();
				//}
				GUILayout.FlexibleSpace();
				SetColorByBool(isMap, new Color(0.75f, 1, 0.5f), Color.white);
				if (GUILayout.Button(new GUIContent() { text = "Map", tooltip = "切换小地图" }, GUILayout.Width(40), GUILayout.Height(40)))
				{
					isMap = !isMap;
				}
				GUI.color = Color.white;
				if (GUILayout.Button(new GUIContent() { text = "Conf", tooltip = "节点图设置" }, GUILayout.Width(40), GUILayout.Height(40)))
				{
					;//ConfWindow.Init();
				}
				GUILayout.Space(5);	
			}		
			GUILayout.Space(2);
			DrawLine(new Rect(46, 0, 2, position.height), new Color(0.25f, 0.25f, 0.25f));
			DrawLine(new Rect(48, 0, 1, position.height), Color.white);
			GUILayout.BeginArea(new Rect(50, 0, position.width - 50, position.height));
			using (new GUILayout.VerticalScope())
			{
				using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, true, true))
                {
                    scrollPos = scrollView.scrollPosition;
                    GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(height));//占位符
					//主区域 (
					DrawGrids(gridX, gridY, gridBgColor, gridLineColor);
					HandleEvents();
					ShowRightMenu();
					if (pendingGraph != activeGraph || activeGraph == null)
					{
						if(pendingGraph != null)
							activeGraph = pendingGraph;
						if (activeGraph != null && activeGraph.transform.parent != null)//设置Up按钮
							enUp = true;
						else
							enUp = false;
						BuildGraph(activeGraph);//显示节点图
						Repaint();
					}
					if(activeGraph == null)//设置第二组按钮
						enAll = false;
					else
						enAll = true;
					DrawConnections();
					SetText();
					PrepareRender();

					BeginWindows();				
					try { Render(); }catch (Exception) { }					
					EndWindows();
										
					if (isDragging)
					{ 
						DrawCurve(dragStartPos, dragEndPos, connectingColor);//绘制待定连线
						if(enHint)
							DrawCurve(hintStartPos, hintEndPos, lineHintColor);//绘制提示线
					}						
					DrawFrame();
					SetMouseStyle();
					//主区域 )
				}				
				using (new GUILayout.HorizontalScope())
				{ 
					EditorGUILayout.LabelField(string.Format("{0}{1}{2}{3}{4}", mousePos, connectionState, selectNum, _temp, _temp2));
					SetColorByBool(isDragging, new Color(0.25f, 1, 0.25f), Color.white);
					EditorGUILayout.LabelField(new GUIContent() {image = NodeWindowEx.square.texture }, GUILayout.Width(18), GUILayout.Width(18));//连线
					SetColorByBool(isScrolling, new Color(0.25f, 1, 0.25f), Color.white);
					EditorGUILayout.LabelField(new GUIContent() { image = NodeWindowEx.square.texture }, GUILayout.Width(18), GUILayout.Width(18));//滚动
					SetColorByBool(isDrawFrame, new Color(0.25f, 1, 0.25f), Color.white);
					EditorGUILayout.LabelField(new GUIContent() { image = NodeWindowEx.square.texture }, GUILayout.Width(18), GUILayout.Width(18));//框选
					SetColorByBool(isMapDragging, new Color(0.25f, 1, 0.25f), Color.white);
					EditorGUILayout.LabelField(new GUIContent() { image = NodeWindowEx.square.texture }, GUILayout.Width(18), GUILayout.Width(18));//拖动节点
					SetColorByBool(isSelectionsMoving, new Color(0.25f, 1, 0.25f), Color.white);
					EditorGUILayout.LabelField(new GUIContent() { image = NodeWindowEx.square.texture }, GUILayout.Width(18), GUILayout.Width(18));//拖动节点
					GUI.color = Color.white;
				}
			}
			GUILayout.EndArea();
			SetNoNode();
		}		
		DrawMiniMap();
	}
	/// <summary>
	/// 刷新节点图
	/// </summary>
	void DoRefresh()
	{
		BuildGraph(activeGraph);
	}
	/// <summary>
	/// 向上
	/// </summary>
	void DoUp()
	{
		if (activeGraph.transform.parent != null)
		{ 
			NodeGraph ng = activeGraph.transform.parent.GetComponentInParent<NodeGraph>();
			if (ng != null)
				Init(ng);			
		}
	}
	/// <summary>
	/// 添加注释
	/// </summary>
	void DoAddComment()
	{
		activeGraph.gameObject.AddComponent<NodeComment>();
		BuildGraph(activeGraph);
	}
	/// <summary>
	/// 对齐网格
	/// </summary>
	void DoSnap()
	{
		foreach (NodeRectEx nodeRect in nodeRects)
		{
			float num = Mathf.Round(nodeRect.nodePos.x / gridX) * gridX;
			float num2 = Mathf.Round(nodeRect.nodePos.y / gridY) * gridY + 1;
			nodeRect.nodePos = new Vector2(num, num2);
			nodeRect.UpdateLayout();
		}
	}
	/// <summary>
	/// 初始化变量
	/// </summary>
	void Init()
	{
		nodeRects.Clear();
		graphNodes.Clear();
		sockets2Rect.Clear();
		selectedNodes.Clear();
		selectionDelta.Clear();
		isDragging = isScrolling = isDrawFrame = isMapDragging = isSelectionsMoving = false;
		activeGraph = null;
	}
	/// <summary>
	/// 通过标志位设置颜色
	/// </summary>
	/// <param name="f">标志位</param>
	/// <param name="trueColor">true时颜色</param>
	/// <param name="falseColor">false时颜色</param>
	void SetColorByBool(bool f, Color trueColor, Color falseColor)
	{
		if (f)
			GUI.color = trueColor;
		else
			GUI.color = falseColor;
	}
	/// <summary>
	/// 画网格
	/// </summary>
	/// <param name="gridX">X间距</param>
	/// <param name="gridY">Y间距</param>
	/// <param name="bgColor">背景色</param>
	/// <param name="lineColor">前景色</param>
	void DrawGrids(float gridX, float gridY, Color bgColor, Color lineColor)
	{
		GUI.color = bgColor;
		GUI.DrawTexture(new Rect(0, 0, width, height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
		GUI.color = lineColor;
		float num = 0;
		while (num < width)//画网格的竖线
		{
			GUI.DrawTexture(new Rect(num, 0, 1, height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
			num += gridX;
		}
		num = 1;
		while (num < height)//画网格的横线
		{
			GUI.DrawTexture(new Rect(0, num, width, 1), Texture2D.whiteTexture, ScaleMode.StretchToFill);
			num += gridY;
		}
		GUI.color = Color.white;
	}
	/// <summary>
	/// 画方块（线）
	/// </summary>
	/// <param name="rect">区域</param>
	/// <param name="color">颜色</param>
	void DrawLine(Rect rect, Color color)
	{
		GUI.color = color;
		GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
		GUI.color = Color.white;
	}
	/// <summary>
	/// 绘制贝塞尔曲线
	/// </summary>
	/// <param name="startPos">起点</param>
	/// <param name="endPos">终点</param>
	/// <param name="color">颜色</param>
	static void DrawCurve(Vector3 startPos, Vector3 endPos, Color color)
	{
		Vector3 startTangent = startPos + Vector3.right * 50f;
		Vector3 endTangent = endPos + Vector3.left * 50f;
		Color color2 = new Color(0f, 0f, 0f, 0.06f);
		for (int i = 0; i < 3; i++)
		{
			Handles.DrawBezier(startPos, endPos, startTangent, endTangent, color2, null, (i + 1) * 5);//阴影
		}
		Handles.DrawBezier(startPos, endPos, startTangent, endTangent, color, null, 2f);
	}
	/// <summary>
	/// 收集节点图中的节点
	/// </summary>
	/// <param name="nodes">收集容器</param>
	/// <param name="graph">节点图</param>
	/// <param name="t">收集对象</param>
	static void CollectNodes(List<Node> nodes, NodeGraph graph, Transform t)
	{
		if (!t.gameObject.activeSelf)//如果当前节点图物体关闭，返回，不收集
			return;
		NodeGraph component = t.GetComponent<NodeGraph>();//获取他的Nodegraph节点
		if (component != null && component != graph)//如果有多个节点图，将没点按钮的节点图加入容器（显示），返回
		{
			nodes.Add(component);
			return;
		}
		foreach (Node item in t.GetComponents<Node>())//获取当前节点图物体上的其他节点，加入容器，返回
		{
			nodes.Add(item);
		}
		for (int j = 0; j < t.childCount; j++)//递归，将上两步操作应用于当前节点图的全部子物体
		{
			CollectNodes(nodes, graph, t.GetChild(j));
		}
	}
	/// <summary>
	/// 构建节点图
	/// </summary>
	/// <param name="graph"></param>
	public void BuildGraph(NodeGraph graph)
	{
		if (graph != null)
		{ 
			Transform transform = graph.transform;//当前节点图黄色节点
			graphNodes = new List<Node>();
			CollectNodes(graphNodes, graph, transform);//收集节点
			UnityEngine.Object[] array = new UnityEngine.Object[graphNodes.Count];
			for (int i = 0; i < graphNodes.Count; i++)//将List<Node>转为Object[]
			{
				array[i] = graphNodes[i];
			}
			Undo.RecordObjects(array, "rebuild graph");//使用Object[]批量记录撤销
			for (int j = 0; j < graphNodes.Count; j++)//每个节点重新获取所有接口
			{
				graphNodes[j].RebuildSockets();
			}
			nodeRects.Clear();
			for (int k = 0; k < graphNodes.Count; k++)//获取并初始化所有节点框
			{
				Node node = graphNodes[k];//遍历收集到的节点
				if (node == activeGraph)//如果是当前的NodeGraph
				{
					if ((node as NodeGraph).inputs.Count > 0)
					{ 
						NodeRectEx nodeRect = new NodeRectEx(this);//画一个框
						nodeRect.InitializeGraphInput(activeGraph);//初始化节点图输入端（红色节点）
						nodeRects.Add(nodeRect);
					}
					if ((node as NodeGraph).outputs.Count > 0)
					{
						NodeRectEx nodeRect2 = new NodeRectEx(this);//画一个框
						nodeRect2.InitializeGraphOutput(activeGraph);//初始化节点图输出端（红色节点）
						nodeRects.Add(nodeRect2);
					}				
				}
				else if (node is NodeComment)//如果是NodeComment节点（绿色节点）
				{
					NodeRectEx nodeRect3 = new NodeRectEx(this);//画一个框
					nodeRect3.InitializeComment(node);//初始化注释节点
					nodeRects.Add(nodeRect3);
				}
				else//其他节点（灰色节点）
				{
					NodeRectEx nodeRect4 = new NodeRectEx(this);//画一个框
					nodeRect4.Initialize(node);//初始化普通节点
					nodeRects.Add(nodeRect4);
				}
			}
			sockets2Rect.Clear();
			for (int l = 0; l < nodeRects.Count; l++)
			{
				NodeRectEx nodeRect5 = nodeRects[l];//选中当前节点框
				nodeRect5.UpdateLayout();//刷新布局
				for (int m = 0; m < nodeRect5.sockets.Count; m++)//获取当前节点框中的所有接口
				{
					sockets2Rect[nodeRect5.sockets[m].socket] = nodeRect5.sockets[m];//接口绑定接口框
				}
			}
		}
		
	}
	/// <summary>
	/// 准备渲染节点图
	/// </summary>
	void PrepareRender()
	{
		if (isTrackSelection)//如果当前节点图勾选追踪选中
		{
			UpdateFromSelection();//显示选中的节点图
		}
		if (activeGraph == null)
			return;
		if (isLiveUpdate && graphNodes != null)//如果勾选了实时刷新，且当前节点图中有节点
		{
			Transform transform = activeGraph.transform;
			List<Node> first = new List<Node>();
			CollectNodes(first, activeGraph, transform);//重新收集节点
			if (!first.SequenceEqual(graphNodes))//如果新收集的节点和之前的不相等
			{
				BuildGraph(activeGraph);//重新绘制节点图内部
			}
		}
	}
	/// <summary>
	/// 渲染节点图
	/// </summary>
	void Render()
	{
        foreach (var item in nodeRects)
        {
			if (item.sockets.Count != 0 || item.nodeType == NodeRectEx.NodeRectType.Comment)
			{
				try//绘制节点框
				{
					bool drawHighlight = selectedNodes.Contains(item);//如果选中的节点等于当前节点
					if (!item.RenderWindow(nodeRects.IndexOf(item), drawHighlight))//绘制节点框背景色
					{
						BuildGraph(activeGraph);//未渲染完成，刷新节点图
					}
				}
				catch
				{
					BuildGraph(activeGraph);//发生异常，刷新节点图
				}
			}
		}
	}
	/// <summary>
	/// 处理各种鼠标事件
	/// </summary>
	void HandleEvents()
	{ 
		Event e = Event.current;
		mousePos = e.mousePosition;
		//卷动节点图
		if (e.type == EventType.MouseDown && e.button == 2)//中键被按下
		{
			isScrolling = true;
			e.Use();
		}
		if (e.type == EventType.MouseDrag && isScrolling)//中键按下拖拽
		{
			scrollPos -= e.delta;
			Repaint();
		}
		if ((e.type == EventType.MouseUp || e.rawType == EventType.MouseUp) && e.button == 2)//中间被松开
		{
			isScrolling = false;
			e.Use();
		}
		//框选节点
		if (e.type == EventType.MouseDown && e.button == 0)
		{
			frameStartPos = e.mousePosition;
			bool isHit = false;
            foreach (var item in nodeRects)
            {
				if (item.HitTest2(frameStartPos))
				{ 
					isHit = true;
					break;
				}
            }
			if(!isHit)
				isDrawFrame = true;
		}
		if (isDrawFrame)
		{
			frameEndPos = e.mousePosition;
			Repaint();
		}
		if (e.type == EventType.MouseUp && e.button == 0)
		{
			if(isDrawFrame)
				DoFramedNode();
			frameStartPos = frameEndPos = Vector2.zero;
			isDrawFrame = false;
		}
		//处理连线
		NodeSocketRectEx tempSocketRect = null;
		foreach (var item in nodeRects)//探测接口
		{
			tempSocketRect = item.HitTest(e.mousePosition);
			if (tempSocketRect != null)
			{
				break;
			}
		}
		//如果鼠标放在接口上按下时
		if (e.type == EventType.MouseDown && e.button == 0 && tempSocketRect != null)
		{
			if (tempSocketRect.socket.GetType() == typeof(NodeInput))//是输入端接口
			{
				tempOut = tempSocketRect;//给线段右边赋值
			}
			else//是输出端接口
			{
				tempIn = tempSocketRect;//给线段左边赋值
			}
			isDragging = true;
			e.Use();
		}
		NodeSocketRectEx hintIn = null, hintOut = null;
		if (isDragging)//拖拽时绘制连接中的线段
		{
			//绘制待定的连线
			if (tempOut != null && tempIn == null)//先点输入
			{
				dragStartPos = e.mousePosition;
				dragEndPos = tempOut.connectPoint;
			}
			if (tempIn != null && tempOut == null)//先点输出
			{
				dragStartPos = tempIn.connectPoint;
				dragEndPos = e.mousePosition;
			}
			if (tempOut == null && tempIn == null)
				dragStartPos = dragEndPos = Vector2.zero;
			//绘制提示线
			if (enHint)
			{
				hintStartPos = Vector2.zero;					
				hintEndPos = Vector2.zero;
				List<NodeSocketRectEx> allSockets = new List<NodeSocketRectEx>();
				foreach (var item in nodeRects)
				{
					foreach (var item2 in item.sockets)
					{
						if (tempOut != null && tempIn == null)//先点输入
						{
							if (item2.socket.GetType() == typeof(NodeOutput))
							{ 
								allSockets.Add(item2);
							}
						}
						if (tempIn != null && tempOut == null)//先点输出
						{
							if (item2.socket.GetType() == typeof(NodeInput))
							{
								allSockets.Add(item2);
							}
						}
					}
				}
				allSockets.Sort((x, y) =>
				{
					float m1 = (x.connectPoint - mousePos).magnitude;
					float m2 = (y.connectPoint - mousePos).magnitude;
					return m1.CompareTo(m2);
				});
				if ((allSockets[0].connectPoint - mousePos).magnitude < 50)
				{
					if (tempOut != null && tempIn == null)//先点输入
					{
						hintStartPos = allSockets[0].connectPoint;
						hintEndPos = tempOut.connectPoint;
						hintIn = allSockets[0];
						hintOut = tempOut;
					}
					if (tempIn != null && tempOut == null)//先点输出
					{
						hintStartPos = tempIn.connectPoint;
						hintEndPos = allSockets[0].connectPoint;
						hintIn = tempIn;
						hintOut = allSockets[0];
					}					
				}
			}			
			Repaint();
		}		
		//如果鼠标松开时
		if (e.type == EventType.MouseUp && e.button == 0)
		{
			if (tempSocketRect != null)//在接口上
			{
				if (tempSocketRect.socket.GetType() == typeof(NodeInput))//是输入端接口
				{
					if (tempOut == null && tempIn != null)//如果线段右侧为空，左侧有值
					{
						tempOut = tempSocketRect;//给右侧赋值
					}
					else//否则说明两次都是输入端接口
					{
						(tempOut.socket as NodeInput).Connect(null);
						tempIn = tempOut = null;//清空
					}
				}
				else//是输出端接口
				{
					if (tempOut != null && tempIn == null)//如果线段左侧为空，右侧有值
					{
						tempIn = tempSocketRect;//给左侧赋值
					}
					else//否则说明两次都是输出端接口
					{
						tempIn = tempOut = null;//清空
					}
				}
			}
			else//没在接口上
			{
				if (hintIn != null && hintOut != null)
				{
					tempIn = hintIn;
					tempOut = hintOut;
				}
				else
				{
					tempIn = tempOut = null;
				}
			}
			DoConnect();
			dragStartPos = dragEndPos = Vector2.zero;
			isDragging = false;
		}		
		//统一移动
		if (e.type == EventType.MouseDown && e.button == 0)
		{
			bool flag = false;
            foreach (var item in selectedNodes)
            {
				if (item.HitTest2(e.mousePosition))
				{ 
					flag = true;
					break;
				}
            }
			if (flag)
			{ 
				isSelectionsMoving = true;
				selectionDelta.Clear();
				Vector2 mousePos = e.mousePosition;
				foreach (var item in selectedNodes)
                {
					selectionDelta.Add(item.nodePos - mousePos);
                }
				e.Use();
			}
		}
		if (isSelectionsMoving)
		{
			Vector2 mousePos = e.mousePosition;
			for (int i = 0; i < selectedNodes.Count; i++)
            {
				selectedNodes[i].nodePos = selectionDelta[i] + mousePos;
				selectedNodes[i].UpdateLayout();
			}
			Repaint();
		}
		if (e.type == EventType.MouseUp && e.button == 0)
		{
			isSelectionsMoving = false;
		}
	}
	/// <summary>
	/// 更新选中的节点图
	/// </summary>
	void UpdateFromSelection()
	{
		if (isUpdateSelection)
		{
			isUpdateSelection = false;
			if (Selection.activeGameObject != null)
			{
				NodeGraph componentInParent = Selection.activeGameObject.GetComponentInParent<NodeGraph>();
				if (componentInParent != null)
				{
					//Init(componentInParent);
					pendingGraph = componentInParent;
				}
			}
		}
	}
	/// <summary>
	/// 绘制已连接的连线
	/// </summary>
	void DrawConnections()
	{
		for (int i = 0; i < nodeRects.Count; i++)//遍历节点图中每个节点
		{
			for (int j = 0; j < nodeRects[i].sockets.Count; j++)//遍历节点里的每个接口
			{
				NodeSocketRectEx nodeSocketRect = nodeRects[i].sockets[j];
				NodeInput nodeInput = nodeSocketRect.socket as NodeInput;
				if (nodeInput != null && nodeInput.GetConnectedOutput() != null)//如果是输入端，且有连接
				{
					NodeSocketRectEx nodeSocketRect2;
					if (sockets2Rect.TryGetValue(nodeInput.GetConnectedOutput(), out nodeSocketRect2))//获取与该输入端相连的输出端的接口
					{
						if (nodeInput is NodeExit)//如果碰到了输入端（输入端为NodeExit；输出端为NodeEntry）
						{
							DrawCurve(nodeSocketRect.connectPoint, nodeSocketRect2.connectPoint, Color.white);//画白线
						}
						else//如果碰到了输出端
						{
							DrawCurve(nodeSocketRect2.connectPoint, nodeSocketRect.connectPoint, lineColor);//画浅蓝线
						}
					}
				}
			}
		}
	}
	/// <summary>
	/// 绘制框选框
	/// </summary>
	void DrawFrame()
	{
		if (isDrawFrame)
		{
			DrawLine(new Rect(frameStartPos, frameEndPos - frameStartPos), new Color(0.664f, 0.797f, 0.93f, 0.5f));
			DrawLine(new Rect(frameStartPos, new Vector2((frameEndPos - frameStartPos).x, 1)), new Color(0f, 0.469f, 0.84f, 0.8f));
			DrawLine(new Rect(frameStartPos, new Vector2(1, (frameEndPos - frameStartPos).y)), new Color(0f, 0.469f, 0.84f, 0.8f));
			DrawLine(new Rect(frameEndPos, new Vector2(-(frameEndPos - frameStartPos).x, 1)), new Color(0f, 0.469f, 0.84f, 0.8f));
			DrawLine(new Rect(frameEndPos, new Vector2(1, -(frameEndPos - frameStartPos).y)), new Color(0f, 0.469f, 0.84f, 0.8f));
		}
	}
	/// <summary>
	/// 处理框选中的节点框
	/// </summary>
	void DoFramedNode()
	{
		Rect frameSrc = new Rect(frameStartPos, frameEndPos - frameStartPos);
		Rect frame = new Rect(frameSrc);
		if (frameSrc.width < 0)
		{
			frame.x += frame.width;
			frame.width = -frame.width;
		}
		if (frameSrc.height < 0)
		{
			frame.y += frame.height;
			frame.height = -frame.height;
		}
		List<NodeRectEx> temp = new List<NodeRectEx>();
		foreach (var item in nodeRects)
		{
			if (frame.Contains(item.thisRect.position) && frame.Contains(item.thisRect.position + item.thisRect.size))
			{
				temp.Add(item);
			}
		}
		if (temp.Count > 0)
		{
			selectedNodes.AddRange(temp);
			selectedNodes = selectedNodes.Distinct().ToList();//去重
		}
		else if (isDrawFrame)
		{
			selectedNodes.Clear();
		}
	}
	/// <summary>
	/// 设置提示信息
	/// </summary>
	void SetText()
	{
		if (tempIn != null)
			connectionState = ", 正在连接输出端：" + tempIn.socket.name;
		if (tempOut != null)
			connectionState = ", 正在连接输入端：" + tempOut.socket.name;
		if (tempIn == null && tempOut == null)
		{
			connectionState = "";
			connectionState = "";
		}
		if (selectedNodes.Count > 0)
			selectNum = ", 选中了" + selectedNodes.Count + "个节点";
		else
			selectNum = "";
	}
	/// <summary>
	/// 连接接口
	/// </summary>
	void DoConnect()
	{
		if (tempIn != null && tempOut != null)//线段左右端就绪
		{
			Undo.RecordObjects(new UnityEngine.Object[]{
				(tempIn.socket as NodeOutput).node,
				(tempOut.socket as NodeInput).node
			}, "Connect");
			(tempOut.socket as NodeInput).Connect(tempIn.socket as NodeOutput);//连接
			tempIn = tempOut = null;
		}
	}
	/// <summary>
	/// 绘制小地图
	/// </summary>
	void DrawMiniMap()
	{
		if (isMap)
		{
			float zoom = Mathf.Max(width / 250, height / 250);//小地图缩小倍率
			float w = width / zoom;//小地图长
			float h = height / zoom;//小地图宽
			float up = 33;//上下非节点图区域边距（滚动条+标签）
			float left = 49 + 15;//左右非节点图区域边距（按钮栏+滚动条）
			Vector2 p = new Vector2(position.width - 20 - w, position.height - 38 - h);//小地图左上角点
																					   //画背景
			EditorGUI.DrawRect(new Rect(p - Vector2.one, new Vector2(w + 2, 1)), Color.black);
			EditorGUI.DrawRect(new Rect(p - Vector2.one, new Vector2(1, h + 2)), Color.black);
			EditorGUI.DrawRect(new Rect(p - new Vector2(1, -h), new Vector2(w + 2, 1)), Color.black);
			EditorGUI.DrawRect(new Rect(p - new Vector2(-w, 1), new Vector2(1, h + 2)), Color.black);
			EditorGUI.DrawRect(new Rect(p, new Vector2(w, h)), new Color(0.5f, 0.5f, 0.5f, 0.25f));
			//画视野
			EditorGUI.DrawRect(new Rect(new Vector2(scrollPos.x / width * w, scrollPos.y / height * h) + p,
										new Vector2((position.width - left) / width * w, (position.height - up) / height * h)),
										new Color(0.25f, 0.25f, 0.25f, 0.5f));
			//画节点
			foreach (var item in nodeRects)
			{
				if (item.sockets.Count > 0)
				{
					Color c;
					switch (item.nodeType)
					{
						case NodeRectEx.NodeRectType.GraphInputs:
						case NodeRectEx.NodeRectType.GraphOutputs:
							c = nodeGraphIOColor; break;
						case NodeRectEx.NodeRectType.Comment:
							c = nodeCommentColor; break;
						default:
							if (item.node is NodeGraph)
								c = nodeGraphColor;
							else
								c = item.node.nodeColour;
							break;
					}
					EditorGUI.DrawRect(new Rect(item.thisRect.position / zoom +
												new Vector2(position.width - 20 - w, position.height - 38 - h),
												item.thisRect.size / zoom), c);
				}
			}
			Event e = Event.current;
			if (e.type == EventType.MouseDown && e.button == 0)
			{
				if (new Rect(p, new Vector2(w, h)).Contains(e.mousePosition))
				{
					isMapDragging = true;
				}
			}
			if (isMapDragging)
			{
				isDrawFrame = false;//取消框选功能
				scrollPos = (e.mousePosition - p - new Vector2((position.width - left) / width * w, (position.height - up) / height * h) / 2) * zoom;
				Repaint();
			}
			if (e.type == EventType.MouseUp && e.button == 0)
			{
				isMapDragging = false;
			}
		}
	}
	/// <summary>
	/// 设置不同操作时鼠标样式
	/// </summary>
	void SetMouseStyle()
	{
		if (isScrolling)
			EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Pan);
		if (isDragging)
			EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
		if (isSelectionsMoving)
			EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.MoveArrow);
		if (isMapDragging)
			EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Zoom);
	}
	/// <summary>
	/// 设置无节点字
	/// </summary>
	void SetNoNode()
	{
		if (nodeRects.Count <= 0)
		{
			GUI.Label(new Rect(Vector2.zero, new Vector2(150, 60)) { center = new Rect(50, 0, position.width - 50, position.height).center }, new GUIContent() { text = "<size=40><color=red>无节点</color></size>" });
		}
	}
	/// <summary>
	/// 打开右键菜单
	/// </summary>
	void ShowRightMenu()
	{
		Event e = Event.current;
		if (e.type == EventType.MouseDown && e.button == 1)
		{
			GenericMenu gm = new GenericMenu();
			gm.AddItem(new GUIContent("添加节点/Net Signal"), false, HandleMenu, typeof(NetSignal));
			gm.AddItem(new GUIContent("添加节点/Signal Unity Event"), false, HandleMenu, typeof(SignalUnityEvent));
			gm.AddItem(new GUIContent("添加节点/SignalCombine"), false, HandleMenu, typeof(HumanAPI.SignalCombine));
			gm.AddItem(new GUIContent("添加节点/SignalTime"), false, HandleMenu, typeof(SignalTime));
			gm.AddItem(new GUIContent("添加节点/SignalMathInRange"), false, HandleMenu, typeof(SignalMathInRange));

			gm.AddItem(new GUIContent("添加节点/Signal/Signal Accumulate"), false, HandleMenu, typeof(SignalAccumulate));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Distance"), false, HandleMenu, typeof(SignalDistance));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Hold"), false, HandleMenu, typeof(SignalHold));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Latch"), false, HandleMenu, typeof(SignalLatch));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Once"), false, HandleMenu, typeof(SignalOnce));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Set Scale"), false, HandleMenu, typeof(SignalSetScale));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Set Rotation"), false, HandleMenu, typeof(SignalSetRotation));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Set Translation"), false, HandleMenu, typeof(SignalSetTranslation));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Teleport"), false, HandleMenu, typeof(SignalTeleport));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Toggle"), false, HandleMenu, typeof(SignalToggle));
			gm.AddItem(new GUIContent("添加节点/Signal/SignalSmooth"), false, HandleMenu, typeof(SignalSmooth));
			gm.AddItem(new GUIContent("添加节点/Signal/SignalCycler"), false, HandleMenu, typeof(SignalCycler));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Counter"), false, HandleMenu, typeof(SignalCounter));
			gm.AddItem(new GUIContent("添加节点/Signal/SignalAmbientLight"), false, HandleMenu, typeof(SignalAmbientLight));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Select"), false, HandleMenu, typeof(SignalSelect));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Gravity"), false, HandleMenu, typeof(SignalGravity));
			gm.AddItem(new GUIContent("添加节点/Signal/Signal Release"), false, HandleMenu, typeof(SignalRelease));

			gm.AddItem(new GUIContent("添加节点/Math/SignalMathAbs"), false, HandleMenu, typeof(SignalMathAbs));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathAdd"), false, HandleMenu, typeof(SignalMathAdd));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathClamp"), false, HandleMenu, typeof(SignalMathClamp));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathDiv"), false, HandleMenu, typeof(SignalMathDiv));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathFract"), false, HandleMenu, typeof(SignalMathFract));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathInverseLerp"), false, HandleMenu, typeof(SignalMathInverseLerp));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathInvert"), false, HandleMenu, typeof(SignalMathInvert));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathLerp"), false, HandleMenu, typeof(SignalMathLerp));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathMul"), false, HandleMenu, typeof(SignalMathMul));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathNegate"), false, HandleMenu, typeof(SignalMathNegate));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathSin"), false, HandleMenu, typeof(SignalMathSin));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathThreshold"), false, HandleMenu, typeof(SignalMathThreshold));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathCompare"), false, HandleMenu, typeof(SignalMathCompare));
			gm.AddItem(new GUIContent("添加节点/Math/Signal Math Constant"), false, HandleMenu, typeof(SignalMathConstant));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathModulo"), false, HandleMenu, typeof(SignalMathModulo));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathRandom"), false, HandleMenu, typeof(SignalMathRandom));
			gm.AddItem(new GUIContent("添加节点/Math/SignalMathRound"), false, HandleMenu, typeof(SignalMathRound));

			gm.AddItem(new GUIContent("添加节点/Lerp/Lerp Emission"), false, HandleMenu, typeof(LerpEmission));
			gm.AddItem(new GUIContent("添加节点/Lerp/Lerp Light Intensity"), false, HandleMenu, typeof(LerpLightIntensity));
			gm.AddItem(new GUIContent("添加节点/Lerp/Lerp Particles"), false, HandleMenu, typeof(LerpParticles));
			gm.AddItem(new GUIContent("添加节点/Lerp/Signal Lerp Volume"), false, HandleMenu, typeof(SignalLerpVolume));

			gm.AddItem(new GUIContent("添加节点/Trigger/Trigger Volume"), false, HandleMenu, typeof(TriggerVolume));
			gm.AddItem(new GUIContent("添加节点/Trigger/Collider Label Trigger Volume"), false, HandleMenu, typeof(ColliderLabelTriggerVolume));
			gm.AddItem(new GUIContent("添加节点/Trigger/Collision By Layer Sensor"), false, HandleMenu, typeof(CollisionByLayerSensor));
			gm.AddItem(new GUIContent("添加节点/Trigger/Collision By Tag Sensor"), false, HandleMenu, typeof(CollisionByTagSensor));
			gm.AddItem(new GUIContent("添加节点/Trigger/Grab Sensor"), false, HandleMenu, typeof(GrabSensor));
			gm.AddSeparator("");
			gm.AddItem(new GUIContent("刷新"), false, DoRefresh);
			gm.AddItem(new GUIContent("对齐网格"), false, DoSnap);
			gm.AddSeparator("");
			gm.AddItem(new GUIContent("切换小地图"), isMap, () => isMap = !isMap);
			gm.ShowAsContext();
		}
	}
	/// <summary>
	/// 处理菜单事件
	/// </summary>
	/// <param name="id">脚本</param>
	void HandleMenu(object script)
	{
		GameObject tar = null;
		try
		{
			if (!(script is NetSignal))
			{
				switch (addLocation)
				{
					case NodeAddLocation.OnRoot: tar = activeGraph.gameObject; break;
					case NodeAddLocation.OnSelection: tar = Selection.activeGameObject; break;
				}
				Undo.AddComponent(tar, (Type)script);
			}
			else
			{
				switch (addNetLocation)
				{
					case NodeAddLocation.OnRoot: tar = activeGraph.gameObject; break;
					case NodeAddLocation.OnSelection: tar = Selection.activeGameObject; break;
				}
				Undo.AddComponent(tar, (Type)script);
			}
		}
		catch (NullReferenceException) { return; }
	}

	/// <summary>
	/// 即将加载的节点图
	/// </summary>
	NodeGraph pendingGraph;
	/// <summary>
	/// 当前显示的节点图
	/// </summary>
	public NodeGraph activeGraph;
	/// <summary>
	/// 当前图中节点框列表
	/// </summary>
	List<NodeRectEx> nodeRects = new List<NodeRectEx>();
	/// <summary>
	/// 当前图中节点列表
	/// </summary>
	List<Node> graphNodes = new List<Node>();
	/// <summary>
	/// 节点接口对应节点接口框
	/// </summary>
	Dictionary<NodeSocket, NodeSocketRectEx> sockets2Rect = new Dictionary<NodeSocket, NodeSocketRectEx>();
	/// <summary>
	/// 框选中的节点们
	/// </summary>
	public static List<NodeRectEx> selectedNodes = new List<NodeRectEx>();
	/// <summary>
	/// 选中节点框相对偏移量
	/// </summary>
	List<Vector2> selectionDelta = new List<Vector2>();

	/// <summary>
	/// 拖拽开始位置
	/// </summary>
	Vector2 dragStartPos;
	/// <summary>
	/// 拖拽结束位置
	/// </summary>
	Vector2 dragEndPos;
	/// <summary>
	/// 提示线起始位置
	/// </summary>
	Vector2 hintStartPos;
	/// <summary>
	/// 提示线结束位置
	/// </summary>
	Vector2 hintEndPos;
	/// <summary>
	/// 滚动条位置
	/// </summary>
	Vector2 scrollPos;
	/// <summary>
	/// 框选起始位置
	/// </summary>
	Vector2 frameStartPos;
	/// <summary>
	/// 框选结束位置
	/// </summary>
	Vector2 frameEndPos;
	/// <summary>
	/// 鼠标位置（显示用）
	/// </summary>
	Vector2 mousePos;
	/// <summary>
	/// 接口连接状态（显示用）
	/// </summary>
	string connectionState;
	/// <summary>
	/// 当前选中的节点数（显示用）
	/// </summary>
	string selectNum;
	string _temp;
	string _temp2;
	NodeSocketRectEx tempOut = null;
	NodeSocketRectEx tempIn = null;
	bool isUpdateSelection;

	/// <summary>
	/// 是否实时更新
	/// </summary>
	static bool isLiveUpdate;
	/// <summary>
	/// 是否跟踪选中节点
	/// </summary>
	static bool isTrackSelection = true;
	/// <summary>
	/// 是否显示小地图
	/// </summary>
	static bool isMap = true;

	/// <summary>
	/// 鼠标是否正在连线
	/// </summary>
	bool isDragging;
	/// <summary>
	/// 是否正在滚动
	/// </summary>
	bool isScrolling;
	/// <summary>
	/// 是否正在框选
	/// </summary>
	bool isDrawFrame;
	/// <summary>
	/// 小地图是否正在拖拽
	/// </summary>
	bool isMapDragging;
	/// <summary>
	/// 是否正在框选移动
	/// </summary>
	bool isSelectionsMoving;

	/// <summary>
	/// Up按钮是否启用
	/// </summary>
	bool enUp;
	/// <summary>
	/// 左侧第二组按钮是否启用
	/// </summary>
	bool enAll;

	/// <summary>
	/// 接口小圈圈
	/// </summary>
	public static Sprite circle;
	public static Sprite square;


	/////////////////
	/// <summary>
	/// 节点图宽度
	/// </summary>
	public static float width = 3200;
	/// <summary>
	/// 节点图高度
	/// </summary>
	public static float height = 2400;
	/// <summary>
	/// 网格X宽度
	/// </summary>
	public static float gridX = 32;
	/// <summary>
	/// 网格Y宽度
	/// </summary>
	public static float gridY = 32;
	/// <summary>
	/// 网格背景色
	/// </summary>
	public static Color gridBgColor = new Color(0.75f, 0.75f, 0.75f);
	/// <summary>
	/// 网格前景色
	/// </summary>
	public static Color gridLineColor = new Color(0.65f, 0.65f, 0.65f);
	/// <summary>
	/// 节点连接线颜色
	/// </summary>
	public static Color lineColor = new Color(0.7f, 0.7f, 1f);
	/// <summary>
	/// 正在连接的连接线颜色
	/// </summary>
	public static Color connectingColor = Color.white;
	/// <summary>
	/// 是否开启连线提示
	/// </summary>
	public static bool enHint = true;
	/// <summary>
	/// 提示连接线颜色
	/// </summary>
	public static Color lineHintColor = Color.red;

	/// <summary>
	/// 节点图节点颜色
	/// </summary>
	public static Color nodeGraphColor = new Color(1f, 0.9f, 0.7f);
	/// <summary>
	/// 节点图I/O节点颜色
	/// </summary>
	public static Color nodeGraphIOColor = new Color(1f, 0.7f, 0.7f);
	/// <summary>
	/// 注释节点颜色
	/// </summary>
	public static Color nodeCommentColor = Color.green;
	/// <summary>
	/// 节点高亮框颜色
	/// </summary>
	public static Color nodeHighlightColor = new Color(1f, 0.5f, 0.3f, 0.5f);
	/// <summary>
	/// 节点接口颜色最小值
	/// </summary>
	public static float minCircleValue = 0.4999f;
	/// <summary>
	/// 节点接口颜色最大值
	/// </summary>
	public static float maxCircleValue = 0.5f;
	/// <summary>
	/// 节点接口颜色
	/// </summary>
	public static Gradient circleColor = new Gradient();
	/// <summary>
	/// 刷新节点图快捷键
	/// </summary>
	public static KeyCode k_refresh = KeyCode.R;
	/// <summary>
	/// 对齐网格快捷键
	/// </summary>
	public static KeyCode k_snap = KeyCode.Q;
	/// <summary>
	/// 属性框是否失焦关闭
	/// </summary>
	public static bool isCloseOnLostFocus = true;
	/// <summary>
	/// 组件添加位置
	/// </summary>
	public static NodeAddLocation addLocation = NodeAddLocation.OnRoot;
	/// <summary>
	/// NetSignal添加位置
	/// </summary>
	public static NodeAddLocation addNetLocation = NodeAddLocation.OnRoot;
	public enum NodeAddLocation
	{
		OnRoot,
		OnSelection
	}
}

/// <summary>
/// 节点框类
/// </summary>
public class NodeRectEx
{
	public NodeRectEx(NodeWindowEx win)
	{
		parentWindow = win;
	}
	/// <summary>
	/// 初始化普通节点
	/// </summary>
	/// <param name="node"></param>
	public void Initialize(Node node)
	{
		nodeType = NodeRectType.Node;//设置类型
		this.node = node;//写入节点
		sockets.Clear();
		List<NodeSocket> list = node.ListNodeSockets();//获取该节点上所有的接口
		for (int i = 0; i < list.Count; i++)//为所有的接口添加接口框
		{
			sockets.Add(new NodeSocketRectEx(list[i], this));
		}
	}
	/// <summary>
	/// 初始化节点图输入节点
	/// </summary>
	/// <param name="node"></param>
	public void InitializeGraphInput(NodeGraph node)
	{
		nodeType = NodeRectType.GraphInputs;
		this.node = node;
		sockets.Clear();
		for (int i = 0; i < node.inputs.Count; i++)
		{
			sockets.Add(new NodeSocketRectEx(node.inputs[i].inputSocket, this) { allowEdit = true });
		}
	}
	/// <summary>
	/// 初始化节点图输出节点
	/// </summary>
	/// <param name="node"></param>
	public void InitializeGraphOutput(NodeGraph node)
	{
		nodeType = NodeRectType.GraphOutputs;
		this.node = node;
		sockets.Clear();
		for (int i = 0; i < node.outputs.Count; i++)
		{
			sockets.Add(new NodeSocketRectEx(node.outputs[i].outputSocket, this) { allowEdit = true });
		}
	}
	/// <summary>
	/// 初始化注释节点
	/// </summary>
	/// <param name="node"></param>
	public void InitializeComment(Node node)
	{
		nodeType = NodeRectType.Comment;
		this.node = node;
		sockets.Clear();
	}
	/// <summary>
	/// 更新节点框布局
	/// </summary>
	public void UpdateLayout()
	{
		float nodeX = 150f;//节点最小宽度
		float nodeY = 20f;//节点最小高度
		if (nodeType == NodeRectType.Comment)
		{
			Vector2 vector = GUI.skin.label.CalcSize(new GUIContent((node as NodeComment).comment));//计算注释节点里的字占用空间大小
			nodeX = Mathf.Max(nodeX, 32f + vector.x);
			nodeY = vector.y;
		}
		thisRect = new Rect(nodePos.x, nodePos.y, nodeX, 16f + nodeY + 4f + sockets.Count * 16f);//高度：标题16，内容，下边距4，每个接口
		float num = 36f;//节点顶部到按钮底部的距离
		for (int i = 0; i < sockets.Count; i++)//绘制接口框
		{
			sockets[i].UpdateLayout(new Rect(0f, num, nodeX, 16f));
			num += 16f;
		}
	}
	/// <summary>
	/// 渲染窗口
	/// </summary>
	/// <param name="id"></param>
	/// <param name="drawHighlight">是否高亮</param>
	/// <returns>渲染完成状态</returns>
	public bool RenderWindow(int id, bool drawHighlight)
	{
		if (node == null)
			return false;
		switch (nodeType)//设置节点背景色
		{
			case NodeRectType.Node:
				if (node is NodeGraph)//如果是节点图
				{
					GUI.backgroundColor = NodeWindowEx.nodeGraphColor;
				}
				else//普通节点
				{
					GUI.backgroundColor = node.nodeColour;//设置成节点的颜色
				}
				break;
			case NodeRectType.GraphInputs://节点图输入输出节点
			case NodeRectType.GraphOutputs:
				GUI.backgroundColor = NodeWindowEx.nodeGraphIOColor;
				break;
			case NodeRectType.Comment://注释节点
				GUI.backgroundColor = NodeWindowEx.nodeCommentColor;
				break;
		}
		thisRect = GUI.Window(id, thisRect, Render, node.Title);//创建一个窗口，同时绘制Render里的东西
		if (nodePos != thisRect.position)
		{
			Undo.RecordObject(node, "Move");
			nodePos = thisRect.position;//记录节点图上的节点移动
			UpdateLayout();
		}
		if (drawHighlight)//如果高亮
		{
			DrawHighlight(thisRect);//画个高亮框
		}
		return true;
	}
	/// <summary>
	/// 在节点框周围绘制高亮框
	/// </summary>
	/// <param name="thisRect">节点框</param>
	void DrawHighlight(Rect thisRect)
	{
		EditorGUI.DrawRect(new Rect(thisRect.x - 8, thisRect.y - 8, thisRect.width + 16, 4), NodeWindowEx.nodeHighlightColor);
		EditorGUI.DrawRect(new Rect(thisRect.x - 8, thisRect.y + thisRect.height + 4, thisRect.width + 16, 4), NodeWindowEx.nodeHighlightColor);
		EditorGUI.DrawRect(new Rect(thisRect.x - 8, thisRect.y - 4, 4, thisRect.height + 8), NodeWindowEx.nodeHighlightColor);
		EditorGUI.DrawRect(new Rect(thisRect.x + thisRect.width + 4, thisRect.y - 4, 4, thisRect.height + 8), NodeWindowEx.nodeHighlightColor);
	}
	/// <summary>
	/// 渲染节点框
	/// </summary>
	/// <param name="id"></param>
	public void Render(int id)
	{
		GUI.color = Color.gray;//节点按钮默认颜色
		GUI.skin.label.alignment = TextAnchor.UpperCenter;//字设置为居中
		if (node is NodeGraph && nodeType == NodeRectType.Node)//如果是黄色节点
		{
			if (GUI.Button(new Rect(8f, 16f, 102f, 20f), node.name))//绘制按钮
			{
				Selection.activeGameObject = node.gameObject;
				NodeWindowEx.selectedNodes.Add(this);
				NodeWindowEx.selectedNodes = NodeWindowEx.selectedNodes.Distinct().ToList();//去重
			}
			if (GUI.Button(new Rect(110f, 16f, 32f, 20f), "▼"))//绘制↓按钮
			{
				NodeWindowEx.Init(node as NodeGraph);//打开下层节点图
			}
		}
		else if (nodeType == NodeRectType.Comment)//如果是绿色节点
		{
			GUI.color = NodeWindowEx.nodeCommentColor;
			string text = EditorGUI.TextArea(new Rect(8, 16, thisRect.width - 16, thisRect.height - 16 - 4), (node as NodeComment).comment);
			if (text != ((NodeComment)node).comment)//节点图里改了字要赋值回去
			{
				((NodeComment)node).comment = text;
				UpdateLayout();
			}
		}
		else if (GUI.Button(new Rect(8f, 16f, 134f, 20f), node.name))//如果是灰色节点和红色节点
		{
			Selection.activeGameObject = node.gameObject;
			NodeWindowEx.selectedNodes.Add(this);
			NodeWindowEx.selectedNodes = NodeWindowEx.selectedNodes.Distinct().ToList();//去重
		}
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		GUI.color = Color.white;
		for (int i = 0; i < sockets.Count; i++)
		{
			sockets[i].Render();
		}
		GUI.DragWindow();
	}
    /// <summary>
    /// 判断选中的接口框
    /// </summary>
    /// <param name="pos">鼠标位置</param>
    /// <returns>选中的接口框</returns>
    public NodeSocketRectEx HitTest(Vector2 pos)
    {
        pos.x -= thisRect.x;
        pos.y -= thisRect.y;
        for (int i = 0; i < sockets.Count; i++)
        {
            if (sockets[i].HitTest(pos))
            {
                return sockets[i];
            }
        }
        return null;
    }
    /// <summary>
    /// 判断是否命中自己
    /// </summary>	
    public bool HitTest2(Vector2 pos)
    {
        return thisRect.Contains(pos);
    }
    /// <summary>
    /// 节点框类型
    /// </summary>
    public NodeRectType nodeType
	{
		get;
		set;
	}
	/// <summary>
	/// 节点框位置
	/// </summary>
	public Vector2 nodePos
	{
		get
		{
			NodeRectType nodeRectType = nodeType;
			Vector2 vector;
			if (nodeRectType != NodeRectType.GraphInputs)
			{
				if (nodeRectType != NodeRectType.GraphOutputs)
				{
					vector = node.pos;
				}
				else
				{
					vector = (node as NodeGraph).outputsPos;
				}
			}
			else
			{
				vector = (node as NodeGraph).inputsPos;
			}
			vector.x = Mathf.Clamp(vector.x, 0f, NodeWindowEx.width - 150);
			vector.y = Mathf.Clamp(vector.y, 0f, NodeWindowEx.height - 70);
			return vector;
		}
		set
		{
			NodeRectType nodeRectType = nodeType;
			if (nodeRectType == NodeRectType.GraphInputs)
			{
				(node as NodeGraph).inputsPos = value;
				return;
			}
			if (nodeRectType != NodeRectType.GraphOutputs)
			{
				node.pos = value;
				return;
			}
			(node as NodeGraph).outputsPos = value;
		}
	}
	/// <summary>
	/// 该节点框所属节点图窗口
	/// </summary>
	public NodeWindowEx parentWindow;
	/// <summary>
	/// 该节点框上所有的接口框
	/// </summary>
	public List<NodeSocketRectEx> sockets = new List<NodeSocketRectEx>();
	/// <summary>
	/// 该节点框所属节点
	/// </summary>
	public Node node;
	/// <summary>
	/// 该节点框大小 
	/// </summary>
	public Rect thisRect;
	/// <summary>
	/// 节点框类型
	/// </summary>
	public enum NodeRectType
	{
		/// <summary>
		/// 普通节点
		/// </summary>
		Node,
		/// <summary>
		/// 节点图输入节点
		/// </summary>
		GraphInputs,
		/// <summary>
		/// 节点图输出节点
		/// </summary>
		GraphOutputs,
		/// <summary>
		/// 注释节点
		/// </summary>
		Comment
	}
}

/// <summary>
/// 节点接口框类
/// </summary>
public class NodeSocketRectEx
{
	public NodeSocketRectEx(NodeSocket ns, NodeRectEx nr)
	{
		socket = ns;
		nodeRect = nr;
	}
	/// <summary>
	/// 更新接口框位置
	/// </summary>
	/// <param name="rect">新位置</param>
	public void UpdateLayout(Rect rect)
	{
		thisRect = rect;
	}
	/// <summary>
	/// 渲染
	/// </summary>
	/// <param name="socket">接口</param>
	public void Render()
	{
		//取值
		float num = 0f;
		if (EditorApplication.isPlaying)
		{
			if (socket is NodeInput)
			{
				num = (socket as NodeInput).value;
			}
			if (socket is NodeOutput)
			{
				num = (socket as NodeOutput).value;
			}
		}
		else
		{
			if (socket is NodeInput)
			{
				num = (socket as NodeInput).initialValue;
			}
			if (socket is NodeOutput)
			{
				num = (socket as NodeOutput).initialValue;
			}
		}
		//画框
		if (socket is NodeInput || socket is NodeEntry)
		{
			hitRect = new Rect(thisRect.x, thisRect.y, 16, 16);
			GUI.Label(new Rect(thisRect.x + 16, thisRect.y, 134, 16), socket.name + ":" + num.ToString("0.###"));
		}
		else
		{
			hitRect = new Rect(thisRect.x + 134, thisRect.y, 16, 16);
			GUI.skin.label.alignment = TextAnchor.UpperRight;
			if (!((socket.node is HumanAPI.SignalCombine) && ((HumanAPI.SignalCombine)socket.node).invert))
				GUI.Label(new Rect(thisRect.x, thisRect.y, 134, 16), socket.name + ":" + num.ToString("0.###"));
			else
				GUI.Label(new Rect(thisRect.x, thisRect.y, 134, 16), "(" + socket.name + ":" + num.ToString("0.###") + ")");
			GUI.skin.label.alignment = TextAnchor.UpperLeft;
		}
		GUI.color = NodeWindowEx.circleColor.Evaluate(Mathf.InverseLerp(NodeWindowEx.minCircleValue, NodeWindowEx.maxCircleValue, num));
		GUI.DrawTextureWithTexCoords(new Rect(hitRect.x - 1, hitRect.y - 1, 18, 18), NodeWindowEx.circle.texture, new Rect(0, 0, 1, 1));
		GUI.color = Color.white;
	}
	/// <summary>
	/// 检测鼠标是否命中当前接口框
	/// </summary>
	/// <param name="localPos">鼠标位置</param>
	/// <returns>命中标志</returns>
	public bool HitTest(Vector2 localPos)
	{
		return hitRect.Contains(localPos);
	}
	/// <summary>
	/// 该接口连接点位置
	/// </summary>
	public Vector2 connectPoint
	{
		get
		{
			if (socket is NodeInput || socket is NodeEntry)
				return new Vector2(thisRect.xMin + nodeRect.thisRect.x, thisRect.center.y + nodeRect.thisRect.y);
			else
				return new Vector2(thisRect.xMax + nodeRect.thisRect.x, thisRect.center.y + nodeRect.thisRect.y);
		}
	}
	/// <summary>
	/// 该布局对应的接口
	/// </summary>
	public NodeSocket socket;
	/// <summary>
	/// 所在的节点框
	/// </summary>
	public NodeRectEx nodeRect;
	public bool allowEdit;
	/// <summary>
	/// 接口框整个大小（150 x 16）px
	/// </summary>
	public Rect thisRect;
	/// <summary>
	/// 命中判定框
	/// </summary>
	public Rect hitRect;
}