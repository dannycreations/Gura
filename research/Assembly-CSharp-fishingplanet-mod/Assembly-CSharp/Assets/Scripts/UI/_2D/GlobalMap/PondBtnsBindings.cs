using System;
using System.Collections.Generic;

namespace Assets.Scripts.UI._2D.GlobalMap
{
	public class PondBtnsBindings
	{
		public static PondBtnBinding GetPondBindings(Ponds pondId)
		{
			return (!PondBtnsBindings.PondToBindings.ContainsKey(pondId)) ? null : PondBtnsBindings.PondToBindings[pondId];
		}

		private static readonly Dictionary<Ponds, PondBtnBinding> PondToBindings = new Dictionary<Ponds, PondBtnBinding>
		{
			{
				Ponds.Rocky,
				new PondBtnBinding(114, 102, 118, 220)
			},
			{
				Ponds.Mudwater,
				new PondBtnBinding(100, 106, 115, 123)
			},
			{
				Ponds.Neherrin,
				new PondBtnBinding(102, 124, 111, 113)
			},
			{
				Ponds.Falcon,
				new PondBtnBinding(140, 115, 121, 114)
			},
			{
				Ponds.Emerald,
				new PondBtnBinding(115, 124, -1, 106)
			},
			{
				Ponds.Everglades,
				new PondBtnBinding(130, 170, 106, 220)
			},
			{
				Ponds.SanJoaquin,
				new PondBtnBinding(-1, 100, 109, 220)
			},
			{
				Ponds.SaintCroix,
				new PondBtnBinding(118, 111, -1, 130)
			},
			{
				Ponds.WhileMoose,
				new PondBtnBinding(109, 115, 121, 100)
			},
			{
				Ponds.LoneStar,
				new PondBtnBinding(100, 123, 118, 220)
			},
			{
				Ponds.KaniqCreek,
				new PondBtnBinding(-1, 124, -1, 109)
			},
			{
				Ponds.Quanchkin,
				new PondBtnBinding(119, 130, 102, 220)
			},
			{
				Ponds.WeepinWillow,
				new PondBtnBinding(111, 160, -1, 170)
			},
			{
				Ponds.Mississippi,
				new PondBtnBinding(123, 113, 115, 220)
			},
			{
				Ponds.AhtubaVolga,
				new PondBtnBinding(150, 109, -1, 170)
			},
			{
				Ponds.LesniVilaPond,
				new PondBtnBinding(200, 140, -1, 170)
			},
			{
				Ponds.Zeekanaal,
				new PondBtnBinding(124, 200, -1, 170)
			},
			{
				Ponds.Tiber,
				new PondBtnBinding(106, 140, 200, 220)
			},
			{
				Ponds.Kyiv,
				new PondBtnBinding(-1, -1, -1, -1)
			},
			{
				Ponds.LaCreuse,
				new PondBtnBinding(-1, -1, -1, -1)
			},
			{
				Ponds.SanderBaggersee,
				new PondBtnBinding(160, 150, -1, 170)
			},
			{
				Ponds.Karelia,
				new PondBtnBinding(-1, -1, -1, -1)
			},
			{
				Ponds.MakuMakuLake,
				new PondBtnBinding(-1, 170, 113, -1)
			}
		};
	}
}
