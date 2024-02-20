using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class SearchAction : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<string> OnFindItem = delegate(string s)
	{
	};

	public void SearchClick(Text text)
	{
		this.OnFindItem(text.text);
	}

	public void ClearFieldAndDeselect(InputField inpField)
	{
		inpField.text = string.Empty;
		UINavigation.SetSelectedGameObject(null);
	}

	public void SelectInputField(InputField inpField)
	{
		UINavigation.SetSelectedGameObject(inpField.gameObject);
	}

	public void ClearFilters()
	{
		this._inputField.text = string.Empty;
	}

	[SerializeField]
	private InputField _inputField;
}
