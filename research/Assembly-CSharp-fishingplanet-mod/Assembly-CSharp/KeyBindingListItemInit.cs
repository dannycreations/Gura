using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Common;
using I2.Loc;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindingListItemInit : MonoBehaviour
{
	public void Init(CustomPlayerAction action)
	{
		this.NameText.text = ScriptLocalization.Get(action.LocalizationKey);
		if (string.IsNullOrEmpty(this.NameText.text))
		{
			LogHelper.Error("___kocha KeyBindingListItemInit:Init - no localization for: {0}", new object[] { action.Name });
		}
		BindingSourceHelper.BindingType[] array = new BindingSourceHelper.BindingType[]
		{
			BindingSourceHelper.BindingType.Controller,
			BindingSourceHelper.BindingType.Keyboard,
			BindingSourceHelper.BindingType.Mouse
		};
		Dictionary<BindingSourceHelper.BindingType, List<BindingSource>> bindings = BindingSourceHelper.GetBindings(array, action.Bindings);
		BindingSource bindingSource;
		BindingSource bindingSource2;
		BindingSourceHelper.GetPrimaryAndSecondaryBindingSource(bindings[BindingSourceHelper.BindingType.Keyboard], out bindingSource, out bindingSource2);
		this.KeyPrimary.GetComponent<BindingInit>().Init(bindingSource, action, BindingSourceHelper.BindingType.Keyboard, 1);
		this.KeySecondary.GetComponent<BindingInit>().Init(bindingSource2, action, BindingSourceHelper.BindingType.Keyboard, 2);
		this.KeyMouse.GetComponent<BindingInit>().Init(bindings[BindingSourceHelper.BindingType.Mouse].FirstOrDefault<BindingSource>(), action, BindingSourceHelper.BindingType.Mouse, 1);
		this.KeyController.GetComponent<BindingInit>().Init(bindings[BindingSourceHelper.BindingType.Controller].FirstOrDefault<BindingSource>(), action, BindingSourceHelper.BindingType.Controller, 1);
	}

	public Text NameText;

	public GameObject KeyPrimary;

	public GameObject KeySecondary;

	public GameObject KeyMouse;

	public GameObject KeyController;
}
