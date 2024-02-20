using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollInitDropDownList : ScrollInit
{
	public override void Awake()
	{
		base.Awake();
		this._controls = new GameObject[this.Content.Length];
		this.ContentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(this.PanelWidth, this.PanelHeight);
		this.ScrollBar.GetComponent<RectTransform>().sizeDelta = new Vector2(this.ScrollBar.GetComponent<RectTransform>().sizeDelta.x, this.PanelHeight);
		for (int i = 0; i < this.Content.Length; i++)
		{
			GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.ButtonInContentPrefab);
			gameObject.transform.Find("Label").GetComponent<Text>().text = this.Content[i];
			if (gameObject.GetComponent<ComboBoxContent>() == null)
			{
				gameObject.AddComponent<ComboBoxContent>();
			}
			gameObject.GetComponent<ComboBoxContent>().Index = i;
			this._controls[i] = gameObject;
			gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(this.OnClickAction));
		}
		this.ContentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(this.PanelWidth, (this.ButtonInContentPrefab.GetComponent<RectTransform>().sizeDelta.y + 5f) * (float)this.Content.Length);
		this._currentSelectedItem = this._controls[0];
		EventSystem.current.SetSelectedGameObject(this._currentSelectedItem);
	}

	public override void Update()
	{
		base.Update();
		if (ControlsController.ControlsActions.DropDownUp.WasPressedMandatory)
		{
			int num = this._currentSelectedItem.GetComponent<ComboBoxContent>().Index;
			num--;
			if (num < 0)
			{
				num = this.Content.Length - 1;
			}
			this._currentSelectedItem = this._controls[num];
			EventSystem.current.SetSelectedGameObject(this._currentSelectedItem);
		}
		if (ControlsController.ControlsActions.DropDownDown.WasPressedMandatory)
		{
			int num2 = this._currentSelectedItem.GetComponent<ComboBoxContent>().Index;
			num2++;
			if (num2 > this._controls.Length - 1)
			{
				num2 = 0;
			}
			this._currentSelectedItem = this._controls[num2];
			EventSystem.current.SetSelectedGameObject(this._currentSelectedItem);
		}
		if (this.MaskPanel.GetComponent<RectTransform>().position.y - base.GetComponent<RectTransform>().sizeDelta.y / 2f > this._currentSelectedItem.GetComponent<RectTransform>().position.y)
		{
			Debug.Log("Change scroll  up");
		}
		if (this.MaskPanel.GetComponent<RectTransform>().position.y + base.GetComponent<RectTransform>().sizeDelta.y / 2f < this._currentSelectedItem.GetComponent<RectTransform>().position.y)
		{
			Debug.Log("Position " + this.MaskPanel.GetComponent<RectTransform>().position.y);
			Debug.Log("Current Position " + this._currentSelectedItem.GetComponent<RectTransform>().position.y);
			Debug.Log("Heigth " + base.GetComponent<RectTransform>().sizeDelta.y);
			Debug.Log("Change scroll");
		}
	}

	private void OnClickAction()
	{
		Debug.Log("ClickAcction");
	}

	public string[] Content;

	public GameObject ButtonInContentPrefab;

	public float PanelHeight = 100f;

	public float PanelWidth = 100f;

	public ScrollInitDropDownList.DropDownListDirection DropDownDirection;

	private GameObject[] _controls;

	private GameObject _currentSelectedItem;

	public enum DropDownListDirection
	{
		Up,
		Down
	}
}
