﻿using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;

namespace Assets.Scripts.UI._2D.Tournaments.UGC
{
	public class ItemSubTypesLocalization
	{
		public static string Localize(ItemSubTypes it, bool isSingle = false)
		{
			Dictionary<ItemSubTypes, string> dictionary = ((!isSingle) ? ItemSubTypesLocalization._loc : ItemSubTypesLocalization._locSingle);
			string text = ((!dictionary.ContainsKey(it)) ? null : dictionary[it]);
			if (string.IsNullOrEmpty(text))
			{
				string text2 = string.Format("ItemSubTypesLocalization:Localize unknown ItemSubTypes:{0} isSingle:{1}", it, isSingle);
				LogHelper.Error(text2, new object[0]);
				PhotonConnectionFactory.Instance.PinError(text2, "UGC");
				return it.ToString();
			}
			int num = text.IndexOf('+');
			if (num != -1)
			{
				return string.Format("{0} {1}", text.Substring(0, num), text.Substring(num + 1));
			}
			return ScriptLocalization.Get(text);
		}

		private static readonly Dictionary<ItemSubTypes, string> _locSingle = new Dictionary<ItemSubTypes, string>
		{
			{
				ItemSubTypes.TelescopicRod,
				"PremShop_TelescopicRod"
			},
			{
				ItemSubTypes.MatchRod,
				"PremShop_MatchRod"
			},
			{
				ItemSubTypes.SpinningRod,
				"PremShop_SpinningRod"
			},
			{
				ItemSubTypes.CastingRod,
				"PremShop_CastingRod"
			},
			{
				ItemSubTypes.FeederRod,
				"PremShop_FeederRod"
			},
			{
				ItemSubTypes.BottomRod,
				"PremShop_BottomRod"
			},
			{
				ItemSubTypes.CarpRod,
				"PremShop_CarpRod"
			},
			{
				ItemSubTypes.SpodRod,
				"PremShop_SpodRod"
			},
			{
				ItemSubTypes.FlyRod,
				"PremShop_FlyRod"
			}
		};

		private static readonly Dictionary<ItemSubTypes, string> _loc = new Dictionary<ItemSubTypes, string>
		{
			{
				ItemSubTypes.TelescopicRod,
				"TelescopicRodsFilter"
			},
			{
				ItemSubTypes.MatchRod,
				"MatchRodsFilter"
			},
			{
				ItemSubTypes.SpinningRod,
				"SpinningRodsFilter"
			},
			{
				ItemSubTypes.CastingRod,
				"CastingRodsFilter"
			},
			{
				ItemSubTypes.FeederRod,
				"FeederRodsCaption"
			},
			{
				ItemSubTypes.BottomRod,
				"BottomRodsCaption"
			},
			{
				ItemSubTypes.CarpRod,
				"CarpRodsCaption"
			},
			{
				ItemSubTypes.SpodRod,
				"SpodRodsCaption"
			},
			{
				ItemSubTypes.MonoLine,
				"MonoLineFilter"
			},
			{
				ItemSubTypes.BraidLine,
				"BraidLineFilter"
			},
			{
				ItemSubTypes.FlurLine,
				"FlurLineFilter"
			},
			{
				ItemSubTypes.CageFeeder,
				"CageFeedersCaption"
			},
			{
				ItemSubTypes.FlatFeeder,
				"FlatFeedersCaption"
			},
			{
				ItemSubTypes.PvaFeeder,
				"PVACaption"
			},
			{
				ItemSubTypes.SpodFeeder,
				"SpodFeedersCaption"
			},
			{
				ItemSubTypes.MonoLeader,
				"MonoLeadersCaption"
			},
			{
				ItemSubTypes.FlurLeader,
				"FluoroLeadersCaption"
			},
			{
				ItemSubTypes.BraidLeader,
				"BraidLeadersCaption"
			},
			{
				ItemSubTypes.SteelLeader,
				"SteelLeadersCaption"
			},
			{
				ItemSubTypes.CarpLeader,
				"CarpLeader"
			},
			{
				ItemSubTypes.Worm,
				"WormFilter"
			},
			{
				ItemSubTypes.Grub,
				"GrubFilter"
			},
			{
				ItemSubTypes.Shad,
				"ShadFilter"
			},
			{
				ItemSubTypes.Tube,
				"TubeFilter"
			},
			{
				ItemSubTypes.Craw,
				"CrawFilter"
			},
			{
				ItemSubTypes.Frog,
				"FrogFilter"
			},
			{
				ItemSubTypes.SimpleHook,
				"SimpleHook"
			},
			{
				ItemSubTypes.BarblessHook,
				"BarblessHook"
			},
			{
				ItemSubTypes.CarpHook,
				"CarpHook"
			},
			{
				ItemSubTypes.CommonJigHeads,
				"UGC_CommonJigHeadsFilter"
			},
			{
				ItemSubTypes.BarblessJigHeads,
				"UGC_BarblessJigHeadsFilter"
			},
			{
				ItemSubTypes.BassJig,
				"BassJigFilter"
			},
			{
				ItemSubTypes.Spoon,
				"SpoonFilter"
			},
			{
				ItemSubTypes.Spinner,
				"SpinnerFilter"
			},
			{
				ItemSubTypes.BarblessSpoons,
				"UGC_BarblessSpoonFilter"
			},
			{
				ItemSubTypes.BarblessSpinners,
				"UGC_BarblessSpinnerFilter"
			},
			{
				ItemSubTypes.Spinnerbait,
				"SpinnerbaitFilter"
			},
			{
				ItemSubTypes.Walker,
				"WalkersFilter"
			},
			{
				ItemSubTypes.Cranckbait,
				"CranckbaitFilter"
			},
			{
				ItemSubTypes.Jerkbait,
				"JerkbaitFilter"
			},
			{
				ItemSubTypes.Swimbait,
				"SwimbaitFilter"
			},
			{
				ItemSubTypes.Minnow,
				"MinnowMenuFilter"
			},
			{
				ItemSubTypes.CommonBait,
				"CommonBaitsFilter"
			},
			{
				ItemSubTypes.InsectsWormBait,
				"InsectsWormBaitsFilter"
			},
			{
				ItemSubTypes.FreshBait,
				"FreshBaitsFilter"
			},
			{
				ItemSubTypes.BoilBait_Boils,
				"BoilBaitsFilter"
			},
			{
				ItemSubTypes.BoilBait_Pellets,
				"PelletsBaitsFilter"
			},
			{
				ItemSubTypes.Popper,
				"PopperFilter"
			},
			{
				ItemSubTypes.ChumBase,
				"ChumBasesCaption"
			},
			{
				ItemSubTypes.ChumParticle,
				"ChumParticlesCaption"
			},
			{
				ItemSubTypes.ChumAroma,
				"ChumAromasCaption"
			},
			{
				ItemSubTypes.ChumCarpbaits_ChumSpodMix,
				"SpodMixCaption"
			},
			{
				ItemSubTypes.ChumMethodMix,
				"UGC_MethodCarp"
			},
			{
				ItemSubTypes.ChumCarpbaits_ChumPellets,
				"PelletsBaitsFilter+ChumBasesCaption"
			}
		};
	}
}