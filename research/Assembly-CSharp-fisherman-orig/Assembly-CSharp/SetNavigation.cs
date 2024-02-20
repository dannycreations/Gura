using System;
using System.Collections.Generic;
using UnityEngine;

public class SetNavigation : MonoBehaviour
{
	public void SetNavigationSettings(bool setNew)
	{
		if (setNew)
		{
			for (int i = 0; i < this.NavigationSettings.Count; i++)
			{
				this.NavigationSettings[i].Selectable.navigation = this.NavigationSettings[i].Navigation;
			}
		}
	}

	public List<SelectableNavigation> NavigationSettings = new List<SelectableNavigation>();
}
