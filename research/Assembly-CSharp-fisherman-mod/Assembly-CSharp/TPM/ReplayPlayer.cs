using System;
using ObjectModel;

namespace TPM
{
	public class ReplayPlayer : IPlayer, ILevelRank
	{
		public ReplayPlayer(ReplaysSetings.PlayerCustomization settings)
		{
			this.UserId = "0";
			this.UserName = "Replay";
			this.Level = 1;
			this.Rank = 0;
			this.TpmCharacterModel = new TPMCharacterModel((Faces)Enum.Parse(typeof(Faces), settings.Head), (Hair)Enum.Parse(typeof(Hair), settings.Hair), (Pants)Enum.Parse(typeof(Pants), settings.Pants), (Hats)Enum.Parse(typeof(Hats), settings.Hat), (Shirts)Enum.Parse(typeof(Shirts), settings.Shirt), (Shoes)Enum.Parse(typeof(Shoes), settings.Shoes));
			this.TpmCharacterModel.SetSkinColor((float)settings.SkinR / 255f, (float)settings.SkinG / 255f, (float)settings.SkinB / 255f);
		}

		public string UserId { get; set; }

		public string UserName { get; set; }

		public int Level { get; set; }

		public int Rank { get; set; }

		public TPMCharacterModel TpmCharacterModel { get; set; }

		public bool IsReplay
		{
			get
			{
				return true;
			}
		}

		public const string DEFAULT_ID = "0";
	}
}
