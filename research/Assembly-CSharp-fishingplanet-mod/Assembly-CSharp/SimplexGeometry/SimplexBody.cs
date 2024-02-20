using System;
using Mono.Simd;

namespace SimplexGeometry
{
	public abstract class SimplexBody
	{
		public SimplexBody cachedSimilarClipBody { get; protected set; }

		public SimplexBody cachedComplexClipBody { get; protected set; }

		public abstract void update();

		public virtual Vector4f Barycenter { get; protected set; }

		public virtual float Volume { get; protected set; }

		public virtual float MassValue { get; protected set; }

		public void SetVertices(Vector4f[] newVertices)
		{
			Array.Copy(newVertices, this.vertices, this.vertices.Length);
			this.update();
		}

		public virtual void Translate(Vector4f offset)
		{
			for (int i = 0; i < this.vertices.Length; i++)
			{
				this.vertices[i] += offset;
			}
			this.update();
		}

		public abstract SimplexBody Clone();

		public abstract Vector4f CalculateInertiaTensor(Vector4f origin, float density = 0f);

		public abstract ProceduralGeometry.MutableMesh GenerateMesh();

		public abstract void SimilarClip(Vector4f point, Vector4f normal);

		public abstract void ComplexClip(Vector4f point, Vector4f normal);

		protected Vector4f[] vertices;
	}
}
