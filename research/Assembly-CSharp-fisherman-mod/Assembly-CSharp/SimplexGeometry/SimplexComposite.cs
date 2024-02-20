using System;
using System.Collections.Generic;
using Mono.Simd;
using Mono.Simd.Math;
using UnityEngine;

namespace SimplexGeometry
{
	public class SimplexComposite : SimplexBody
	{
		public SimplexComposite()
		{
		}

		public SimplexComposite(float massValue)
		{
			this.MassValue = massValue;
			this.MassHull = massValue;
		}

		public SimplexComposite(SimplexBody[] parts)
		{
			this.parts = parts;
			this.update();
		}

		public SimplexComposite(Vector4f[] vertices, int[] indices)
		{
			this.vertices = vertices;
			this.parts = new Tetrahedron[indices.Length / 4];
			for (int i = 0; i < this.parts.Length; i++)
			{
				this.parts[i] = new Tetrahedron(vertices[indices[i * 4]], vertices[indices[i * 4 + 1]], vertices[indices[i * 4 + 2]], vertices[indices[i * 4 + 3]]);
			}
			this.update();
		}

		public List<SimplexBody> Cargo { get; protected set; }

		public List<Triangle> Surface { get; protected set; }

		public float MassHull { get; protected set; }

		public Vector4f BarycenterHull { get; protected set; }

		public Vector4f InertiaTensorHull { get; protected set; }

		public float VolumeSurface { get; protected set; }

		public void SetMass(float massValue)
		{
			this.MassValue = ((massValue <= 0f) ? 0f : massValue);
		}

		public void SetBarycenter(Vector4f barycenter)
		{
			this.Barycenter = barycenter;
		}

		public override SimplexBody Clone()
		{
			SimplexBody[] array = new SimplexBody[this.parts.Length];
			for (int i = 0; i < this.parts.Length; i++)
			{
				array[i] = this.parts[i].Clone();
			}
			SimplexComposite simplexComposite = new SimplexComposite(array);
			simplexComposite.MassValue = this.MassValue;
			simplexComposite.MassHull = this.MassHull;
			simplexComposite.InertiaTensor = this.InertiaTensor;
			simplexComposite.InertiaTensorHull = this.InertiaTensorHull;
			simplexComposite.VolumeSurface = this.VolumeSurface;
			if (this.Cargo != null)
			{
				simplexComposite.Cargo = new List<SimplexBody>();
				foreach (SimplexBody simplexBody in this.Cargo)
				{
					simplexComposite.Cargo.Add(simplexBody);
				}
			}
			if (this.Surface != null)
			{
				simplexComposite.Surface = new List<Triangle>();
				foreach (Triangle triangle in this.Surface)
				{
					simplexComposite.Surface.Add(triangle);
				}
			}
			return simplexComposite;
		}

		public override void Translate(Vector4f offset)
		{
			for (int i = 0; i < this.parts.Length; i++)
			{
				this.parts[i].Translate(offset);
				this.parts[i].update();
			}
			if (this.Cargo != null)
			{
				foreach (SimplexBody simplexBody in this.Cargo)
				{
					simplexBody.Translate(offset);
					simplexBody.update();
				}
			}
			if (this.Surface != null)
			{
				foreach (Triangle triangle in this.Surface)
				{
					triangle.Translate(offset);
					triangle.update();
				}
			}
			this.update();
		}

		public SimplexBody Part(int index)
		{
			return this.parts[index];
		}

		public override void update()
		{
			this.Volume = 0f;
			this.VolumeSurface = 0f;
			this.Barycenter = Vector4f.Zero;
			for (int i = 0; i < this.parts.Length; i++)
			{
				this.parts[i].update();
				this.Volume += this.parts[i].Volume;
			}
			if (this.Surface == null)
			{
				if (!Mathf.Approximately(this.Volume, 0f))
				{
					for (int j = 0; j < this.parts.Length; j++)
					{
						this.Barycenter += this.parts[j].Barycenter * new Vector4f(this.parts[j].Volume / this.Volume);
					}
				}
			}
			else
			{
				foreach (Triangle triangle in this.Surface)
				{
					triangle.update();
					this.VolumeSurface += triangle.Volume;
				}
				if (!Mathf.Approximately(this.VolumeSurface, 0f))
				{
					foreach (Triangle triangle2 in this.Surface)
					{
						this.Barycenter += triangle2.Barycenter * new Vector4f(triangle2.Volume / this.VolumeSurface);
					}
				}
			}
			if (this.complexClipParts == null || this.complexClipParts.Length != this.parts.Length)
			{
				this.complexClipParts = new SimplexBody[this.parts.Length];
			}
			this.BarycenterHull = this.Barycenter;
			this.UpdateCargo();
		}

		public virtual void UpdateCargo()
		{
			if (this.Cargo == null)
			{
				return;
			}
			this.MassValue = this.MassHull;
			this.Barycenter = this.BarycenterHull;
			this.InertiaTensor = this.InertiaTensorHull;
			if (this.Cargo.Count > 0)
			{
				foreach (SimplexBody simplexBody in this.Cargo)
				{
					simplexBody.update();
					this.MassValue += simplexBody.MassValue;
				}
				if (!Mathf.Approximately(this.MassValue, 0f))
				{
					this.Barycenter *= new Vector4f(this.MassHull / this.MassValue);
					foreach (SimplexBody simplexBody2 in this.Cargo)
					{
						this.Barycenter += simplexBody2.Barycenter * new Vector4f(simplexBody2.MassValue / this.MassValue);
					}
					if (this.Barycenter != this.BarycenterHull)
					{
						float num = this.MassHull * (this.Barycenter - this.BarycenterHull).SqrMagnitude();
						ref Vector4f ptr = ref this.InertiaTensor;
						this.InertiaTensor.set_Component(0, ptr.get_Component(0) + num);
						ptr = ref this.InertiaTensor;
						this.InertiaTensor.set_Component(1, ptr.get_Component(1) + num);
						ptr = ref this.InertiaTensor;
						this.InertiaTensor.set_Component(2, ptr.get_Component(2) + num);
					}
					foreach (SimplexBody simplexBody3 in this.Cargo)
					{
						this.InertiaTensor += simplexBody3.CalculateInertiaTensor(this.Barycenter, 0f);
					}
					this.InertiaTensor.W = 1f;
				}
			}
		}

		public override Vector4f CalculateInertiaTensor(Vector4f origin, float density = 0f)
		{
			Vector4f vector4f = Vector4f.Zero;
			float num = this.MassHull;
			if (this.Surface == null)
			{
				if (density > 0f)
				{
					num = density * this.Volume;
				}
				for (int i = 0; i < this.parts.Length; i++)
				{
					vector4f += this.parts[i].CalculateInertiaTensor(this.BarycenterHull, density);
				}
			}
			else
			{
				if (density > 0f)
				{
					num = density * this.VolumeSurface;
				}
				foreach (Triangle triangle in this.Surface)
				{
					vector4f += triangle.CalculateInertiaTensor(this.BarycenterHull, density);
				}
			}
			vector4f.W = 1f;
			this.InertiaTensorHull = vector4f;
			if (origin != this.BarycenterHull)
			{
				float num2 = num * (this.BarycenterHull - origin).SqrMagnitude();
				ref Vector4f ptr = ref vector4f;
				vector4f.set_Component(0, ptr.get_Component(0) + num2);
				ptr = ref vector4f;
				vector4f.set_Component(1, ptr.get_Component(1) + num2);
				ptr = ref vector4f;
				vector4f.set_Component(2, ptr.get_Component(2) + num2);
			}
			if (this.Cargo != null)
			{
				foreach (SimplexBody simplexBody in this.Cargo)
				{
					vector4f += simplexBody.CalculateInertiaTensor(origin, density);
				}
				vector4f.W = 1f;
			}
			this.InertiaTensor = vector4f;
			return vector4f;
		}

		public override ProceduralGeometry.MutableMesh GenerateMesh()
		{
			ProceduralGeometry.MutableMesh mutableMesh = new ProceduralGeometry.MutableMesh(1);
			for (int i = 0; i < this.parts.Length; i++)
			{
				mutableMesh.Merge(this.parts[i].GenerateMesh(), 0);
			}
			return mutableMesh;
		}

		public override void SimilarClip(Vector4f point, Vector4f normal)
		{
		}

		public override void ComplexClip(Vector4f point, Vector4f normal)
		{
			for (int i = 0; i < this.parts.Length; i++)
			{
				this.parts[i].ComplexClip(point, normal);
				this.complexClipParts[i] = this.parts[i].cachedComplexClipBody;
			}
			if (base.cachedComplexClipBody == null)
			{
				base.cachedComplexClipBody = new SimplexComposite(this.complexClipParts);
			}
			base.cachedComplexClipBody.update();
		}

		protected SimplexBody[] parts;

		public Vector4f InertiaTensor;

		private SimplexBody[] complexClipParts;
	}
}
