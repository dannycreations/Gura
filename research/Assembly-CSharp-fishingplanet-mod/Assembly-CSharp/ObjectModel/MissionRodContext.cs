using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectModel
{
	public class MissionRodContext : List<InventoryItem>
	{
		public MissionRodContext(MissionsContext context, Rod rod)
		{
			this.Context = context;
			if (rod != null)
			{
				base.Add(rod);
				base.AddRange(context.GetProfile().Inventory.Where(delegate(InventoryItem i)
				{
					bool flag2;
					if (i.Storage == StoragePlaces.ParentItem)
					{
						Guid? parentItemInstanceId = i.ParentItemInstanceId;
						bool flag = parentItemInstanceId != null;
						Guid? instanceId = rod.InstanceId;
						flag2 = flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault());
					}
					else
					{
						flag2 = false;
					}
					return flag2;
				}));
			}
		}

		public MissionsContext Context { get; private set; }

		public bool IsEmpty
		{
			get
			{
				return this.RodId == 0;
			}
		}

		public int RodId
		{
			get
			{
				return this.Rod.ItemId;
			}
		}

		public Rod Rod
		{
			get
			{
				return this.OfType<Rod>().FirstOrDefault<Rod>() ?? new Rod();
			}
		}

		public float LeaderLength
		{
			get
			{
				return this.Rod.LeaderLength;
			}
		}

		public int ReelId
		{
			get
			{
				return this.Reel.ItemId;
			}
		}

		public Reel Reel
		{
			get
			{
				return this.OfType<Reel>().FirstOrDefault<Reel>() ?? new Reel();
			}
		}

		public int LineId
		{
			get
			{
				return this.Line.ItemId;
			}
		}

		public Line Line
		{
			get
			{
				return this.OfType<Line>().FirstOrDefault<Line>() ?? new Line();
			}
		}

		public int BobberId
		{
			get
			{
				return this.Bobber.ItemId;
			}
		}

		public Bobber Bobber
		{
			get
			{
				return this.OfType<Bobber>().FirstOrDefault<Bobber>() ?? new Bobber();
			}
		}

		public int HookId
		{
			get
			{
				return this.Hook.ItemId;
			}
		}

		public Hook Hook
		{
			get
			{
				return this.OfType<Hook>().FirstOrDefault<Hook>() ?? new Hook();
			}
		}

		public int JigHeadId
		{
			get
			{
				return this.JigHead.ItemId;
			}
		}

		public JigHead JigHead
		{
			get
			{
				return this.OfType<JigHead>().FirstOrDefault<JigHead>() ?? new JigHead();
			}
		}

		public int LureId
		{
			get
			{
				return this.Lure.ItemId;
			}
		}

		public Lure Lure
		{
			get
			{
				return this.OfType<Lure>().FirstOrDefault<Lure>() ?? new Lure();
			}
		}

		public int BaitId
		{
			get
			{
				return this.Bait.ItemId;
			}
		}

		public Bait Bait
		{
			get
			{
				return this.OfType<Bait>().FirstOrDefault<Bait>() ?? new Bait();
			}
		}

		public int JigBaitId
		{
			get
			{
				return this.JigBait.ItemId;
			}
		}

		public JigBait JigBait
		{
			get
			{
				return this.OfType<JigBait>().FirstOrDefault<JigBait>() ?? new JigBait();
			}
		}

		public int FeederId
		{
			get
			{
				return this.Feeder.ItemId;
			}
		}

		public Feeder Feeder
		{
			get
			{
				return this.OfType<Feeder>().FirstOrDefault<Feeder>() ?? new Feeder();
			}
		}

		public int SinkerId
		{
			get
			{
				return this.Sinker.ItemId;
			}
		}

		public Sinker Sinker
		{
			get
			{
				return this.Where((InventoryItem i) => i.ItemType == ItemTypes.Sinker).OfType<Sinker>().FirstOrDefault<Sinker>() ?? new Sinker();
			}
		}

		public int LeaderId
		{
			get
			{
				return this.Leader.ItemId;
			}
		}

		public Leader Leader
		{
			get
			{
				return this.OfType<Leader>().FirstOrDefault<Leader>() ?? new Leader();
			}
		}

		public int ChumId
		{
			get
			{
				return this.Chum.ItemId;
			}
		}

		public Chum Chum
		{
			get
			{
				Chum chum;
				if ((chum = this.OfType<Chum>().FirstOrDefault<Chum>()) == null)
				{
					chum = new Chum
					{
						Ingredients = new List<ChumIngredient>()
					};
				}
				return chum;
			}
		}

		public int BellId
		{
			get
			{
				return this.Bell.ItemId;
			}
		}

		public Bell Bell
		{
			get
			{
				return this.OfType<Bell>().FirstOrDefault<Bell>() ?? new Bell();
			}
		}

		public IEnumerable<int> Ids
		{
			get
			{
				return this.Select((InventoryItem i) => i.ItemId);
			}
		}

		public IEnumerable<int> TackleIds
		{
			get
			{
				return from i in this.OfType<TerminalTackleItem>()
					select i.ItemId;
			}
		}
	}
}
