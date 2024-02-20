using System;
using UnityEngine;

public class VisibleComponent : MonoBehaviour
{
	public void Show()
	{
		if (this._isEnabled)
		{
			base.gameObject.SetActive(true);
			if (this._changer != null)
			{
				this._changer.OnTransformChildrenChanged();
			}
		}
	}

	public void Hide()
	{
		if (this._isEnabled)
		{
			base.gameObject.SetActive(false);
		}
	}

	public void SetEnable(bool flag)
	{
		this._isEnabled = flag;
	}

	[SerializeField]
	private ChildrenChangedListener _changer;

	private bool _isEnabled = true;
}
