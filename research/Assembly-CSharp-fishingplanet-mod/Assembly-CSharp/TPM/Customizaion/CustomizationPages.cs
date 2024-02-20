using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using I2.Loc;
using InControl;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TPM.Customizaion
{
	public class CustomizationPages : TabControl
	{
		public static Gender CurGender
		{
			get
			{
				return CustomizationPages._curGender;
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event CustomizationPages.GenderChangedDelegate EGenderChanged;

		public CustomizationPages.Modes CurrentModes
		{
			get
			{
				return this._curMode;
			}
			set
			{
				this._curMode = value;
				for (int i = 0; i < this._buttonGroups.Length; i++)
				{
					CustomizationPages.ButtonGroup buttonGroup = this._buttonGroups[i];
					for (int j = 0; j < buttonGroup.buttons.Length; j++)
					{
						buttonGroup.buttons[j].SetActive(buttonGroup.mode == this._curMode);
					}
				}
			}
		}

		private void RefreshHelpText(InputModuleManager.InputType type)
		{
			if (type != InputModuleManager.InputType.GamePad)
			{
				if (this.HelpText != null)
				{
					this.HelpText.text = string.Empty;
				}
				return;
			}
			bool flag = false;
			if (this.HelpText != null)
			{
				this.HelpText.text = string.Format(ScriptLocalization.Get("CharacterCustomizationHelp"), HotkeyIcons.GetIcoByActionName(ControlsController.ControlsActions.SubmitRename.Name, out flag));
			}
		}

		protected void Awake()
		{
			this._rootPosition = this._viewRoot.position;
			this.RefreshHelpText(SettingsManager.InputType);
			InputModuleManager.OnInputTypeChanged += this.RefreshHelpText;
			this._selectBinding = new HotkeyBinding
			{
				Hotkey = InputControlType.Action1,
				LocalizationKey = "Select"
			};
			for (int i = 0; i < this._buttonGroups.Length; i++)
			{
				CustomizationPages.ButtonGroup buttonGroup = this._buttonGroups[i];
				for (int j = 0; j < buttonGroup.buttons.Length; j++)
				{
					buttonGroup.buttons[j].SetActive(buttonGroup.mode == this._curMode);
				}
			}
			this._curFace = this._defaultMaleFace;
			this._curHair = this._defaultMaleHair;
			this._curPants = this._defaultMalePants;
			for (int k = 0; k < this._viewRoot.childCount; k++)
			{
				Transform child = this._viewRoot.GetChild(k);
				CustomizationLayerSettings component = child.GetComponent<CustomizationLayerSettings>();
				Animator component2 = child.GetComponent<Animator>();
				if (component != null && component2 != null)
				{
					this._viewLayers[component.layer] = component2;
				}
			}
			for (int l = 0; l < 2; l++)
			{
				this._genderDependentPrefabs[l] = new List<CustomizationPages.GenderDependentRecord>();
			}
			List<CustomizationPages.GenderDependentRecord> list = this._genderDependentPrefabs[0];
			List<CustomizationPages.GenderDependentRecord> list2 = this._genderDependentPrefabs[1];
			ShirtRecord shirt = this._settings.GetShirt(this._maleShirt);
			GameObject gameObject = TPMCharacterCustomization.LoadPrefabByPath(shirt.mModelPath);
			HandLength handLength = shirt.mHandLength;
			ShirtRecord shirt2 = this._settings.GetShirt(this._femaleShirt);
			GameObject gameObject2 = TPMCharacterCustomization.LoadPrefabByPath(shirt2.fModelPath);
			HandLength handLength2 = shirt2.fHandLength;
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			if (profile != null)
			{
				InventoryItem inventoryItemGameObject = this.GetInventoryItemGameObject(ItemSubTypes.Waistcoat);
				if (inventoryItemGameObject != null && inventoryItemGameObject.HandsLength != null)
				{
					gameObject = TPMCharacterCustomization.LoadPrefabByPath(inventoryItemGameObject.MaleAsset3rdPerson);
					gameObject2 = TPMCharacterCustomization.LoadPrefabByPath(inventoryItemGameObject.FemaleAsset3rdPerson);
					handLength = (HandLength)inventoryItemGameObject.HandsLength.Value;
					handLength2 = (HandLength)inventoryItemGameObject.HandsLength.Value;
				}
				InventoryItem inventoryItemGameObject2 = this.GetInventoryItemGameObject(ItemSubTypes.Hat);
				if (inventoryItemGameObject2 != null)
				{
					list.Add(new CustomizationPages.GenderDependentRecord(CustomizedParts.Hat, TPMCharacterCustomization.LoadPrefabByPath(inventoryItemGameObject2.MaleAsset3rdPerson)));
					list2.Add(new CustomizationPages.GenderDependentRecord(CustomizedParts.Hat, TPMCharacterCustomization.LoadPrefabByPath(inventoryItemGameObject2.FemaleAsset3rdPerson)));
				}
				Dictionary<string, string> settings = profile.Settings;
				if (settings.ContainsKey("TpmHeadType") && settings["TpmHairType"] != "0")
				{
					this._curFace = (Faces)Convert.ToInt32(settings["TpmHeadType"]);
					this._curHair = (Hair)Convert.ToInt32(settings["TpmHairType"]);
					this._curPants = (Pants)Convert.ToInt32(settings["TpmPantsType"]);
					this._curSkinColorType = (SkinColor)((!settings.ContainsKey("TpmSkinColorA")) ? 3 : Convert.ToInt32(settings["TpmSkinColorA"]));
					this._curEyeColorType = (EyeColor)((!settings.ContainsKey("TpmEyeColorA")) ? 2 : Convert.ToInt32(settings["TpmEyeColorA"]));
				}
			}
			list.Add(new CustomizationPages.GenderDependentRecord(CustomizedParts.Shirt, gameObject));
			list.Add(new CustomizationPages.GenderDependentRecord(CustomizedParts.Boots, this._settings.GetShoes(Gender.Male, this._maleShoes)));
			list.Add(new CustomizationPages.GenderDependentRecord(CustomizedParts.Hands, TPMCharacterCustomization.LoadPrefabByPath(this._settings.GetHands(Gender.Male, handLength).modelPath)));
			list2.Add(new CustomizationPages.GenderDependentRecord(CustomizedParts.Shirt, gameObject2));
			list2.Add(new CustomizationPages.GenderDependentRecord(CustomizedParts.Boots, this._settings.GetShoes(Gender.Female, this._femaleShoes)));
			list2.Add(new CustomizationPages.GenderDependentRecord(CustomizedParts.Hands, TPMCharacterCustomization.LoadPrefabByPath(this._settings.GetHands(Gender.Female, handLength2).modelPath)));
			for (int m = 0; m < this._settings.Heads.Length; m++)
			{
				this._facesMap[this._settings.Heads[m].face] = this._settings.Heads[m];
			}
			for (int n = 0; n < this._settings.Hairs.Length; n++)
			{
				this._hairsMap[this._settings.Hairs[n].hair] = this._settings.Hairs[n];
			}
			for (int num = 0; num < this._settings.Pants.Length; num++)
			{
				this._pantsMap[this._settings.Pants[num].pants] = this._settings.Pants[num];
			}
		}

		private void RefreshUINavigation()
		{
			this.Navigation.ForceUpdate();
		}

		protected override void Start()
		{
			base.Start();
			for (int i = 0; i < base.Pages.Count; i++)
			{
				GameObject gameObject = base.Pages[i].gameObject;
				if (gameObject.GetComponent<GenderPage>() != null)
				{
					this._genderPage = gameObject.GetComponent<GenderPage>();
					this._genderPage.EGenderChanged += this.OnGenderChanged;
					this._genderPage.OnShow.AddListener(new UnityAction(this.RefreshUINavigation));
				}
				else if (gameObject.GetComponent<FacePage>() != null)
				{
					this._facePage = gameObject.GetComponent<FacePage>();
					this._facePage.EFaceChanged += this.OnFaceChanged;
					this._facePage.ESkinColorChanged += this.OnSkinColorChanged;
					this._facePage.EEyeColorChanged += this.OnEyeColorChanged;
					this._facePage.OnShow.AddListener(new UnityAction(this.RefreshUINavigation));
				}
				else if (gameObject.GetComponent<HairPage>() != null)
				{
					this._hairPage = gameObject.GetComponent<HairPage>();
					this._hairPage.EHairChanged += this.OnHairChanged;
					this._hairPage.OnShow.AddListener(new UnityAction(this.RefreshUINavigation));
				}
				else if (gameObject.GetComponent<PantsPage>() != null)
				{
					this._pantsPage = gameObject.GetComponent<PantsPage>();
					this._pantsPage.EPantsChanged += this.OnPantsChanged;
					this._pantsPage.OnShow.AddListener(new UnityAction(this.RefreshUINavigation));
				}
			}
			Gender gender = TPMCharacterCustomization.Instance.GetGender(this._curFace);
			this._facePage.Init((gender != Gender.Male) ? this._defaultMaleFace : this._curFace, (gender != Gender.Female) ? this._defaultFemaleFace : this._curFace, this._curSkinColorType, this._curEyeColorType);
			this._hairPage.Init((gender != Gender.Male) ? this._defaultMaleHair : this._curHair, (gender != Gender.Female) ? this._defaultFemaleHair : this._curHair);
			this._pantsPage.Init((gender != Gender.Male) ? this._defaultMalePants : this._curPants, (gender != Gender.Female) ? this._defaultFemalePants : this._curPants);
			this._curSkinColor = this._facePage.GetSelectedSkinColor();
			this._curEyeColor = this._facePage.GetSelectedEyeColor();
			this._genderPage.Init(gender);
		}

		private InventoryItem GetInventoryItemGameObject(ItemSubTypes itemType)
		{
			return PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && x.ItemSubType == itemType);
		}

		private void OnGenderChanged(Gender gender)
		{
			this._viewRoot.position = new Vector3(0f, -1000f, 0f);
			this._isIgnorAnimatorRebindState = true;
			List<CustomizationPages.GenderDependentRecord> list = this._genderDependentPrefabs[(int)gender];
			for (int i = 0; i < list.Count; i++)
			{
				this.ChangeLayerModel(list[i].viewLayer, list[i].model, gender);
			}
			CustomizationPages._curGender = gender;
			CustomizationPages.EGenderChanged(gender);
			this._isIgnorAnimatorRebindState = false;
			this.SyncAllAnimations(true);
			base.StartCoroutine(this.ShowCharacter());
		}

		private IEnumerator ShowCharacter()
		{
			yield return new WaitForSeconds(0.2f);
			this._viewRoot.position = this._rootPosition;
			yield break;
		}

		private void ChangeLayerModel(CustomizedParts layer, GameObject pModel, Gender gender)
		{
			Animator animator = this._viewLayers[layer];
			if (animator.transform.childCount == 1)
			{
				Transform child = animator.transform.GetChild(0);
				child.SetParent(null);
				Object.Destroy(child.gameObject);
			}
			else if (animator.transform.childCount > 1)
			{
				LogHelper.Error("Unexpected case when {0} children found", new object[] { animator.transform.childCount });
			}
			if (pModel == null)
			{
				return;
			}
			GameObject gameObject = Object.Instantiate<GameObject>(pModel);
			gameObject.SetActive(true);
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				Transform child2 = gameObject.transform.GetChild(i);
				child2.gameObject.layer = 13;
				SkinnedMeshRenderer component = child2.GetComponent<SkinnedMeshRenderer>();
				if (component != null)
				{
					component.sharedMesh = Object.Instantiate<Mesh>(component.sharedMesh);
				}
			}
			Transform transform = new GameObject("Root").transform;
			transform.parent = gameObject.transform;
			transform.localPosition = Vector3.zero;
			Transform transform2 = gameObject.transform.Find("Hips");
			if (transform2 != null)
			{
				transform2.parent = transform;
			}
			animator.avatar = ((gender != Gender.Male) ? this._femaleAvatar : this._maleAvatar);
			animator.SetBool("isMale", gender == Gender.Male);
			gameObject.transform.SetParent(animator.transform, false);
			gameObject.transform.localPosition = Vector3.zero;
		}

		private void OnHairChanged(Hair hair)
		{
			this.ChangeLayerModel(CustomizedParts.Hair, TPMCharacterCustomization.LoadPrefabByPath(this._hairsMap[hair].modelPath), CustomizationPages._curGender);
			this._curHair = hair;
			this.SyncAllAnimations(false);
		}

		private void OnPantsChanged(Pants pants)
		{
			this.ChangeLayerModel(CustomizedParts.Pants, this._settings.GetPants(this._pantsMap[pants].pants, (CustomizationPages._curGender != Gender.Female) ? this._maleShoes : this._femaleShoes), CustomizationPages._curGender);
			this._curPants = pants;
			this.SyncAllAnimations(false);
		}

		private void SyncAllAnimations(bool resetPosition = false)
		{
			if (this._isIgnorAnimatorRebindState)
			{
				return;
			}
			AnimatorStateInfo currentAnimatorStateInfo = this._viewLayers.Values.ElementAt(0).GetCurrentAnimatorStateInfo(0);
			float normalizedTime = currentAnimatorStateInfo.normalizedTime;
			foreach (Animator animator in this._viewLayers.Values)
			{
				animator.Rebind();
				animator.SetBool("isMale", CustomizationPages._curGender == Gender.Male);
				animator.Play(currentAnimatorStateInfo.fullPathHash, 0, normalizedTime);
				if (resetPosition)
				{
					animator.transform.localPosition = Vector3.zero;
					animator.transform.localRotation = Quaternion.identity;
				}
			}
		}

		private void OnEyeColorChanged(EyeColor colorType, Color color)
		{
			this.SetLayerModelColor(CustomizedParts.Head, ColorGroup.EYES, color);
			this._curEyeColorType = colorType;
			this._curEyeColor = color;
		}

		private void OnSkinColorChanged(SkinColor colorType, Color color)
		{
			color = TPMCharacterCustomization.ChangeSkinColorRange(color);
			this.SetLayerModelColor(CustomizedParts.Head, ColorGroup.SKIN, color);
			this.SetLayerModelColor(CustomizedParts.Hands, ColorGroup.SKIN, color);
			this._curSkinColorType = colorType;
			this._curSkinColor = color;
		}

		private void OnFaceChanged(Faces face)
		{
			this.ChangeLayerModel(CustomizedParts.Head, TPMCharacterCustomization.LoadPrefabByPath(this._facesMap[face].headModelPath), CustomizationPages._curGender);
			this._curFace = face;
			this.SetLayerModelColor(CustomizedParts.Head, ColorGroup.SKIN, this._curSkinColor);
			this.SetLayerModelColor(CustomizedParts.Head, ColorGroup.EYES, this._curEyeColor);
			this.SetLayerModelColor(CustomizedParts.Hands, ColorGroup.SKIN, this._curSkinColor);
			this.SyncAllAnimations(false);
		}

		private void SetLayerModelColor(CustomizedParts layer, ColorGroup colorGroup, Color color)
		{
			Animator animator = this._viewLayers[layer];
			Transform child = animator.transform.GetChild(0);
			ModelColorSettings component = child.GetComponent<ModelColorSettings>();
			if (component != null)
			{
				SkinnedMeshRenderer renderer = component.GetRenderer(colorGroup);
				if (renderer != null)
				{
					TPMCharacterCustomization.SetMeshColor(renderer, color);
				}
				else
				{
					LogHelper.Error("Can't find SkinnedMeshRenderer in {0} color group for {1} model", new object[] { colorGroup, child });
				}
			}
			else
			{
				LogHelper.Error("Can't find ModelColorSettings component in {0} model", new object[] { child });
			}
		}

		public void OnFinishCustomization()
		{
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			if (profile != null)
			{
				Dictionary<string, string> settings = profile.Settings;
				Dictionary<string, string> dictionary = settings;
				string text = "TpmHeadType";
				int curFace = (int)this._curFace;
				dictionary[text] = curFace.ToString();
				Dictionary<string, string> dictionary2 = settings;
				string text2 = "TpmHairType";
				int curHair = (int)this._curHair;
				dictionary2[text2] = curHair.ToString();
				Dictionary<string, string> dictionary3 = settings;
				string text3 = "TpmPantsType";
				int curPants = (int)this._curPants;
				dictionary3[text3] = curPants.ToString();
				settings["TpmSkinColorR"] = this._curSkinColor.r.ToString();
				settings["TpmSkinColorG"] = this._curSkinColor.g.ToString();
				settings["TpmSkinColorB"] = this._curSkinColor.b.ToString();
				Dictionary<string, string> dictionary4 = settings;
				string text4 = "TpmSkinColorA";
				int curSkinColorType = (int)this._curSkinColorType;
				dictionary4[text4] = curSkinColorType.ToString();
				settings["TpmEyeColorR"] = this._curEyeColor.r.ToString();
				settings["TpmEyeColorG"] = this._curEyeColor.g.ToString();
				settings["TpmEyeColorB"] = this._curEyeColor.b.ToString();
				Dictionary<string, string> dictionary5 = settings;
				string text5 = "TpmEyeColorA";
				int curEyeColorType = (int)this._curEyeColorType;
				dictionary5[text5] = curEyeColorType.ToString();
				PhotonConnectionFactory.Instance.UpdateProfileSettings(profile.Settings);
			}
			base.StartCoroutine(this.CloseDelayed());
		}

		public void OnCancelCustomization()
		{
			base.StartCoroutine(this.CloseDelayed());
		}

		private IEnumerator CloseDelayed()
		{
			yield return new WaitForEndOfFrame();
			if (this.CloseAction != null)
			{
				this.CloseAction();
			}
			yield break;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			InputModuleManager.OnInputTypeChanged -= this.RefreshHelpText;
			CustomizationPages.EGenderChanged -= this.OnGenderChanged;
			if (this._genderPage != null)
			{
				this._genderPage.OnShow.RemoveListener(new UnityAction(this.RefreshUINavigation));
				this._genderPage.EGenderChanged -= this.OnGenderChanged;
				this._genderPage = null;
			}
			if (this._facePage != null)
			{
				this._facePage.OnShow.RemoveListener(new UnityAction(this.RefreshUINavigation));
				this._facePage.EFaceChanged -= this.OnFaceChanged;
				this._facePage.ESkinColorChanged -= this.OnSkinColorChanged;
				this._facePage.EEyeColorChanged -= this.OnEyeColorChanged;
				this._facePage = null;
			}
			if (this._hairPage != null)
			{
				this._hairPage.OnShow.RemoveListener(new UnityAction(this.RefreshUINavigation));
				this._hairPage.EHairChanged -= this.OnHairChanged;
				this._hairPage = null;
			}
			if (this._pantsPage != null)
			{
				this._pantsPage.OnShow.RemoveListener(new UnityAction(this.RefreshUINavigation));
				this._pantsPage.EPantsChanged -= this.OnPantsChanged;
				this._pantsPage = null;
			}
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(1))
			{
				this._prevCursorX = Input.mousePosition.x;
			}
			else if (Input.GetMouseButton(1))
			{
				float num = Input.mousePosition.x - this._prevCursorX;
				int num2 = ((num <= 0f) ? 1 : (-1));
				this._viewRoot.Rotate(Vector3.up, (float)num2 * this._rotationSettings.CalcSpeedFromCursorMovement(num) * Time.deltaTime);
				this._prevCursorX = Input.mousePosition.x;
			}
			if (InputManager.ActiveDevice.GetControl(InputControlType.RightStickLeft).Value > 0f)
			{
				this._viewRoot.Rotate(Vector3.up, this._rotationSettings.MinSpeed * Time.deltaTime);
			}
			if (InputManager.ActiveDevice.GetControl(InputControlType.RightStickRight).Value > 0f)
			{
				this._viewRoot.Rotate(Vector3.up, -this._rotationSettings.MinSpeed * Time.deltaTime);
			}
		}

		private void LateUpdate()
		{
			if (this._nextUpdateAt < Time.time)
			{
				this._nextUpdateAt = Time.time + 30f;
				foreach (Animator animator in this._viewLayers.Values)
				{
					animator.transform.localPosition = new Vector3(animator.transform.localPosition.x, 0f, animator.transform.localPosition.z);
				}
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static CustomizationPages()
		{
			CustomizationPages.EGenderChanged = delegate
			{
			};
		}

		public UINavigation Navigation;

		private const int CUSTOMIZATION_LAYER_INDEX = 13;

		private static Gender _curGender = Gender.Male;

		public CustomizationPages.CloseActionDelegate CloseAction;

		[SerializeField]
		private TPMCharacterCustomization _settings;

		[SerializeField]
		private Shirts _maleShirt;

		[SerializeField]
		private Shirts _femaleShirt;

		[SerializeField]
		private Shoes _maleShoes;

		[SerializeField]
		private Shoes _femaleShoes;

		[SerializeField]
		private Faces _defaultMaleFace;

		[SerializeField]
		private Faces _defaultFemaleFace;

		[SerializeField]
		private Hair _defaultMaleHair;

		[SerializeField]
		private Hair _defaultFemaleHair;

		[SerializeField]
		private Pants _defaultMalePants;

		[SerializeField]
		private Pants _defaultFemalePants;

		[SerializeField]
		private SkinColor _curSkinColorType;

		[SerializeField]
		private EyeColor _curEyeColorType;

		[SerializeField]
		private Transform _viewRoot;

		[SerializeField]
		private CustomizationPages.CharacterRotationSettings _rotationSettings;

		[SerializeField]
		private CustomizationPages.ButtonGroup[] _buttonGroups = new CustomizationPages.ButtonGroup[2];

		[SerializeField]
		private CustomizationPages.Modes _curMode;

		[SerializeField]
		private Avatar _maleAvatar;

		[SerializeField]
		private Avatar _femaleAvatar;

		private List<CustomizationPages.GenderDependentRecord>[] _genderDependentPrefabs = new List<CustomizationPages.GenderDependentRecord>[2];

		private Dictionary<CustomizedParts, Animator> _viewLayers = new Dictionary<CustomizedParts, Animator>();

		private Dictionary<Faces, HeadRecord> _facesMap = new Dictionary<Faces, HeadRecord>();

		private Dictionary<Hair, HairRecord> _hairsMap = new Dictionary<Hair, HairRecord>();

		private Dictionary<Pants, PantsRecord> _pantsMap = new Dictionary<Pants, PantsRecord>();

		private Faces _curFace;

		private Hair _curHair;

		private Pants _curPants;

		private Color _curSkinColor;

		private Color _curEyeColor;

		private Color _curHairColor;

		private GenderPage _genderPage;

		private HairPage _hairPage;

		private FacePage _facePage;

		private PantsPage _pantsPage;

		private bool _isIgnorAnimatorRebindState;

		private HotkeyBinding _selectBinding;

		private Vector3 _rootPosition;

		public TextMeshProUGUI HelpText;

		private float _prevCursorX;

		private float _nextUpdateAt = -1f;

		private struct GenderDependentRecord
		{
			public GenderDependentRecord(CustomizedParts viewLayer, GameObject obj)
			{
				this.viewLayer = viewLayer;
				this.model = obj;
			}

			public CustomizedParts viewLayer;

			public GameObject model;
		}

		[Serializable]
		public class CharacterRotationSettings
		{
			public float MinSpeed
			{
				get
				{
					return this._minSpeed;
				}
			}

			public float CalcSpeedFromCursorMovement(float dx)
			{
				dx = Math.Abs(dx);
				if ((double)dx < 0.01)
				{
					return 0f;
				}
				float num = Math.Min(Math.Abs(dx) / this._cursorDxForMaxSpeed, 1f);
				return this._minSpeed * (1f - num) + this._maxSpeed * num;
			}

			[SerializeField]
			private float _minSpeed;

			[SerializeField]
			private float _maxSpeed;

			[SerializeField]
			private float _cursorDxForMaxSpeed;
		}

		public enum Modes
		{
			FISRT_CUSTOMIZATION,
			RECUSTOMIZATION
		}

		[Serializable]
		public class ButtonGroup
		{
			public GameObject[] buttons;

			public CustomizationPages.Modes mode;
		}

		public delegate void GenderChangedDelegate(Gender gender);

		public delegate void CloseActionDelegate();
	}
}
