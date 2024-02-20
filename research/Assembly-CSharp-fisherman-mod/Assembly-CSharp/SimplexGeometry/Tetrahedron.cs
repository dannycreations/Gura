using System;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace SimplexGeometry
{
	public class Tetrahedron : SimplexBody
	{
		public Tetrahedron()
		{
			this.vertices = new Vector4f[4];
			this.SetVertices(Vector4f.Zero, Vector4f.Zero, Vector4f.Zero, Vector4f.Zero);
			base.cachedSimilarClipBody = null;
			base.cachedComplexClipBody = null;
		}

		public Tetrahedron(Vector4f v0, Vector4f v1, Vector4f v2, Vector4f v3)
		{
			this.vertices = new Vector4f[4];
			this.SetVertices(v0, v1, v2, v3);
			base.cachedSimilarClipBody = null;
			base.cachedComplexClipBody = null;
		}

		public void SetVertices(Vector4f v0, Vector4f v1, Vector4f v2, Vector4f v3)
		{
			if (this.vertices.Length == 4)
			{
				this.vertices[0] = v0;
				this.vertices[1] = v1;
				this.vertices[2] = v2;
				this.vertices[3] = v3;
			}
			else
			{
				base.SetVertices(new Vector4f[] { v0, v1, v2, v3 });
			}
		}

		public override SimplexBody Clone()
		{
			return new Tetrahedron(this.vertices[0], this.vertices[1], this.vertices[2], this.vertices[3])
			{
				MassValue = this.MassValue
			};
		}

		public override void update()
		{
			this.Barycenter = Vector4fExtensions.quarter3 * (this.vertices[0] + this.vertices[1] + this.vertices[2] + this.vertices[3]);
			Vector4f vector4f = this.vertices[0] - this.vertices[3];
			Vector4f vector4f2 = this.vertices[1] - this.vertices[3];
			Vector4f vector4f3 = this.vertices[2] - this.vertices[3];
			this.Volume = Mathf.Abs(Vector4fExtensions.Dot(vector4f, Vector4fExtensions.Cross(vector4f2, vector4f3)) / 6f);
		}

		public override Vector4f CalculateInertiaTensor(Vector4f origin, float density = 0f)
		{
			Vector4f vector4f;
			if (density > 0f)
			{
				vector4f..ctor(density * this.Volume * 0.1f);
			}
			else
			{
				vector4f..ctor(this.MassValue * 0.1f);
			}
			Vector4f vector4f2 = this.vertices[0] - origin;
			Vector4f vector4f3 = this.vertices[1] - origin;
			Vector4f vector4f4 = this.vertices[2] - origin;
			Vector4f vector4f5 = this.vertices[3] - origin;
			Vector4f vector4f6 = vector4f2 * vector4f2;
			Vector4f vector4f7 = vector4f2 * vector4f3;
			Vector4f vector4f8 = vector4f2 * vector4f4;
			Vector4f vector4f9 = vector4f2 * vector4f5;
			Vector4f vector4f10 = vector4f3 * vector4f3;
			Vector4f vector4f11 = vector4f3 * vector4f4;
			Vector4f vector4f12 = vector4f3 * vector4f5;
			Vector4f vector4f13 = vector4f4 * vector4f4;
			Vector4f vector4f14 = vector4f4 * vector4f5;
			Vector4f vector4f15 = vector4f5 * vector4f5;
			Vector4f vector4f16;
			vector4f16..ctor(vector4f6.Y + vector4f7.Y + vector4f10.Y + vector4f8.Y + vector4f11.Y + vector4f13.Y + vector4f9.Y + vector4f12.Y + vector4f14.Y + vector4f15.Y + vector4f6.Z + vector4f7.Z + vector4f10.Z + vector4f8.Z + vector4f11.Z + vector4f13.Z + vector4f9.Z + vector4f12.Z + vector4f14.Z + vector4f15.Z, vector4f6.X + vector4f7.X + vector4f10.X + vector4f8.X + vector4f11.X + vector4f13.X + vector4f9.X + vector4f12.X + vector4f14.X + vector4f15.X + vector4f6.Z + vector4f7.Z + vector4f10.Z + vector4f8.Z + vector4f11.Z + vector4f13.Z + vector4f9.Z + vector4f12.Z + vector4f14.Z + vector4f15.Z, vector4f6.X + vector4f7.X + vector4f10.X + vector4f8.X + vector4f11.X + vector4f13.X + vector4f9.X + vector4f12.X + vector4f14.X + vector4f15.X + vector4f6.Y + vector4f7.Y + vector4f10.Y + vector4f8.Y + vector4f11.Y + vector4f13.Y + vector4f9.Y + vector4f12.Y + vector4f14.Y + vector4f15.Y, 1f);
			return vector4f16 * vector4f;
		}

		public override ProceduralGeometry.MutableMesh GenerateMesh()
		{
			ProceduralGeometry.MutableMesh mutableMesh = new ProceduralGeometry.MutableMesh(1);
			mutableMesh.vertices.Add(this.vertices[0].AsVector3());
			mutableMesh.vertices.Add(this.vertices[1].AsVector3());
			mutableMesh.vertices.Add(this.vertices[2].AsVector3());
			mutableMesh.vertices.Add(this.vertices[1].AsVector3());
			mutableMesh.vertices.Add(this.vertices[2].AsVector3());
			mutableMesh.vertices.Add(this.vertices[3].AsVector3());
			mutableMesh.vertices.Add(this.vertices[0].AsVector3());
			mutableMesh.vertices.Add(this.vertices[3].AsVector3());
			mutableMesh.vertices.Add(this.vertices[1].AsVector3());
			mutableMesh.vertices.Add(this.vertices[0].AsVector3());
			mutableMesh.vertices.Add(this.vertices[3].AsVector3());
			mutableMesh.vertices.Add(this.vertices[2].AsVector3());
			mutableMesh.vertices.AddRange(mutableMesh.vertices.ToArray());
			mutableMesh.triangles.AddRange(new int[]
			{
				0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
				10, 11, 12, 14, 13, 15, 17, 16, 18, 20,
				19, 21, 23, 22
			});
			return mutableMesh;
		}

		public override void SimilarClip(Vector4f point, Vector4f normal)
		{
			if (base.cachedSimilarClipBody == null)
			{
				base.cachedSimilarClipBody = this.Clone();
			}
			int num = -1;
			float num2 = 0f;
			float num3 = 1f;
			for (int i = 0; i < this.vertices.Length; i++)
			{
				float num4 = Vector4fExtensions.Dot(this.vertices[i] - point, normal);
				if (num4 < num3)
				{
					num3 = num4;
				}
				if (num4 > num2)
				{
					num = i;
					num2 = num4;
				}
			}
			if (num < 0)
			{
				base.cachedSimilarClipBody.SetVertices(new Vector4f[]
				{
					Vector4f.Zero,
					Vector4f.Zero,
					Vector4f.Zero,
					Vector4f.Zero
				});
			}
			else if (num3 >= 0f)
			{
				base.cachedSimilarClipBody.SetVertices(this.vertices);
			}
			else
			{
				Vector4f vector4f;
				Math3d.SegmentPlaneIntersection4f(out vector4f, this.vertices[num], this.vertices[(num + 1) % this.vertices.Length], normal, point);
				Vector4f vector4f2;
				Math3d.SegmentPlaneIntersection4f(out vector4f2, this.vertices[num], this.vertices[(num + 2) % this.vertices.Length], normal, point);
				Vector4f vector4f3;
				Math3d.SegmentPlaneIntersection4f(out vector4f3, this.vertices[num], this.vertices[(num + 3) % this.vertices.Length], normal, point);
				(base.cachedSimilarClipBody as Tetrahedron).SetVertices(vector4f, vector4f2, vector4f3, this.vertices[num]);
			}
		}

		public override void ComplexClip(Vector4f point, Vector4f normal)
		{
			if (base.cachedComplexClipBody == null)
			{
				this.complexClipParts = new Tetrahedron[]
				{
					new Tetrahedron(),
					new Tetrahedron(),
					new Tetrahedron()
				};
				base.cachedComplexClipBody = new SimplexComposite(new SimplexBody[]
				{
					this.complexClipParts[0],
					this.complexClipParts[1],
					this.complexClipParts[2]
				});
			}
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < this.vertices.Length; i++)
			{
				if (Vector4fExtensions.Dot(this.vertices[i] - point, normal) >= 0f)
				{
					this._clippedVertices[num] = i;
					num++;
				}
				else
				{
					this._discardedVertices[num2] = i;
					num2++;
				}
			}
			if (num == 0)
			{
				this.complexClipParts[0].SetVertices(Vector4f.Zero, Vector4f.Zero, Vector4f.Zero, Vector4f.Zero);
				this.complexClipParts[1].SetVertices(Vector4f.Zero, Vector4f.Zero, Vector4f.Zero, Vector4f.Zero);
				this.complexClipParts[2].SetVertices(Vector4f.Zero, Vector4f.Zero, Vector4f.Zero, Vector4f.Zero);
			}
			else if (num == 4)
			{
				this.complexClipParts[0].SetVertices(this.vertices[0], this.vertices[1], this.vertices[2], this.vertices[3]);
				this.complexClipParts[1].SetVertices(Vector4f.Zero, Vector4f.Zero, Vector4f.Zero, Vector4f.Zero);
				this.complexClipParts[2].SetVertices(Vector4f.Zero, Vector4f.Zero, Vector4f.Zero, Vector4f.Zero);
			}
			else if (num == 1)
			{
				Vector4f vector4f;
				Math3d.SegmentPlaneIntersection4f(out vector4f, this.vertices[this._clippedVertices[0]], this.vertices[(this._clippedVertices[0] + 1) % this.vertices.Length], normal, point);
				Vector4f vector4f2;
				Math3d.SegmentPlaneIntersection4f(out vector4f2, this.vertices[this._clippedVertices[0]], this.vertices[(this._clippedVertices[0] + 2) % this.vertices.Length], normal, point);
				Vector4f vector4f3;
				Math3d.SegmentPlaneIntersection4f(out vector4f3, this.vertices[this._clippedVertices[0]], this.vertices[(this._clippedVertices[0] + 3) % this.vertices.Length], normal, point);
				this.complexClipParts[0].SetVertices(this.vertices[this._clippedVertices[0]], vector4f, vector4f2, vector4f3);
				this.complexClipParts[1].SetVertices(Vector4f.Zero, Vector4f.Zero, Vector4f.Zero, Vector4f.Zero);
				this.complexClipParts[2].SetVertices(Vector4f.Zero, Vector4f.Zero, Vector4f.Zero, Vector4f.Zero);
			}
			else if (num == 2)
			{
				Vector4f vector4f4;
				Math3d.SegmentPlaneIntersection4f(out vector4f4, this.vertices[this._clippedVertices[0]], this.vertices[this._discardedVertices[0]], normal, point);
				Vector4f vector4f5;
				Math3d.SegmentPlaneIntersection4f(out vector4f5, this.vertices[this._clippedVertices[0]], this.vertices[this._discardedVertices[1]], normal, point);
				Vector4f vector4f6;
				Math3d.SegmentPlaneIntersection4f(out vector4f6, this.vertices[this._clippedVertices[1]], this.vertices[this._discardedVertices[0]], normal, point);
				Vector4f vector4f7;
				Math3d.SegmentPlaneIntersection4f(out vector4f7, this.vertices[this._clippedVertices[1]], this.vertices[this._discardedVertices[1]], normal, point);
				this.complexClipParts[0].SetVertices(this.vertices[this._clippedVertices[0]], vector4f4, vector4f5, this.vertices[this._clippedVertices[1]]);
				this.complexClipParts[1].SetVertices(this.vertices[this._clippedVertices[1]], vector4f6, vector4f7, vector4f4);
				this.complexClipParts[2].SetVertices(this.vertices[this._clippedVertices[1]], vector4f7, vector4f5, vector4f4);
			}
			else if (num == 3)
			{
				Vector4f vector4f8;
				Math3d.SegmentPlaneIntersection4f(out vector4f8, this.vertices[this._clippedVertices[0]], this.vertices[this._discardedVertices[0]], normal, point);
				Vector4f vector4f9;
				Math3d.SegmentPlaneIntersection4f(out vector4f9, this.vertices[this._clippedVertices[1]], this.vertices[this._discardedVertices[0]], normal, point);
				Vector4f vector4f10;
				Math3d.SegmentPlaneIntersection4f(out vector4f10, this.vertices[this._clippedVertices[2]], this.vertices[this._discardedVertices[0]], normal, point);
				this.complexClipParts[0].SetVertices(this.vertices[this._clippedVertices[0]], this.vertices[this._clippedVertices[1]], this.vertices[this._clippedVertices[2]], vector4f8);
				this.complexClipParts[1].SetVertices(vector4f8, vector4f9, vector4f10, this.vertices[this._clippedVertices[1]]);
				this.complexClipParts[2].SetVertices(vector4f8, vector4f10, this.vertices[this._clippedVertices[2]], this.vertices[this._clippedVertices[1]]);
			}
			base.cachedComplexClipBody.update();
		}

		private int[] _clippedVertices = new int[4];

		private int[] _discardedVertices = new int[4];

		private Tetrahedron[] complexClipParts;
	}
}
