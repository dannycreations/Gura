using System;
using System.Diagnostics;
using UnityEngine;

namespace TPM.Customizaion
{
	public class FacePage : TabPage
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event FacePage.FaceChangedDelegate EFaceChanged = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event FacePage.SkinColorChangedDelegate ESkinColorChanged = delegate
		{
		};

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event FacePage.EyeColorChangedDelegate EEyeColorChanged = delegate
		{
		};

		protected override void Awake()
		{
			base.Awake();
			this._faces[0] = this._maleFaces;
			this._faces[1] = this._femaleFaces;
			for (int i = 0; i < this._faces.Length; i++)
			{
				this._faces[i].Init(base.gameObject, false);
				this._faces[i].EToggleSelected += this.FaceChanged;
			}
			this._skinColors.Init(base.gameObject, false);
			this._skinColors.EToggleSelected += this.SkinColorChanged;
			this._eyeColors.Init(base.gameObject, false);
			this._eyeColors.EToggleSelected += this.EyeColorChanged;
			CustomizationPages.EGenderChanged += this.CustomizationPagesOnOnGenderChanged;
		}

		public void Init(Faces mface, Faces fface, SkinColor skinColor, EyeColor eyesColor)
		{
			this._faces[0].SetInitialValue(mface, true);
			this._faces[1].SetInitialValue(fface, true);
			this._skinColors.SetInitialValue(skinColor, true);
			this._eyeColors.SetInitialValue(eyesColor, true);
		}

		public Color GetSelectedSkinColor()
		{
			FacePage.SkinColorsToggleRecord toggleByValue = this._skinColors.GetToggleByValue();
			return toggleByValue.Color;
		}

		public Color GetSelectedEyeColor()
		{
			FacePage.EyeColorsToggleRecord toggleByValue = this._eyeColors.GetToggleByValue();
			return toggleByValue.Color;
		}

		private void CustomizationPagesOnOnGenderChanged(Gender gender)
		{
			this._faces[(int)(Gender.Female - gender)].SetActive(false);
			this._faces[(int)gender].SetActive(true);
		}

		private void FaceChanged(FacePage.FacesToggleRecord obj)
		{
			this.EFaceChanged(obj.Type);
		}

		private void SkinColorChanged(FacePage.SkinColorsToggleRecord obj)
		{
			this.ESkinColorChanged(obj.Type, obj.Color);
		}

		private void EyeColorChanged(FacePage.EyeColorsToggleRecord obj)
		{
			this.EEyeColorChanged(obj.Type, obj.Color);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			for (int i = 0; i < this._faces.Length; i++)
			{
				this._faces[i].Init(base.gameObject, true);
			}
			CustomizationPages.EGenderChanged -= this.CustomizationPagesOnOnGenderChanged;
		}

		[SerializeField]
		private FacePage.FacesToggles _maleFaces;

		[SerializeField]
		private FacePage.FacesToggles _femaleFaces;

		[SerializeField]
		private FacePage.SkinColorsToggles _skinColors;

		[SerializeField]
		private FacePage.EyeColorsToggles _eyeColors;

		private FacePage.FacesToggles[] _faces = new FacePage.FacesToggles[2];

		[Serializable]
		public class FacesToggleRecord : ToggleGroupHandlerRecord<Faces>
		{
		}

		[Serializable]
		public class FacesToggles : ToggleGroupHandler<Faces, FacePage.FacesToggleRecord>
		{
		}

		[Serializable]
		public class EyeColorsToggleRecord : ToggleGroupHandlerRecord<EyeColor>
		{
			public Color Color;
		}

		[Serializable]
		public class EyeColorsToggles : ToggleGroupHandler<EyeColor, FacePage.EyeColorsToggleRecord>
		{
		}

		[Serializable]
		public class SkinColorsToggleRecord : ToggleGroupHandlerRecord<SkinColor>
		{
			public Color Color;
		}

		[Serializable]
		public class SkinColorsToggles : ToggleGroupHandler<SkinColor, FacePage.SkinColorsToggleRecord>
		{
		}

		public delegate void FaceChangedDelegate(Faces face);

		public delegate void SkinColorChangedDelegate(SkinColor colorType, Color color);

		public delegate void EyeColorChangedDelegate(EyeColor colorType, Color color);
	}
}
