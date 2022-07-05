using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 配置文件
/// </summary>
public class NodeWindowSettings : ScriptableObject
{
	[Range(1650, 10000)]
	public float width = 3200;
	[Range(1100, 10000)]
	public float height = 2400;
	[Range(16, 256)]
	public float gridX = 32;
	[Range(16, 256)]
	public float gridY = 32;
	public Color gridBgColor = new Color(0.75f, 0.75f, 0.75f);
	public Color gridLineColor = new Color(0.65f, 0.65f, 0.65f);
	public Color lineColor = new Color(0.7f, 0.7f, 1f);
	public Color lineHighlightColor = new Color(1f, 0.5f, 0);
	public Color connectingColor = Color.white;
	public bool enHint = true;
	public Color lineHintColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
	public Color nodeGraphColor = new Color(1f, 0.9f, 0.7f);
	public Color nodeGraphIOColor = new Color(1f, 0.7f, 0.7f);
	public Color nodeCommentColor = Color.green;
	public Color nodeHighlightColor = new Color(1f, 0.5f, 0.3f, 0.5f);
	public float minCircleValue = 0.4999f;
	public float maxCircleValue = 0.5f;
	public Gradient circleColor = new Gradient();
	public KeyCode k_refresh = KeyCode.R;
	public KeyCode k_snap = KeyCode.Q;
	public bool isCloseOnLostFocus = true;
	public NodeAddLocation addLocation = NodeAddLocation.OnRoot;
	public NodeAddLocation addNetLocation = NodeAddLocation.OnRoot;

	public void InitColor()
	{
		GradientColorKey[] colorKey = new GradientColorKey[2];
		colorKey[0].color = Color.white;
		colorKey[0].time = 0.0f;
		colorKey[1].color = Color.green;
		colorKey[1].time = 1.0f;
		GradientAlphaKey[] alphaKey = new GradientAlphaKey[1];
		alphaKey[0].alpha = 1.0f;
		alphaKey[0].time = 1f;
		circleColor.SetKeys(colorKey, alphaKey);
	}
	public enum NodeAddLocation
	{
		OnRoot = 1,
		OnSelection = 2
	}
}