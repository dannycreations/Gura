using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phy
{
	public class HeightFieldQuadTree<T> : QuadTree<T> where T : class, IPlaneXZResident
	{
		public HeightFieldQuadTree(float minX, float minZ, float maxX, float maxZ, float initY, int maxResidentsInLeaf = 0)
			: base(minX, minZ, maxX, maxZ, maxResidentsInLeaf)
		{
			this.hd = new HeightFieldQuadTree<T>.HeightData[4];
			this.hdRequests = new Queue<HeightFieldQuadTree<T>.HeightData>();
			this.hdRefreshList = new LinkedList<HeightFieldQuadTree<T>.HeightData>();
			this.hfparent = null;
			this.hd[QuadTree<T>.NegXNegZIndex] = new HeightFieldQuadTree<T>.HeightData(new Vector3(minX, initY, minZ));
			this.hd[QuadTree<T>.PosXNegZIndex] = new HeightFieldQuadTree<T>.HeightData(new Vector3(maxX, initY, minZ));
			this.hd[QuadTree<T>.PosXPosZIndex] = new HeightFieldQuadTree<T>.HeightData(new Vector3(maxX, initY, maxZ));
			this.hd[QuadTree<T>.NegXPosZIndex] = new HeightFieldQuadTree<T>.HeightData(new Vector3(minX, initY, maxZ));
			this.SendHeightDataRequest(this.hd[QuadTree<T>.NegXNegZIndex]);
			this.SendHeightDataRequest(this.hd[QuadTree<T>.PosXNegZIndex]);
			this.SendHeightDataRequest(this.hd[QuadTree<T>.PosXPosZIndex]);
			this.SendHeightDataRequest(this.hd[QuadTree<T>.NegXPosZIndex]);
		}

		protected HeightFieldQuadTree(HeightFieldQuadTree<T> parent, Vector2Int quadrant, HeightFieldQuadTree<T>.HeightData newData)
			: base(parent, quadrant)
		{
			this.hfparent = parent;
			this.hd = new HeightFieldQuadTree<T>.HeightData[4];
			int num = QuadTree<T>.QuadrantToIndex(quadrant);
			this.hd[num] = parent.hd[num];
			Vector2Int vector2Int = QuadTree<T>.OppositeQudrant(quadrant);
			int num2 = QuadTree<T>.QuadrantToIndex(vector2Int);
			this.hd[num2] = newData;
			Vector2Int vector2Int2 = QuadTree<T>.RotateClockwiseQuadrant(quadrant);
			Vector2Int vector2Int3 = QuadTree<T>.RotateClockwiseQuadrant(vector2Int);
			int num3 = QuadTree<T>.QuadrantToIndex(vector2Int2);
			int num4 = QuadTree<T>.QuadrantToIndex(vector2Int3);
			Vector2Int vector2Int4 = QuadTree<T>.CommonSide(quadrant, vector2Int2);
			Vector2Int vector2Int5 = QuadTree<T>.CommonSide(quadrant, vector2Int3);
			Vector2Int vector2Int6 = vector2Int2 - quadrant;
			Vector2Int vector2Int7 = vector2Int3 - quadrant;
			HeightFieldQuadTree<T> heightFieldQuadTree = base.GetAdjacentNode(vector2Int4) as HeightFieldQuadTree<T>;
			HeightFieldQuadTree<T> heightFieldQuadTree2 = base.GetAdjacentNode(vector2Int5) as HeightFieldQuadTree<T>;
			HeightFieldQuadTree<T> heightFieldQuadTree3 = base.GetAdjacentNode(vector2Int6) as HeightFieldQuadTree<T>;
			HeightFieldQuadTree<T> heightFieldQuadTree4 = base.GetAdjacentNode(vector2Int7) as HeightFieldQuadTree<T>;
			if (heightFieldQuadTree != null)
			{
				if (heightFieldQuadTree.level < base.level)
				{
					if (heightFieldQuadTree3 == null)
					{
						this.hd[num3] = new HeightFieldQuadTree<T>.HeightData(parent, (parent.hd[num].PositionDst + parent.hd[num3].PositionDst) * 0.5f);
					}
					else
					{
						this.hd[num3] = heightFieldQuadTree3.hd[QuadTree<T>.QuadrantToIndex(QuadTree<T>.MirrorQuadrant(vector2Int2, vector2Int6))];
					}
				}
				else
				{
					this.hd[num3] = heightFieldQuadTree.hd[QuadTree<T>.QuadrantToIndex(QuadTree<T>.MirrorQuadrant(vector2Int2, vector2Int4))];
					if (heightFieldQuadTree3 == null)
					{
						this.SendHeightDataRequest(this.hd[num3]);
					}
				}
			}
			else
			{
				if (heightFieldQuadTree3 == null)
				{
					this.hd[num3] = new HeightFieldQuadTree<T>.HeightData(parent, (parent.hd[num].PositionDst + parent.hd[num3].PositionDst) * 0.5f);
				}
				else
				{
					this.hd[num3] = heightFieldQuadTree3.hd[QuadTree<T>.QuadrantToIndex(QuadTree<T>.MirrorQuadrant(vector2Int2, vector2Int6))];
				}
				this.SendHeightDataRequest(this.hd[num3]);
			}
			if (heightFieldQuadTree2 != null)
			{
				if (heightFieldQuadTree2.level < base.level)
				{
					if (heightFieldQuadTree4 == null)
					{
						this.hd[num4] = new HeightFieldQuadTree<T>.HeightData(parent, (parent.hd[num].PositionDst + parent.hd[num4].PositionDst) * 0.5f);
					}
					else
					{
						this.hd[num4] = heightFieldQuadTree4.hd[QuadTree<T>.QuadrantToIndex(QuadTree<T>.MirrorQuadrant(vector2Int3, vector2Int7))];
					}
				}
				else
				{
					this.hd[num4] = heightFieldQuadTree2.hd[QuadTree<T>.QuadrantToIndex(QuadTree<T>.MirrorQuadrant(vector2Int3, vector2Int5))];
					if (heightFieldQuadTree4 == null)
					{
						this.SendHeightDataRequest(this.hd[num4]);
					}
				}
			}
			else
			{
				if (heightFieldQuadTree4 == null)
				{
					this.hd[num4] = new HeightFieldQuadTree<T>.HeightData(parent, (parent.hd[num].PositionDst + parent.hd[num4].PositionDst) * 0.5f);
				}
				else
				{
					this.hd[num4] = heightFieldQuadTree4.hd[QuadTree<T>.QuadrantToIndex(QuadTree<T>.MirrorQuadrant(vector2Int3, vector2Int7))];
				}
				this.SendHeightDataRequest(this.hd[num4]);
			}
		}

		private float maxY
		{
			get
			{
				return Mathf.Max(new float[]
				{
					this.hd[0].PositionDst.y,
					this.hd[1].PositionDst.y,
					this.hd[2].PositionDst.y,
					this.hd[3].PositionDst.y
				});
			}
		}

		public override void Procreate(int depth = 0)
		{
			if (base.level <= 5 || (this.center == null && ((HeightFieldQuadTree<T>)base.parent).Curvature() > 0.2f))
			{
				object obj = this.refreshLock;
				lock (obj)
				{
					this.children = new HeightFieldQuadTree<T>[4];
					this.center = new HeightFieldQuadTree<T>.HeightData(this, new Vector3((base.minX + base.maxX) * 0.5f, 0f, (base.minZ + base.maxZ) * 0.5f));
					this.children[QuadTree<T>.PosXPosZIndex] = new HeightFieldQuadTree<T>(this, QuadTree<T>.PosXPosZ, this.center);
					this.children[QuadTree<T>.NegXPosZIndex] = new HeightFieldQuadTree<T>(this, QuadTree<T>.NegXPosZ, this.center);
					this.children[QuadTree<T>.NegXNegZIndex] = new HeightFieldQuadTree<T>(this, QuadTree<T>.NegXNegZ, this.center);
					this.children[QuadTree<T>.PosXNegZIndex] = new HeightFieldQuadTree<T>(this, QuadTree<T>.PosXNegZ, this.center);
					this.SendHeightDataRequest(this.center);
				}
				if (depth > 0)
				{
					for (int i = 0; i < this.children.Length; i++)
					{
						this.children[i].Procreate(depth - 1);
					}
				}
			}
		}

		private void beginRefreshingHD()
		{
			for (int i = 0; i < this.hd.Length; i++)
			{
				this.hd[i].BeginRefreshing();
			}
			for (int j = 0; j < this.children.Length; j++)
			{
				(this.children[j] as HeightFieldQuadTree<T>).beginRefreshingHD();
			}
		}

		private void refreshHD()
		{
			if (this.hdRefreshList != null)
			{
				foreach (HeightFieldQuadTree<T>.HeightData heightData in this.hdRefreshList)
				{
					heightData.Refresh();
				}
				for (int i = 0; i < this.hdAddList.Count; i++)
				{
					this.hdRefreshList.AddLast(this.hdAddList[i]);
				}
				for (int j = 0; j < this.hdRemoveList.Count; j++)
				{
					this.hdRefreshList.Remove(this.hdRemoveList[j]);
				}
				this.hdAddList.Clear();
				this.hdRemoveList.Clear();
			}
		}

		public void RefreshHeightData()
		{
			object obj = this.refreshLock;
			lock (obj)
			{
				this.beginRefreshingHD();
				this.refreshHD();
			}
		}

		public void SendHeightDataRequest(HeightFieldQuadTree<T>.HeightData hdreq)
		{
			if (this.hfparent != null)
			{
				this.hfparent.SendHeightDataRequest(hdreq);
			}
			else
			{
				object obj = this.hdRequests;
				lock (obj)
				{
					this.hdRequests.Enqueue(hdreq);
				}
			}
		}

		public void AddToRefreshList(HeightFieldQuadTree<T>.HeightData hd)
		{
			if (this.hfparent != null)
			{
				this.hfparent.AddToRefreshList(hd);
			}
			else
			{
				this.hdAddList.Add(hd);
			}
		}

		public void RemoveFromRefreshList(HeightFieldQuadTree<T>.HeightData hd)
		{
			if (this.hfparent != null)
			{
				this.hfparent.RemoveFromRefreshList(hd);
			}
			else
			{
				this.hdRemoveList.Add(hd);
			}
		}

		public float GetHeightAtPoint(Vector3 point)
		{
			HeightFieldQuadTree<T> heightFieldQuadTree = base.GetNodeAtPoint(point.x, point.z, true) as HeightFieldQuadTree<T>;
			Vector3 vector;
			if (heightFieldQuadTree != null)
			{
				return heightFieldQuadTree.InterpolateTriangular(point.x, point.y, point.z, 0f, out vector, true);
			}
			return this.ExtrapolateTriangular(point.x, point.y, point.z, 0f, out vector, true);
		}

		public float GetHeightAtPoint(Vector3 point, out Vector3 normal)
		{
			HeightFieldQuadTree<T> heightFieldQuadTree = base.GetNodeAtPoint(point.x, point.z, true) as HeightFieldQuadTree<T>;
			if (heightFieldQuadTree != null)
			{
				return heightFieldQuadTree.InterpolateTriangular(point.x, point.y, point.z, 0f, out normal, true);
			}
			return this.ExtrapolateTriangular(point.x, point.y, point.z, 0f, out normal, true);
		}

		public float InterpolateTriangular(float x, float y, float z, float time, out Vector3 normal, bool optimizeY = true)
		{
			if (optimizeY && y > this.maxY)
			{
				normal = Vector3.up;
				return this.maxY;
			}
			normal = Vector3.up;
			float num = (x - base.minX) * base.invDX;
			float num2 = (z - base.minZ) * base.invDZ;
			Vector3 vector = this.hd[QuadTree<T>.NegXNegZIndex].Current(time);
			Vector3 vector2 = this.hd[QuadTree<T>.PosXPosZIndex].Current(time);
			if (num < num2)
			{
				Vector3 vector3 = this.hd[QuadTree<T>.NegXPosZIndex].Current(time);
				normal = Math3d.PlaneNormalFrom3Points(vector, vector3, vector2);
				return vector.y + (vector2.y - vector3.y) * num + (vector3.y - vector.y) * num2;
			}
			Vector3 vector4 = this.hd[QuadTree<T>.PosXNegZIndex].Current(time);
			normal = Math3d.PlaneNormalFrom3Points(vector, vector2, vector4);
			return vector.y + (vector4.y - vector.y) * num + (vector2.y - vector4.y) * num2;
		}

		public float ExtrapolateTriangular(float x, float y, float z, float time, out Vector3 normal, bool optimizeY = true)
		{
			return this.InterpolateTriangular(Mathf.Clamp(x, base.minX, base.maxX), y, Mathf.Clamp(z, base.minZ, base.maxZ), time, out normal, optimizeY);
		}

		public float InterpolateBilinear(float x, float y, float z, float time, out Vector3 normal, bool optimizeY = true)
		{
			if (optimizeY && y > this.maxY)
			{
				normal = Vector3.up;
				return this.maxY;
			}
			float num = (x - base.minX) * base.invDX;
			float num2 = (z - base.minZ) * base.invDZ;
			float y2 = this.hd[QuadTree<T>.NegXNegZIndex].Current(time).y;
			float y3 = this.hd[QuadTree<T>.NegXPosZIndex].Current(time).y;
			float num3 = Mathf.Lerp(y2, this.hd[QuadTree<T>.PosXNegZIndex].Current(time).y, num);
			float num4 = Mathf.Lerp(y3, this.hd[QuadTree<T>.PosXPosZIndex].Current(time).y, num);
			Vector3 vector;
			vector..ctor(x, Mathf.Lerp(num3, num4, num2), z);
			Vector3 vector2;
			vector2..ctor(x, num3, base.minZ);
			Vector3 vector3;
			vector3..ctor(base.minX, Mathf.Lerp(y2, y3, num2), z);
			normal = Math3d.PlaneNormalFrom3Points(vector, vector2, vector3);
			return vector.y;
		}

		public float ExtrapolateBilinear(float x, float y, float z, float time, out Vector3 normal, bool optimizeY = true)
		{
			return this.InterpolateBilinear(Mathf.Clamp(x, base.minX, base.maxX), y, Mathf.Clamp(z, base.minZ, base.maxZ), time, out normal, optimizeY);
		}

		public float Curvature()
		{
			Vector3 vector = Math3d.PlaneNormalFrom3Points(this.hd[0].PositionAuth, this.hd[1].PositionAuth, this.hd[2].PositionAuth);
			Vector3 vector2 = Math3d.PlaneNormalFrom3Points(this.hd[2].PositionAuth, this.hd[1].PositionAuth, this.hd[3].PositionAuth);
			float num = Mathf.Abs(Vector3.Dot(this.hd[3].PositionAuth - this.hd[1].PositionAuth, vector));
			float num2 = Mathf.Abs(Vector3.Dot(this.center.PositionAuth - this.hd[1].PositionAuth, vector));
			float num3 = Mathf.Abs(Vector3.Dot(this.hd[2].PositionAuth - this.hd[1].PositionAuth, vector2));
			float num4 = Mathf.Abs(Vector3.Dot(this.center.PositionAuth - this.hd[1].PositionAuth, vector2));
			return Mathf.Max(new float[] { num, num2, num3, num4 }) / (base.maxX - base.minX);
		}

		public HeightFieldQuadTree<T>.HeightData[] hd;

		public HeightFieldQuadTree<T>.HeightData center;

		public Queue<HeightFieldQuadTree<T>.HeightData> hdRequests;

		public LinkedList<HeightFieldQuadTree<T>.HeightData> hdRefreshList;

		private HeightFieldQuadTree<T> hfparent;

		private const float minProcreationCurvature = 0.2f;

		private object refreshLock = new object();

		private List<HeightFieldQuadTree<T>.HeightData> hdAddList = new List<HeightFieldQuadTree<T>.HeightData>();

		private List<HeightFieldQuadTree<T>.HeightData> hdRemoveList = new List<HeightFieldQuadTree<T>.HeightData>();

		public class HeightDataRequest
		{
			public HeightFieldQuadTree<T>[] nodes { get; private set; }

			public Vector2Int[] quadrants { get; private set; }

			public Vector3 position { get; private set; }

			public float height;

			public Vector3 normal;
		}

		public class HeightData
		{
			public HeightData(Vector3 p)
			{
				this.positionSrc = p;
				this.positionDst = p;
			}

			public HeightData(HeightFieldQuadTree<T> sQuad, Vector3 positionXZ)
			{
				this.sourceQuad = sQuad;
				Vector3 vector;
				this.positionDst = new Vector3(positionXZ.x, sQuad.InterpolateTriangular(positionXZ.x, 0f, positionXZ.z, 1f, out vector, false), positionXZ.z);
				this.positionSrc = this.positionDst;
				sQuad.AddToRefreshList(this);
			}

			public bool isAuthentic { get; private set; }

			public bool isChanging { get; private set; }

			public bool needRefresh
			{
				get
				{
					return this.isChanging || this.sourceQuad != null;
				}
			}

			public void SetAuthentic(Vector3 p, bool immediately = false)
			{
				this.positionAuth = p;
				this.isAuthentic = true;
				this.isChanging = true;
				if (immediately)
				{
					this.positionDst = this.positionAuth;
					this.positionSrc = this.positionDst;
					if (this.sourceQuad != null)
					{
						this.sourceQuad.RemoveFromRefreshList(this);
					}
				}
			}

			public Vector3 PositionDst
			{
				get
				{
					return this.positionDst;
				}
			}

			public Vector3 PositionAuth
			{
				get
				{
					return this.positionAuth;
				}
			}

			public Vector3 Current(float progress)
			{
				return Vector3.Lerp(this.positionSrc, this.positionDst, progress);
			}

			public void BeginRefreshing()
			{
				this.refreshed = false;
			}

			public void Refresh()
			{
				if (this.refreshed || !this.needRefresh)
				{
					return;
				}
				if (this.isAuthentic)
				{
					this.positionSrc = this.positionDst;
					float num = this.positionAuth.y - this.positionDst.y;
					if (Mathf.Abs(num) < 0.025f)
					{
						this.positionDst.y = this.positionAuth.y;
						if (Mathf.Approximately(this.positionSrc.y, this.positionDst.y))
						{
							if (this.sourceQuad != null)
							{
								this.sourceQuad.RemoveFromRefreshList(this);
							}
							this.isChanging = false;
						}
					}
					else
					{
						this.positionDst.y = this.positionDst.y + Mathf.Sign(num) * 0.025f;
						this.isChanging = true;
					}
				}
				else if (this.sourceQuad != null)
				{
					this.sourceQuad.hd[0].Refresh();
					this.sourceQuad.hd[1].Refresh();
					this.sourceQuad.hd[2].Refresh();
					this.sourceQuad.hd[3].Refresh();
					this.positionSrc = this.positionDst;
					Vector3 vector;
					this.positionDst = new Vector3(this.positionSrc.x, this.sourceQuad.InterpolateTriangular(this.positionSrc.x, this.positionSrc.y, this.positionSrc.z, 1f, out vector, false), this.positionSrc.z);
				}
				this.refreshed = true;
			}

			private const float MaxStepPerFrame = 0.025f;

			private HeightFieldQuadTree<T> sourceQuad;

			private Vector3 positionSrc;

			private Vector3 positionDst;

			private Vector3 positionAuth;

			private bool refreshed;
		}
	}
}
