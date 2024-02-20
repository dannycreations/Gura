using System;
using System.Diagnostics;
using UnityEngine;

public class ShowConverter : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnActive = delegate(bool b)
	{
	};

	public void ShowConverterClick()
	{
		this._messageBox = GUITools.AddChild(MessageBoxList.Instance.gameObject, MessageBoxList.Instance.converterPrefab);
		this._messageBox.GetComponent<AlphaFade>().HideFinished += this.ShowConverter_HideFinished;
		this._messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this._messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this._messageBox.GetComponent<ConverterHandler>().Init();
		this._messageBox.GetComponent<AlphaFade>().ShowPanel();
		this.OnActive(true);
	}

	private void ShowConverter_HideFinished(object sender, EventArgsAlphaFade e)
	{
		Object.Destroy(this._messageBox);
		this.OnActive(false);
	}

	private GameObject _messageBox;
}
