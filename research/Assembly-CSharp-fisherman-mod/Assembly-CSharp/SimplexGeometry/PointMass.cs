using System;
using Mono.Simd;
using Mono.Simd.Math;

namespace SimplexGeometry
{
	public class PointMass : SimplexBody
	{
		public PointMass()
		{
			this.vertices = new Vector4f[1];
			this.SetVertices(Vector4f.Zero);
			base.cachedSimilarClipBody = null;
			base.cachedComplexClipBody = null;
			this.MassValue = 0f;
		}

		public PointMass(float m, Vector4f v)
		{
			this.vertices = new Vector4f[1];
			this.SetVertices(v);
			base.cachedSimilarClipBody = null;
			base.cachedComplexClipBody = null;
			this.MassValue = m;
		}

		public void SetVertices(Vector4f v)
		{
			if (this.vertices.Length == 1)
			{
				this.vertices[0] = v;
			}
			else
			{
				base.SetVertices(new Vector4f[] { v });
			}
		}

		public override SimplexBody Clone()
		{
			return new PointMass(this.MassValue, this.vertices[0])
			{
				MassValue = this.MassValue
			};
		}

		public override void update()
		{
			this.Barycenter = this.vertices[0];
			this.Volume = 0f;
		}

		public override Vector4f CalculateInertiaTensor(Vector4f origin, float density = 0f)
		{
			float num = (this.vertices[0] - origin).SqrMagnitude() * this.MassValue;
			return new Vector4f(num, num, num, 1f);
		}

		public override ProceduralGeometry.MutableMesh GenerateMesh()
		{
			return new ProceduralGeometry.MutableMesh(1);
		}

		public override void SimilarClip(Vector4f point, Vector4f normal)
		{
		}

		public override void ComplexClip(Vector4f point, Vector4f normal)
		{
		}
	}
}
