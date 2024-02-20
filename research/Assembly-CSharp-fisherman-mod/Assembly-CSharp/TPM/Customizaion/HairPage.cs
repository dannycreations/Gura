using System;
using System.Diagnostics;
using UnityEngine;

namespace TPM.Customizaion
{
	public class HairPage : TabPage
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HairPage.HairChangedDelegate EHairChanged = delegate
		{
		};

		protected override void Awake()
		{
			base.Awake();
			this._hair[0] = this._maleHair;
			this._hair[1] = this._femaleHair;
			for (int i = 0; i < this._hair.Length; i++)
			{
				this._hair[i].Init(base.gameObject, false);
				this._hair[i].EToggleSelected += this.HairChanged;
			}
			CustomizationPages.EGenderChanged += this.CustomizationPagesOnOnGenderChanged;
		}

		public void Init(Hair mhair, Hair fhair)
		{
			this._hair[0].SetInitialValue(mhair, true);
			this._hair[1].SetInitialValue(fhair, true);
		}

		private void CustomizationPagesOnOnGenderChanged(Gender gender)
		{
			this._hair[(int)(Gender.Female - gender)].SetActive(false);
			this._hair[(int)gender].SetActive(true);
		}

		private void HairChanged(HairPage.HairToggleRecord obj)
		{
			this.EHairChanged(obj.Type);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			for (int i = 0; i < this._hair.Length; i++)
			{
				this._hair[i].Destroy();
			}
			CustomizationPages.EGenderChanged -= this.CustomizationPagesOnOnGenderChanged;
		}

		[SerializeField]
		private HairPage.HairToggles _maleHair;

		[SerializeField]
		private HairPage.HairToggles _femaleHair;

		private HairPage.HairToggles[] _hair = new HairPage.HairToggles[2];

		[Serializable]
		public class HairToggleRecord : ToggleGroupHandlerRecord<Hair>
		{
		}

		[Serializable]
		public class HairToggles : ToggleGroupHandler<Hair, HairPage.HairToggleRecord>
		{
		}

		public delegate void HairChangedDelegate(Hair hair);
	}
}
