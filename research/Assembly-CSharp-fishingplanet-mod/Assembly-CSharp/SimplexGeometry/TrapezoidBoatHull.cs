using System;
using System.Collections.Generic;
using Mono.Simd;
using Mono.Simd.Math;

namespace SimplexGeometry
{
	public class TrapezoidBoatHull : BoatHull
	{
		public TrapezoidBoatHull(float bowLength, float bowWidth, float midLength, float width, float height, float bottomWidth, Vector4f anchor, float massValue = 0f, bool autoCenter = true)
			: base(massValue)
		{
			Vector4f vector4f;
			vector4f..ctor(-width * 0.5f, 0f, midLength * 0.5f, 0f);
			Vector4f vector4f2;
			vector4f2..ctor(width * 0.5f, 0f, midLength * 0.5f, 0f);
			Vector4f vector4f3;
			vector4f3..ctor(width * 0.5f, 0f, -midLength * 0.5f, 0f);
			Vector4f vector4f4;
			vector4f4..ctor(-width * 0.5f, 0f, -midLength * 0.5f, 0f);
			Vector4f vector4f5;
			vector4f5..ctor(-bottomWidth * 0.5f, -height, midLength * 0.5f, 0f);
			Vector4f vector4f6;
			vector4f6..ctor(bottomWidth * 0.5f, -height, midLength * 0.5f, 0f);
			Vector4f vector4f7;
			vector4f7..ctor(bottomWidth * 0.5f, -height, -midLength * 0.5f, 0f);
			Vector4f vector4f8;
			vector4f8..ctor(-bottomWidth * 0.5f, -height, -midLength * 0.5f, 0f);
			Vector4f vector4f9;
			vector4f9..ctor(-bowWidth * 0.5f, 0f, midLength * 0.5f + bowLength, 0f);
			Vector4f vector4f10;
			vector4f10..ctor(bowWidth * 0.5f, 0f, midLength * 0.5f + bowLength, 0f);
			this.MiddleA = new TriangularPrism(vector4f5, vector4f, vector4f2, vector4f8, vector4f4, vector4f3);
			this.MiddleB = new TriangularPrism(vector4f2, vector4f6, vector4f5, vector4f3, vector4f7, vector4f8);
			this.Bow = new TriangularPrism(vector4f9, vector4f, vector4f5, vector4f10, vector4f2, vector4f6);
			this.parts = new SimplexBody[] { this.MiddleA, this.MiddleB, this.Bow };
			base.Cargo = new List<SimplexBody>();
			this.update();
			if (autoCenter)
			{
				this.MiddleA.Translate(this.Barycenter.Negative());
				this.MiddleB.Translate(this.Barycenter.Negative());
				this.Bow.Translate(this.Barycenter.Negative());
			}
			else
			{
				this.MiddleA.Translate(anchor.Negative());
				this.MiddleB.Translate(anchor.Negative());
				this.Bow.Translate(anchor.Negative());
			}
			this.update();
		}

		public TriangularPrism MiddleA { get; protected set; }

		public TriangularPrism MiddleB { get; protected set; }

		public TriangularPrism Bow { get; protected set; }
	}
}
