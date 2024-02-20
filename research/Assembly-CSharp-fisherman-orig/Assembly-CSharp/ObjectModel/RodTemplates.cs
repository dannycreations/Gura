using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectModel
{
	public class RodTemplates
	{
		public RodTemplates(Inventory inventory)
		{
			this.inventory = inventory;
		}

		public static RodTemplates.CustomPredicate Or(params RodTemplates.CustomPredicate[] predicates)
		{
			return (Rod r, List<InventoryItem> e) => predicates.Any((RodTemplates.CustomPredicate p) => p(r, e));
		}

		public static RodTemplates.CustomPredicate And(params RodTemplates.CustomPredicate[] predicates)
		{
			return (Rod r, List<InventoryItem> e) => predicates.All((RodTemplates.CustomPredicate p) => p(r, e));
		}

		public RodTemplate MatchedTemplate(Rod rod)
		{
			if (rod.Durability == 0)
			{
				return RodTemplate.UnEquiped;
			}
			List<InventoryItem> rodEquipment = this.inventory.GetRodEquipment(rod);
			return RodTemplates.MatchedTemplateComplete(rod, rodEquipment);
		}

		public RodTemplate MatchedTemplateWith(Rod rod, InventoryItem item)
		{
			if (rod.Durability == 0)
			{
				return RodTemplate.UnEquiped;
			}
			List<InventoryItem> rodEquipment = this.inventory.GetRodEquipment(rod);
			if (!rodEquipment.Contains(item))
			{
				rodEquipment.Add(item);
			}
			return RodTemplates.MatchedTemplateComplete(rod, rodEquipment);
		}

		public RodTemplate MatchedTemplatePartialWith(Rod rod, InventoryItem item)
		{
			if (rod.Durability == 0)
			{
				return RodTemplate.UnEquiped;
			}
			List<InventoryItem> rodEquipment = this.inventory.GetRodEquipment(rod);
			if (!rodEquipment.Contains(item))
			{
				rodEquipment.Add(item);
			}
			return RodTemplates.MatchedTemplatePartial(rod, rodEquipment);
		}

		public static RodTemplate MatchedTemplateComplete(Rod rod, List<InventoryItem> rodEquipment)
		{
			foreach (RodTemplates.RodTemplateDesc rodTemplateDesc in RodTemplates.Templates)
			{
				if (RodTemplates.MatchTemplate(rod, rodEquipment, rodTemplateDesc, rod.ItemSubType, true))
				{
					return rodTemplateDesc.Template;
				}
			}
			return RodTemplate.UnEquiped;
		}

		public static ItemTypes[] GetTypesForTemplate(RodTemplate template)
		{
			if (template == RodTemplate.UnEquiped)
			{
				return new ItemTypes[0];
			}
			return RodTemplates.Templates.Where((RodTemplates.RodTemplateDesc x) => x.Template == template).SelectMany((RodTemplates.RodTemplateDesc x) => x.ItemTypes).Distinct<ItemTypes>()
				.ToArray<ItemTypes>();
		}

		public RodTemplate MatchedLargestTemplatePartial(Rod rod)
		{
			if (rod.Durability == 0)
			{
				return RodTemplate.UnEquiped;
			}
			List<InventoryItem> rodEquipment = this.inventory.GetRodEquipment(rod);
			return RodTemplates.MatchedLargestTemplatePartial(rod, rodEquipment);
		}

		public RodTemplate MatchedTemplatePartial(Rod rod)
		{
			if (rod.Durability == 0)
			{
				return RodTemplate.UnEquiped;
			}
			List<InventoryItem> rodEquipment = this.inventory.GetRodEquipment(rod);
			return RodTemplates.MatchedTemplatePartial(rod, rodEquipment);
		}

		public static RodTemplate MatchedTemplatePartial(Rod rod, List<InventoryItem> rodEquipment)
		{
			rodEquipment = (from i in rodEquipment
				where !(i is Chum)
				where !(i is Bell)
				select i).ToList<InventoryItem>();
			foreach (RodTemplates.RodTemplateDesc rodTemplateDesc in RodTemplates.TemplatesPartial)
			{
				if (RodTemplates.MatchTemplate(rod, rodEquipment, rodTemplateDesc, rod.ItemSubType, true))
				{
					return rodTemplateDesc.Template;
				}
			}
			return RodTemplate.UnEquiped;
		}

		public static RodTemplate MatchedLargestTemplatePartial(Rod rod, List<InventoryItem> rodEquipment)
		{
			rodEquipment = (from i in rodEquipment
				where !(i is Chum)
				where !(i is Bell)
				select i).ToList<InventoryItem>();
			RodTemplates.RodTemplateDesc rodTemplateDesc = (from x in RodTemplates.TemplatesPartial
				where RodTemplates.MatchTemplate(rod, rodEquipment, x, rod.ItemSubType, true)
				orderby RodTemplates.Templates.FirstOrDefault((RodTemplates.RodTemplateDesc y) => y.Template == x.Template).ItemTypes.Length descending
				select x).FirstOrDefault<RodTemplates.RodTemplateDesc>();
			if (rodTemplateDesc != null)
			{
				return rodTemplateDesc.Template;
			}
			return RodTemplate.UnEquiped;
		}

		private static bool MatchTemplate(Rod rod, List<InventoryItem> items, RodTemplates.RodTemplateDesc template, ItemSubTypes rodType, bool checkPredicate)
		{
			if (template.RodTypes != null && template.RodTypes.Length > 0 && !template.RodTypes.Contains(rodType))
			{
				return false;
			}
			foreach (ItemTypes itemTypes2 in template.ItemTypes)
			{
				bool flag = false;
				foreach (InventoryItem inventoryItem in items)
				{
					if (inventoryItem.ItemType == itemTypes2 && inventoryItem.Durability > 0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (!template.IsPartial)
			{
				foreach (InventoryItem inventoryItem2 in items)
				{
					bool flag2 = false;
					foreach (ItemTypes itemTypes4 in template.ItemTypes)
					{
						if (inventoryItem2.ItemType == itemTypes4 && inventoryItem2.Durability > 0)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						return false;
					}
				}
				if (checkPredicate && template.Predicate != null && !template.Predicate(rod, items))
				{
					return false;
				}
				return true;
			}
			List<RodTemplates.RodTemplateDesc> list = RodTemplates.Templates.Where((RodTemplates.RodTemplateDesc x) => x.Template == template.Template).ToList<RodTemplates.RodTemplateDesc>();
			if (list.Count == 0)
			{
				return false;
			}
			RodTemplates.RodTemplateDesc rodTemplateDesc = null;
			foreach (RodTemplates.RodTemplateDesc rodTemplateDesc2 in list)
			{
				bool flag3 = true;
				foreach (InventoryItem inventoryItem3 in items)
				{
					bool flag4 = false;
					foreach (ItemTypes itemTypes6 in rodTemplateDesc2.ItemTypes)
					{
						if (inventoryItem3.ItemType == itemTypes6 && inventoryItem3.Durability > 0)
						{
							flag4 = true;
							break;
						}
					}
					if (!flag4)
					{
						flag3 = false;
						break;
					}
				}
				if (flag3)
				{
					rodTemplateDesc = rodTemplateDesc2;
					break;
				}
			}
			if (rodTemplateDesc == null)
			{
				return false;
			}
			if (checkPredicate && template.Predicate != null && !template.Predicate(rod, items))
			{
				return false;
			}
			return true;
		}

		private static bool SpinLeaderCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			Leader leader = rodEquipment.OfType<Leader>().FirstOrDefault<Leader>();
			return leader != null && (leader.ItemSubType == ItemSubTypes.FlurLeader || leader.ItemSubType == ItemSubTypes.TitaniumLeader);
		}

		private static bool MonoLeaderCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			Leader leader = rodEquipment.OfType<Leader>().FirstOrDefault<Leader>();
			return leader != null && leader.ItemSubType == ItemSubTypes.MonoLeader;
		}

		private static bool CageFeederCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			Feeder feeder = rodEquipment.OfType<Feeder>().FirstOrDefault<Feeder>();
			return feeder != null && feeder.ItemSubType == ItemSubTypes.CageFeeder;
		}

		private static bool PvaFeederCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			Feeder feeder = rodEquipment.OfType<Feeder>().FirstOrDefault<Feeder>();
			return feeder != null && feeder.ItemSubType == ItemSubTypes.PvaFeeder;
		}

		private static bool FlatFeederCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			Feeder feeder = rodEquipment.OfType<Feeder>().FirstOrDefault<Feeder>();
			return feeder != null && feeder.ItemSubType == ItemSubTypes.FlatFeeder;
		}

		private static bool SpodFeederCustomPredicateWithChumCount(Rod rod, List<InventoryItem> rodEquipment)
		{
			SpodFeeder spodFeeder = rodEquipment.OfType<SpodFeeder>().FirstOrDefault<SpodFeeder>();
			if (spodFeeder == null)
			{
				return false;
			}
			List<Chum> list = rodEquipment.OfType<Chum>().ToList<Chum>();
			return list.Count == spodFeeder.SlotCount;
		}

		private static bool FlippingRigCustomPredicatePartial(Rod rod, List<InventoryItem> rodEquipment)
		{
			Lure lure = rodEquipment.OfType<Lure>().FirstOrDefault<Lure>();
			return lure != null && lure.ItemSubType == ItemSubTypes.BassJig;
		}

		private static bool FlippingRigCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			JigBait jigBait = rodEquipment.OfType<JigBait>().FirstOrDefault<JigBait>();
			return jigBait != null && RodTemplates.FlippingRigCustomPredicatePartial(rod, rodEquipment) && (jigBait.ItemSubType == ItemSubTypes.Shad || jigBait.ItemSubType == ItemSubTypes.Worm || jigBait.ItemSubType == ItemSubTypes.Craw || jigBait.ItemSubType == ItemSubTypes.Tube || jigBait.ItemSubType == ItemSubTypes.Grub || jigBait.ItemSubType == ItemSubTypes.Slug);
		}

		private static bool SpinnerTailsCustomPredicatePartial(Rod rod, List<InventoryItem> rodEquipment)
		{
			Lure lure = rodEquipment.OfType<Lure>().FirstOrDefault<Lure>();
			return lure != null && (lure.ItemSubType == ItemSubTypes.BarblessSpinners || lure.ItemSubType == ItemSubTypes.Spinner);
		}

		private static bool SpinnerTailsCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			JigBait jigBait = rodEquipment.OfType<JigBait>().FirstOrDefault<JigBait>();
			return jigBait != null && RodTemplates.SpinnerTailsCustomPredicatePartial(rod, rodEquipment) && jigBait.ItemSubType == ItemSubTypes.Tail;
		}

		private static bool SpinnerbaitTailsCustomPredicatePartial(Rod rod, List<InventoryItem> rodEquipment)
		{
			Lure lure = rodEquipment.OfType<Lure>().FirstOrDefault<Lure>();
			return lure != null && (lure.ItemSubType == ItemSubTypes.Spinnerbait || lure.ItemSubType == ItemSubTypes.BuzzBait);
		}

		private static bool SpinnerbaitTailsCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			JigBait jigBait = rodEquipment.OfType<JigBait>().FirstOrDefault<JigBait>();
			return jigBait != null && RodTemplates.SpinnerbaitTailsCustomPredicatePartial(rod, rodEquipment) && (jigBait.ItemSubType == ItemSubTypes.Shad || jigBait.ItemSubType == ItemSubTypes.Worm || jigBait.ItemSubType == ItemSubTypes.Craw || jigBait.ItemSubType == ItemSubTypes.Tube || jigBait.ItemSubType == ItemSubTypes.Grub || jigBait.ItemSubType == ItemSubTypes.Tail || jigBait.ItemSubType == ItemSubTypes.Slug);
		}

		private static bool CarolinaRigCustomPredicatePartial(Rod rod, List<InventoryItem> rodEquipment)
		{
			Leader leader = rodEquipment.OfType<Leader>().FirstOrDefault<Leader>();
			return leader != null && leader.ItemSubType == ItemSubTypes.CarolinaRig;
		}

		private static bool CarolinaRigCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			JigBait jigBait = rodEquipment.OfType<JigBait>().FirstOrDefault<JigBait>();
			return jigBait != null && RodTemplates.CarolinaRigCustomPredicatePartial(rod, rodEquipment) && (jigBait.ItemSubType == ItemSubTypes.Shad || jigBait.ItemSubType == ItemSubTypes.Worm || jigBait.ItemSubType == ItemSubTypes.Craw || jigBait.ItemSubType == ItemSubTypes.Tube || jigBait.ItemSubType == ItemSubTypes.Grub || jigBait.ItemSubType == ItemSubTypes.Slug);
		}

		private static bool TexasRigCustomPredicatePartial(Rod rod, List<InventoryItem> rodEquipment)
		{
			Leader leader = rodEquipment.OfType<Leader>().FirstOrDefault<Leader>();
			return leader != null && leader.ItemSubType == ItemSubTypes.TexasRig;
		}

		private static bool TexasRigCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			JigBait jigBait = rodEquipment.OfType<JigBait>().FirstOrDefault<JigBait>();
			return jigBait != null && RodTemplates.TexasRigCustomPredicatePartial(rod, rodEquipment) && (jigBait.ItemSubType == ItemSubTypes.Shad || jigBait.ItemSubType == ItemSubTypes.Worm || jigBait.ItemSubType == ItemSubTypes.Craw || jigBait.ItemSubType == ItemSubTypes.Tube || jigBait.ItemSubType == ItemSubTypes.Grub || jigBait.ItemSubType == ItemSubTypes.Slug);
		}

		private static bool ThreewayRigCustomPredicatePartial(Rod rod, List<InventoryItem> rodEquipment)
		{
			Leader leader = rodEquipment.OfType<Leader>().FirstOrDefault<Leader>();
			return leader != null && leader.ItemSubType == ItemSubTypes.ThreewayRig;
		}

		private static bool ThreewayRigCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			JigBait jigBait = rodEquipment.OfType<JigBait>().FirstOrDefault<JigBait>();
			return jigBait != null && RodTemplates.ThreewayRigCustomPredicatePartial(rod, rodEquipment) && (jigBait.ItemSubType == ItemSubTypes.Shad || jigBait.ItemSubType == ItemSubTypes.Worm || jigBait.ItemSubType == ItemSubTypes.Craw || jigBait.ItemSubType == ItemSubTypes.Tube || jigBait.ItemSubType == ItemSubTypes.Grub);
		}

		private static bool OffsetJigCustomPredicatePartial(Rod rod, List<InventoryItem> rodEquipment)
		{
			Hook hook = rodEquipment.OfType<Hook>().FirstOrDefault<Hook>();
			return hook != null && hook.ItemSubType == ItemSubTypes.OffsetHook;
		}

		private static bool OffsetJigCustomPredicate(Rod rod, List<InventoryItem> rodEquipment)
		{
			JigBait jigBait = rodEquipment.OfType<JigBait>().FirstOrDefault<JigBait>();
			return jigBait != null && RodTemplates.OffsetJigCustomPredicatePartial(rod, rodEquipment) && (jigBait.ItemSubType == ItemSubTypes.Shad || jigBait.ItemSubType == ItemSubTypes.Worm || jigBait.ItemSubType == ItemSubTypes.Craw || jigBait.ItemSubType == ItemSubTypes.Tube || jigBait.ItemSubType == ItemSubTypes.Grub || jigBait.ItemSubType == ItemSubTypes.Slug);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static RodTemplates()
		{
			RodTemplates.RodTemplateDesc[] array = new RodTemplates.RodTemplateDesc[28];
			array[0] = new RodTemplates.RodTemplateDesc(RodTemplate.Float, RodTemplates.FloatRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Bobber,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.Bait
			}, new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate), false);
			array[1] = new RodTemplates.RodTemplateDesc(RodTemplate.Float, RodTemplates.FloatRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Bobber,
				ItemTypes.Hook,
				ItemTypes.Bait
			}, null, false);
			array[2] = new RodTemplates.RodTemplateDesc(RodTemplate.Jig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader,
				ItemTypes.JigHead,
				ItemTypes.JigBait
			}, new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate), false);
			array[3] = new RodTemplates.RodTemplateDesc(RodTemplate.Jig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.JigHead,
				ItemTypes.JigBait
			}, null, false);
			array[4] = new RodTemplates.RodTemplateDesc(RodTemplate.Lure, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader,
				ItemTypes.Lure
			}, new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate), false);
			array[5] = new RodTemplates.RodTemplateDesc(RodTemplate.Lure, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Lure
			}, null, false);
			int num = 6;
			RodTemplate rodTemplate = RodTemplate.FlippingRig;
			ItemSubTypes[] spinningRodSubTypes = RodTemplates.SpinningRodSubTypes;
			ItemTypes[] array2 = new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader,
				ItemTypes.Lure,
				ItemTypes.JigBait
			};
			RodTemplates.CustomPredicate[] array3 = new RodTemplates.CustomPredicate[2];
			array3[0] = new RodTemplates.CustomPredicate(RodTemplates.FlippingRigCustomPredicate);
			array3[1] = new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate);
			array[num] = new RodTemplates.RodTemplateDesc(rodTemplate, spinningRodSubTypes, array2, RodTemplates.And(array3), false);
			array[7] = new RodTemplates.RodTemplateDesc(RodTemplate.FlippingRig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Lure,
				ItemTypes.JigBait
			}, new RodTemplates.CustomPredicate(RodTemplates.FlippingRigCustomPredicate), false);
			int num2 = 8;
			RodTemplate rodTemplate2 = RodTemplate.SpinnerTails;
			ItemSubTypes[] spinningRodSubTypes2 = RodTemplates.SpinningRodSubTypes;
			ItemTypes[] array4 = new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader,
				ItemTypes.Lure,
				ItemTypes.JigBait
			};
			RodTemplates.CustomPredicate[] array5 = new RodTemplates.CustomPredicate[2];
			array5[0] = new RodTemplates.CustomPredicate(RodTemplates.SpinnerTailsCustomPredicate);
			array5[1] = new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate);
			array[num2] = new RodTemplates.RodTemplateDesc(rodTemplate2, spinningRodSubTypes2, array4, RodTemplates.And(array5), false);
			array[9] = new RodTemplates.RodTemplateDesc(RodTemplate.SpinnerTails, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Lure,
				ItemTypes.JigBait
			}, new RodTemplates.CustomPredicate(RodTemplates.SpinnerTailsCustomPredicate), false);
			int num3 = 10;
			RodTemplate rodTemplate3 = RodTemplate.SpinnerbaitTails;
			ItemSubTypes[] spinningRodSubTypes3 = RodTemplates.SpinningRodSubTypes;
			ItemTypes[] array6 = new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader,
				ItemTypes.Lure,
				ItemTypes.JigBait
			};
			RodTemplates.CustomPredicate[] array7 = new RodTemplates.CustomPredicate[2];
			array7[0] = new RodTemplates.CustomPredicate(RodTemplates.SpinnerbaitTailsCustomPredicate);
			array7[1] = new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate);
			array[num3] = new RodTemplates.RodTemplateDesc(rodTemplate3, spinningRodSubTypes3, array6, RodTemplates.And(array7), false);
			array[11] = new RodTemplates.RodTemplateDesc(RodTemplate.SpinnerbaitTails, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Lure,
				ItemTypes.JigBait
			}, new RodTemplates.CustomPredicate(RodTemplates.SpinnerbaitTailsCustomPredicate), false);
			array[12] = new RodTemplates.RodTemplateDesc(RodTemplate.CarolinaRig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader,
				ItemTypes.Sinker,
				ItemTypes.Hook,
				ItemTypes.JigBait
			}, new RodTemplates.CustomPredicate(RodTemplates.CarolinaRigCustomPredicate), false);
			array[13] = new RodTemplates.RodTemplateDesc(RodTemplate.TexasRig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader,
				ItemTypes.Sinker,
				ItemTypes.Hook,
				ItemTypes.JigBait
			}, new RodTemplates.CustomPredicate(RodTemplates.TexasRigCustomPredicate), false);
			array[14] = new RodTemplates.RodTemplateDesc(RodTemplate.ThreewayRig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader,
				ItemTypes.Sinker,
				ItemTypes.Hook,
				ItemTypes.JigBait
			}, new RodTemplates.CustomPredicate(RodTemplates.ThreewayRigCustomPredicate), false);
			int num4 = 15;
			RodTemplate rodTemplate4 = RodTemplate.OffsetJig;
			ItemSubTypes[] spinningRodSubTypes4 = RodTemplates.SpinningRodSubTypes;
			ItemTypes[] array8 = new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.JigBait
			};
			RodTemplates.CustomPredicate[] array9 = new RodTemplates.CustomPredicate[2];
			array9[0] = new RodTemplates.CustomPredicate(RodTemplates.OffsetJigCustomPredicate);
			array9[1] = new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate);
			array[num4] = new RodTemplates.RodTemplateDesc(rodTemplate4, spinningRodSubTypes4, array8, RodTemplates.And(array9), false);
			array[16] = new RodTemplates.RodTemplateDesc(RodTemplate.OffsetJig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Hook,
				ItemTypes.JigBait
			}, new RodTemplates.CustomPredicate(RodTemplates.OffsetJigCustomPredicate), false);
			array[17] = new RodTemplates.RodTemplateDesc(RodTemplate.PVACarp, RodTemplates.CarpRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Sinker,
				ItemTypes.Feeder,
				ItemTypes.Chum,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.Bait
			}, new RodTemplates.CustomPredicate(RodTemplates.PvaFeederCustomPredicate), false);
			array[18] = new RodTemplates.RodTemplateDesc(RodTemplate.ClassicCarp, RodTemplates.CarpRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Sinker,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.Bait
			}, null, false);
			array[19] = new RodTemplates.RodTemplateDesc(RodTemplate.MethodCarp, RodTemplates.CarpRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Feeder,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.Bait
			}, new RodTemplates.CustomPredicate(RodTemplates.FlatFeederCustomPredicate), false);
			array[20] = new RodTemplates.RodTemplateDesc(RodTemplate.MethodCarp, RodTemplates.CarpRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Feeder,
				ItemTypes.Chum,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.Bait
			}, new RodTemplates.CustomPredicate(RodTemplates.FlatFeederCustomPredicate), false);
			array[21] = new RodTemplates.RodTemplateDesc(RodTemplate.Spod, RodTemplates.SpodRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Feeder,
				ItemTypes.Chum
			}, new RodTemplates.CustomPredicate(RodTemplates.SpodFeederCustomPredicateWithChumCount), false);
			int num5 = 22;
			RodTemplate rodTemplate5 = RodTemplate.Bottom;
			ItemSubTypes[] bottomRodSubTypes = RodTemplates.BottomRodSubTypes;
			ItemTypes[] array10 = new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Sinker,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.Bait
			};
			RodTemplates.CustomPredicate[] array11 = new RodTemplates.CustomPredicate[2];
			array11[0] = new RodTemplates.CustomPredicate(RodTemplates.MonoLeaderCustomPredicate);
			array11[1] = new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate);
			array[num5] = new RodTemplates.RodTemplateDesc(rodTemplate5, bottomRodSubTypes, array10, RodTemplates.Or(array11), false);
			int num6 = 23;
			RodTemplate rodTemplate6 = RodTemplate.Bottom;
			ItemSubTypes[] bottomRodSubTypes2 = RodTemplates.BottomRodSubTypes;
			ItemTypes[] array12 = new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Sinker,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.Bait,
				ItemTypes.Bell
			};
			RodTemplates.CustomPredicate[] array13 = new RodTemplates.CustomPredicate[2];
			array13[0] = new RodTemplates.CustomPredicate(RodTemplates.MonoLeaderCustomPredicate);
			array13[1] = new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate);
			array[num6] = new RodTemplates.RodTemplateDesc(rodTemplate6, bottomRodSubTypes2, array12, RodTemplates.Or(array13), false);
			int num7 = 24;
			RodTemplate rodTemplate7 = RodTemplate.Bottom;
			ItemSubTypes[] bottomRodSubTypes3 = RodTemplates.BottomRodSubTypes;
			ItemTypes[] array14 = new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Feeder,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.Bait
			};
			RodTemplates.CustomPredicate[] array15 = new RodTemplates.CustomPredicate[2];
			array15[0] = new RodTemplates.CustomPredicate(RodTemplates.CageFeederCustomPredicate);
			int num8 = 1;
			RodTemplates.CustomPredicate[] array16 = new RodTemplates.CustomPredicate[2];
			array16[0] = new RodTemplates.CustomPredicate(RodTemplates.MonoLeaderCustomPredicate);
			array16[1] = new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate);
			array15[num8] = RodTemplates.Or(array16);
			array[num7] = new RodTemplates.RodTemplateDesc(rodTemplate7, bottomRodSubTypes3, array14, RodTemplates.And(array15), false);
			int num9 = 25;
			RodTemplate rodTemplate8 = RodTemplate.Bottom;
			ItemSubTypes[] bottomRodSubTypes4 = RodTemplates.BottomRodSubTypes;
			ItemTypes[] array17 = new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Feeder,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.Bait,
				ItemTypes.Bell
			};
			RodTemplates.CustomPredicate[] array18 = new RodTemplates.CustomPredicate[2];
			array18[0] = new RodTemplates.CustomPredicate(RodTemplates.CageFeederCustomPredicate);
			int num10 = 1;
			RodTemplates.CustomPredicate[] array19 = new RodTemplates.CustomPredicate[2];
			array19[0] = new RodTemplates.CustomPredicate(RodTemplates.MonoLeaderCustomPredicate);
			array19[1] = new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate);
			array18[num10] = RodTemplates.Or(array19);
			array[num9] = new RodTemplates.RodTemplateDesc(rodTemplate8, bottomRodSubTypes4, array17, RodTemplates.And(array18), false);
			int num11 = 26;
			RodTemplate rodTemplate9 = RodTemplate.Bottom;
			ItemSubTypes[] bottomRodSubTypes5 = RodTemplates.BottomRodSubTypes;
			ItemTypes[] array20 = new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Feeder,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.Bait,
				ItemTypes.Chum
			};
			RodTemplates.CustomPredicate[] array21 = new RodTemplates.CustomPredicate[2];
			array21[0] = new RodTemplates.CustomPredicate(RodTemplates.CageFeederCustomPredicate);
			int num12 = 1;
			RodTemplates.CustomPredicate[] array22 = new RodTemplates.CustomPredicate[2];
			array22[0] = new RodTemplates.CustomPredicate(RodTemplates.MonoLeaderCustomPredicate);
			array22[1] = new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate);
			array21[num12] = RodTemplates.Or(array22);
			array[num11] = new RodTemplates.RodTemplateDesc(rodTemplate9, bottomRodSubTypes5, array20, RodTemplates.And(array21), false);
			int num13 = 27;
			RodTemplate rodTemplate10 = RodTemplate.Bottom;
			ItemSubTypes[] bottomRodSubTypes6 = RodTemplates.BottomRodSubTypes;
			ItemTypes[] array23 = new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Feeder,
				ItemTypes.Leader,
				ItemTypes.Hook,
				ItemTypes.Bait,
				ItemTypes.Chum,
				ItemTypes.Bell
			};
			RodTemplates.CustomPredicate[] array24 = new RodTemplates.CustomPredicate[2];
			array24[0] = new RodTemplates.CustomPredicate(RodTemplates.CageFeederCustomPredicate);
			int num14 = 1;
			RodTemplates.CustomPredicate[] array25 = new RodTemplates.CustomPredicate[2];
			array25[0] = new RodTemplates.CustomPredicate(RodTemplates.MonoLeaderCustomPredicate);
			array25[1] = new RodTemplates.CustomPredicate(RodTemplates.SpinLeaderCustomPredicate);
			array24[num14] = RodTemplates.Or(array25);
			array[num13] = new RodTemplates.RodTemplateDesc(rodTemplate10, bottomRodSubTypes6, array23, RodTemplates.And(array24), false);
			RodTemplates.Templates = array;
			RodTemplates.RodTemplateDesc[] array26 = new RodTemplates.RodTemplateDesc[17];
			array26[0] = new RodTemplates.RodTemplateDesc(RodTemplate.Float, RodTemplates.FloatRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Bobber,
				ItemTypes.Hook
			}, null, true);
			array26[1] = new RodTemplates.RodTemplateDesc(RodTemplate.Float, RodTemplates.FloatRodSubTypes, new ItemTypes[] { ItemTypes.Reel }, null, true);
			array26[2] = new RodTemplates.RodTemplateDesc(RodTemplate.Jig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.JigHead
			}, null, true);
			array26[3] = new RodTemplates.RodTemplateDesc(RodTemplate.Jig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.JigBait
			}, null, true);
			array26[4] = new RodTemplates.RodTemplateDesc(RodTemplate.Lure, RodTemplates.SpinningRodSubTypes, new ItemTypes[] { ItemTypes.Reel }, null, true);
			array26[5] = new RodTemplates.RodTemplateDesc(RodTemplate.OffsetJig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Hook
			}, new RodTemplates.CustomPredicate(RodTemplates.OffsetJigCustomPredicatePartial), true);
			array26[6] = new RodTemplates.RodTemplateDesc(RodTemplate.FlippingRig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Lure
			}, new RodTemplates.CustomPredicate(RodTemplates.FlippingRigCustomPredicatePartial), true);
			array26[7] = new RodTemplates.RodTemplateDesc(RodTemplate.SpinnerTails, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Lure
			}, new RodTemplates.CustomPredicate(RodTemplates.SpinnerTailsCustomPredicatePartial), true);
			array26[8] = new RodTemplates.RodTemplateDesc(RodTemplate.SpinnerbaitTails, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Lure
			}, new RodTemplates.CustomPredicate(RodTemplates.SpinnerbaitTailsCustomPredicatePartial), true);
			array26[9] = new RodTemplates.RodTemplateDesc(RodTemplate.CarolinaRig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader
			}, new RodTemplates.CustomPredicate(RodTemplates.CarolinaRigCustomPredicatePartial), true);
			array26[10] = new RodTemplates.RodTemplateDesc(RodTemplate.TexasRig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader
			}, new RodTemplates.CustomPredicate(RodTemplates.TexasRigCustomPredicatePartial), true);
			array26[11] = new RodTemplates.RodTemplateDesc(RodTemplate.ThreewayRig, RodTemplates.SpinningRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Leader
			}, new RodTemplates.CustomPredicate(RodTemplates.ThreewayRigCustomPredicatePartial), true);
			array26[12] = new RodTemplates.RodTemplateDesc(RodTemplate.ClassicCarp, RodTemplates.CarpRodSubTypes, new ItemTypes[] { ItemTypes.Reel }, null, true);
			array26[13] = new RodTemplates.RodTemplateDesc(RodTemplate.MethodCarp, RodTemplates.CarpRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Feeder
			}, new RodTemplates.CustomPredicate(RodTemplates.FlatFeederCustomPredicate), true);
			array26[14] = new RodTemplates.RodTemplateDesc(RodTemplate.PVACarp, RodTemplates.CarpRodSubTypes, new ItemTypes[]
			{
				ItemTypes.Reel,
				ItemTypes.Line,
				ItemTypes.Feeder
			}, new RodTemplates.CustomPredicate(RodTemplates.PvaFeederCustomPredicate), true);
			array26[15] = new RodTemplates.RodTemplateDesc(RodTemplate.Spod, RodTemplates.SpodRodSubTypes, new ItemTypes[] { ItemTypes.Reel }, null, true);
			array26[16] = new RodTemplates.RodTemplateDesc(RodTemplate.Bottom, RodTemplates.BottomRodSubTypes, new ItemTypes[] { ItemTypes.Reel }, null, true);
			RodTemplates.TemplatesPartial = array26;
		}

		private readonly Inventory inventory;

		public static readonly ItemSubTypes[] FloatRodSubTypes = new ItemSubTypes[]
		{
			ItemSubTypes.TelescopicRod,
			ItemSubTypes.MatchRod
		};

		public static readonly ItemSubTypes[] SpinningRodSubTypes = new ItemSubTypes[]
		{
			ItemSubTypes.SpinningRod,
			ItemSubTypes.CastingRod
		};

		public static readonly ItemSubTypes[] BottomRodSubTypes = new ItemSubTypes[]
		{
			ItemSubTypes.BottomRod,
			ItemSubTypes.FeederRod
		};

		public static readonly ItemSubTypes[] CarpRodSubTypes = new ItemSubTypes[] { ItemSubTypes.CarpRod };

		public static readonly ItemSubTypes[] SpodRodSubTypes = new ItemSubTypes[] { ItemSubTypes.SpodRod };

		private static readonly RodTemplates.RodTemplateDesc[] Templates;

		private static readonly RodTemplates.RodTemplateDesc[] TemplatesPartial;

		public delegate bool CustomPredicate(Rod rod, List<InventoryItem> rodEquipment);

		private class RodTemplateDesc
		{
			public RodTemplateDesc(RodTemplate template, ItemSubTypes[] rodTypes, ItemTypes[] itemTypes, RodTemplates.CustomPredicate predicate = null, bool isPartial = false)
			{
				this.Template = template;
				this.ItemTypes = itemTypes;
				this.RodTypes = rodTypes;
				this.Predicate = predicate;
				this.IsPartial = isPartial;
			}

			public RodTemplate Template { get; private set; }

			public ItemTypes[] ItemTypes { get; private set; }

			public ItemSubTypes[] RodTypes { get; private set; }

			public RodTemplates.CustomPredicate Predicate { get; private set; }

			public bool IsPartial { get; private set; }
		}
	}
}
