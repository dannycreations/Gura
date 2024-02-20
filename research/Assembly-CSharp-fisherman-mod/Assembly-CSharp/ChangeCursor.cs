using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnPointerActive = delegate(bool b)
	{
	};

	private void OnEnable()
	{
		this.selectable = base.GetComponent<Selectable>();
	}

	private void OnDisable()
	{
		this.RestoreCursor();
	}

	private void Update()
	{
		if (this._btnIncreasing != null)
		{
			if (this._isPointerActive && this.IsMouseActive)
			{
				if (this._increasing == null)
				{
					this._increasing = new DateTime?(DateTime.Now);
				}
				else
				{
					DateTime? increasing = this._increasing;
					double totalSeconds = ((increasing == null) ? null : new TimeSpan?(DateTime.Now - increasing.GetValueOrDefault())).Value.TotalSeconds;
					if (totalSeconds >= 0.4)
					{
						this._btnIncreasing.OnSubmit(null);
					}
				}
			}
			else if (this._increasing != null)
			{
				this._increasing = null;
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (EventSystem.current.GetComponent<CursorManager>().GetCursor != CursorType.Loading && (this.selectable == null || this.selectable.IsInteractable()))
		{
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Selecting);
			this._isPointerActive = true;
			this.OnPointerActive(true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this.RestoreCursor();
	}

	private void RestoreCursor()
	{
		if (EventSystem.current != null && EventSystem.current.GetComponent<CursorManager>().GetCursor != CursorType.Loading)
		{
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
			this._isPointerActive = false;
			this.OnPointerActive(false);
		}
	}

	private bool IsMouseActive
	{
		get
		{
			return Input.GetMouseButton(0);
		}
	}

	[SerializeField]
	private Button _btnIncreasing;

	private const double TimeForIncreasing = 0.4;

	private Selectable selectable;

	private DateTime? _increasing;

	private bool _isPointerActive;
}
