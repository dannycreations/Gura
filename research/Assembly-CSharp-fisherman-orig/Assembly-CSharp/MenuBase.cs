using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuBase : MonoBehaviour
{
	public bool IsActive { get; protected set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnActive = delegate
	{
	};

	public void FastHidePanel()
	{
		this.AlphaFade.FastHidePanel();
	}

	public void Activate(bool flag)
	{
		this.ActivePanel(flag);
	}

	protected virtual void CheckPanel()
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (EventSystem.current.currentSelectedGameObject == this.GetSelectedGameObject())
			{
				if (!this.IsActive)
				{
					this.ActivePanel(true);
				}
			}
			else if ((EventSystem.current.currentSelectedGameObject == null || this.IsAnySelected()) && this.IsActive)
			{
				this.ActivePanel(false);
			}
		}
	}

	protected virtual GameObject GetSelectedGameObject()
	{
		return null;
	}

	protected virtual bool IsAnySelected()
	{
		return true;
	}

	protected virtual void ActivePanel(bool flag)
	{
		this.IsActive = flag;
		this.OnActive(flag);
		base.GetComponent<UINavigation>().enabled = flag;
		if (flag)
		{
			this.AlphaFade.ShowPanel();
		}
		else
		{
			this.AlphaFade.HidePanel();
		}
	}

	[SerializeField]
	protected AlphaFade AlphaFade;
}
