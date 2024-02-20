using System;
using System.Collections.Generic;
using Phy;
using UnityEngine;

public class RaycastCacheTest : MonoBehaviour
{
	private void Start()
	{
		this.tree = new QuadTree<PointXZResident>(this.minXZ.x, this.minXZ.y, this.maxXZ.x, this.maxXZ.y, 0);
		this.triangulation = new HeightFieldTriangulation(this.minXZ.x, this.minXZ.y, this.maxXZ.x, this.maxXZ.y, 5, 10);
		this.triIDs = new List<int>();
		this.hfqt = new HeightFieldQuadTree<PointXZResident>(this.minXZ.x, this.minXZ.y, this.maxXZ.x, this.maxXZ.y, 0f, 0);
		this.hfqt.Procreate(3);
	}

	private void drawTree<T>(QuadTree<T> tree) where T : class, IPlaneXZResident
	{
		Debug.DrawLine(new Vector3(tree.minX, 0f, tree.minZ), new Vector3(tree.maxX, 0f, tree.minZ), Color.green);
		Debug.DrawLine(new Vector3(tree.maxX, 0f, tree.minZ), new Vector3(tree.maxX, 0f, tree.maxZ), Color.green);
		Debug.DrawLine(new Vector3(tree.maxX, 0f, tree.maxZ), new Vector3(tree.minX, 0f, tree.maxZ), Color.green);
		Debug.DrawLine(new Vector3(tree.minX, 0f, tree.maxZ), new Vector3(tree.minX, 0f, tree.minZ), Color.green);
		for (int i = 0; i < 4; i++)
		{
			QuadTree<T> child = tree.GetChild(i);
			if (child == null)
			{
				break;
			}
			this.drawTree<T>(child);
		}
	}

	private void drawTriangleResidents(QuadTree<HeightFieldTriangulation.Triangle> tree)
	{
		this.triIDs.Clear();
		for (LinkedListNode<HeightFieldTriangulation.Triangle> linkedListNode = tree.FirstResident; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			HeightFieldTriangulation.Triangle value = linkedListNode.Value;
			this.triIDs.Add(value.ID);
			Color color = Color.blue;
			Vector3 vector = Vector3.zero;
			if (this.selectedNode != null && this.selectedNode.FirstResident != null && this.selectedNode.FirstResident.List.Contains(value))
			{
				color = Color.cyan;
				vector = Vector3.up * 0.01f;
			}
			if (value == this.selectedResident)
			{
				color = Color.red;
				vector = Vector3.up * 0.01f;
			}
			Debug.DrawLine(value.nodeA.position + vector, value.nodeB.position + vector, color);
			Debug.DrawLine(value.nodeB.position + vector, value.nodeC.position + vector, color);
			Debug.DrawLine(value.nodeC.position + vector, value.nodeA.position + vector, color);
		}
	}

	private void drawPointResidents(QuadTree<PointXZResident> tree)
	{
		for (LinkedListNode<PointXZResident> linkedListNode = tree.FirstResident; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			PointXZResident value = linkedListNode.Value;
			Vector3 position = value.position;
			if (value == this.selectedResident)
			{
				Debug.DrawLine(position - Vector3.right * 0.015f, position + Vector3.right * 0.015f, Color.red);
				Debug.DrawLine(position - Vector3.forward * 0.015f, position + Vector3.forward * 0.015f, Color.red);
			}
			else
			{
				Debug.DrawLine(position - Vector3.right * 0.01f, position + Vector3.right * 0.01f, Color.yellow);
				Debug.DrawLine(position - Vector3.forward * 0.01f, position + Vector3.forward * 0.01f, Color.yellow);
			}
		}
	}

	private void drawTriangle()
	{
		Color color = ((!Math3d.PointInsideTriangleXZ(this.point.position, this.triA.position, this.triB.position, this.triC.position)) ? Color.red : Color.green);
		Vector3 vector = Math3d.ClosestPointOnTriangleXZ(this.point.position, this.triA.position, this.triB.position, this.triC.position);
		Debug.DrawLine(this.point.position, vector, Color.yellow);
		Debug.DrawLine(this.triA.position, this.triB.position, color);
		Debug.DrawLine(this.triB.position, this.triC.position, color);
		Debug.DrawLine(this.triC.position, this.triA.position, color);
		Vector3 vector2 = Vector3.Min(this.rectA.position, this.rectB.position);
		Vector3 vector3 = Vector3.Max(this.rectA.position, this.rectB.position);
		Color color2 = ((!Math3d.TriangleOverlapAABBXZ(this.triA.position, this.triB.position, this.triC.position, vector2, vector3)) ? Color.red : Color.green);
		Debug.DrawLine(new Vector3(vector2.x, 0f, vector2.z), new Vector3(vector3.x, 0f, vector2.z), color2);
		Debug.DrawLine(new Vector3(vector3.x, 0f, vector2.z), new Vector3(vector3.x, 0f, vector3.z), color2);
		Debug.DrawLine(new Vector3(vector3.x, 0f, vector3.z), new Vector3(vector2.x, 0f, vector3.z), color2);
		Debug.DrawLine(new Vector3(vector2.x, 0f, vector3.z), new Vector3(vector2.x, 0f, vector2.z), color2);
	}

	private void gridTest()
	{
		float num = 20f;
		int num2 = 0;
		while ((float)num2 < num)
		{
			Vector3 vector = Vector3.zero;
			int num3 = 0;
			while ((float)num3 < num)
			{
				Vector3 vector2;
				vector2..ctor(Mathf.Lerp(this.triangulation.qtree.minX, this.triangulation.qtree.maxX, (float)num2 / num), 0f, Mathf.Lerp(this.triangulation.qtree.minZ, this.triangulation.qtree.maxZ, (float)num3 / num));
				if (num3 == 0)
				{
					vector = vector2;
				}
				Debug.DrawLine(vector, vector2, Color.red);
				vector = vector2;
				num3++;
			}
			num2++;
		}
		int num4 = 0;
		while ((float)num4 < num)
		{
			Vector3 vector3 = Vector3.zero;
			int num5 = 0;
			while ((float)num5 < num)
			{
				Vector3 vector4;
				vector4..ctor(Mathf.Lerp(this.triangulation.qtree.minX, this.triangulation.qtree.maxX, (float)num5 / num), 0f, Mathf.Lerp(this.triangulation.qtree.minZ, this.triangulation.qtree.maxZ, (float)num4 / num));
				if (num5 == 0)
				{
					vector3 = vector4;
				}
				Debug.DrawLine(vector3, vector4, Color.red);
				vector3 = vector4;
				num5++;
			}
			num4++;
		}
	}

	private void drawHFQT(HeightFieldQuadTree<PointXZResident> tree)
	{
		Vector3 vector = tree.hd[0].Current(0f);
		Vector3 vector2 = tree.hd[1].Current(0f);
		Vector3 vector3 = tree.hd[2].Current(0f);
		Vector3 vector4 = tree.hd[3].Current(0f);
		Debug.DrawLine(vector, vector2, Color.green);
		Debug.DrawLine(vector2, vector4, Color.green);
		Debug.DrawLine(vector4, vector3, Color.green);
		Debug.DrawLine(vector3, vector, Color.green);
		for (int i = 0; i < tree.ChildrenCount; i++)
		{
			this.drawHFQT(tree.GetChild(i) as HeightFieldQuadTree<PointXZResident>);
		}
	}

	private void Update()
	{
		if (this.hfqt.hdRequests.Count > 0)
		{
			HeightFieldQuadTree<PointXZResident>.HeightData heightData = this.hfqt.hdRequests.Dequeue();
			Vector3 vector = heightData.Current(0f);
			vector.y = (Mathf.Sin(vector.x * 10f) + Mathf.Cos(vector.z * 15f)) * 0.1f;
			heightData.SetAuthentic(vector, false);
		}
		this.drawHFQT(this.hfqt);
		if (this.AddResidentTrigger)
		{
			this.triangulation.AddNode(this.point.position, Vector3.up);
			this.AddResidentTrigger = false;
		}
	}

	public Vector2 minXZ;

	public Vector2 maxXZ;

	public bool AddResidentTrigger;

	public bool SearchNearestTrigger;

	public bool CheckConsistencyTrigger;

	public bool GridTestTrigger;

	public Transform point;

	public Transform triA;

	public Transform triB;

	public Transform triC;

	public Transform rectA;

	public Transform rectB;

	public List<int> triIDs;

	public int DrawID;

	private QuadTree<PointXZResident> tree;

	private IPlaneXZResident selectedResident;

	private HeightFieldTriangulation triangulation;

	private QuadTree<HeightFieldTriangulation.Triangle> selectedNode;

	private HeightFieldQuadTree<PointXZResident> hfqt;
}
