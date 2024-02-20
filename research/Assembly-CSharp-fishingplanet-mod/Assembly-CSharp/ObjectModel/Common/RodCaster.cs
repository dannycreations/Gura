using System;

namespace ObjectModel.Common
{
	public class RodCaster
	{
		public float CurrentMaxCastLength
		{
			get
			{
				if (this.CastingForce == 0f)
				{
					return this.RodLength * 2f;
				}
				if (this.TackleWeight < this.ReelCastWeightMin)
				{
					return this.RodLength * 2f;
				}
				float num;
				if (this.TackleWeight <= this.AbsoluteX1)
				{
					num = this.RodMaxCastLength * 0.1f;
				}
				else if (this.TackleWeight <= this.AbsoluteX2)
				{
					num = RodCaster.Interpolate(this.AbsoluteX1, this.RodMaxCastLength * 0.1f, this.AbsoluteX2, this.RodMaxCastLength * 0.6f, this.TackleWeight);
				}
				else if (this.TackleWeight <= this.RodCastWeightMin)
				{
					num = RodCaster.Interpolate(this.AbsoluteX2, this.RodMaxCastLength * 0.6f, this.RodCastWeightMin, this.RodMaxCastLength * 0.85f, this.TackleWeight);
				}
				else if (this.TackleWeight <= this.Peak)
				{
					num = RodCaster.Interpolate(this.RodCastWeightMin, this.RodMaxCastLength * 0.85f, this.Peak, this.RodMaxCastLength, this.TackleWeight);
				}
				else if (this.TackleWeight <= this.RodCastWeightMax)
				{
					num = RodCaster.Interpolate(this.Peak, this.RodMaxCastLength, this.RodCastWeightMax, this.RodMaxCastLength * 0.95f, this.TackleWeight);
				}
				else if (this.TackleWeight <= this.AbsoluteX3)
				{
					num = RodCaster.Interpolate(this.RodCastWeightMax, this.RodMaxCastLength * 0.95f, this.AbsoluteX3, this.RodMaxCastLength * 0.7f, this.TackleWeight);
				}
				else if (this.TackleWeight <= this.AbsoluteX4)
				{
					num = RodCaster.Interpolate(this.AbsoluteX3, this.RodMaxCastLength * 0.7f, this.AbsoluteX4, this.RodMaxCastLength * 0.3f, this.TackleWeight);
				}
				else
				{
					num = this.RodMaxCastLength * 0.3f;
				}
				num *= this.ReelFriction * this.TackleWindage * ((this.LineType != ItemSubTypes.BraidLine) ? 1f : 0.95f) * this.LineThicknessEffect * this.CastingForce;
				return Math.Max(this.RodLength * 2f, num);
			}
		}

		public static bool CanBreakRod(float tackleWeight, float rodCastWeightMax)
		{
			return tackleWeight > rodCastWeightMax * 2f;
		}

		public static bool CanBreakLine(float tackleWeight, float lineMaxLoad)
		{
			return tackleWeight > RodCaster.Interpolate(0.9f, 0.01f, 3.6f, 0.08f, lineMaxLoad) - 0.001f;
		}

		public float RodBreakProbability
		{
			get
			{
				if (this.CastingForce == 0f)
				{
					return 0f;
				}
				return (this.TackleWeight > this.AbsoluteX4) ? ((this.TackleWeight < this.AbsoluteX5) ? (RodCaster.Interpolate(this.AbsoluteX4, 0f, this.AbsoluteX5, 1f, this.TackleWeight) * this.CastingForce) : 1f) : 0f;
			}
		}

		public float LineBreakProbability
		{
			get
			{
				if (this.CastingForce == 0f)
				{
					return 0f;
				}
				if (this.TackleWeight <= this.LineStartBreakMass)
				{
					return 0f;
				}
				float num = RodCaster.Interpolate(this.LineStartBreakMass, 0f, this.LineEndBreakMass, 1f, this.TackleWeight) * this.CastingForce;
				return Math.Min(num, 1f);
			}
		}

		public float BacklashProbability
		{
			get
			{
				if (this.CastingForce == 0f)
				{
					return 0f;
				}
				if (this.TackleWeight <= this.RodCastWeightMin || this.TackleWeight <= this.ReelCastWeightMin)
				{
					return this.ReelBacklashProbability * 4f * this.CastingForce;
				}
				if (this.TackleWeight > this.RodCastWeightMax)
				{
					return this.ReelBacklashProbability * 4f * this.CastingForce;
				}
				return this.ReelBacklashProbability * this.CastingForce;
			}
		}

		public float BacklashedMaxCastLength
		{
			get
			{
				return this.CurrentMaxCastLength * 0.5f;
			}
		}

		public float BacklashedLostLineLength
		{
			get
			{
				return this.CurrentMaxCastLength + 5f;
			}
		}

		public float BacklashRemoveTimeout
		{
			get
			{
				return RodCaster.Interpolate(10f, 5f, 50f, 20f, this.BacklashedMaxCastLength);
			}
		}

		private float Peak
		{
			get
			{
				return (this.RodCastWeightMin + this.RodCastWeightMax) / 2f;
			}
		}

		private float LineThicknessEffect
		{
			get
			{
				if (this.LineThickness <= 0.35f)
				{
					return RodCaster.Interpolate(0.05f, 1f, 0.35f, 0.8f, this.LineThickness);
				}
				if (this.LineThickness <= 1f)
				{
					return RodCaster.Interpolate(0.35f, 0.8f, 1f, 0.6f, this.LineThickness);
				}
				return 0.6f;
			}
		}

		private float AbsoluteX1
		{
			get
			{
				return this.RodCastWeightMin * 0.100000024f;
			}
		}

		private float AbsoluteX2
		{
			get
			{
				return this.RodCastWeightMin * 0.5f;
			}
		}

		private float AbsoluteX3
		{
			get
			{
				return this.RodCastWeightMax * 1.5f;
			}
		}

		private float AbsoluteX4
		{
			get
			{
				return this.RodCastWeightMax * 2f;
			}
		}

		private float AbsoluteX5
		{
			get
			{
				return this.RodCastWeightMax * 2.5f;
			}
		}

		private float LineStartBreakMass
		{
			get
			{
				return RodCaster.Interpolate(0.9f, 0.01f, 3.6f, 0.08f, this.LineMaxLoad) - 0.001f;
			}
		}

		private float LineEndBreakMass
		{
			get
			{
				return RodCaster.Interpolate(0.9f, 0.015f, 3.6f, 0.12f, this.LineMaxLoad) - 0.001f;
			}
		}

		private static float Interpolate(float x1, float y1, float x2, float y2, float x)
		{
			return y1 + (y2 - y1) / (x2 - x1) * (x - x1);
		}

		public float RodMaxCastLength;

		public float RodCastWeightMin;

		public float RodCastWeightMax;

		public float RodLength;

		public ItemSubTypes LineType;

		public float LineThickness;

		public float LineMaxLoad;

		public float ReelFriction;

		public float ReelCastWeightMin;

		public float ReelBacklashProbability;

		public float TackleWindage;

		public float TackleWeight;

		public float CastingForce;

		private const float X1 = 0.9f;

		private const float X2 = 0.5f;

		private const float X3 = 0.5f;

		private const float X4 = 1f;

		private const float X5 = 1.5f;

		private const float Y1 = 0.1f;

		private const float Y2 = 0.6f;

		private const float Y3 = 0.85f;

		private const float Y4 = 0.95f;

		private const float Y5 = 0.7f;

		private const float Y6 = 0.3f;

		private const float RegularLineFriction = 1f;

		private const float BraidLineFriction = 0.95f;

		private const float LineX1 = 0.05f;

		private const float LineX2 = 0.35f;

		private const float LineX3 = 1f;

		private const float LineY1 = 1f;

		private const float LineY2 = 0.8f;

		private const float LineY3 = 0.6f;

		private const float BreakY1 = 0f;

		private const float BreakY2 = 1f;

		private const float LineBreakX1 = 0.9f;

		private const float LineBreakX2 = 3.6f;

		private const float LineBreakStartY1 = 0.01f;

		private const float LineBreakStartY2 = 0.08f;

		private const float LineBreakEndY1 = 0.015f;

		private const float LineBreakEndY2 = 0.12f;

		private const float LineBreakStep = 0.001f;

		private const float BacklashLowWeightMultiplier = 4f;

		private const float BacklashHighWeightMultiplier = 4f;

		private const float BacklashCastShortage = 0.5f;

		private const float BacklashAddLostLine = 5f;

		private const float BacklashX1 = 10f;

		private const float BacklashX2 = 50f;

		private const float BacklashY1 = 5f;

		private const float BacklashY2 = 20f;
	}
}
