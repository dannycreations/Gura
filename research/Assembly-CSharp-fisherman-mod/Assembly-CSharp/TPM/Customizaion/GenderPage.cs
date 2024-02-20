using System;
using System.Diagnostics;
using UnityEngine;

namespace TPM.Customizaion
{
	public class GenderPage : TabPage
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event GenderPage.GenderChangedDelegate EGenderChanged = delegate
		{
		};

		protected override void Awake()
		{
			base.Awake();
			this._genders.Init(base.gameObject, false);
			this._genders.EToggleSelected += this.OnChangeGender;
		}

		public void Init(Gender gender)
		{
			this._genders.SetInitialValue(gender, false);
		}

		private void OnChangeGender(GenderToggleRecord obj)
		{
			this.EGenderChanged(obj.Type);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this._genders.EToggleSelected -= this.OnChangeGender;
			this._genders.Destroy();
		}

		[SerializeField]
		private GenderToggles _genders;

		public delegate void GenderChangedDelegate(Gender gender);
	}
}
