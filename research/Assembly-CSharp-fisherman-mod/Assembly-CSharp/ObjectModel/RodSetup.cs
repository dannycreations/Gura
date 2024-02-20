using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public class RodSetup
	{
		public RodSetup()
		{
			this.ItemIDs = new int[0];
		}

		[JsonIgnore]
		public Profile Profile
		{
			set
			{
				this.profile = value;
			}
		}

		[JsonProperty]
		public Guid InstanceId { get; set; }

		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public int[] ItemIDs { get; set; }

		[JsonProperty]
		public double LineLength { get; set; }

		[JsonProperty]
		public DateTime? LastActivityTime { get; set; }

		[JsonProperty]
		public List<InventoryItem> Items
		{
			get
			{
				if (this.profile == null)
				{
					return this._Items;
				}
				if (this.profile.SerializationIsInProgress)
				{
					return null;
				}
				return this._Items;
			}
			set
			{
				if (value == null)
				{
					this._Items = new List<InventoryItem>();
					this.ItemIDs = new int[0];
				}
				else
				{
					this._Items = value.Select(new Func<InventoryItem, InventoryItem>(Inventory.CloneItem)).ToList<InventoryItem>();
					this.ItemIDs = this._Items.Select((InventoryItem i) => i.ItemId).ToArray<int>();
				}
			}
		}

		[JsonIgnore]
		public IEnumerable<InventoryItem> ItemsToEquip
		{
			get
			{
				if (this.profile == null)
				{
					return this.Items;
				}
				if (this.profile.Inventory.IsStorageAvailable())
				{
					return this.Items;
				}
				return this.Items.Where((InventoryItem i) => !(i is Rod));
			}
		}

		[JsonIgnore]
		public bool IsEmpty
		{
			get
			{
				return this.ItemIDs == null || this.ItemIDs.Length == 0;
			}
		}

		[JsonIgnore]
		public bool IsPartialRodTemplate
		{
			get
			{
				return this.PartialRodTemplate != RodTemplate.UnEquiped;
			}
		}

		[JsonIgnore]
		public bool IsCompleteRodTemplate
		{
			get
			{
				return this.CompleteRodTemplate != RodTemplate.UnEquiped;
			}
		}

		[JsonIgnore]
		public RodTemplate RodTemplate
		{
			get
			{
				RodTemplate completeRodTemplate = this.CompleteRodTemplate;
				if (completeRodTemplate != RodTemplate.UnEquiped)
				{
					return completeRodTemplate;
				}
				RodTemplate partialRodTemplate = this.PartialRodTemplate;
				if (partialRodTemplate != RodTemplate.UnEquiped)
				{
					return partialRodTemplate;
				}
				return RodTemplate.UnEquiped;
			}
		}

		[JsonIgnore]
		private RodTemplate CompleteRodTemplate
		{
			get
			{
				Rod rod = this.Rod;
				List<InventoryItem> list = this.Items.Where((InventoryItem i) => !object.ReferenceEquals(i, rod)).ToList<InventoryItem>();
				return RodTemplates.MatchedTemplateComplete(rod, list);
			}
		}

		[JsonIgnore]
		private RodTemplate PartialRodTemplate
		{
			get
			{
				Rod rod = this.Rod;
				List<InventoryItem> list = this.Items.Where((InventoryItem i) => !object.ReferenceEquals(i, rod)).ToList<InventoryItem>();
				return RodTemplates.MatchedTemplatePartial(rod, list);
			}
		}

		public List<InventoryItem> GetMissingItems(int slot)
		{
			Dictionary<InventoryItem, InventoryItem> map = this.profile.Inventory.GetItemsToEquipSetup(this, slot);
			return map.Keys.Where((InventoryItem i) => map[i] == null).ToList<InventoryItem>();
		}

		[JsonIgnore]
		public IEnumerable<InventoryItem> ItemsPrefferableInEquipmentStorage
		{
			get
			{
				return this.Items.Where((InventoryItem i) => !(i is Bobber) && i is TerminalTackleItem);
			}
		}

		[JsonIgnore]
		public Dictionary<InventoryItem, int> ItemsToSplit
		{
			get
			{
				return this.Items.Where((InventoryItem i) => !(i is Rod)).ToDictionary((InventoryItem i) => i, delegate(InventoryItem i)
				{
					if (i is Line)
					{
						return this.DesirableLineLength;
					}
					return 1;
				});
			}
		}

		[JsonIgnore]
		public Rod Rod
		{
			get
			{
				return this.Items.OfType<Rod>().FirstOrDefault<Rod>();
			}
		}

		[JsonIgnore]
		public Reel Reel
		{
			get
			{
				return this.Items.OfType<Reel>().FirstOrDefault<Reel>();
			}
		}

		[JsonIgnore]
		public Line Line
		{
			get
			{
				return this.Items.OfType<Line>().FirstOrDefault<Line>();
			}
		}

		[JsonIgnore]
		public int DesirableLineLength
		{
			get
			{
				return (int)Math.Round((double)this.Reel.LineCapacity / Math.Round((double)this.Line.Thickness, 3) / 100.0, 0, MidpointRounding.AwayFromZero);
			}
		}

		[JsonIgnore]
		private Profile profile;

		private List<InventoryItem> _Items = new List<InventoryItem>();
	}
}
