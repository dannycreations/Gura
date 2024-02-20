using System;
using System.Collections.Generic;
using Mono.Simd;
using Mono.Simd.Math;

namespace SimplexGeometry
{
	public class TriangularBoatHull : BoatHull
	{
		public TriangularBoatHull(float bowLength, float midLength, float sternLength, float width, float height, float massValue = 0f)
			: base(massValue)
		{
			this.BowLeft = new Vector4f(-width * 0.5f, 0f, midLength * 0.5f, 0f);
			this.BowRight = new Vector4f(width * 0.5f, 0f, midLength * 0.5f, 0f);
			this.SternLeft = new Vector4f(-width * 0.5f, 0f, -midLength * 0.5f, 0f);
			this.SternRight = new Vector4f(width * 0.5f, 0f, -midLength * 0.5f, 0f);
			this.BowBottom = new Vector4f(0f, -height, midLength * 0.5f, 0f);
			this.SternBottom = new Vector4f(0f, -height, -midLength * 0.5f, 0f);
			base.BowPoint = new Vector4f(0f, 0f, midLength * 0.5f + bowLength, 0f);
			base.SternPoint = new Vector4f(0f, 0f, -midLength * 0.5f - sternLength, 0f);
			this.Bow = new Tetrahedron(base.BowPoint, this.BowLeft, this.BowRight, this.BowBottom);
			this.Middle = new TriangularPrism(this.BowLeft, this.BowRight, this.BowBottom, this.SternLeft, this.SternRight, this.SternBottom);
			this.Stern = new Tetrahedron(base.SternPoint, this.SternLeft, this.SternRight, this.SternBottom);
			this.parts = new SimplexBody[] { this.Bow, this.Middle, this.Stern };
			base.Cargo = new List<SimplexBody>();
			this.update();
			this.Bow.Translate(this.Barycenter.Negative());
			this.Middle.Translate(this.Barycenter.Negative());
			this.Stern.Translate(this.Barycenter.Negative());
			foreach (SimplexBody simplexBody in base.Cargo)
			{
				simplexBody.Translate(this.Barycenter.Negative());
			}
			this.update();
		}

		public TriangularBoatHull(Tetrahedron Bow, TriangularPrism Middle, Tetrahedron Stern, float massValue = 0f)
			: base(massValue)
		{
			this.Bow = Bow;
			this.Middle = Middle;
			this.Stern = Stern;
		}

		public Tetrahedron Bow { get; protected set; }

		public TriangularPrism Middle { get; protected set; }

		public Tetrahedron Stern { get; protected set; }

		public Vector4f BowLeft { get; protected set; }

		public Vector4f BowRight { get; protected set; }

		public Vector4f BowBottom { get; protected set; }

		public Vector4f SternLeft { get; protected set; }

		public Vector4f SternRight { get; protected set; }

		public Vector4f SternBottom { get; protected set; }

		public override SimplexBody Clone()
		{
			return new TriangularBoatHull(this.Bow.Clone() as Tetrahedron, this.Middle.Clone() as TriangularPrism, this.Stern.Clone() as Tetrahedron, this.MassValue);
		}
	}
}
