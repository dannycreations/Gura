using System;
using System.Linq;

namespace ObjectModel
{
	public class TacklePhysicalParams
	{
		public TacklePhysicalParams(RodTackleProxy rodTackleProxy)
		{
			this.rodTackleProxy = rodTackleProxy;
		}

		public float TackleWindage
		{
			get
			{
				if (this.rodTackleProxy.RodTemplate == RodTemplate.UnEquiped)
				{
					return 0f;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Float)
				{
					return ((this.rodTackleProxy.Bobber == null) ? 0f : this.rodTackleProxy.Bobber.Windage) * ((this.rodTackleProxy.Bait == null) ? 0f : this.rodTackleProxy.Bait.Windage);
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Lure && this.rodTackleProxy.Lure != null)
				{
					return (this.rodTackleProxy.Lure == null) ? 0f : this.rodTackleProxy.Lure.Windage;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Jig && this.rodTackleProxy.Bait != null)
				{
					return (this.rodTackleProxy.Bait == null) ? 0f : this.rodTackleProxy.Bait.Windage;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Bottom)
				{
					return 1f;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.ClassicCarp || this.rodTackleProxy.RodTemplate == RodTemplate.MethodCarp)
				{
					return 1f;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.PVACarp)
				{
					return 1f;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Spod)
				{
					return 1f;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.FlippingRig || this.rodTackleProxy.RodTemplate == RodTemplate.SpinnerTails || this.rodTackleProxy.RodTemplate == RodTemplate.SpinnerbaitTails)
				{
					return ((this.rodTackleProxy.Lure == null) ? 0f : this.rodTackleProxy.Lure.Windage) * ((this.rodTackleProxy.Bait == null) ? 0f : this.rodTackleProxy.Bait.Windage);
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.CarolinaRig || this.rodTackleProxy.RodTemplate == RodTemplate.TexasRig || this.rodTackleProxy.RodTemplate == RodTemplate.ThreewayRig)
				{
					return ((this.rodTackleProxy.Sinker == null) ? 0f : this.rodTackleProxy.Sinker.Windage) * ((this.rodTackleProxy.Bait == null) ? 0f : this.rodTackleProxy.Bait.Windage);
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.OffsetJig)
				{
					return (this.rodTackleProxy.Bait == null) ? 0f : this.rodTackleProxy.Bait.Windage;
				}
				throw new NotImplementedException("TackleWindage calculation is not implemented for RodTemplate = " + Enum.GetName(typeof(RodTemplate), this.rodTackleProxy.RodTemplate));
			}
		}

		public float TackleMass
		{
			get
			{
				if (this.rodTackleProxy.RodTemplate == RodTemplate.UnEquiped)
				{
					return 0f;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Float)
				{
					float num2;
					if (this.rodTackleProxy.Bobber != null)
					{
						float num = (float)((double)this.rodTackleProxy.Bobber.SinkerMass);
						double? weight = this.rodTackleProxy.Bobber.Weight;
						num2 = (float)((double)num + ((weight == null) ? 0.0 : weight.Value));
					}
					else
					{
						num2 = (float)0.0;
					}
					float num3;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight2 = this.rodTackleProxy.Hook.Weight;
						num3 = (float)((weight2 == null) ? 0.0 : weight2.Value);
					}
					else
					{
						num3 = (float)0.0;
					}
					float num4 = num2 + num3;
					float num5;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight3 = this.rodTackleProxy.Bait.Weight;
						num5 = (float)((weight3 == null) ? 0.0 : weight3.Value);
					}
					else
					{
						num5 = (float)0.0;
					}
					return num4 + num5;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Lure)
				{
					float num6;
					if (this.rodTackleProxy.Lure != null)
					{
						double? weight4 = this.rodTackleProxy.Lure.Weight;
						num6 = (float)((weight4 == null) ? 0.0 : weight4.Value);
					}
					else
					{
						num6 = (float)0.0;
					}
					return num6;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Jig)
				{
					float num7;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight5 = this.rodTackleProxy.Hook.Weight;
						num7 = (float)((weight5 == null) ? 0.0 : weight5.Value);
					}
					else
					{
						num7 = (float)0.0;
					}
					float num8;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight6 = this.rodTackleProxy.Bait.Weight;
						num8 = (float)((weight6 == null) ? 0.0 : weight6.Value);
					}
					else
					{
						num8 = (float)0.0;
					}
					return num7 + num8;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Bottom)
				{
					float num9;
					if (this.rodTackleProxy.Sinker != null)
					{
						double? weight7 = this.rodTackleProxy.Sinker.Weight;
						num9 = (float)((weight7 == null) ? 0.0 : weight7.Value);
					}
					else
					{
						num9 = (float)0.0;
					}
					float num10;
					if (this.rodTackleProxy.Feeder != null)
					{
						double? weight8 = this.rodTackleProxy.Feeder.Weight;
						num10 = (float)((weight8 == null) ? 0.0 : weight8.Value);
					}
					else
					{
						num10 = (float)0.0;
					}
					float num11 = num9 + num10;
					float num12;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight9 = this.rodTackleProxy.Hook.Weight;
						num12 = (float)((weight9 == null) ? 0.0 : weight9.Value);
					}
					else
					{
						num12 = (float)0.0;
					}
					float num13 = num11 + num12;
					float num14;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight10 = this.rodTackleProxy.Bait.Weight;
						num14 = (float)((weight10 == null) ? 0.0 : weight10.Value);
					}
					else
					{
						num14 = (float)0.0;
					}
					float num15 = num13 + num14;
					double num16;
					if (this.rodTackleProxy.Chum != null)
					{
						num16 = (double)this.rodTackleProxy.Chum.ToList<Chum>().SumFloat(delegate(Chum c)
						{
							double? weight23 = c.Weight;
							return (float)((weight23 == null) ? 0.0 : weight23.Value);
						});
					}
					else
					{
						num16 = (double)0f;
					}
					return (float)((double)num15 + num16);
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.ClassicCarp || this.rodTackleProxy.RodTemplate == RodTemplate.MethodCarp || this.rodTackleProxy.RodTemplate == RodTemplate.PVACarp)
				{
					float num17;
					if (this.rodTackleProxy.Sinker != null)
					{
						double? weight11 = this.rodTackleProxy.Sinker.Weight;
						num17 = (float)((weight11 == null) ? 0.0 : weight11.Value);
					}
					else
					{
						num17 = (float)0.0;
					}
					float num18;
					if (this.rodTackleProxy.Feeder != null)
					{
						double? weight12 = this.rodTackleProxy.Feeder.Weight;
						num18 = (float)((weight12 == null) ? 0.0 : weight12.Value);
					}
					else
					{
						num18 = (float)0.0;
					}
					float num19 = num17 + num18;
					float num20;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight13 = this.rodTackleProxy.Hook.Weight;
						num20 = (float)((weight13 == null) ? 0.0 : weight13.Value);
					}
					else
					{
						num20 = (float)0.0;
					}
					float num21 = num19 + num20;
					float num22;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight14 = this.rodTackleProxy.Bait.Weight;
						num22 = (float)((weight14 == null) ? 0.0 : weight14.Value);
					}
					else
					{
						num22 = (float)0.0;
					}
					float num23 = num21 + num22;
					double num24;
					if (this.rodTackleProxy.Chum != null)
					{
						num24 = (double)this.rodTackleProxy.Chum.ToList<Chum>().SumFloat(delegate(Chum c)
						{
							double? weight24 = c.Weight;
							return (float)((weight24 == null) ? 0.0 : weight24.Value);
						});
					}
					else
					{
						num24 = (double)0f;
					}
					return (float)((double)num23 + num24);
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Spod)
				{
					float num25;
					if (this.rodTackleProxy.Feeder != null)
					{
						double? weight15 = this.rodTackleProxy.Feeder.Weight;
						num25 = (float)((weight15 == null) ? 0.0 : weight15.Value);
					}
					else
					{
						num25 = (float)0.0;
					}
					double num26;
					if (this.rodTackleProxy.Chum != null)
					{
						num26 = (double)this.rodTackleProxy.Chum.ToList<Chum>().SumFloat(delegate(Chum c)
						{
							double? weight25 = c.Weight;
							return (float)((weight25 == null) ? 0.0 : weight25.Value);
						});
					}
					else
					{
						num26 = (double)0f;
					}
					return (float)((double)num25 + num26);
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.FlippingRig || this.rodTackleProxy.RodTemplate == RodTemplate.SpinnerTails || this.rodTackleProxy.RodTemplate == RodTemplate.SpinnerbaitTails)
				{
					float num27;
					if (this.rodTackleProxy.Lure != null)
					{
						double? weight16 = this.rodTackleProxy.Lure.Weight;
						num27 = (float)((weight16 == null) ? 0.0 : weight16.Value);
					}
					else
					{
						num27 = (float)0.0;
					}
					float num28;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight17 = this.rodTackleProxy.Bait.Weight;
						num28 = (float)((weight17 == null) ? 0.0 : weight17.Value);
					}
					else
					{
						num28 = (float)0.0;
					}
					return num27 + num28;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.CarolinaRig || this.rodTackleProxy.RodTemplate == RodTemplate.TexasRig || this.rodTackleProxy.RodTemplate == RodTemplate.ThreewayRig)
				{
					float num29;
					if (this.rodTackleProxy.Sinker != null)
					{
						double? weight18 = this.rodTackleProxy.Sinker.Weight;
						num29 = (float)((weight18 == null) ? 0.0 : weight18.Value);
					}
					else
					{
						num29 = (float)0.0;
					}
					float num30;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight19 = this.rodTackleProxy.Hook.Weight;
						num30 = (float)((weight19 == null) ? 0.0 : weight19.Value);
					}
					else
					{
						num30 = (float)0.0;
					}
					float num31 = num29 + num30;
					float num32;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight20 = this.rodTackleProxy.Bait.Weight;
						num32 = (float)((weight20 == null) ? 0.0 : weight20.Value);
					}
					else
					{
						num32 = (float)0.0;
					}
					return num31 + num32;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.OffsetJig)
				{
					float num33;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight21 = this.rodTackleProxy.Hook.Weight;
						num33 = (float)((weight21 == null) ? 0.0 : weight21.Value);
					}
					else
					{
						num33 = (float)0.0;
					}
					float num34;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight22 = this.rodTackleProxy.Bait.Weight;
						num34 = (float)((weight22 == null) ? 0.0 : weight22.Value);
					}
					else
					{
						num34 = (float)0.0;
					}
					return num33 + num34;
				}
				throw new NotImplementedException("TackleMass calculation is not implemented for RodTemplate = " + Enum.GetName(typeof(RodTemplate), this.rodTackleProxy.RodTemplate));
			}
		}

		public float TackleMassForLeader
		{
			get
			{
				if (this.rodTackleProxy.RodTemplate == RodTemplate.UnEquiped)
				{
					return 0f;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Float)
				{
					float num;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight = this.rodTackleProxy.Hook.Weight;
						num = (float)((weight == null) ? 0.0 : weight.Value);
					}
					else
					{
						num = (float)0.0;
					}
					float num2;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight2 = this.rodTackleProxy.Bait.Weight;
						num2 = (float)((weight2 == null) ? 0.0 : weight2.Value);
					}
					else
					{
						num2 = (float)0.0;
					}
					return num + num2;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Lure)
				{
					float num3;
					if (this.rodTackleProxy.Lure != null)
					{
						double? weight3 = this.rodTackleProxy.Lure.Weight;
						num3 = (float)((weight3 == null) ? 0.0 : weight3.Value);
					}
					else
					{
						num3 = (float)0.0;
					}
					return num3;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Jig)
				{
					float num4;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight4 = this.rodTackleProxy.Hook.Weight;
						num4 = (float)((weight4 == null) ? 0.0 : weight4.Value);
					}
					else
					{
						num4 = (float)0.0;
					}
					float num5;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight5 = this.rodTackleProxy.Bait.Weight;
						num5 = (float)((weight5 == null) ? 0.0 : weight5.Value);
					}
					else
					{
						num5 = (float)0.0;
					}
					return num4 + num5;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Bottom)
				{
					float num6;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight6 = this.rodTackleProxy.Hook.Weight;
						num6 = (float)((weight6 == null) ? 0.0 : weight6.Value);
					}
					else
					{
						num6 = (float)0.0;
					}
					float num7;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight7 = this.rodTackleProxy.Bait.Weight;
						num7 = (float)((weight7 == null) ? 0.0 : weight7.Value);
					}
					else
					{
						num7 = (float)0.0;
					}
					return num6 + num7;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.ClassicCarp || this.rodTackleProxy.RodTemplate == RodTemplate.MethodCarp || this.rodTackleProxy.RodTemplate == RodTemplate.PVACarp)
				{
					float num8;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight8 = this.rodTackleProxy.Hook.Weight;
						num8 = (float)((weight8 == null) ? 0.0 : weight8.Value);
					}
					else
					{
						num8 = (float)0.0;
					}
					float num9;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight9 = this.rodTackleProxy.Bait.Weight;
						num9 = (float)((weight9 == null) ? 0.0 : weight9.Value);
					}
					else
					{
						num9 = (float)0.0;
					}
					return num8 + num9;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.Spod)
				{
					return 0f;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.FlippingRig || this.rodTackleProxy.RodTemplate == RodTemplate.SpinnerTails || this.rodTackleProxy.RodTemplate == RodTemplate.SpinnerbaitTails)
				{
					float num10;
					if (this.rodTackleProxy.Lure != null)
					{
						double? weight10 = this.rodTackleProxy.Lure.Weight;
						num10 = (float)((weight10 == null) ? 0.0 : weight10.Value);
					}
					else
					{
						num10 = (float)0.0;
					}
					float num11;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight11 = this.rodTackleProxy.Bait.Weight;
						num11 = (float)((weight11 == null) ? 0.0 : weight11.Value);
					}
					else
					{
						num11 = (float)0.0;
					}
					return num10 + num11;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.CarolinaRig || this.rodTackleProxy.RodTemplate == RodTemplate.TexasRig || this.rodTackleProxy.RodTemplate == RodTemplate.ThreewayRig)
				{
					float num12;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight12 = this.rodTackleProxy.Hook.Weight;
						num12 = (float)((weight12 == null) ? 0.0 : weight12.Value);
					}
					else
					{
						num12 = (float)0.0;
					}
					float num13;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight13 = this.rodTackleProxy.Bait.Weight;
						num13 = (float)((weight13 == null) ? 0.0 : weight13.Value);
					}
					else
					{
						num13 = (float)0.0;
					}
					return num12 + num13;
				}
				if (this.rodTackleProxy.RodTemplate == RodTemplate.OffsetJig)
				{
					float num14;
					if (this.rodTackleProxy.Hook != null)
					{
						double? weight14 = this.rodTackleProxy.Hook.Weight;
						num14 = (float)((weight14 == null) ? 0.0 : weight14.Value);
					}
					else
					{
						num14 = (float)0.0;
					}
					float num15;
					if (this.rodTackleProxy.Bait != null)
					{
						double? weight15 = this.rodTackleProxy.Bait.Weight;
						num15 = (float)((weight15 == null) ? 0.0 : weight15.Value);
					}
					else
					{
						num15 = (float)0.0;
					}
					return num14 + num15;
				}
				throw new NotImplementedException("TackleMass calculation is not implemented for RodTemplate = " + Enum.GetName(typeof(RodTemplate), this.rodTackleProxy.RodTemplate));
			}
		}

		private readonly RodTackleProxy rodTackleProxy;
	}
}
