using System;
using System.Collections.Generic;
using UnityEngine;

public class HUDLinesDrawer : MonoBehaviour
{
	private void Start()
	{
		HUDLinesDrawer.Instance = this;
		this.lines = new List<HUDLinesDrawer.Line>();
		if (Application.isPlaying)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	private void Update()
	{
	}

	public void Clear()
	{
		this.lines.Clear();
	}

	public void DrawGizmoPlot(float[] y, float miny, float maxy, Vector2 lbPos, Vector2 rtPos, Color lineColor, Color frameColor, bool drawFrame = true)
	{
		float num = rtPos.x - lbPos.x;
		float num2 = rtPos.y - lbPos.y;
		HUDLinesDrawer.Line line = new HUDLinesDrawer.Line(lineColor, y.Length);
		HUDLinesDrawer.Line line2 = new HUDLinesDrawer.Line(frameColor, 5);
		if (drawFrame)
		{
			line2.points[0] = lbPos;
			line2.points[1] = new Vector2(rtPos.x, lbPos.y);
			line2.points[2] = rtPos;
			line2.points[3] = new Vector2(lbPos.x, rtPos.y);
			line2.points[4] = lbPos;
		}
		Vector2 zero = Vector2.zero;
		for (int i = 0; i < y.Length; i++)
		{
			Vector2 vector;
			vector..ctor((float)i / (float)y.Length, (y[i] - miny) / (maxy - miny));
			line.points[i] = lbPos + new Vector2(vector.x * num, vector.y * num2);
		}
		this.lines.Add(line2);
		this.lines.Add(line);
	}

	public void DrawGizmoPlotBars(float[] y, float miny, float maxy, Vector2 lbPos, Vector2 rtPos, Color lineColor, Color frameColor, bool drawFrame = true)
	{
		float num = rtPos.x - lbPos.x;
		float num2 = rtPos.y - lbPos.y;
		HUDLinesDrawer.Line line = new HUDLinesDrawer.Line(lineColor, y.Length * 4);
		HUDLinesDrawer.Line line2 = new HUDLinesDrawer.Line(frameColor, 5);
		if (drawFrame)
		{
			line2.points[0] = lbPos;
			line2.points[1] = new Vector2(rtPos.x, lbPos.y);
			line2.points[2] = rtPos;
			line2.points[3] = new Vector2(lbPos.x, rtPos.y);
			line2.points[4] = lbPos;
		}
		for (int i = 0; i < y.Length; i++)
		{
			Vector2 vector;
			vector..ctor((float)i / (float)y.Length, (y[i] - miny) / (maxy - miny));
			float num3 = (float)(i + 1) / (float)y.Length;
			line.points[i * 4] = lbPos + new Vector2(vector.x * num, 0f);
			line.points[i * 4 + 1] = lbPos + new Vector2(vector.x * num, vector.y * num2);
			line.points[i * 4 + 2] = lbPos + new Vector2(num3 * num, vector.y * num2);
			line.points[i * 4 + 3] = lbPos + new Vector2(num3 * num, 0f);
		}
		this.lines.Add(line2);
		this.lines.Add(line);
	}

	private void OnPostRender()
	{
		if (!this.mat)
		{
			Debug.LogError("Please Assign a material on the inspector");
			return;
		}
		GL.PushMatrix();
		this.mat.SetPass(0);
		GL.LoadOrtho();
		for (int i = 0; i < this.lines.Count; i++)
		{
			GL.Begin(1);
			GL.Color(this.lines[i].color);
			for (int j = 0; j < this.lines[i].points.Length - 1; j++)
			{
				GL.Vertex(new Vector2(this.lines[i].points[j].x / (float)Screen.width, this.lines[i].points[j].y / (float)Screen.height));
				GL.Vertex(new Vector2(this.lines[i].points[j + 1].x / (float)Screen.width, this.lines[i].points[j + 1].y / (float)Screen.height));
			}
			GL.End();
		}
		GL.PopMatrix();
	}

	private void OnDestroy()
	{
		HUDLinesDrawer.Instance = null;
	}

	public static HUDLinesDrawer Instance;

	public Material mat;

	public List<HUDLinesDrawer.Line> lines;

	public struct Line
	{
		public Line(Color color, int count)
		{
			this.color = color;
			this.points = new Vector2[count];
		}

		public Color color;

		public Vector2[] points;
	}
}
