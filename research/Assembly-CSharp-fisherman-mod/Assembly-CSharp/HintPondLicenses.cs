using System;
using UnityEngine;

public class HintPondLicenses : HintColorBase
{
	public override void SetObserver(ManagedHint observer, int id)
	{
		base.SetObserver(observer, id);
		GameObject gameObject = Object.Instantiate<GameObject>(this._shinePrefab, base.transform.parent);
		this._shines.Add(gameObject.GetComponent<RectTransform>());
	}
}
