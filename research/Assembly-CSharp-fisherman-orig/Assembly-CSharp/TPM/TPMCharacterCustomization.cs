using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

namespace TPM
{
	public class TPMCharacterCustomization : MonoBehaviour
	{
		public GameObject PlayerPrefab
		{
			get
			{
				return this._playerPrefab;
			}
		}

		public GameObject MainPlayerPrefab
		{
			get
			{
				return this._mainPlayerPrefab;
			}
		}

		public TPMFullIKDebugSettings IKSettings
		{
			get
			{
				return this._iKSettings;
			}
		}

		public GameObject IKPartPrefab
		{
			get
			{
				return this._ikPartPrefab;
			}
		}

		public Transform RootWithPrecreatedPlayers { get; set; }

		public TPMModelSettings GetGenderSettings(Gender gender)
		{
			return this._genderSettings[(int)((byte)gender)];
		}

		public Gender GetGender(Player playerData)
		{
			return this.GetGender(playerData.TpmCharacterModel.Head);
		}

		public Gender GetGender(Faces face)
		{
			HeadRecord head = TPMCharacterCustomization.Instance.GetHead(face);
			return head.gender;
		}

		public Gender GetMyGender()
		{
			if (!StaticUserData.IS_TPM_ENABLED)
			{
				return Gender.Male;
			}
			Dictionary<string, string> settings = PhotonConnectionFactory.Instance.Profile.Settings;
			Faces faces = (Faces)((!settings.ContainsKey("TpmHeadType")) ? 0 : Convert.ToInt32(settings["TpmHeadType"]));
			return this.GetGender(faces);
		}

		public static DebugCustomizationSettings DebugSettings1 { get; private set; }

		public static DebugCustomizationSettings DebugSettings2 { get; private set; }

		public static TPMCharacterCustomization Instance { get; private set; }

		public ShirtRecord[] Shirts
		{
			get
			{
				return this._shirts;
			}
		}

		public HeadRecord[] Heads
		{
			get
			{
				return this._heads;
			}
		}

		public HairRecord[] Hairs
		{
			get
			{
				return this._hairs;
			}
		}

		public HatsRecord[] Hats
		{
			get
			{
				return this._hats;
			}
		}

		public HandsRecord[] Hands
		{
			get
			{
				return this._hands;
			}
		}

		public PantsRecord[] Pants
		{
			get
			{
				return this._pants;
			}
		}

		public ShoesRecord[] Shoes
		{
			get
			{
				return this._shoes;
			}
		}

		private void Awake()
		{
			this._genderSettings[0] = this._maleSettings;
			this._genderSettings[1] = this._femaleSettings;
			TPMCharacterCustomization.Instance = this;
			TPMCharacterCustomization.DebugSettings1 = this._debugSettings1;
			TPMCharacterCustomization.DebugSettings2 = this._debugSettings2;
		}

		private void OnDestroy()
		{
			try
			{
				TPMCharacterCustomization.Instance = null;
			}
			catch
			{
			}
		}

		public ShirtRecord GetShirt(Shirts shirt)
		{
			for (int i = 0; i < this._shirts.Length; i++)
			{
				if (this._shirts[i].shirts == shirt)
				{
					return this._shirts[i];
				}
			}
			LogHelper.Error("Can't find model for the shirt {0}", new object[] { shirt });
			return null;
		}

		public Shirts GetShirtByPath(Gender gender, string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return TPM.Shirts.Default;
			}
			path = path.ToLower();
			if (gender == Gender.Male)
			{
				for (int i = 0; i < this._shirts.Length; i++)
				{
					if (this._shirts[i].mModelPath.ToLower() == path)
					{
						return this._shirts[i].shirts;
					}
				}
			}
			else
			{
				for (int j = 0; j < this._shirts.Length; j++)
				{
					if (this._shirts[j].fModelPath.ToLower() == path)
					{
						return this._shirts[j].shirts;
					}
				}
			}
			return TPM.Shirts.Default;
		}

		public HeadRecord GetHead(Faces face)
		{
			for (int i = 0; i < this._heads.Length; i++)
			{
				if (this._heads[i].face == face)
				{
					return this._heads[i];
				}
			}
			LogHelper.Error("Can't find head model for {0}", new object[] { face });
			return null;
		}

		public HairRecord GetHair(Hair hair)
		{
			for (int i = 0; i < this._hairs.Length; i++)
			{
				if (this._hairs[i].hair == hair)
				{
					return this._hairs[i];
				}
			}
			LogHelper.Error("Can't find hair model for {0}", new object[] { hair });
			return null;
		}

		public HatsRecord GetHat(Hats hat)
		{
			if (hat == TPM.Hats.None)
			{
				return null;
			}
			for (int i = 0; i < this._hats.Length; i++)
			{
				if (this._hats[i].hat == hat)
				{
					return this._hats[i];
				}
			}
			LogHelper.Error("Can't find hat model for {0}", new object[] { hat });
			return null;
		}

		public Hats GetHatByPath(Gender gender, string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return TPM.Hats.None;
			}
			path = path.ToLower();
			if (gender == Gender.Male)
			{
				for (int i = 0; i < this._hats.Length; i++)
				{
					if (this._hats[i].mModelPath.ToLower() == path)
					{
						return this._hats[i].hat;
					}
				}
			}
			else
			{
				for (int j = 0; j < this._hats.Length; j++)
				{
					if (this._hats[j].fModelPath.ToLower() == path)
					{
						return this._hats[j].hat;
					}
				}
			}
			return TPM.Hats.None;
		}

		public HandLengthRecord GetHands(Gender gender, HandLength shirtHandLength)
		{
			for (int i = 0; i < this._hands.Length; i++)
			{
				if (this._hands[i].gender == gender)
				{
					for (int j = 0; j < this._hands[i].hands.Length; j++)
					{
						if (this._hands[i].hands[j].handsLength == shirtHandLength)
						{
							return this._hands[i].hands[j];
						}
					}
				}
			}
			LogHelper.Error("Can't find hands model for gender {0} and hands length {1}", new object[] { gender, shirtHandLength });
			return null;
		}

		public GameObject GetPants(Pants pants, Shoes shoes)
		{
			string pantsPath = this.GetPantsPath(pants, shoes);
			if (string.IsNullOrEmpty(pantsPath))
			{
				return null;
			}
			return TPMCharacterCustomization.LoadPrefabByPath(pantsPath);
		}

		public string GetPantsPath(Pants pants, Shoes shoes)
		{
			for (int i = 0; i < this._pants.Length; i++)
			{
				if (this._pants[i].pants == pants)
				{
					for (int j = 0; j < this._pants[i].models.Length; j++)
					{
						if (this._pants[i].models[j].Shoes == shoes)
						{
							string modelPath = this._pants[i].models[j].modelPath;
							if (string.IsNullOrEmpty(modelPath))
							{
								LogHelper.Error("There is not path for {0}, {1}", new object[] { pants, shoes });
							}
							return modelPath;
						}
					}
					LogHelper.Warning("Use default model {2} for {0} pants and {1} shoes", new object[]
					{
						pants,
						shoes,
						this._pants[i].modelPath
					});
					return this._pants[i].modelPath;
				}
			}
			LogHelper.Error("Can't find pants model for {0}", new object[] { pants });
			return null;
		}

		public GameObject GetShoes(Gender gender, Shoes shoes)
		{
			string shoesPath = this.GetShoesPath(gender, shoes);
			if (string.IsNullOrEmpty(shoesPath))
			{
				return null;
			}
			return TPMCharacterCustomization.LoadPrefabByPath(shoesPath);
		}

		public string GetShoesPath(Gender gender, Shoes shoes)
		{
			for (int i = 0; i < this._shoes.Length; i++)
			{
				if (this._shoes[i].Shoes == shoes)
				{
					string text = ((gender != Gender.Male) ? this._shoes[i].fModelPath : this._shoes[i].mModelPath);
					if (string.IsNullOrEmpty(text))
					{
						LogHelper.Error("There is not path for {0}, {1}", new object[] { gender, shoes });
					}
					return text;
				}
			}
			LogHelper.Error("Can't find shoes model for {0}", new object[] { shoes });
			return null;
		}

		public static GameObject LoadPrefabByPath(string path)
		{
			GameObject gameObject = (GameObject)Resources.Load(path, typeof(GameObject));
			if (gameObject == null)
			{
				LogHelper.Error("Can't find resource from path {0}", new object[] { path });
			}
			return gameObject;
		}

		public static void SetMeshColor(SkinnedMeshRenderer renderer, Color color)
		{
			int num = renderer.sharedMesh.vertices.Length;
			Color[] array = new Color[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = color;
			}
			renderer.sharedMesh.colors = array;
		}

		internal static Color ChangeSkinColorRange(Color color)
		{
			return new Color(1f - (TPMCharacterCustomization.baseSkinColor.r - color.r) / TPMCharacterCustomization.baseSkinColor.r, 1f - (TPMCharacterCustomization.baseSkinColor.g - color.g) / TPMCharacterCustomization.baseSkinColor.g, 1f - (TPMCharacterCustomization.baseSkinColor.b - color.b) / TPMCharacterCustomization.baseSkinColor.b, color.a);
		}

		[SerializeField]
		private GameObject _playerPrefab;

		[SerializeField]
		private GameObject _mainPlayerPrefab;

		[SerializeField]
		private TPMFullIKDebugSettings _iKSettings;

		[SerializeField]
		private GameObject _ikPartPrefab;

		[SerializeField]
		private TPMModelSettings _maleSettings;

		[SerializeField]
		private TPMModelSettings _femaleSettings;

		private TPMModelSettings[] _genderSettings = new TPMModelSettings[2];

		public ShirtRecord[] _shirts;

		public HeadRecord[] _heads;

		public HairRecord[] _hairs;

		public HatsRecord[] _hats;

		public HandsRecord[] _hands = new HandsRecord[2];

		public PantsRecord[] _pants;

		public ShoesRecord[] _shoes;

		[SerializeField]
		private DebugCustomizationSettings _debugSettings1;

		[SerializeField]
		private DebugCustomizationSettings _debugSettings2;

		private static Color baseSkinColor = new Color(0.95686275f, 0.87058824f, 0.8f, 1f);
	}
}
