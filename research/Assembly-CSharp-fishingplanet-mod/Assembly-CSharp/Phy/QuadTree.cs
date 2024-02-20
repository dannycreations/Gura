using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phy
{
	public class QuadTree<T> where T : class, IPlaneXZResident
	{
		public QuadTree(float minX, float minZ, float maxX, float maxZ, int maxResidentsInLeaf = 0)
		{
			this.children = new QuadTree<T>[0];
			this.residents = new LinkedList<T>();
			this.level = 0;
			this.parent = null;
			this.maxResidentsInLeaf = maxResidentsInLeaf;
			this.minXZ = new Vector2(minX, minZ);
			this.maxXZ = new Vector2(maxX, maxZ);
			this.invDX = 1f / (maxX - minX);
			this.invDZ = 1f / (maxZ - minZ);
			this.ID = 0;
			QuadTree<T>.idcounter = 0;
		}

		protected QuadTree(QuadTree<T> parent, Vector2Int quadrant)
		{
			this.ID = ++QuadTree<T>.idcounter;
			this.parent = parent;
			this.quadrant = quadrant;
			this.children = new QuadTree<T>[0];
			this.residents = new LinkedList<T>();
			this.level = parent.level + 1;
			Vector2 vector = (parent.minXZ + parent.maxXZ) * 0.5f;
			float num = parent.minXZ.x;
			float num2 = vector.x;
			float num3 = parent.minXZ.y;
			float num4 = vector.y;
			if (quadrant.x != 0)
			{
				num = vector.x;
				num2 = parent.maxXZ.x;
			}
			if (quadrant.y != 0)
			{
				num3 = vector.y;
				num4 = parent.maxXZ.y;
			}
			this.minXZ = new Vector2(num, num3);
			this.maxXZ = new Vector2(num2, num4);
			this.invDX = 1f / (this.maxX - this.minX);
			this.invDZ = 1f / (this.maxZ - this.minZ);
			for (LinkedListNode<T> linkedListNode = parent.residents.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				T value = linkedListNode.Value;
				if (value.TestAABB(this.minX, this.maxX, this.minZ, this.maxZ))
				{
					this.residents.AddLast(linkedListNode.Value);
				}
			}
		}

		public QuadTree<T> parent { get; private set; }

		public Vector2Int quadrant { get; private set; }

		public Vector2 minXZ { get; private set; }

		public Vector2 maxXZ { get; private set; }

		public float minX
		{
			get
			{
				return this.minXZ.x;
			}
		}

		public float maxX
		{
			get
			{
				return this.maxXZ.x;
			}
		}

		public float minZ
		{
			get
			{
				return this.minXZ.y;
			}
		}

		public float maxZ
		{
			get
			{
				return this.maxXZ.y;
			}
		}

		public int ID { get; private set; }

		public int level { get; private set; }

		public float invDX { get; private set; }

		public float invDZ { get; private set; }

		public static int QuadrantToIndex(Vector2Int quadrant)
		{
			return quadrant.x + quadrant.y * 2;
		}

		public static Vector2Int OppositeQudrant(Vector2Int quadrant)
		{
			return Vector2Int.one - quadrant;
		}

		public static Vector2Int RotateClockwiseQuadrant(Vector2Int quadrant)
		{
			return new Vector2Int(quadrant.y, 1 - quadrant.x);
		}

		public static Vector2Int RotateCounterClockwiseQuadrant(Vector2Int quadrant)
		{
			return new Vector2Int(1 - quadrant.y, quadrant.x);
		}

		public static Vector2Int CommonSide(Vector2Int q1, Vector2Int q2)
		{
			return new Vector2Int(~(-(q1.x ^ q2.x)) & ((q1.x << 1) - 1), ~(-(q1.y ^ q2.y)) & ((q1.y << 1) - 1));
		}

		public static Vector2Int MirrorQuadrant(Vector2Int q, Vector2Int dir)
		{
			return new Vector2Int(((1 - q.x) & dir.x) | (q.x & ~dir.x), ((1 - q.y) & dir.y) | (q.y & ~dir.y));
		}

		public QuadTree<T> GetChild(Vector2Int quadrant)
		{
			int num = QuadTree<T>.QuadrantToIndex(quadrant);
			if (num < this.children.Length)
			{
				return this.children[num];
			}
			return null;
		}

		public QuadTree<T> GetChild(int index)
		{
			if (index < this.children.Length)
			{
				return this.children[index];
			}
			return null;
		}

		public int ChildrenCount
		{
			get
			{
				return this.children.Length;
			}
		}

		public int ResidentsCount
		{
			get
			{
				return this.residents.Count;
			}
		}

		public int DensityCounter
		{
			get
			{
				return this.densityCounter;
			}
		}

		public void IncDensityCounter()
		{
			this.densityCounter++;
		}

		public LinkedListNode<T> FirstResident
		{
			get
			{
				return this.residents.First;
			}
		}

		public virtual void Procreate(int depth = 0)
		{
			this.children = new QuadTree<T>[4];
			this.children[QuadTree<T>.PosXPosZIndex] = new QuadTree<T>(this, QuadTree<T>.PosXPosZ);
			this.children[QuadTree<T>.NegXPosZIndex] = new QuadTree<T>(this, QuadTree<T>.NegXPosZ);
			this.children[QuadTree<T>.NegXNegZIndex] = new QuadTree<T>(this, QuadTree<T>.NegXNegZ);
			this.children[QuadTree<T>.PosXNegZIndex] = new QuadTree<T>(this, QuadTree<T>.PosXNegZ);
			if (depth > 0)
			{
				for (int i = 0; i < this.children.Length; i++)
				{
					this.children[i].Procreate(depth - 1);
				}
			}
		}

		public void RemoveResident(T resident)
		{
			LinkedListNode<T> linkedListNode = this.residents.Find(resident);
			if (linkedListNode != null)
			{
				this.residents.Remove(linkedListNode);
				for (int i = 0; i < this.children.Length; i++)
				{
					this.children[i].RemoveResident(resident);
				}
			}
		}

		public QuadTree<T> AddResident(T candidate, int destLevel)
		{
			if (candidate.TestAABB(this.minX, this.maxX, this.minZ, this.maxZ) && (this.level != destLevel || this.maxResidentsInLeaf <= 0 || this.residents.Count < this.maxResidentsInLeaf))
			{
				if (destLevel > this.level && this.children.Length == 0 && candidate.ProcreateFactor(this.minX, this.maxX, this.minZ, this.maxZ))
				{
					this.Procreate(0);
				}
				QuadTree<T> quadTree = this;
				this.residents.AddLast(candidate);
				for (int i = 0; i < this.children.Length; i++)
				{
					quadTree = this.children[i].AddResident(candidate, destLevel) ?? quadTree;
				}
				return quadTree;
			}
			return null;
		}

		public bool TestPoint(float x, float z)
		{
			return x >= this.minX && z >= this.minZ && x <= this.maxX && z <= this.maxZ;
		}

		public Vector2 GetVertexPosition(Vector2Int quadrant)
		{
			return new Vector2(this.minX + (float)quadrant.x * (this.maxX - this.minX), this.minZ + (float)quadrant.y * (this.maxZ - this.minZ));
		}

		public float DistanceToPoint(float x, float z)
		{
			bool flag = x >= this.minX && x < this.maxX;
			bool flag2 = z >= this.minZ && z < this.maxZ;
			if (flag && flag2)
			{
				return 0f;
			}
			if (flag)
			{
				return Mathf.Min(this.minZ - z, z - this.maxZ);
			}
			if (flag2)
			{
				return Mathf.Min(this.minX - x, x - this.maxX);
			}
			if (x < this.minX)
			{
				if (z < this.minZ)
				{
					return MathHelper.Hypot(this.minX - x, this.minZ - z);
				}
				if (z >= this.maxZ)
				{
					return MathHelper.Hypot(this.minX - x, z - this.maxZ);
				}
			}
			if (z < this.minZ)
			{
				return MathHelper.Hypot(x - this.maxX, this.minZ - z);
			}
			return MathHelper.Hypot(x - this.maxX, z - this.maxZ);
		}

		public bool TestCircle(float px, float pz, float radius)
		{
			bool flag = px >= this.minX && px <= this.maxX;
			bool flag2 = pz >= this.minZ && pz <= this.maxZ;
			if (flag && flag2)
			{
				return true;
			}
			if (flag)
			{
				if (pz >= this.maxZ && pz <= this.maxZ + radius)
				{
					return true;
				}
				if (pz <= this.minZ && pz <= this.minZ - radius)
				{
					return true;
				}
			}
			if (flag2)
			{
				if (px >= this.maxX && px <= this.maxX + radius)
				{
					return true;
				}
				if (px <= this.minX && px <= this.minX - radius)
				{
					return true;
				}
			}
			Vector2 vector;
			vector..ctor(px, pz);
			Vector2 vector2;
			vector2..ctor(this.minX, this.minZ);
			Vector2 vector3;
			vector3..ctor(this.minX, this.maxZ);
			Vector2 vector4;
			vector4..ctor(this.maxX, this.maxZ);
			Vector2 vector5;
			vector5..ctor(this.maxX, this.minZ);
			float num = radius * radius;
			return (vector - vector2).sqrMagnitude <= num || (vector - vector3).sqrMagnitude <= num || (vector - vector4).sqrMagnitude <= num || (vector - vector5).sqrMagnitude <= num;
		}

		public QuadTree<T> GetNodeAtPoint(float x, float z, bool testAABB = true)
		{
			if (testAABB && !this.TestPoint(x, z))
			{
				return null;
			}
			if (this.children.Length == 0)
			{
				return this;
			}
			Vector2Int vector2Int;
			vector2Int..ctor(Mathf.RoundToInt((x - this.minX) * this.invDX), Mathf.RoundToInt((z - this.minZ) * this.invDZ));
			return this.children[QuadTree<T>.QuadrantToIndex(vector2Int)].GetNodeAtPoint(x, z, false);
		}

		public bool CheckHierarchicalConsistency()
		{
			if (this.parent != null)
			{
				for (LinkedListNode<T> linkedListNode = this.residents.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
				{
					if (!this.parent.residents.Contains(linkedListNode.Value))
					{
						object[] array = new object[5];
						array[0] = "Node level ";
						array[1] = this.level;
						array[2] = " resident ID ";
						int num = 3;
						T value = linkedListNode.Value;
						array[num] = value.ID;
						array[4] = " is not present in parent node";
						Debug.LogError(string.Concat(array));
						return false;
					}
				}
			}
			if (this.children.Length > 0)
			{
				for (int i = 0; i < this.children.Length; i++)
				{
					if (!this.children[i].CheckHierarchicalConsistency())
					{
						return false;
					}
				}
				return true;
			}
			return true;
		}

		public T GetNearestResident(float x, float z, ref float maxDist)
		{
			if (this.residents.Count > 0 && this.TestCircle(x, z, maxDist))
			{
				T t = (T)((object)null);
				if (this.children.Length > 0)
				{
					int num = -1;
					for (int i = 0; i < this.children.Length; i++)
					{
						if (this.children[i].TestPoint(x, z))
						{
							T nearestResident = this.children[i].GetNearestResident(x, z, ref maxDist);
							if (nearestResident != null)
							{
								t = nearestResident;
								if (Mathf.Approximately(maxDist, 0f))
								{
									return t;
								}
							}
							num = i;
							break;
						}
					}
					for (int j = 0; j < this.children.Length; j++)
					{
						if (j != num)
						{
							T nearestResident2 = this.children[j].GetNearestResident(x, z, ref maxDist);
							if (nearestResident2 != null)
							{
								t = nearestResident2;
								if (Mathf.Approximately(maxDist, 0f))
								{
									return t;
								}
							}
						}
					}
				}
				else
				{
					float num2 = maxDist;
					for (LinkedListNode<T> linkedListNode = this.residents.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
					{
						T value = linkedListNode.Value;
						float num3 = value.DistanceToPoint(x, z);
						if (num3 < num2)
						{
							t = linkedListNode.Value;
							num2 = num3;
							maxDist = num3;
							if (Mathf.Approximately(maxDist, 0f))
							{
								return t;
							}
						}
					}
				}
				return t;
			}
			return (T)((object)null);
		}

		public QuadTree<T> GetAdjacentNode(Vector2Int dir)
		{
			if (this.parent == null)
			{
				return null;
			}
			Vector2Int vector2Int = this.quadrant + dir;
			if ((vector2Int.x == 0 || vector2Int.x == 1) && (vector2Int.y == 0 || vector2Int.y == 1))
			{
				return this.parent.children[QuadTree<T>.QuadrantToIndex(vector2Int)];
			}
			QuadTree<T> adjacentNode = this.parent.GetAdjacentNode(dir);
			if (adjacentNode != null && adjacentNode.children.Length > 0)
			{
				vector2Int.x = (vector2Int.x + 2) % 2;
				vector2Int.y = (vector2Int.y + 2) % 2;
				return adjacentNode.children[QuadTree<T>.QuadrantToIndex(vector2Int)];
			}
			return adjacentNode;
		}

		public static readonly Vector2Int PosXPosZ = new Vector2Int(1, 1);

		public static readonly Vector2Int NegXPosZ = new Vector2Int(0, 1);

		public static readonly Vector2Int NegXNegZ = new Vector2Int(0, 0);

		public static readonly Vector2Int PosXNegZ = new Vector2Int(1, 0);

		public static readonly Vector2Int PosX = new Vector2Int(1, 0);

		public static readonly Vector2Int NegX = new Vector2Int(-1, 0);

		public static readonly Vector2Int PosZ = new Vector2Int(0, 1);

		public static readonly Vector2Int NegZ = new Vector2Int(0, -1);

		public static readonly int PosXPosZIndex = QuadTree<T>.QuadrantToIndex(QuadTree<T>.PosXPosZ);

		public static readonly int NegXPosZIndex = QuadTree<T>.QuadrantToIndex(QuadTree<T>.NegXPosZ);

		public static readonly int NegXNegZIndex = QuadTree<T>.QuadrantToIndex(QuadTree<T>.NegXNegZ);

		public static readonly int PosXNegZIndex = QuadTree<T>.QuadrantToIndex(QuadTree<T>.PosXNegZ);

		public static readonly Vector2Int[] IndexToQuadrant = new Vector2Int[]
		{
			QuadTree<T>.NegXNegZ,
			QuadTree<T>.PosXNegZ,
			QuadTree<T>.NegXPosZ,
			QuadTree<T>.PosXPosZ
		};

		private static int idcounter = 0;

		protected QuadTree<T>[] children;

		private LinkedList<T> residents;

		private int maxResidentsInLeaf;

		private int densityCounter;
	}
}
