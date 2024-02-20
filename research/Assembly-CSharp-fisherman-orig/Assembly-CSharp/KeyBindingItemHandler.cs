using System;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.Common;
using I2.Loc;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindingItemHandler : MonoBehaviour
{
	public ControlsActionsCategories Category { get; private set; }

	public bool HasBindings { get; private set; }

	public void Init(ControlsActionsCategories c, string titleLocId)
	{
		this.Category = c;
		this._title.text = ScriptLocalization.Get(titleLocId).ToUpper();
	}

	public void Init(CustomPlayerAction action)
	{
		this.HasBindings = true;
		this._bb.PointerEnter += delegate(bool b)
		{
			Text keyPrimary = this._keyPrimary;
			Font font = ((!b) ? this._normalFont : this._selectedFont);
			this._mouse.font = font;
			font = font;
			this._keySecondary.font = font;
			keyPrimary.font = font;
		};
		this._action = action;
		this.Category = action.Category;
		this._title.text = ScriptLocalization.Get(action.LocalizationKey);
		if (string.IsNullOrEmpty(this._title.text))
		{
			LogHelper.Error("___kocha KeyBindingItemHandler:Init - no localization for: {0}", new object[] { action.Name });
		}
		this.OnBindingsChanged();
	}

	public void OnBindingsChanged()
	{
		Dictionary<BindingSourceHelper.BindingType, List<BindingSource>> bindings = BindingSourceHelper.GetBindings(new BindingSourceHelper.BindingType[]
		{
			BindingSourceHelper.BindingType.Keyboard,
			BindingSourceHelper.BindingType.Mouse
		}, this._action.Bindings);
		BindingSource bindingSource;
		BindingSource bindingSource2;
		BindingSourceHelper.GetPrimaryAndSecondaryBindingSource(bindings[BindingSourceHelper.BindingType.Keyboard], out bindingSource, out bindingSource2);
		this._keyPrimary.text = ((!(bindingSource != null)) ? string.Empty : bindingSource.Name);
		this._keySecondary.text = ((!(bindingSource2 != null)) ? string.Empty : bindingSource2.Name);
		this._mouse.text = ((bindings[BindingSourceHelper.BindingType.Mouse].Count <= 0) ? string.Empty : bindings[BindingSourceHelper.BindingType.Mouse][0].Name);
	}

	[SerializeField]
	private Text _title;

	[SerializeField]
	private Text _keyPrimary;

	[SerializeField]
	private Text _keySecondary;

	[SerializeField]
	private Text _mouse;

	[SerializeField]
	private BorderedButton _bb;

	[SerializeField]
	private Font _selectedFont;

	[SerializeField]
	private Font _normalFont;

	private CustomPlayerAction _action;
}
