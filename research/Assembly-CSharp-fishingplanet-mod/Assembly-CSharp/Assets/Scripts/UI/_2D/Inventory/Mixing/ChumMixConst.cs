using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ObjectModel;

namespace Assets.Scripts.UI._2D.Inventory.Mixing
{
	public class ChumMixConst
	{
		public static IList<Types> Ingredients
		{
			get
			{
				return ChumMixConst._ingredients;
			}
		}

		public static Dictionary<Types, string> Titles
		{
			get
			{
				return ChumMixConst._titles;
			}
		}

		public static Dictionary<Types, float> PrcAdd
		{
			get
			{
				return ChumMixConst._prcAddIngredients;
			}
		}

		public static Dictionary<Types, Func<int, int>> SlotsLevelsFuncs
		{
			get
			{
				return ChumMixConst._slotsLevelsFuncs;
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static ChumMixConst()
		{
			Dictionary<Types, Func<int, int>> dictionary = new Dictionary<Types, Func<int, int>>();
			dictionary.Add(Types.Base, new Func<int, int>(Inventory.GetChumSlotUnlockLevelForBase));
			dictionary.Add(Types.Aroma, new Func<int, int>(Inventory.GetChumSlotUnlockLevelForAroma));
			dictionary.Add(Types.Particle, new Func<int, int>(Inventory.GetChumSlotUnlockLevelForParticle));
			ChumMixConst._slotsLevelsFuncs = dictionary;
		}

		private static readonly IList<Types> _ingredients = new ReadOnlyCollection<Types>(new List<Types>
		{
			Types.Base,
			Types.Aroma,
			Types.Particle,
			Types.Water
		});

		private static readonly Dictionary<Types, string> _titles = new Dictionary<Types, string>
		{
			{
				Types.Base,
				"BasesCaption"
			},
			{
				Types.Aroma,
				"ChumAromasCaption"
			},
			{
				Types.Particle,
				"ChumParticlesCaption"
			},
			{
				Types.Water,
				"WaterCaption"
			}
		};

		private static readonly Dictionary<Types, float> _prcAddIngredients = new Dictionary<Types, float>
		{
			{
				Types.Water,
				0f
			},
			{
				Types.Base,
				Inventory.MixBasePossiblePercentageMin
			},
			{
				Types.Aroma,
				Inventory.MixAromaPossiblePercentage
			},
			{
				Types.Particle,
				Inventory.MixParticlePossiblePercentage
			}
		};

		private static readonly Dictionary<Types, Func<int, int>> _slotsLevelsFuncs;
	}
}
