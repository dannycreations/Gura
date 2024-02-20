using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class InputFieldInScrollRectFixer : MonoBehaviour
	{
		private IEnumerator Start()
		{
			this._InputField = base.GetComponent<InputField>();
			GameObject go = new GameObject("InputFieldFixer");
			RectTransform goRT = go.AddComponent<RectTransform>();
			goRT.SetParent(this._InputField.transform, false);
			goRT.SetAsLastSibling();
			goRT.anchorMin = Vector2.zero;
			goRT.anchorMax = Vector2.one;
			goRT.sizeDelta = Vector2.zero;
			Image image = go.AddComponent<Image>();
			image.color = Color.clear;
			this._Button = go.AddComponent<Button>();
			this._Button.transition = 0;
			this._Button.onClick.AddListener(delegate
			{
				this._InputField.enabled = true;
				this._InputField.ActivateInputField();
				this._Button.gameObject.SetActive(false);
			});
			this._InputField.onEndEdit.AddListener(delegate(string _)
			{
				this._InputField.enabled = false;
				this._Button.gameObject.SetActive(true);
			});
			yield return null;
			this._InputField.enabled = false;
			yield break;
		}

		private InputField _InputField;

		private Button _Button;
	}
}
