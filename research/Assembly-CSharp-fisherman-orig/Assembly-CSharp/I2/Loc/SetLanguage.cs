using System;
using UnityEngine;

namespace I2.Loc
{
	[AddComponentMenu("I2/Localization/SetLanguage")]
	public class SetLanguage : MonoBehaviour
	{
		private void OnClick()
		{
			this.ApplyLanguage();
		}

		public void ApplyLanguage()
		{
			if (LocalizationManager.HasLanguage(this._Language, true, true))
			{
				LocalizationManager.CurrentLanguage = this._Language;
			}
		}

		public string _Language;
	}
}
