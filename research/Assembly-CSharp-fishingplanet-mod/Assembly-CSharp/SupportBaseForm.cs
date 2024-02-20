using System;
using UnityEngine;

public class SupportBaseForm : MonoBehaviour
{
	public void SetupWindow(SupportCategory type)
	{
		base.gameObject.SetActive(true);
		this._current = type;
		this.ClearForm();
	}

	public void Deactivate()
	{
		base.gameObject.SetActive(false);
	}

	protected virtual void ClearForm()
	{
	}

	protected SupportCategory _current;
}
