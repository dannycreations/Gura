using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ObjectModel.Common;

namespace ObjectModel
{
	[JsonObject(0)]
	public class InventoryItem : CharItem, IEqualityComparer<InventoryItem>
	{
		[NoClone(true)]
		public int ItemId { get; set; }

		[NoClone(true)]
		public Guid? InstanceId { get; set; }

		public virtual string Name { get; set; }

		public string Desc { get; set; }

		public string Params { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		[NoClone(true)]
		public StoragePlaces Storage { get; set; }

		[NoClone(true)]
		public bool? IsHidden { get; set; }

		public int RaretyId { get; set; }

		[JsonIgnore]
		[NoClone(true)]
		public InventoryItem ParentItem
		{
			get
			{
				return this.parentItem;
			}
			set
			{
				this.parentItem = value;
				this.parentItemId = ((this.parentItem != null) ? this.parentItem.InstanceId : null);
			}
		}

		[NoClone(true)]
		public Guid? ParentItemInstanceId
		{
			get
			{
				if (this.ParentItem != null)
				{
					return this.ParentItem.InstanceId;
				}
				return this.parentItemId;
			}
			set
			{
				this.parentItemId = value;
			}
		}

		[JsonConverter(typeof(StringEnumConverter))]
		public ItemTypes ItemType { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public ItemSubTypes ItemSubType { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public InventorySpecialItem SpecialItem { get; set; }

		[JsonIgnore]
		public virtual AllowedEquipment[] AllowedEquipment
		{
			get
			{
				return null;
			}
		}

		public virtual int Slot { get; set; }

		[NoClone(true)]
		public RentProps Rent { get; set; }

		public NestedItem[] NestedItems { get; set; }

		public int OriginalMinLevel { get; set; }

		public int MinLevel { get; set; }

		[NoClone(true)]
		public DateTime DateReceived { get; set; }

		[NoClone(true)]
		public virtual int Count { get; set; }

		public string Asset { get; set; }

		public virtual int? ThumbnailBID { get; set; }

		public virtual int? DollThumbnailBID { get; set; }

		[JsonConfig]
		public virtual double? Weight { get; set; }

		[JsonConfig]
		public virtual double? Length { get; set; }

		public bool IsBoP { get; set; }

		public bool IsUnwearable { get; set; }

		public bool IsRepairable { get; set; }

		public virtual bool IsUnstockable { get; set; }

		public virtual bool IsGiftable { get; set; }

		public virtual bool IsOccupyInventorySpace
		{
			get
			{
				return this.IsHidden != true;
			}
		}

		public bool IsUpdatable { get; set; }

		public int? MaxDurability { get; set; }

		[JsonIgnore]
		public virtual bool IsConsumable
		{
			get
			{
				return false;
			}
		}

		[NoClone(true)]
		public int Durability
		{
			get
			{
				return this.durability;
			}
			set
			{
				this.durability = value;
				if (this.durability < this.MinDurability)
				{
					this.MinDurability = this.durability;
				}
			}
		}

		public DateTime? GlobalStart { get; set; }

		public DateTime? GlobalEnd { get; set; }

		public DateTime? LocalStart { get; set; }

		public DateTime? LocalEnd { get; set; }

		[NoClone(true)]
		public int MinDurability { get; set; }

		public Brand Brand { get; set; }

		[JsonIgnore]
		public string BrandName
		{
			get
			{
				return this.Brand.Name;
			}
		}

		public Technology[] Technologies { get; set; }

		public double? PriceSilver { get; set; }

		public double? PriceGold { get; set; }

		public double? BageSilver { get; set; }

		public double? BageGold { get; set; }

		public DateTime? LastUpdated { get; set; }

		public double? DiscountPrice { get; set; }

		[JsonIgnore]
		public bool IsDiscount
		{
			get
			{
				return this.DiscountPrice != null && this.DiscountPrice > 0.0;
			}
		}

		public bool IsNew { get; set; }

		public virtual int GetMaxAggregatesCount(InventoryItem item, Inventory inventory)
		{
			if (this.AllowedEquipment == null)
			{
				return 0;
			}
			foreach (AllowedEquipment allowedEquipment2 in this.AllowedEquipment)
			{
				if (allowedEquipment2.ItemType == item.ItemType || allowedEquipment2.ItemSubType == item.ItemSubType)
				{
					return allowedEquipment2.MaxItems;
				}
			}
			return 0;
		}

		public virtual void SetProperty(string propertyName, string propertyValue)
		{
			if (propertyName == "Durability")
			{
				this.Durability = int.Parse(propertyValue, CultureInfo.InvariantCulture);
			}
			else if (propertyName == "Length")
			{
				this.Length = new double?(double.Parse(propertyValue, CultureInfo.InvariantCulture));
			}
			else if (propertyName == "Count")
			{
				this.Count = int.Parse(propertyValue, CultureInfo.InvariantCulture);
			}
			else if (propertyName == "Weight")
			{
				this.Weight = ((!string.IsNullOrEmpty(propertyValue)) ? new double?(double.Parse(propertyValue, CultureInfo.InvariantCulture)) : null);
			}
			else if (propertyName == "IsHidden")
			{
				this.IsHidden = ((!string.IsNullOrEmpty(propertyValue)) ? new bool?(bool.Parse(propertyValue)) : null);
			}
		}

		public virtual bool CanAggregate(InventoryItem newChild, List<InventoryItem> children)
		{
			return false;
		}

		public virtual bool IsBroken()
		{
			return false;
		}

		public override string ToString()
		{
			return string.Format("{0} #{1} ({2}) '{3}'{5}, Durability: {4}", new object[]
			{
				Enum.GetName(typeof(ItemSubTypes), this.ItemSubType),
				this.ItemId,
				this.InstanceId,
				this.Name,
				this.Durability,
				(!(this is Rod) || this.Slot <= 0) ? string.Empty : string.Format(" (slot {0})", this.Slot)
			});
		}

		public bool Equals(InventoryItem x, InventoryItem y)
		{
			return x.GetHashCode(x) == y.GetHashCode(y);
		}

		public int GetHashCode(InventoryItem obj)
		{
			return this.InstanceId.GetHashCode();
		}

		public virtual void Init()
		{
			this.GlobalStart = null;
			this.GlobalEnd = null;
			this.LocalStart = null;
			this.LocalEnd = null;
		}

		[JsonIgnore]
		public bool CanSell
		{
			get
			{
				return this.IsSellable && this.SellPrice != null;
			}
		}

		[JsonIgnore]
		protected virtual bool IsSellable
		{
			get
			{
				return false;
			}
		}

		[JsonIgnore]
		public virtual Amount SellPrice
		{
			get
			{
				if (!this.IsSellable)
				{
					return null;
				}
				double num = 0.0;
				string text = "SC";
				if (this.MaxDurability == null)
				{
					return null;
				}
				if ((double)this.Durability / (double)this.MaxDurability.Value < 0.05)
				{
					return null;
				}
				if (this.PriceGold != null)
				{
					text = "GC";
					num = this.PriceGold.Value * (double)this.Durability / (double)this.MaxDurability.Value * 0.2;
				}
				else if (this.PriceSilver != null)
				{
					text = "SC";
					num = this.PriceSilver.Value * (double)this.Durability / (double)this.MaxDurability.Value * 0.2;
				}
				if (num < 0.4)
				{
					return null;
				}
				if (num < 1.0)
				{
					return new Amount
					{
						Currency = text,
						Value = 1
					};
				}
				return new Amount
				{
					Currency = text,
					Value = (int)Math.Round(num, MidpointRounding.AwayFromZero)
				};
			}
		}

		[JsonIgnore]
		public virtual bool IsStockableByAmount
		{
			get
			{
				return false;
			}
		}

		[JsonIgnore]
		[NoClone(true)]
		public virtual float Amount
		{
			get
			{
				return (float)this.Count;
			}
			set
			{
				this.Count = (int)value;
			}
		}

		public virtual bool CanCombineWith(InventoryItem targetItem)
		{
			return !this.IsUnstockable && this.ItemId == targetItem.ItemId && base.GetType() == targetItem.GetType();
		}

		public virtual bool CombineWith(InventoryItem item, int count)
		{
			this.Count += count;
			if (item.Count == count)
			{
				return true;
			}
			item.Count -= count;
			return false;
		}

		public virtual bool CombineWith(InventoryItem item, float amount)
		{
			int num = (int)amount;
			return this.CombineWith(item, num);
		}

		public virtual void SplitTo(InventoryItem newItem, int count)
		{
			newItem.Count = count;
			this.Count -= count;
		}

		public virtual void SplitTo(InventoryItem newItem, float amount)
		{
			int num = (int)amount;
			this.SplitTo(newItem, num);
		}

		public virtual void ApplyWear(int damage)
		{
			this.Durability -= damage;
			if (this.Durability < 0)
			{
				this.Durability = 0;
			}
		}

		public virtual void RepairItem(int damageRestored)
		{
			this.Durability += damageRestored;
			if (this.MaxDurability != null)
			{
				int? maxDurability = this.MaxDurability;
				if (this.Durability > maxDurability)
				{
					this.Durability = this.MaxDurability.Value;
				}
			}
		}

		public virtual void MakeCloneOfItem(InventoryItem source, bool fullClone = false)
		{
			this.ItemId = source.ItemId;
			this.MakeCloneOf(source, fullClone);
		}

		public bool IsAvailableGlobally(DateTime now)
		{
			return (this.GlobalStart == null || now >= this.GlobalStart.Value) && (this.GlobalEnd == null || now <= this.GlobalEnd.Value);
		}

		public bool IsAvailableLocally(DateTime now)
		{
			return (this.LocalStart == null || now >= this.LocalStart.Value) && (this.LocalEnd == null || now <= this.LocalEnd.Value);
		}

		[JsonIgnore]
		private Guid? parentItemId;

		[JsonIgnore]
		private InventoryItem parentItem;

		[JsonIgnore]
		private int durability;

		protected const double SellPriceRatio = 0.2;

		protected const double MinDurbilityRatio = 0.05;

		protected const double MinSellPrice = 0.4;
	}
}
