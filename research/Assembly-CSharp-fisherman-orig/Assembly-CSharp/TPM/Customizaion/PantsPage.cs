using System;
using System.Diagnostics;
using UnityEngine;

namespace TPM.Customizaion
{
	public class PantsPage : TabPage
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event PantsPage.PantsChangedDelegate EPantsChanged = delegate
		{
		};

		protected override void Awake()
		{
			base.Awake();
			this._pants[0] = this._malePants;
			this._pants[1] = this._femalePants;
			for (int i = 0; i < this._pants.Length; i++)
			{
				this._pants[i].Init(base.gameObject, false);
				this._pants[i].EToggleSelected += this.PantsChanged;
			}
			CustomizationPages.EGenderChanged += this.CustomizationPagesOnOnGenderChanged;
		}

		public void Init(Pants mpants, Pants fpants)
		{
			this._pants[0].SetInitialValue(mpants, true);
			this._pants[1].SetInitialValue(fpants, true);
		}

		private void CustomizationPagesOnOnGenderChanged(Gender gender)
		{
			this._pants[(int)(Gender.Female - gender)].SetActive(false);
			this._pants[(int)gender].SetActive(true);
		}

		private void PantsChanged(PantsPage.PantsToggleRecord obj)
		{
			this.EPantsChanged(obj.Type);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			for (int i = 0; i < this._pants.Length; i++)
			{
				this._pants[i].Destroy();
			}
			CustomizationPages.EGenderChanged -= this.CustomizationPagesOnOnGenderChanged;
		}

		[SerializeField]
		private PantsPage.PantsToggles _malePants;

		[SerializeField]
		private PantsPage.PantsToggles _femalePants;

		private PantsPage.PantsToggles[] _pants = new PantsPage.PantsToggles[2];

		[Serializable]
		public class PantsToggleRecord : ToggleGroupHandlerRecord<Pants>
		{
		}

		[Serializable]
		public class PantsToggles : ToggleGroupHandler<Pants, PantsPage.PantsToggleRecord>
		{
		}

		public delegate void PantsChangedDelegate(Pants pants);
	}
}
