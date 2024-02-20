using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class Rod : InventoryItem, IRod
	{
		[JsonConfig]
		public float MaxLoad { get; set; }

		[JsonIgnore]
		public float LineWeight
		{
			get
			{
				return this.MaxLoad;
			}
		}

		[JsonConfig]
		public float CurveTest { get; set; }

		[JsonConfig]
		public float Action { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public RodAction ExtendedAction { get; set; }

		[JsonConfig]
		public float Progressive { get; set; }

		[JsonConfig]
		public float MaxCastLength { get; set; }

		[JsonConfig]
		public float CastWeightMin { get; set; }

		[JsonConfig]
		public float CastWeightMax { get; set; }

		[JsonIgnore]
		[NoClone(true)]
		public bool IsCasted { get; set; }

		[JsonConfig]
		[NoClone(true)]
		public float LeaderLength { get; set; }

		[JsonIgnore]
		public override AllowedEquipment[] AllowedEquipment
		{
			get
			{
				return new AllowedEquipment[]
				{
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Reel),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Line),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Bait),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Bell),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Bobber),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Feeder),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Hook),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.JigHead),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.JigBait),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Lure),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Sinker),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Chum),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Leader),
						MaxItems = 1
					}
				};
			}
		}

		[JsonConfig]
		public AllowedEquipment[] AllowedReels { get; set; }

		[NoClone(true)]
		public override int Slot { get; set; }

		public override int GetMaxAggregatesCount(InventoryItem item, Inventory inventory)
		{
			if (!(item is Reel) || this.AllowedReels == null)
			{
				return base.GetMaxAggregatesCount(item, inventory);
			}
			foreach (AllowedEquipment allowedEquipment in this.AllowedReels)
			{
				if (allowedEquipment.ItemSubType == item.ItemSubType)
				{
					return allowedEquipment.MaxItems;
				}
			}
			return 0;
		}

		public override bool CanAggregate(InventoryItem newChild, List<InventoryItem> children)
		{
			if (newChild.ItemType == ItemTypes.Reel || newChild.ItemType == ItemTypes.Line)
			{
				return true;
			}
			if (base.ItemSubType == ItemSubTypes.CastingRod && newChild.ItemType == ItemTypes.Reel && newChild.ItemSubType != ItemSubTypes.CastReel)
			{
				return false;
			}
			if (base.ItemSubType == ItemSubTypes.CastingRod || base.ItemSubType == ItemSubTypes.SpinningRod)
			{
				if (newChild.ItemType != ItemTypes.Lure && newChild.ItemType != ItemTypes.JigBait && newChild.ItemType != ItemTypes.JigHead && newChild.ItemType != ItemTypes.Sinker && newChild.ItemType != ItemTypes.Hook && newChild.ItemType != ItemTypes.Leader)
				{
					return false;
				}
				if (newChild.ItemSubType == ItemSubTypes.Shad || newChild.ItemSubType == ItemSubTypes.Worm || newChild.ItemSubType == ItemSubTypes.Craw || newChild.ItemSubType == ItemSubTypes.Tube || newChild.ItemSubType == ItemSubTypes.Grub || newChild.ItemSubType == ItemSubTypes.Slug)
				{
					if (children.Any((InventoryItem i) => i.ItemType == ItemTypes.JigHead))
					{
						return true;
					}
					if (children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.BassJig))
					{
						return true;
					}
					if (children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.Spinnerbait))
					{
						return true;
					}
					if (children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.BuzzBait))
					{
						return true;
					}
					return children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.OffsetHook);
				}
				else if (newChild.ItemSubType == ItemSubTypes.Tail)
				{
					if (children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.Spinner))
					{
						return true;
					}
					if (children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.BarblessSpinners))
					{
						return true;
					}
					if (children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.Spinnerbait))
					{
						return true;
					}
					return children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.BuzzBait);
				}
				else
				{
					if (newChild.ItemType == ItemTypes.Leader)
					{
						if (newChild.ItemSubType != ItemSubTypes.CarolinaRig && newChild.ItemSubType != ItemSubTypes.TexasRig && newChild.ItemSubType != ItemSubTypes.ThreewayRig && newChild.ItemSubType != ItemSubTypes.FlurLeader && newChild.ItemSubType != ItemSubTypes.TitaniumLeader)
						{
							return false;
						}
						if (newChild.ItemSubType == ItemSubTypes.CarolinaRig || newChild.ItemSubType == ItemSubTypes.TexasRig || newChild.ItemSubType == ItemSubTypes.ThreewayRig)
						{
							if (children.Any((InventoryItem i) => i.ItemType == ItemTypes.Lure || i.ItemType == ItemTypes.JigHead))
							{
								return false;
							}
						}
					}
					if (newChild.ItemType == ItemTypes.Lure || newChild.ItemType == ItemTypes.JigHead)
					{
						if (children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.CarolinaRig || i.ItemSubType == ItemSubTypes.TexasRig || i.ItemSubType == ItemSubTypes.ThreewayRig))
						{
							return false;
						}
					}
					if (newChild.ItemType == ItemTypes.Sinker)
					{
						if (newChild.ItemSubType != ItemSubTypes.SpinningSinker && newChild.ItemSubType != ItemSubTypes.DropSinker)
						{
							return false;
						}
						if (newChild.ItemSubType == ItemSubTypes.SpinningSinker)
						{
							if (children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.CarolinaRig || i.ItemSubType == ItemSubTypes.TexasRig))
							{
								return true;
							}
						}
						if (newChild.ItemSubType == ItemSubTypes.DropSinker)
						{
							if (children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.ThreewayRig))
							{
								return true;
							}
						}
						return false;
					}
					else if (newChild.ItemType == ItemTypes.Hook && newChild.ItemSubType != ItemSubTypes.OffsetHook)
					{
						return false;
					}
				}
			}
			if (base.ItemSubType == ItemSubTypes.MatchRod || base.ItemSubType == ItemSubTypes.TelescopicRod)
			{
				if (newChild.ItemType != ItemTypes.Bait && newChild.ItemType != ItemTypes.Bobber && newChild.ItemType != ItemTypes.Hook && newChild.ItemType != ItemTypes.Leader)
				{
					return false;
				}
				if (newChild.ItemType == ItemTypes.Leader && newChild.ItemSubType != ItemSubTypes.FlurLeader && newChild.ItemSubType != ItemSubTypes.TitaniumLeader)
				{
					return false;
				}
			}
			if (base.ItemSubType == ItemSubTypes.FeederRod || base.ItemSubType == ItemSubTypes.BottomRod)
			{
				if (newChild.ItemType != ItemTypes.Feeder && newChild.ItemType != ItemTypes.Sinker && newChild.ItemType != ItemTypes.Leader && newChild.ItemType != ItemTypes.Hook && newChild.ItemType != ItemTypes.Bell && newChild.ItemType != ItemTypes.Bait && newChild.ItemType != ItemTypes.Chum)
				{
					return false;
				}
				if (newChild.ItemType == ItemTypes.Feeder && newChild.ItemSubType != ItemSubTypes.CageFeeder)
				{
					return false;
				}
				if (newChild.ItemType == ItemTypes.Chum)
				{
					if (children.Any((InventoryItem i) => i.ItemType == ItemTypes.Sinker))
					{
						return false;
					}
				}
				if (newChild.ItemType == ItemTypes.Chum)
				{
					if (!children.Any((InventoryItem i) => i.ItemType == ItemTypes.Feeder))
					{
						return false;
					}
				}
				if (newChild.ItemType == ItemTypes.Chum && Chum.ChumSubtype((Chum)newChild) != ItemSubTypes.ChumGroundbaits)
				{
					return false;
				}
				if (newChild.ItemType == ItemTypes.Leader && newChild.ItemSubType != ItemSubTypes.MonoLeader && newChild.ItemSubType != ItemSubTypes.FlurLeader && newChild.ItemSubType != ItemSubTypes.TitaniumLeader)
				{
					return false;
				}
			}
			if (base.ItemSubType == ItemSubTypes.CarpRod)
			{
				if (newChild.ItemType != ItemTypes.Feeder && newChild.ItemType != ItemTypes.Sinker && newChild.ItemType != ItemTypes.Leader && newChild.ItemType != ItemTypes.Hook && newChild.ItemType != ItemTypes.Bait && newChild.ItemType != ItemTypes.Chum)
				{
					return false;
				}
				if (newChild.ItemType == ItemTypes.Feeder && newChild.ItemSubType != ItemSubTypes.FlatFeeder && newChild.ItemSubType != ItemSubTypes.PvaFeeder)
				{
					return false;
				}
				if (newChild.ItemType == ItemTypes.Bait && !Inventory.IsCarpBait(newChild))
				{
					return false;
				}
				if (newChild.ItemType == ItemTypes.Chum)
				{
					if (!children.Any((InventoryItem i) => i.ItemType == ItemTypes.Feeder))
					{
						return false;
					}
				}
				if (newChild.ItemType == ItemTypes.Chum)
				{
					if (children.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.FlatFeeder) && Chum.ChumSubtype((Chum)newChild) != ItemSubTypes.ChumMethodMix)
					{
						return false;
					}
				}
				if (newChild.ItemType == ItemTypes.Leader && newChild.ItemSubType != ItemSubTypes.CarpLeader)
				{
					return false;
				}
			}
			if (base.ItemSubType == ItemSubTypes.SpodRod)
			{
				if (newChild.ItemType != ItemTypes.Feeder && newChild.ItemType != ItemTypes.Chum)
				{
					return false;
				}
				if (newChild.ItemType == ItemTypes.Feeder && newChild.ItemSubType != ItemSubTypes.SpodFeeder)
				{
					return false;
				}
			}
			return true;
		}

		[JsonIgnore]
		protected override bool IsSellable
		{
			get
			{
				return base.Rent == null;
			}
		}

		[JsonIgnore]
		public bool IsRodOnDoll
		{
			get
			{
				return base.Storage == StoragePlaces.Hands || base.Storage == StoragePlaces.Doll || base.Storage == StoragePlaces.Shore;
			}
		}
	}
}
