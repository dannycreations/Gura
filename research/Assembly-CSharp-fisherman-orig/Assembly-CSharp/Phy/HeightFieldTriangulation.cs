using System;
using System.Collections.Generic;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace Phy
{
	public class HeightFieldTriangulation
	{
		public HeightFieldTriangulation(float minX, float minZ, float maxX, float maxZ, int qtreeMaxDepth, int qtreeMaxDensity)
		{
			this.qtreeMaxDepth = qtreeMaxDepth;
			this.qtreeMaxDensity = qtreeMaxDensity;
			this.nodes = new List<HeightFieldTriangulation.Node>();
			this.qtree = new QuadTree<HeightFieldTriangulation.Quad>(minX, minZ, maxX, maxZ, 0);
			this.minXZ = new Vector2(minX, minZ);
			this.maxXZ = new Vector2(maxX, maxZ);
			this.boundingRadius = Mathf.Max(maxX - minX, maxZ - minZ) * Mathf.Sqrt(2f) * 0.5f;
		}

		public QuadTree<HeightFieldTriangulation.Quad> qtree { get; private set; }

		public void GridInitialize(int gridSize, float[,] heights, Vector3[,] normals)
		{
			Vector2 vector = (this.maxXZ - this.minXZ) / (float)gridSize;
			for (int i = 0; i < gridSize; i++)
			{
				for (int j = 0; j < gridSize; j++)
				{
					HeightFieldTriangulation.Node node = new HeightFieldTriangulation.Node(new Vector3(this.minXZ.x + vector.x * (float)i, heights[i, j], this.minXZ.y + vector.y * (float)j), normals[i, j]);
					this.nodes.Add(node);
				}
			}
			int num = Mathf.FloorToInt(Mathf.Log((float)gridSize, 2f));
			for (int k = 0; k < gridSize - 1; k++)
			{
				for (int l = 0; l < gridSize - 1; l++)
				{
					int num2 = k + l * gridSize;
					int num3 = k + (l + 1) * gridSize;
					HeightFieldTriangulation.Quad quad = new HeightFieldTriangulation.Quad(this.nodes[num3], this.nodes[num3 + 1], this.nodes[num2 + 1], this.nodes[num2]);
					this.qtree.AddResident(quad, num);
				}
			}
		}

		public void AddNode(Vector3 position, Vector3 normal)
		{
			if (this.qtree.TestPoint(position.x, position.z))
			{
				float num = this.boundingRadius;
				QuadTree<HeightFieldTriangulation.Quad> nodeAtPoint = this.qtree.GetNodeAtPoint(position.x, position.z, true);
				if (nodeAtPoint.DensityCounter < this.qtreeMaxDensity)
				{
					HeightFieldTriangulation.Quad nearestResident = nodeAtPoint.GetNearestResident(position.x, position.z, ref num);
					nodeAtPoint.IncDensityCounter();
					if (!Mathf.Approximately(num, 0f))
					{
						Debug.LogError("dist = " + num);
					}
					Vector3 centerXZ = nearestResident.centerXZ;
					centerXZ.y = position.y;
					HeightFieldTriangulation.Node node = new HeightFieldTriangulation.Node(centerXZ, normal);
					this.nodes.Add(node);
					HeightFieldTriangulation.Node node2 = new HeightFieldTriangulation.Node((nearestResident.nNxPz.position + nearestResident.nPxPz.position) * 0.5f, (nearestResident.nNxPz.normal + nearestResident.nPxPz.normal).normalized);
					HeightFieldTriangulation.Node node3 = new HeightFieldTriangulation.Node((nearestResident.nPxPz.position + nearestResident.nPxNz.position) * 0.5f, (nearestResident.nPxPz.normal + nearestResident.nPxNz.normal).normalized);
					HeightFieldTriangulation.Node node4 = new HeightFieldTriangulation.Node((nearestResident.nPxNz.position + nearestResident.nNxNz.position) * 0.5f, (nearestResident.nPxNz.normal + nearestResident.nNxNz.normal).normalized);
					HeightFieldTriangulation.Node node5 = new HeightFieldTriangulation.Node((nearestResident.nNxNz.position + nearestResident.nNxPz.position) * 0.5f, (nearestResident.nNxNz.normal + nearestResident.nNxPz.normal).normalized);
					HeightFieldTriangulation.Quad quad = new HeightFieldTriangulation.Quad(nearestResident.nNxPz, node2, node, node5);
					HeightFieldTriangulation.Quad quad2 = new HeightFieldTriangulation.Quad(node2, nearestResident.nPxPz, node3, node);
					HeightFieldTriangulation.Quad quad3 = new HeightFieldTriangulation.Quad(node, node3, nearestResident.nPxNz, node4);
					HeightFieldTriangulation.Quad quad4 = new HeightFieldTriangulation.Quad(node5, node, node4, nearestResident.nNxNz);
					this.qtree.RemoveResident(nearestResident);
					this.qtree.AddResident(quad, this.qtreeMaxDepth);
					this.qtree.AddResident(quad2, this.qtreeMaxDepth);
					this.qtree.AddResident(quad3, this.qtreeMaxDepth);
					this.qtree.AddResident(quad4, this.qtreeMaxDepth);
				}
			}
		}

		public HeightFieldTriangulation.Quad GetQuadAtPoint(float px, float pz)
		{
			if (this.qtree.TestPoint(px, pz))
			{
				float num = this.boundingRadius;
				QuadTree<HeightFieldTriangulation.Quad> nodeAtPoint = this.qtree.GetNodeAtPoint(px, pz, true);
				return nodeAtPoint.GetNearestResident(px, pz, ref num);
			}
			return null;
		}

		public HeightFieldTriangulation.Quad GetQuadAtPoint(Vector3 position)
		{
			return this.GetQuadAtPoint(position.x, position.z);
		}

		public void Sample(Vector4f position, out Vector4f groundPoint, out Vector4f groundNormal)
		{
			if (this.qtree.TestPoint(position.X, position.Z))
			{
				float num = this.boundingRadius;
				QuadTree<HeightFieldTriangulation.Quad> nodeAtPoint = this.qtree.GetNodeAtPoint(position.X, position.Z, true);
				HeightFieldTriangulation.Quad nearestResident = nodeAtPoint.GetNearestResident(position.X, position.Z, ref num);
				Vector3 up = Vector3.up;
				float num2 = nearestResident.BarycentricInterpolate(position.X, position.Z, out up);
				groundPoint..ctor(position.X, num2, position.Z, 0f);
				groundNormal = up.AsVector4f();
			}
			else
			{
				groundPoint..ctor(position.X, 0f, position.Z, 0f);
				groundNormal = Vector4fExtensions.up;
			}
		}

		private List<HeightFieldTriangulation.Node> nodes;

		private Vector2 minXZ;

		private Vector2 maxXZ;

		private float boundingRadius;

		private int qtreeMaxDepth;

		private int qtreeMaxDensity;

		private int gridSize;

		public class Triangle : IPlaneXZResident
		{
			public Triangle(HeightFieldTriangulation.Node A, HeightFieldTriangulation.Node B, HeightFieldTriangulation.Node C)
			{
				this.nodeA = A;
				this.nodeB = B;
				this.nodeC = C;
				this.pointA = new Vector3(A.position.x, 0f, A.position.z);
				this.pointB = new Vector3(B.position.x, 0f, B.position.z);
				this.pointC = new Vector3(C.position.x, 0f, C.position.z);
				this.ID = HeightFieldTriangulation.Triangle.idcounter;
				HeightFieldTriangulation.Triangle.idcounter++;
			}

			public HeightFieldTriangulation.Node nodeA { get; private set; }

			public HeightFieldTriangulation.Node nodeB { get; private set; }

			public HeightFieldTriangulation.Node nodeC { get; private set; }

			public int ID { get; private set; }

			public bool TestAABB(float minX, float maxX, float minZ, float maxZ)
			{
				return Math3d.TriangleOverlapAABBXZ(this.pointA, this.pointB, this.pointC, new Vector3(minX, 0f, minZ), new Vector3(maxX, 0f, maxZ));
			}

			public bool ProcreateFactor(float minX, float maxX, float minZ, float maxZ)
			{
				return Math3d.PointOverlapAABBXZ(this.pointA, minX, maxX, minZ, maxZ);
			}

			public float DistanceToPoint(float px, float pz)
			{
				return Math3d.PointToTriangleDistanceXZ(new Vector3(px, 0f, pz), this.pointA, this.pointB, this.pointC);
			}

			public bool TestPoint(float px, float pz)
			{
				return Math3d.PointInsideTriangleXZ(new Vector3(px, 0f, pz), this.pointA, this.pointB, this.pointC);
			}

			public float BarycentricInterpolate(float x, float z, out Vector3 normal)
			{
				float num = 1f / ((this.pointB.z - this.pointC.z) * (this.pointA.x - this.pointC.x) + (this.pointC.x - this.pointB.x) * (this.pointA.z - this.pointC.z));
				float num2 = num * ((this.pointB.z - this.pointC.z) * (x - this.pointC.x) + (this.pointC.x - this.pointB.x) * (z - this.pointC.z));
				float num3 = num * ((this.pointC.z - this.pointA.z) * (x - this.pointC.x) + (this.pointA.x - this.pointC.x) * (z - this.pointC.z));
				float num4 = 1f - num2 - num3;
				normal = (this.nodeA.normal * num2 + this.nodeB.normal * num3 + this.nodeC.normal * num4).normalized;
				return this.nodeA.position.y * num2 + this.nodeB.position.y * num3 + this.nodeC.position.y * num4;
			}

			private Vector3 pointA;

			private Vector3 pointB;

			private Vector3 pointC;

			protected static int idcounter;
		}

		public class Quad : IPlaneXZResident
		{
			public Quad(HeightFieldTriangulation.Node nNxPz, HeightFieldTriangulation.Node nPxPz, HeightFieldTriangulation.Node nPxNz, HeightFieldTriangulation.Node nNxNz)
			{
				this.nNxPz = nNxPz;
				this.nPxPz = nPxPz;
				this.nPxNz = nPxNz;
				this.nNxNz = nNxNz;
				this.minXZ = Vector3.Min(Vector3.Min(nNxPz.position, nPxPz.position), Vector3.Min(nPxNz.position, nNxNz.position));
				this.maxXZ = Vector3.Max(Vector3.Max(nNxPz.position, nPxPz.position), Vector3.Max(nPxNz.position, nNxNz.position));
				this.centerXZ = (this.maxXZ + this.minXZ) * 0.5f;
			}

			public HeightFieldTriangulation.Node nNxPz { get; private set; }

			public HeightFieldTriangulation.Node nPxPz { get; private set; }

			public HeightFieldTriangulation.Node nPxNz { get; private set; }

			public HeightFieldTriangulation.Node nNxNz { get; private set; }

			public Vector3 minXZ { get; private set; }

			public Vector3 maxXZ { get; private set; }

			public Vector3 centerXZ { get; private set; }

			public bool TestAABB(float minX, float maxX, float minZ, float maxZ)
			{
				return Math3d.BoxBoxOverlapAABBXZ(minX, maxX, minZ, maxZ, this.minXZ.x, this.maxXZ.x, this.minXZ.z, this.maxXZ.z);
			}

			public float DistanceToPoint(float px, float pz)
			{
				return Math3d.PointToAABBXZDistance(px, pz, this.minXZ.x, this.minXZ.z, this.maxXZ.x, this.maxXZ.z);
			}

			public bool ProcreateFactor(float minX, float maxX, float minZ, float maxZ)
			{
				return Math3d.PointOverlapAABBXZ(this.centerXZ, minX, maxX, minZ, maxZ);
			}

			public float BarycentricInterpolate(float x, float z, out Vector3 normal)
			{
				float num = (x - this.minXZ.x) / (this.maxXZ.x - this.minXZ.x);
				float num2 = (z - this.minXZ.z) / (this.maxXZ.z - this.minXZ.z);
				float num3 = Mathf.Lerp(this.nNxPz.position.y, this.nPxPz.position.y, num);
				float num4 = Mathf.Lerp(this.nNxNz.position.y, this.nPxNz.position.y, num);
				Vector3 normalized = Vector3.Lerp(this.nNxPz.normal, this.nPxPz.normal, num).normalized;
				Vector3 normalized2 = Vector3.Lerp(this.nNxNz.normal, this.nPxNz.normal, num).normalized;
				normal = Vector3.Lerp(normalized, normalized2, num2).normalized;
				return Mathf.Lerp(num3, num4, num2);
			}

			public bool TestPoint(float px, float pz)
			{
				return Math3d.PointOverlapAABBXZ(new Vector3(px, 0f, pz), this.minXZ.x, this.maxXZ.x, this.minXZ.z, this.maxXZ.z);
			}

			public int ID
			{
				get
				{
					return 0;
				}
			}
		}

		public class Node : IPlaneXZResident
		{
			public Node(Vector3 position, Vector3 normal)
			{
				this.position = position;
				this.normal = normal;
			}

			public Vector3 position { get; private set; }

			public bool TestAABB(float minX, float maxX, float minZ, float maxZ)
			{
				return Math3d.PointOverlapAABBXZ(this.position, minX, maxX, minZ, maxZ);
			}

			public bool ProcreateFactor(float minX, float maxX, float minZ, float maxZ)
			{
				return true;
			}

			public float DistanceToPoint(float px, float pz)
			{
				return MathHelper.Hypot(px - this.position.x, pz - this.position.z);
			}

			public int ID
			{
				get
				{
					return 0;
				}
			}

			public Vector3 normal;
		}
	}
}
