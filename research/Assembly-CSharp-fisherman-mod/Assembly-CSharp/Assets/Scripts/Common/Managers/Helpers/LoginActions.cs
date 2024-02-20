using System;

namespace Assets.Scripts.Common.Managers.Helpers
{
	public class LoginActions
	{
		public void LoadingPlayerStartGameAction(ActivityState currentForm)
		{
			if (this.helpers.MenuPrefabsList.loadingForm.GetComponent<LoadProfileAndGotoGame>() == null)
			{
				ActivityState loadingFormAS = this.helpers.MenuPrefabsList.loadingFormAS;
				loadingFormAS.gameObject.AddComponent<LoadProfileAndGotoGame>().BaseForm = this.helpers.MenuPrefabsList.loadingForm;
				this.helpers.ChangeFormAction(currentForm, loadingFormAS, true, false);
			}
		}

		public void StartGameAction()
		{
			if (this.helpers.MenuPrefabsList.startForm.GetComponent<LoadProfileAndGotoGame>() == null)
			{
				this.helpers.MenuPrefabsList.startForm.AddComponent<LoadProfileAndGotoGame>().BaseForm = this.helpers.MenuPrefabsList.startForm;
			}
		}

		private MenuHelpers helpers = new MenuHelpers();
	}
}
