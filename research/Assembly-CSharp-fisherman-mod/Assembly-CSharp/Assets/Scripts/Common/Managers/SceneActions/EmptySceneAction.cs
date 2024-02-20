using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

namespace Assets.Scripts.Common.Managers.SceneActions
{
	public class EmptySceneAction : ISceneAction
	{
		public void Action(SceneStatuses status, MonoBehaviour sender, object parameters = null)
		{
			GameObject firstForm = this.helpers.MenuPrefabsList.firstForm;
			if (status != SceneStatuses.Empty)
			{
				if (status == SceneStatuses.GoToStart)
				{
					this.helpers.MenuPrefabsList.startFormAS.GetComponent<StartFormInit>().Init();
					this.helpers.HideForm(firstForm, false);
					this.helpers.ShowForm(this.helpers.MenuPrefabsList.startFormAS, false);
				}
			}
			else
			{
				this.helpers.ShowForm(firstForm, false);
			}
		}

		private MenuHelpers helpers = new MenuHelpers();
	}
}
