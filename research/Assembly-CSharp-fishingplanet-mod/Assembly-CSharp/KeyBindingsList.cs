using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindingsList : MonoBehaviour
{
	private void OnEnable()
	{
		KeyBindingsList.panelActive = true;
	}

	private void OnDisable()
	{
		KeyBindingsList.panelActive = false;
	}

	public void LoadList()
	{
		this.ClearList();
		FieldInfo[] fields = typeof(ControlsActions).GetFields();
		this._actionsCount = 0;
		bool flag = true;
		foreach (FieldInfo fieldInfo in fields)
		{
			bool flag2 = Attribute.IsDefined(fieldInfo, typeof(ControlsActionAttribute));
			if (flag2)
			{
				this._actionsCount++;
				GameObject gameObject = GUITools.AddChild(base.gameObject, this.KeyBindingListItemPrefab);
				gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
				gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
				gameObject.GetComponent<Image>().enabled = flag;
				flag = !flag;
				object value = ControlsController.ControlsActions.GetType().GetField(fieldInfo.Name).GetValue(ControlsController.ControlsActions);
				CustomPlayerAction customPlayerAction = (CustomPlayerAction)value;
				gameObject.GetComponent<KeyBindingListItemInit>().Init(customPlayerAction);
			}
		}
		this.SetSizeForContent();
	}

	public void ClearList()
	{
		IEnumerator enumerator = base.gameObject.GetComponent<Transform>().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				Object.Destroy(transform.gameObject);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
	}

	private void SetSizeForContent()
	{
		float num = (float)(base.gameObject.GetComponent<VerticalLayoutGroup>().padding.top + base.gameObject.GetComponent<VerticalLayoutGroup>().padding.bottom) + base.gameObject.GetComponent<VerticalLayoutGroup>().spacing * (float)this._actionsCount + this.KeyBindingListItemPrefab.GetComponent<LayoutElement>().minHeight * (float)this._actionsCount + 1f;
		base.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(base.gameObject.GetComponent<RectTransform>().rect.width, num);
		if (base.gameObject.GetComponent<RectTransform>().rect.height > this.MaskContent.GetComponent<RectTransform>().rect.height)
		{
			this.ScrollBar.gameObject.SetActive(true);
			this.MaskContent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
		}
	}

	public GameObject KeyBindingListItemPrefab;

	private int _actionsCount;

	public GameObject ScrollBar;

	public GameObject MaskContent;

	public static bool panelActive;
}
