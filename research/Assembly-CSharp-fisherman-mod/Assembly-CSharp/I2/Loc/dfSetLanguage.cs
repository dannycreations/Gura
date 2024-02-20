using System;
using UnityEngine;

namespace I2.Loc
{
	public class dfSetLanguage : MonoBehaviour
	{
		private void OnClick()
		{
			if (LocalizationManager.HasLanguage(this.Language, true, true))
			{
				LocalizationManager.CurrentLanguage = this.Language;
			}
		}

		public string Language;
	}
}
