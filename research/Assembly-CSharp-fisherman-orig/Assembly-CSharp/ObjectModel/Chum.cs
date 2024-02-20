using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(0)]
	public class Chum : TerminalTackleItem, IChum
	{
		public void InitBasicChumProperties()
		{
			base.ItemId = int.MaxValue;
			base.ItemType = ItemTypes.Chum;
			base.ItemSubType = ItemSubTypes.Chum;
			this.IsUnstockable = true;
			this.Count = 1;
			base.Durability = 1;
		}

		[NoClone(true)]
		public override string Name { get; set; }

		[NoClone(true)]
		public byte[] ImageData { get; set; }

		[NoClone(true)]
		public Guid? SplitFromInstanceId { get; set; }

		[JsonIgnore]
		public DateTime? BeginSplitTime { get; set; }

		[NoClone(true)]
		public List<ChumIngredient> Ingredients { get; set; }

		[JsonIgnore]
		public List<ChumBase> ChumBase
		{
			get
			{
				return (this.Ingredients == null) ? new List<ChumBase>() : this.Ingredients.OfType<ChumBase>().ToList<ChumBase>();
			}
		}

		[JsonIgnore]
		public List<ChumAroma> ChumAroma
		{
			get
			{
				return (this.Ingredients == null) ? new List<ChumAroma>() : this.Ingredients.OfType<ChumAroma>().ToList<ChumAroma>();
			}
		}

		[JsonIgnore]
		public List<ChumParticle> ChumParticle
		{
			get
			{
				return (this.Ingredients == null) ? new List<ChumParticle>() : this.Ingredients.OfType<ChumParticle>().ToList<ChumParticle>();
			}
		}

		[JsonIgnore]
		public float PercentageBase
		{
			get
			{
				return this.ChumBase.SumFloat((ChumBase i) => i.Percentage);
			}
		}

		[JsonIgnore]
		public float PercentageAroma
		{
			get
			{
				return this.ChumAroma.SumFloat((ChumAroma i) => i.Percentage);
			}
		}

		[JsonIgnore]
		public float PercentageParticle
		{
			get
			{
				return this.ChumParticle.SumFloat((ChumParticle i) => i.Percentage);
			}
		}

		[NoClone(true)]
		public override double? Weight { get; set; }

		[NoClone(true)]
		public double? WaterWeight { get; set; }

		[NoClone(true)]
		public TimeSpan MixTime { get; set; }

		[NoClone(true)]
		public override int? ThumbnailBID { get; set; }

		[NoClone(true)]
		public override int? DollThumbnailBID { get; set; }

		[NoClone(true)]
		public bool WasFilled { get; set; }

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ChumIngredient chumIngredient in this.ChumBase.OfType<ChumIngredient>().Union(this.ChumAroma.OfType<ChumIngredient>()).Union(this.ChumParticle.OfType<ChumIngredient>()))
			{
				stringBuilder.AppendFormat("{0} #{1} ({2}) '{3}', Weight: {4}", new object[]
				{
					Enum.GetName(typeof(ItemSubTypes), chumIngredient.ItemSubType),
					chumIngredient.ItemId,
					chumIngredient.InstanceId,
					chumIngredient.Name,
					chumIngredient.Weight
				});
			}
			return stringBuilder.ToString();
		}

		[JsonIgnore]
		public List<FishTypeAttractivity> BaseFishTypeAttractivity { get; set; }

		[JsonIgnore]
		public int BaseViscosity { get; set; }

		[JsonIgnore]
		public EffectiveTemperatureRange BaseEffectiveTemperatureRange { get; set; }

		[JsonIgnore]
		public ChumFragmentation BaseChumFragmentation { get; set; }

		[JsonIgnore]
		public float BaseEffectiveTime { get; set; }

		[JsonIgnore]
		public float BaseDecayTime { get; set; }

		[JsonIgnore]
		public float BaseSpawnTime { get; set; }

		[JsonIgnore]
		public float BaseUsageTime { get; set; }

		[JsonIgnore]
		public List<FishTypeAttractivity> AromatizerFishTypeAttractivity { get; set; }

		[JsonIgnore]
		public List<FishTypeAttractivity> AromatizerFishDenial { get; set; }

		[JsonIgnore]
		public float AromatizerFeederRadius { get; set; }

		[JsonIgnore]
		public float AromatizerManualFeedRadius { get; set; }

		[JsonIgnore]
		public float AromatizerEffectiveTime { get; set; }

		[JsonIgnore]
		public float AromatizerDecayTime { get; set; }

		[JsonIgnore]
		public float AromatizerSpawnTime { get; set; }

		[JsonIgnore]
		public float AromatizerUsageTime { get; set; }

		[JsonIgnore]
		public List<ParticleFishInfluence> BiteInfluence { get; set; }

		[JsonIgnore]
		public float ParticleFishSizeMod { get; set; }

		[JsonIgnore]
		public float ParticleUsageTime { get; set; }

		[JsonIgnore]
		public float LifetimeModifier { get; set; }

		public static ItemSubTypes ChumSubtype(Chum chum)
		{
			ChumBase chumBase = chum.ChumBase.FirstOrDefault<ChumBase>();
			return (chumBase == null) ? ItemSubTypes.All : chumBase.ItemSubType;
		}

		[JsonIgnore]
		public ChumBase HeaviestChumBase
		{
			get
			{
				return this.ChumBase.OrderByDescending((ChumBase b) => b.Weight).FirstOrDefault<ChumBase>();
			}
		}

		public void UpdateIngredients(bool updateWeight = true)
		{
			if (this.Ingredients == null)
			{
				this.Ingredients = new List<ChumIngredient>();
			}
			foreach (ChumBase chumBase in this.ChumBase)
			{
				if (chumBase.BaseFishTypeAttractivity == null)
				{
					chumBase.BaseFishTypeAttractivity = new List<FishTypeAttractivity>();
				}
			}
			foreach (ChumAroma chumAroma in this.ChumAroma)
			{
				if (chumAroma.AromatizerFishTypeAttractivity == null)
				{
					chumAroma.AromatizerFishTypeAttractivity = new List<FishTypeAttractivity>();
				}
				if (chumAroma.AromatizerFishDenial == null)
				{
					chumAroma.AromatizerFishDenial = new List<FishTypeAttractivity>();
				}
			}
			foreach (ChumParticle chumParticle in this.ChumParticle)
			{
				if (chumParticle.BiteInfluence == null)
				{
					chumParticle.BiteInfluence = new List<ParticleFishInfluence>();
				}
			}
			double num = this.Ingredients.SumDouble(delegate(ChumIngredient i)
			{
				double? weight2 = i.Weight;
				return (weight2 == null) ? 0.0 : weight2.Value;
			});
			if (updateWeight)
			{
				ItemSubTypes itemSubTypes = Chum.ChumSubtype(this);
				if (itemSubTypes != ItemSubTypes.ChumGroundbaits && itemSubTypes != ItemSubTypes.ChumMethodMix)
				{
					this.WaterWeight = new double?(0.0);
				}
				else
				{
					this.WaterWeight = new double?(Math.Round(num / 2.0, 2, MidpointRounding.AwayFromZero));
				}
				double? waterWeight = this.WaterWeight;
				this.Weight = ((waterWeight == null) ? null : new double?(num + waterWeight.GetValueOrDefault()));
			}
			foreach (ChumIngredient chumIngredient in this.Ingredients)
			{
				ChumIngredient chumIngredient2 = chumIngredient;
				double num2;
				if (num == 0.0)
				{
					num2 = 0.0;
				}
				else
				{
					double num3 = 100.0;
					double? weight = chumIngredient.Weight;
					num2 = num3 * ((weight == null) ? 0.0 : weight.Value) / num;
				}
				chumIngredient2.Percentage = num2.RoundFloat();
			}
			double baseWeight = this.ChumBase.SumDouble(delegate(ChumBase i)
			{
				double? weight3 = i.Weight;
				return (weight3 == null) ? 0.0 : weight3.Value;
			});
			this.BaseFishTypeAttractivity = (from a in this.ChumBase.SelectMany((ChumBase b) => b.BaseFishTypeAttractivity.Select(delegate(FishTypeAttractivity a)
				{
					double? weight4 = b.Weight;
					return new
					{
						Weight = ((weight4 == null) ? 0.0 : weight4.Value),
						Attractivity = a
					};
				}))
				where a.Weight > 0.0
				group a by new
				{
					a.Attractivity.FishId,
					a.Attractivity.FishCode
				}).Select(delegate(g)
			{
				double baseWeightPerFishType = g.ToList().SumDouble(a => a.Weight);
				return new FishTypeAttractivity
				{
					FishId = g.Key.FishId,
					FishCode = g.Key.FishCode,
					Attraction = g.ToList().SumDouble(a => (double)a.Attractivity.Attraction * a.Weight / baseWeightPerFishType).RoundFloat()
				};
			}).ToList<FishTypeAttractivity>();
			this.BaseViscosity = this.ChumBase.SumInt(delegate(ChumBase b)
			{
				double num4 = (double)b.BaseViscosity;
				double? weight5 = b.Weight;
				return (int)(num4 * ((weight5 == null) ? 0.0 : weight5.Value) / baseWeight);
			});
			this.BaseEffectiveTemperatureRange = new EffectiveTemperatureRange
			{
				T1 = this.ChumBase.SumDouble(delegate(ChumBase b)
				{
					double num5 = (double)b.BaseEffectiveTemperatureRange.T1;
					double? weight6 = b.Weight;
					return num5 * ((weight6 == null) ? 0.0 : weight6.Value) / baseWeight;
				}).RoundFloat(),
				T2 = this.ChumBase.SumDouble(delegate(ChumBase b)
				{
					double num6 = (double)b.BaseEffectiveTemperatureRange.T2;
					double? weight7 = b.Weight;
					return num6 * ((weight7 == null) ? 0.0 : weight7.Value) / baseWeight;
				}).RoundFloat(),
				T3 = this.ChumBase.SumDouble(delegate(ChumBase b)
				{
					double num7 = (double)b.BaseEffectiveTemperatureRange.T3;
					double? weight8 = b.Weight;
					return num7 * ((weight8 == null) ? 0.0 : weight8.Value) / baseWeight;
				}).RoundFloat(),
				T4 = this.ChumBase.SumDouble(delegate(ChumBase b)
				{
					double num8 = (double)b.BaseEffectiveTemperatureRange.T4;
					double? weight9 = b.Weight;
					return num8 * ((weight9 == null) ? 0.0 : weight9.Value) / baseWeight;
				}).RoundFloat()
			};
			this.BaseChumFragmentation = new ChumFragmentation
			{
				SpawnInitPortion = this.ChumBase.SumDouble(delegate(ChumBase b)
				{
					double num9 = (double)b.BaseChumFragmentation.SpawnInitPortion;
					double? weight10 = b.Weight;
					return num9 * ((weight10 == null) ? 0.0 : weight10.Value) / baseWeight;
				}).RoundFloat(),
				SpawnInterval = this.ChumBase.SumDouble(delegate(ChumBase b)
				{
					double num10 = (double)b.BaseChumFragmentation.SpawnInterval;
					double? weight11 = b.Weight;
					return num10 * ((weight11 == null) ? 0.0 : weight11.Value) / baseWeight;
				}).RoundFloat(),
				SpawnRepeats = this.ChumBase.SumDouble(delegate(ChumBase b)
				{
					double num11 = (double)b.BaseChumFragmentation.SpawnRepeats;
					double? weight12 = b.Weight;
					return num11 * ((weight12 == null) ? 0.0 : weight12.Value) / baseWeight;
				}).RoundFloat()
			};
			this.BaseEffectiveTime = this.ChumBase.SumDouble(delegate(ChumBase b)
			{
				double num12 = (double)b.BaseEffectiveTime;
				double? weight13 = b.Weight;
				return num12 * ((weight13 == null) ? 0.0 : weight13.Value) / baseWeight;
			}).RoundFloat();
			this.BaseDecayTime = this.ChumBase.SumDouble(delegate(ChumBase b)
			{
				double num13 = (double)b.BaseDecayTime;
				double? weight14 = b.Weight;
				return num13 * ((weight14 == null) ? 0.0 : weight14.Value) / baseWeight;
			}).RoundFloat();
			this.BaseSpawnTime = this.ChumBase.SumDouble(delegate(ChumBase b)
			{
				double num14 = (double)b.BaseSpawnTime;
				double? weight15 = b.Weight;
				return num14 * ((weight15 == null) ? 0.0 : weight15.Value) / baseWeight;
			}).RoundFloat();
			this.BaseUsageTime = this.ChumBase.SumDouble(delegate(ChumBase b)
			{
				double num15 = (double)b.BaseUsageTime;
				double? weight16 = b.Weight;
				return num15 * ((weight16 == null) ? 0.0 : weight16.Value) / baseWeight;
			}).RoundFloat();
			double aromaWeight = this.ChumAroma.SumDouble(delegate(ChumAroma i)
			{
				double? weight17 = i.Weight;
				return (weight17 == null) ? 0.0 : weight17.Value;
			});
			this.AromatizerFishTypeAttractivity = (from a in this.ChumAroma.SelectMany((ChumAroma b) => b.AromatizerFishTypeAttractivity.Select(delegate(FishTypeAttractivity a)
				{
					double? weight18 = b.Weight;
					return new
					{
						Weight = ((weight18 == null) ? 0.0 : weight18.Value),
						Attractivity = a
					};
				}))
				where a.Weight > 0.0
				group a by new
				{
					a.Attractivity.FishId,
					a.Attractivity.FishCode
				}).Select(delegate(g)
			{
				double baseWeightPerFishType = g.ToList().SumDouble(a => a.Weight);
				return new FishTypeAttractivity
				{
					FishId = g.Key.FishId,
					FishCode = g.Key.FishCode,
					Attraction = g.ToList().SumDouble(a => (double)a.Attractivity.Attraction * a.Weight / baseWeightPerFishType).RoundFloat()
				};
			}).ToList<FishTypeAttractivity>();
			this.AromatizerFishDenial = (from a in this.ChumAroma.SelectMany((ChumAroma b) => b.AromatizerFishDenial.Select(delegate(FishTypeAttractivity a)
				{
					double? weight19 = b.Weight;
					return new
					{
						Weight = ((weight19 == null) ? 0.0 : weight19.Value),
						Attractivity = a
					};
				}))
				where a.Weight > 0.0
				group a by new
				{
					a.Attractivity.FishId,
					a.Attractivity.FishCode
				}).Select(delegate(g)
			{
				double baseWeightPerFishType = g.ToList().SumDouble(a => a.Weight);
				return new FishTypeAttractivity
				{
					FishId = g.Key.FishId,
					FishCode = g.Key.FishCode,
					Attraction = g.ToList().SumDouble(a => (double)a.Attractivity.Attraction * a.Weight / baseWeightPerFishType).RoundFloat()
				};
			}).ToList<FishTypeAttractivity>();
			this.AromatizerFeederRadius = this.ChumAroma.SumDouble(delegate(ChumAroma b)
			{
				double num16 = (double)b.AromatizerFeederRadius;
				double? weight20 = b.Weight;
				return num16 * ((weight20 == null) ? 0.0 : weight20.Value) / aromaWeight;
			}).RoundFloat();
			this.AromatizerEffectiveTime = this.ChumAroma.SumDouble(delegate(ChumAroma b)
			{
				double num17 = (double)b.AromatizerEffectiveTime;
				double? weight21 = b.Weight;
				return num17 * ((weight21 == null) ? 0.0 : weight21.Value) / aromaWeight;
			}).RoundFloat();
			this.AromatizerDecayTime = this.ChumAroma.SumDouble(delegate(ChumAroma b)
			{
				double num18 = (double)b.AromatizerDecayTime;
				double? weight22 = b.Weight;
				return num18 * ((weight22 == null) ? 0.0 : weight22.Value) / aromaWeight;
			}).RoundFloat();
			this.AromatizerSpawnTime = this.ChumAroma.SumDouble(delegate(ChumAroma b)
			{
				double num19 = (double)b.AromatizerSpawnTime;
				double? weight23 = b.Weight;
				return num19 * ((weight23 == null) ? 0.0 : weight23.Value) / aromaWeight;
			}).RoundFloat();
			this.AromatizerUsageTime = this.ChumAroma.SumDouble(delegate(ChumAroma b)
			{
				double num20 = (double)b.AromatizerUsageTime;
				double? weight24 = b.Weight;
				return num20 * ((weight24 == null) ? 0.0 : weight24.Value) / aromaWeight;
			}).RoundFloat();
			double particleWeight = this.ChumParticle.SumDouble(delegate(ChumParticle i)
			{
				double? weight25 = i.Weight;
				return (weight25 == null) ? 0.0 : weight25.Value;
			});
			this.BiteInfluence = (from a in this.ChumParticle.SelectMany((ChumParticle b) => b.BiteInfluence.Select(delegate(ParticleFishInfluence a)
				{
					double? weight26 = b.Weight;
					return new
					{
						Weight = ((weight26 == null) ? 0.0 : weight26.Value),
						Influence = a
					};
				}))
				where a.Weight > 0.0
				group a by new
				{
					a.Influence.FishId,
					a.Influence.FishCode
				}).Select(delegate(g)
			{
				double baseWeightPerFishType = g.ToList().SumDouble(a => a.Weight);
				return new ParticleFishInfluence
				{
					FishId = g.Key.FishId,
					FishCode = g.Key.FishCode,
					WeightMod = g.ToList().SumDouble(a => (double)a.Influence.WeightMod * a.Weight / baseWeightPerFishType).RoundFloat()
				};
			}).ToList<ParticleFishInfluence>();
			this.ParticleFishSizeMod = (float)this.ChumParticle.SumInt(delegate(ChumParticle b)
			{
				double num21 = (double)b.ParticleFishSizeMod;
				double? weight27 = b.Weight;
				return (int)(num21 * ((weight27 == null) ? 0.0 : weight27.Value) / particleWeight);
			});
			this.ParticleUsageTime = this.ChumParticle.SumDouble(delegate(ChumParticle b)
			{
				double num22 = (double)b.ParticleUsageTime;
				double? weight28 = b.Weight;
				return num22 * ((weight28 == null) ? 0.0 : weight28.Value) / particleWeight;
			}).RoundFloat();
			this.LifetimeModifier = this.ChumParticle.SumDouble(delegate(ChumParticle b)
			{
				double num23 = (double)b.LifetimeModifier;
				double? weight29 = b.Weight;
				return num23 * ((weight29 == null) ? 0.0 : weight29.Value) / particleWeight;
			}).RoundFloat();
		}

		[JsonIgnore]
		public TimeSpan UsedTime { get; private set; }

		[JsonIgnore]
		public float BaseUsageTimeRemain
		{
			get
			{
				return this.BaseUsageTime - (float)this.UsedTime.TotalHours;
			}
		}

		[JsonIgnore]
		public TimeSpan RemainTime
		{
			get
			{
				return TimeSpan.FromHours((double)this.BaseUsageTimeRemain);
			}
		}

		[JsonIgnore]
		public float BaseUsageTimePercentage
		{
			get
			{
				if (this.BaseUsageTime == 0f)
				{
					return 100f;
				}
				return (this.UsedTime.TotalSeconds / (double)this.BaseUsageTime * 100.0).RoundFloat();
			}
		}

		[JsonIgnore]
		public float BaseUsageTimePercentageRemain
		{
			get
			{
				if (this.BaseUsageTime == 0f)
				{
					return 0f;
				}
				return ((double)this.BaseUsageTimeRemain / (double)this.BaseUsageTime * 100.0).RoundFloat();
			}
		}

		[JsonIgnore]
		public bool IsExpired
		{
			get
			{
				return !new int[] { 3200, 3970, 3980 }.Contains(base.ItemId) && this.BaseUsageTimeRemain <= 0f;
			}
		}

		public void SetPondTimeSpent(TimeSpan pondTimeSpent)
		{
			if (this.BaseUsageTime == 0f && this.Ingredients != null && this.Ingredients.Any<ChumIngredient>())
			{
				this.UpdateIngredients(false);
			}
			this.UsedTime = pondTimeSpent.Subtract(this.MixTime);
		}

		[JsonIgnore]
		public override bool IsOccupyInventorySpace
		{
			get
			{
				return false;
			}
		}

		[JsonIgnore]
		public override bool IsStockableByAmount
		{
			get
			{
				return true;
			}
		}

		[JsonIgnore]
		[NoClone(true)]
		public override float Amount
		{
			get
			{
				double? weight = this.Weight;
				return (float)((weight == null) ? 0.0 : weight.Value);
			}
			set
			{
				this.Weight = new double?((double)value);
			}
		}

		public override void SplitTo(InventoryItem newItem, float amount)
		{
			double? weight = this.Weight;
			if (((weight == null) ? 0.0 : weight.Value) < (double)amount)
			{
				double? weight2 = this.Weight;
				amount = (float)((weight2 == null) ? 0.0 : weight2.Value);
			}
			double? weight3 = this.Weight;
			this.Weight = new double?(((weight3 == null) ? 0.0 : weight3.Value) - (double)amount);
			Chum chum = (Chum)newItem;
			chum.Weight = new double?((double)amount);
			chum.Count = 1;
			chum.Ingredients = this.Ingredients;
			chum.Name = this.Name;
			chum.ImageData = this.ImageData;
			chum.MixTime = this.MixTime;
			chum.ThumbnailBID = this.ThumbnailBID;
			chum.DollThumbnailBID = this.DollThumbnailBID;
			if (this.SplitFromInstanceId == null)
			{
				this.SplitFromInstanceId = base.InstanceId;
			}
			chum.SplitFromInstanceId = this.SplitFromInstanceId;
		}

		public override bool CanCombineWith(InventoryItem targetItem)
		{
			Chum chum = targetItem as Chum;
			if (chum == null)
			{
				return false;
			}
			if (this.SplitFromInstanceId != null)
			{
				Guid? splitFromInstanceId = this.SplitFromInstanceId;
				bool flag = splitFromInstanceId != null;
				Guid? splitFromInstanceId2 = chum.SplitFromInstanceId;
				if (flag == (splitFromInstanceId2 != null) && (splitFromInstanceId == null || splitFromInstanceId.GetValueOrDefault() == splitFromInstanceId2.GetValueOrDefault()))
				{
					return chum.IsHidden != true;
				}
			}
			return false;
		}

		public override bool CombineWith(InventoryItem item, float amount)
		{
			Chum chum = item as Chum;
			if (chum == null)
			{
				throw new InvalidOperationException("Can not combine Chum with " + item.GetType().Name);
			}
			double? weight = this.Weight;
			this.Weight = ((weight == null) ? null : new double?(weight.GetValueOrDefault() + (double)amount));
			if ((double)Math.Abs(chum.Amount - amount) <= 1E-05)
			{
				return true;
			}
			Chum chum2 = chum;
			double? weight2 = chum2.Weight;
			chum2.Weight = ((weight2 == null) ? null : new double?(weight2.GetValueOrDefault() - (double)amount));
			return false;
		}

		public override bool IsBroken()
		{
			return this.WasFilled && base.IsHidden != true;
		}

		public override void SetProperty(string propertyName, string propertyValue)
		{
			if (propertyName == "WasFilled")
			{
				this.WasFilled = bool.Parse(propertyValue);
			}
			else
			{
				base.SetProperty(propertyName, propertyValue);
			}
		}

		[NoClone(true)]
		public bool WasThrown { get; set; }

		[NoClone(true)]
		public bool CancelRequested { get; set; }

		[NoClone(true)]
		public bool BeginFillRequested { get; set; }

		[NoClone(true)]
		public bool FinishFillRequested { get; set; }

		[NoClone(true)]
		public Guid? ThrownChumInstanceId { get; set; }

		[JsonIgnore]
		public bool HasWeight
		{
			get
			{
				return this.Weight != null && this.Weight > 1E-05;
			}
		}
	}
}
