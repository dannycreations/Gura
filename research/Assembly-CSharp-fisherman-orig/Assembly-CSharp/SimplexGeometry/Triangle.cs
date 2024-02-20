using System;
using Mono.Simd;
using Mono.Simd.Math;

namespace SimplexGeometry
{
	public class Triangle : SimplexBody
	{
		public Triangle()
		{
			this.vertices = new Vector4f[3];
			this.SetVertices(Vector4f.Zero, Vector4f.Zero, Vector4f.Zero);
			base.cachedSimilarClipBody = null;
			base.cachedComplexClipBody = null;
		}

		public Triangle(Vector4f v0, Vector4f v1, Vector4f v2)
		{
			this.vertices = new Vector4f[3];
			this.SetVertices(v0, v1, v2);
			base.cachedSimilarClipBody = null;
			base.cachedComplexClipBody = null;
		}

		public void SetVertices(Vector4f v0, Vector4f v1, Vector4f v2)
		{
			if (this.vertices.Length == 3)
			{
				this.vertices[0] = v0;
				this.vertices[1] = v1;
				this.vertices[2] = v2;
			}
			else
			{
				base.SetVertices(new Vector4f[] { v0, v1, v2 });
			}
		}

		public override SimplexBody Clone()
		{
			return new Triangle(this.vertices[0], this.vertices[1], this.vertices[2])
			{
				MassValue = this.MassValue
			};
		}

		public override void update()
		{
			this.Barycenter = (this.vertices[0] + this.vertices[1] + this.vertices[2]) * new Vector4f(0.33333334f);
			Vector4f vector4f = this.vertices[0] - this.vertices[2];
			Vector4f vector4f2 = this.vertices[1] - this.vertices[2];
			this.Volume = Vector4fExtensions.Cross(vector4f, vector4f2).Magnitude() / 2f;
		}

		public override Vector4f CalculateInertiaTensor(Vector4f origin, float density = 0f)
		{
			Vector4f vector4f;
			if (density > 0f)
			{
				vector4f..ctor(density * this.Volume / 6f);
			}
			else
			{
				vector4f..ctor(this.MassValue / 6f);
			}
			Vector4f vector4f2 = this.vertices[0] - origin;
			Vector4f vector4f3 = this.vertices[1] - origin;
			Vector4f vector4f4 = this.vertices[2] - origin;
			Vector4f vector4f5 = vector4f2 * vector4f2;
			Vector4f vector4f6 = vector4f2 * vector4f3;
			Vector4f vector4f7 = vector4f2 * vector4f4;
			Vector4f vector4f8 = vector4f3 * vector4f3;
			Vector4f vector4f9 = vector4f3 * vector4f4;
			Vector4f vector4f10 = vector4f4 * vector4f4;
			Vector4f vector4f11;
			vector4f11..ctor(vector4f5.Y + vector4f6.Y + vector4f8.Y + vector4f7.Y + vector4f9.Y + vector4f10.Y + vector4f5.Z + vector4f6.Z + vector4f8.Z + vector4f7.Z + vector4f9.Z + vector4f10.Z, vector4f5.X + vector4f6.X + vector4f8.X + vector4f7.X + vector4f9.X + vector4f10.X + vector4f5.Z + vector4f6.Z + vector4f8.Z + vector4f7.Z + vector4f9.Z + vector4f10.Z, vector4f5.X + vector4f6.X + vector4f8.X + vector4f7.X + vector4f9.X + vector4f10.X + vector4f5.Y + vector4f6.Y + vector4f8.Y + vector4f7.Y + vector4f9.Y + vector4f10.Y, 1f);
			return vector4f11 * vector4f;
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
