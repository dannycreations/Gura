using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
{
	protected virtual void Start()
	{
	}

	private void Update()
	{
		if (this._context != null)
		{
			return;
		}
		if (this._isPointered)
		{
			if (this._currentTime >= this.ShowDelay)
			{
				this.ShowPopup();
			}
			else
			{
				this._currentTime += Time.deltaTime;
			}
		}
	}

	private void ShowPopup()
	{
		GameObject gameObject = GameObject.Find(this.PopupParentName);
		GameObject gameObject2 = (GameObject)Resources.Load(this.PanelPrefabName, typeof(GameObject));
		if (gameObject == null || gameObject2 == null)
		{
			return;
		}
		this._context = GUITools.AddChild(gameObject, gameObject2);
		Vector3 position = base.transform.position;
		Vector3 vector = gameObject.GetComponent<RectTransform>().InverseTransformPoint(position) - new Vector3(0f, this._context.GetComponent<RectTransform>().sizeDelta.y, 0f);
		if (!base.GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.Contains(vector - new Vector3(0f, this._context.GetComponent<RectTransform>().sizeDelta.y, 0f)))
		{
			vector += 2f * new Vector3(0f, this._context.GetComponent<RectTransform>().sizeDelta.y, 0f);
		}
		Vector3 popupDirectionPivot = this.GetPopupDirectionPivot(Input.mousePosition, gameObject2);
		this._context.GetComponent<RectTransform>().pivot = popupDirectionPivot;
		this._context.GetComponent<RectTransform>().localPosition = vector;
		this.SetValueToPanel(this._context);
	}

	protected virtual void SetValueToPanel(GameObject popupPanel)
	{
		Text component = popupPanel.transform.Find("Content").GetComponent<Text>();
		if (this.PopupValue.StartsWith("["))
		{
			component.text = ScriptLocalization.Get(this.PopupValue.Trim(new char[] { '[', ']' }));
		}
		else
		{
			component.text = this.PopupValue;
		}
		if (this.AutoResize)
		{
			popupPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(popupPanel.GetComponent<RectTransform>().sizeDelta.x, Mathf.Max(component.preferredHeight, popupPanel.GetComponent<RectTransform>().sizeDelta.y));
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this._isPointered = false;
		this._currentTime = 0f;
		Object.Destroy(this._context);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this._isPointered = true;
	}

	private Vector3 GetPopupDirectionPivot(Vector3 pointPosition, GameObject popupPanel)
	{
		Resolution currentResolution = Screen.currentResolution;
		float y = popupPanel.GetComponent<RectTransform>().sizeDelta.y;
		float x = popupPanel.GetComponent<RectTransform>().sizeDelta.x;
		if (pointPosition.y - y > 0f && pointPosition.x - x > 0f)
		{
			return new Vector3(1f, 1f, -2f);
		}
		if (pointPosition.y - y >= 0f && pointPosition.x + x <= (float)currentResolution.width)
		{
			return new Vector3(0f, 1f, 2f);
		}
		if (pointPosition.y + y <= (float)currentResolution.height && pointPosition.x - x >= 0f)
		{
			return new Vector3(1f, 0f, -2f);
		}
		if (pointPosition.y + y <= (float)currentResolution.height && pointPosition.x + x <= (float)currentResolution.width)
		{
			return new Vector3(0f, 0f, 2f);
		}
		return new Vector3(1f, 1f, -2f);
	}

	private void OnDisable()
	{
		this.OnPointerExit(null);
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (!this._isSelected && this.OnSelected != null)
		{
			this.OnSelected.Invoke();
		}
		this._isSelected = true;
		this.OnPointerEnter(null);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		this._isSelected = false;
		this.OnPointerExit(null);
	}

	public float ShowDelay = 1f;

	public string PanelPrefabName = "SimplePopupPrefab";

	public string PopupParentName;

	public string PopupValue = string.Empty;

	public bool AutoResize = true;

	private float _currentTime;

	private GameObject _context;

	private bool _isPointered;

	private bool _isSelected;

	public UnityEvent OnSelected;
}
