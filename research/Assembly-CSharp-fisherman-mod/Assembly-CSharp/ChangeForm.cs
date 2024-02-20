using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class ChangeForm : MonoBehaviour
{
	public virtual void ChangeWithHideDashboard(ActivityState form)
	{
		if (base.GetComponent<Toggle>() != null && !base.GetComponent<Toggle>().isOn)
		{
			return;
		}
		if (StaticUserData.CurrentForm != null && form != StaticUserData.CurrentForm)
		{
			this._helpers.ChangeFormAction(StaticUserData.CurrentForm.gameObject, form.gameObject, StaticUserData.IsShowDashboard, false);
		}
		form.isActive = true;
		form.Show(false);
	}

	public static bool IsChangeOverrriden
	{
		get
		{
			return ChangeForm.OnChange != null;
		}
	}

	public virtual void Change(ActivityState form, bool immediate = false)
	{
		if (ChangeForm.OnChange != null)
		{
			ChangeForm.OnChange(form, this);
		}
		else
		{
			if (base.GetComponent<Toggle>() != null && !base.GetComponent<Toggle>().isOn)
			{
				return;
			}
			if (StaticUserData.CurrentForm != null && form != StaticUserData.CurrentForm)
			{
				this._helpers.ChangeFormAction(StaticUserData.CurrentForm, form, true, immediate);
			}
			else
			{
				form.Show(immediate);
			}
		}
	}

	public void ClearOnChange()
	{
		ChangeForm.OnChange = null;
	}

	public virtual void ChangeWithShowDashboard(ActivityState form)
	{
		if (base.GetComponent<Toggle>() != null && !base.GetComponent<Toggle>().isOn)
		{
			return;
		}
		if (StaticUserData.CurrentForm != null && form != StaticUserData.CurrentForm)
		{
			if (!StaticUserData.IsShowDashboard)
			{
				this._helpers.MenuPrefabsList.topMenuFormAS.Show(false);
			}
			if (this._helpers.MenuPrefabsList.helpPanel != null)
			{
				this._helpers.MenuPrefabsList.helpPanelAS.Show(false);
			}
			this._helpers.ChangeFormAction(StaticUserData.CurrentForm.gameObject, form.gameObject, true, false);
		}
		form.Show(false);
	}

	private MenuHelpers _helpers = new MenuHelpers();

	public static Action<ActivityState, ChangeForm> OnChange;
}
