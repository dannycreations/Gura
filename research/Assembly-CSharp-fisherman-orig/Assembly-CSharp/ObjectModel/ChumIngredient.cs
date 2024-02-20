using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(0)]
	public class ChumIngredient : InventoryItem
	{
		[JsonIgnore]
		public float Percentage { get; set; }

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

		[JsonConfig]
		[NoClone(true)]
		public override double? Weight { get; set; }

		[JsonConfig]
		public double? PackWeight { get; set; }

		public override void Init()
		{
			this.PackWeight = this.Weight;
		}

		public override bool CombineWith(InventoryItem item, float amount)
		{
			if (item.GetType() != base.GetType())
			{
				throw new InvalidOperationException(string.Format("Can not combine {0} with {1}", base.GetType(), item.GetType().Name));
			}
			ChumIngredient chumIngredient = item as ChumIngredient;
			double? weight = this.Weight;
			this.Weight = ((weight == null) ? null : new double?(weight.GetValueOrDefault() + (double)amount));
			if ((double)Math.Abs(chumIngredient.Amount - amount) <= 1E-05)
			{
				return true;
			}
			ChumIngredient chumIngredient2 = chumIngredient;
			double? weight2 = chumIngredient2.Weight;
			chumIngredient2.Weight = ((weight2 == null) ? null : new double?(weight2.GetValueOrDefault() - (double)amount));
			return false;
		}
	}
}
