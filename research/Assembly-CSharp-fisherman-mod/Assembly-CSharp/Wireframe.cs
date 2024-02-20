using System;
using System.Collections;
using UnityEngine;

public class Wireframe : MonoBehaviour
{
	private void Start()
	{
		if (this.lineMaterial == null)
		{
			this.lineMaterial = new Material("Shader \"Lines/Colored Blended\" {SubShader { Pass {   BindChannels { Bind \"Color\",color }   Blend SrcAlpha OneMinusSrcAlpha   ZWrite on Cull Off Fog { Mode Off }} } }");
		}
		this.lineMaterial.hideFlags = 61;
		this.lineMaterial.shader.hideFlags = 61;
		this.lines_List = new ArrayList();
		MeshFilter component = base.gameObject.GetComponent<MeshFilter>();
		Mesh mesh = component.mesh;
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		int num = 0;
		while (num + 2 < triangles.Length)
		{
			this.lines_List.Add(vertices[triangles[num]]);
			this.lines_List.Add(vertices[triangles[num + 1]]);
			this.lines_List.Add(vertices[triangles[num + 2]]);
			num += 3;
		}
		this.lines = (Vector3[])this.lines_List.ToArray(typeof(Vector3));
		this.lines_List.Clear();
		this.size = this.lines.Length;
	}

	private void DrawQuad(Vector3 p1, Vector3 p2)
	{
		float num = 1f / (float)Screen.width * this.lineWidth * 0.5f;
		Vector3 vector = Camera.main.transform.position - (p2 + p1) / 2f;
		Vector3 vector2 = p2 - p1;
		Vector3 vector3 = Vector3.Cross(vector, vector2).normalized * num;
		GL.Vertex(p1 - vector3);
		GL.Vertex(p1 + vector3);
		GL.Vertex(p2 + vector3);
		GL.Vertex(p2 - vector3);
	}

	private Vector3 to_world(Vector3 vec)
	{
		return base.gameObject.transform.TransformPoint(vec);
	}

	private void OnRenderObject()
	{
		base.gameObject.GetComponent<Renderer>().enabled = this.render_mesh_normaly;
		if (this.lines == null || (float)this.lines.Length < this.lineWidth)
		{
			MonoBehaviour.print("No lines");
		}
		else
		{
			this.lineMaterial.SetPass(0);
			GL.Color(this.lineColor);
			if (this.lineWidth == 1f)
			{
				GL.Begin(1);
				int num = 0;
				while (num + 2 < this.lines.Length)
				{
					Vector3 vector = this.to_world(this.lines[num]);
					Vector3 vector2 = this.to_world(this.lines[num + 1]);
					Vector3 vector3 = this.to_world(this.lines[num + 2]);
					if (this.render_lines_1st)
					{
						GL.Vertex(vector);
						GL.Vertex(vector2);
					}
					if (this.render_lines_2nd)
					{
						GL.Vertex(vector2);
						GL.Vertex(vector3);
					}
					if (this.render_lines_3rd)
					{
						GL.Vertex(vector3);
						GL.Vertex(vector);
					}
					num += 3;
				}
			}
			else
			{
				GL.Begin(7);
				int num2 = 0;
				while (num2 + 2 < this.lines.Length)
				{
					Vector3 vector4 = this.to_world(this.lines[num2]);
					Vector3 vector5 = this.to_world(this.lines[num2 + 1]);
					Vector3 vector6 = this.to_world(this.lines[num2 + 2]);
					if (this.render_lines_1st)
					{
						this.DrawQuad(vector4, vector5);
					}
					if (this.render_lines_2nd)
					{
						this.DrawQuad(vector5, vector6);
					}
					if (this.render_lines_3rd)
					{
						this.DrawQuad(vector6, vector4);
					}
					num2 += 3;
				}
			}
			GL.End();
		}
	}

	public bool render_mesh_normaly = true;

	public bool render_lines_1st;

	public bool render_lines_2nd;

	public bool render_lines_3rd;

	public Color lineColor = new Color(0f, 1f, 1f);

	public Color backgroundColor = new Color(0f, 0.5f, 0.5f);

	public bool ZWrite = true;

	public bool AWrite = true;

	public bool blend = true;

	public float lineWidth = 3f;

	public int size;

	private Vector3[] lines;

	private ArrayList lines_List;

	public Material lineMaterial;
}
