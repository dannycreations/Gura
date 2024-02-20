using System;
using UnityEngine;

public class HelpWindowHandlerController : HelpWindowHandlerBase
{
	protected override void Start()
	{
		GameObject gameObject = ((!SettingsManager.RightHandedLayout) ? this.XBoxControllerLeftHanded : this.XBoxController);
		GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, this._contentController.transform);
		gameObject2.SetActive(true);
		base.Start();
	}

	[SerializeField]
	private GameObject _contentController;

	[SerializeField]
	private GameObject PSController;

	[SerializeField]
	private GameObject XBoxController;

	[SerializeField]
	private GameObject PSControllerLeftHanded;

	[SerializeField]
	private GameObject XBoxControllerLeftHanded;
}
