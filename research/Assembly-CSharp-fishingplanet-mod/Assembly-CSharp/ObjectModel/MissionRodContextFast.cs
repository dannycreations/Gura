using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectModel
{
	public class MissionRodContextFast : List<InventoryItem>
	{
		public MissionRodContextFast(MissionsContext context, Rod rod)
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
			this.Rod = this.OfType<Rod>().FirstOrDefault<Rod>() ?? new Rod();
			this.Reel = this.OfType<Reel>().FirstOrDefault<Reel>() ?? new Reel();
			this.Line = this.OfType<Line>().FirstOrDefault<Line>() ?? new Line();
			this.Bobber = this.OfType<Bobber>().FirstOrDefault<Bobber>() ?? new Bobber();
			this.Hook = this.OfType<Hook>().FirstOrDefault<Hook>() ?? new Hook();
			this.JigHead = this.OfType<JigHead>().FirstOrDefault<JigHead>() ?? new JigHead();
			this.Lure = this.OfType<Lure>().FirstOrDefault<Lure>() ?? new Lure();
			this.Bait = this.OfType<Bait>().FirstOrDefault<Bait>() ?? new Bait();
			this.JigBait = this.OfType<JigBait>().FirstOrDefault<JigBait>() ?? new JigBait();
			this.Feeder = this.OfType<Feeder>().FirstOrDefault<Feeder>() ?? new Feeder();
			this.Sinker = this.Where((InventoryItem i) => i.ItemType == ItemTypes.Sinker).OfType<Sinker>().FirstOrDefault<Sinker>() ?? new Sinker();
			this.Leader = this.OfType<Leader>().FirstOrDefault<Leader>() ?? new Leader();
			Chum chum;
			if ((chum = this.OfType<Chum>().FirstOrDefault<Chum>()) == null)
			{
				chum = new Chum
				{
					Ingredients = new List<ChumIngredient>()
				};
			}
			this.Chum = chum;
			this.Bell = this.OfType<Bell>().FirstOrDefault<Bell>() ?? new Bell();
			this.Ids = this.Select((InventoryItem i) => i.ItemId);
			this.TackleIds = from i in this
				where !(i is Rod) && !(i is Reel) && !(i is Line)
				select i.ItemId;
		}

		public MissionsContext Context { get; private set; }

		public bool Present
		{
			get
			{
				return this.RodId != 0;
			}
		}

		public int RodId
		{
			get
			{
				return this.Rod.ItemId;
			}
		}

		public Rod Rod { get; private set; }

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

		public Reel Reel { get; private set; }

		public int LineId
		{
			get
			{
				return this.Line.ItemId;
			}
		}

		public Line Line { get; private set; }

		public int BobberId
		{
			get
			{
				return this.Bobber.ItemId;
			}
		}

		public Bobber Bobber { get; private set; }

		public int HookId
		{
			get
			{
				return this.Hook.ItemId;
			}
		}

		public Hook Hook { get; private set; }

		public int JigHeadId
		{
			get
			{
				return this.JigHead.ItemId;
			}
		}

		public JigHead JigHead { get; private set; }

		public int LureId
		{
			get
			{
				return this.Lure.ItemId;
			}
		}

		public Lure Lure { get; private set; }

		public int BaitId
		{
			get
			{
				return this.Bait.ItemId;
			}
		}

		public Bait Bait { get; private set; }

		public int JigBaitId
		{
			get
			{
				return this.JigBait.ItemId;
			}
		}

		public JigBait JigBait { get; private set; }

		public int FeederId
		{
			get
			{
				return this.Feeder.ItemId;
			}
		}

		public Feeder Feeder { get; private set; }

		public int SinkerId
		{
			get
			{
				return this.Sinker.ItemId;
			}
		}

		public Sinker Sinker { get; private set; }

		public int LeaderId
		{
			get
			{
				return this.Leader.ItemId;
			}
		}

		public Leader Leader { get; private set; }

		public int ChumId
		{
			get
			{
				return this.Chum.ItemId;
			}
		}

		public Chum Chum { get; private set; }

		public int BellId
		{
			get
			{
				return this.Bell.ItemId;
			}
		}

		public Bell Bell { get; private set; }

		public IEnumerable<int> Ids { get; private set; }

		public IEnumerable<int> TackleIds { get; private set; }
	}
}
