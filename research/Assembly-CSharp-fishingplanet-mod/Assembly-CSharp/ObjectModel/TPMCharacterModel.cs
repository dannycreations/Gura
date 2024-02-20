using System;
using ExitGames.Client.Photon;
using TPM;
using UnityEngine;

namespace ObjectModel
{
	public class TPMCharacterModel
	{
		public TPMCharacterModel(int headType, int hairType, int pantsType, Hashtable outfit, Hashtable actorProperties)
		{
			if (hairType == 0)
			{
				this.HeadType = 5;
				this.HairType = 2;
				this.PantsType = 0;
				this.SetDefaultColors();
			}
			else
			{
				this.HeadType = headType;
				this.HairType = hairType;
				this.PantsType = pantsType;
				if (actorProperties.ContainsKey(104))
				{
					this.SkinColorR = (int)(Math3d.ParseFloat((string)actorProperties[104]) * 255f);
					this.SkinColorG = (int)(Math3d.ParseFloat((string)actorProperties[105]) * 255f);
					this.SkinColorB = (int)(Math3d.ParseFloat((string)actorProperties[106]) * 255f);
					this.EyeColorR = (int)(Math3d.ParseFloat((string)actorProperties[108]) * 255f);
					this.EyeColorG = (int)(Math3d.ParseFloat((string)actorProperties[109]) * 255f);
					this.EyeColorB = (int)(Math3d.ParseFloat((string)actorProperties[110]) * 255f);
				}
				else
				{
					this.SetDefaultColors();
				}
			}
			this.Outfit = outfit;
		}

		public TPMCharacterModel(Faces head, Hair hair, Pants pants, Hats hat, Shirts shirt, Shoes shoes)
		{
			this.HeadType = (int)head;
			this.HairType = (int)hair;
			this.PantsType = (int)pants;
			this.Hat = hat;
			this.Shirt = shirt;
			this.Shoes = shoes;
			this.SetDefaultColors();
		}

		public void SetSkinColor(float r, float g, float b)
		{
			if (r > 1f || g > 1f || b > 1f)
			{
				this.SkinColorR = (int)r;
				this.SkinColorG = (int)g;
				this.SkinColorB = (int)b;
			}
			else
			{
				this.SkinColorR = (int)(r * 255f);
				this.SkinColorG = (int)(g * 255f);
				this.SkinColorB = (int)(b * 255f);
			}
		}

		private void SetDefaultColors()
		{
			this.SkinColorR = 89;
			this.SkinColorG = 80;
			this.SkinColorB = 59;
			this.EyeColorR = 0;
			this.EyeColorG = 0;
			this.EyeColorB = 255;
		}

		private Hashtable Outfit { get; set; }

		private int HeadType { get; set; }

		public Faces Head
		{
			get
			{
				return (Faces)this.HeadType;
			}
		}

		private int HairType { get; set; }

		public Hair Hair
		{
			get
			{
				return (Hair)this.HairType;
			}
		}

		public Hats Hat { get; private set; }

		public string GetHatPath(Gender gender)
		{
			if (this.Outfit == null)
			{
				HatsRecord hat = TPMCharacterCustomization.Instance.GetHat(this.Hat);
				if (hat != null)
				{
					return (gender != Gender.Male) ? hat.fModelPath : hat.mModelPath;
				}
				return null;
			}
			else
			{
				ItemAssetInfo inventoryItem = this.GetInventoryItem(ItemSubTypes.Hat);
				if (inventoryItem == null)
				{
					return null;
				}
				return (gender != Gender.Male) ? inventoryItem.FemaleAsset3rdPerson : inventoryItem.MaleAsset3rdPerson;
			}
		}

		public Shirts Shirt { get; private set; }

		public string GetShirtPath(Gender gender)
		{
			if (this.Outfit == null)
			{
				ShirtRecord shirt = TPMCharacterCustomization.Instance.GetShirt(this.Shirt);
				return (gender != Gender.Male) ? shirt.fModelPath : shirt.mModelPath;
			}
			ItemAssetInfo inventoryItem = this.GetInventoryItem(ItemSubTypes.Waistcoat);
			if (inventoryItem == null)
			{
				ShirtRecord shirt2 = TPMCharacterCustomization.Instance.GetShirt(Shirts.Default);
				return (gender != Gender.Male) ? shirt2.fModelPath : shirt2.mModelPath;
			}
			string text = ((gender != Gender.Male) ? inventoryItem.FemaleAsset3rdPerson : inventoryItem.MaleAsset3rdPerson);
			if (string.IsNullOrEmpty(text))
			{
				ShirtRecord shirt3 = TPMCharacterCustomization.Instance.GetShirt(Shirts.Default);
				return (gender != Gender.Male) ? shirt3.fModelPath : shirt3.mModelPath;
			}
			return text;
		}

		public HandLength GetShirtHandLength(Gender gender)
		{
			if (this.Outfit == null)
			{
				ShirtRecord shirt = TPMCharacterCustomization.Instance.GetShirt(this.Shirt);
				return (gender != Gender.Male) ? shirt.fHandLength : shirt.mHandLength;
			}
			ItemAssetInfo inventoryItem = this.GetInventoryItem(ItemSubTypes.Waistcoat);
			if (inventoryItem == null)
			{
				ShirtRecord shirt2 = TPMCharacterCustomization.Instance.GetShirt(Shirts.Default);
				return (gender != Gender.Male) ? shirt2.fHandLength : shirt2.mHandLength;
			}
			if (inventoryItem.HandsLength != null)
			{
				return (HandLength)inventoryItem.HandsLength.Value;
			}
			LogHelper.Error("Shirt for asset \"{0}\" has no HandsLength property value - use default instead", new object[] { inventoryItem.Asset });
			ShirtRecord shirt3 = TPMCharacterCustomization.Instance.GetShirt(Shirts.Default);
			return (gender != Gender.Male) ? shirt3.fHandLength : shirt3.mHandLength;
		}

		private ItemAssetInfo GetInventoryItem(ItemSubTypes itemType)
		{
			if (this.Outfit.ContainsKey((byte)itemType))
			{
				return CacheLibrary.AssetsCache.GetItemAssetPath((int)this.Outfit[(byte)itemType]);
			}
			return null;
		}

		public int SkinColorR { get; set; }

		public int SkinColorG { get; set; }

		public int SkinColorB { get; set; }

		public Color SkinColor
		{
			get
			{
				return new Color((float)this.SkinColorR / 255f, (float)this.SkinColorG / 255f, (float)this.SkinColorB / 255f);
			}
		}

		public int EyeColorR { get; set; }

		public int EyeColorG { get; set; }

		public int EyeColorB { get; set; }

		public int EyeColorA { get; set; }

		public Color EyeColor
		{
			get
			{
				return new Color((float)this.EyeColorR / 255f, (float)this.EyeColorG / 255f, (float)this.EyeColorB / 255f, (float)this.EyeColorA / 255f);
			}
		}

		public int HairColorR { get; set; }

		public int HairColorG { get; set; }

		public int HairColorB { get; set; }

		public int HairColorA { get; set; }

		public Color HairColor
		{
			get
			{
				return new Color((float)this.HairColorR / 255f, (float)this.HairColorG / 255f, (float)this.HairColorB / 255f, (float)this.HairColorA / 255f);
			}
		}

		private int PantsType { get; set; }

		public Pants Pants
		{
			get
			{
				return (Pants)this.PantsType;
			}
		}

		public Shoes Shoes { get; private set; }

		public string GetShoesPath(Gender gender, out Shoes shoes)
		{
			if (this.Outfit == null)
			{
				shoes = this.Shoes;
				return TPMCharacterCustomization.Instance.GetShoesPath(gender, shoes);
			}
			ItemAssetInfo inventoryItem = this.GetInventoryItem(ItemSubTypes.Boots);
			if (inventoryItem == null)
			{
				shoes = Shoes.BOOTS;
				return TPMCharacterCustomization.Instance.GetShoesPath(gender, shoes);
			}
			string text = ((gender != Gender.Male) ? inventoryItem.FemaleAsset3rdPerson : inventoryItem.MaleAsset3rdPerson);
			if (string.IsNullOrEmpty(text))
			{
				shoes = Shoes.BOOTS;
				text = TPMCharacterCustomization.Instance.GetShoesPath(gender, shoes);
			}
			else
			{
				shoes = Shoes.BOOTS;
			}
			return text;
		}
	}
}
