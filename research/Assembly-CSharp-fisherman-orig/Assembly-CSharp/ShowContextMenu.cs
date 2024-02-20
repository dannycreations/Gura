using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ShowContextMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	private void Update()
	{
		if (ControlsController.ControlsActions.GetMouseButton(1) && this._isPointer && this._previouslyPosition != Input.mousePosition && RectTransformUtility.RectangleContainsScreenPoint(base.GetComponent<RectTransform>(), Input.mousePosition, MenuHelpers.Instance.GUICamera))
		{
			ContextMenuAction[] array = Object.FindObjectsOfType<ContextMenuAction>();
			foreach (ContextMenuAction contextMenuAction in array)
			{
				contextMenuAction.GetComponent<ContextMenuAction>().Hide();
			}
			this._previouslyPosition = Input.mousePosition;
			this.Show();
		}
	}

	protected virtual void Show()
	{
		this._context = GUITools.AddChild(this.RootPanel, this.ContextMenuPrefab);
		this._context.GetComponent<RectTransform>().sizeDelta = this.ContentPrefab.GetComponent<RectTransform>().sizeDelta;
		this._content = GUITools.AddChild(this._context, this.ContentPrefab);
		this._content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		ContextMenuAction component = this._context.GetComponent<ContextMenuAction>();
		Vector2 vector;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(this.RootPanel.GetComponent<RectTransform>(), Input.mousePosition, MenuHelpers.Instance.GUICamera, ref vector);
		Vector3 vector2 = vector;
		vector2.z = 0f;
		this._context.GetComponent<RectTransform>().anchoredPosition = vector2;
		component.Show(vector2);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this._isPointer = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this._isPointer = true;
	}

	public virtual GameObject ContentPrefab
	{
		get
		{
			return new GameObject();
		}
	}

	public GameObject ContextMenuPrefab;

	public GameObject RootPanel;

	private bool _isPointer;

	protected GameObject _context;

	protected GameObject _content;

	private Vector3 _previouslyPosition = new Vector3(0f, 0f, 0f);
}
