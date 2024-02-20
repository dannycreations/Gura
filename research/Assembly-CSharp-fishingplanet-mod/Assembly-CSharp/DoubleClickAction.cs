using System;
using System.Diagnostics;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoubleClickAction : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ActionCalled;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!base.enabled)
		{
			return;
		}
		if (Time.time - this.firstClickTime > 0.3f)
		{
			this.firstClickTime = Time.time;
		}
		else
		{
			this.Invoke();
		}
	}

	public void Invoke()
	{
		if (!base.enabled)
		{
			return;
		}
		if (this.ActionCalled != null)
		{
			this.ActionCalled(this, new EventArgs());
		}
	}

	public void InvokeConfirmed()
	{
		if (!base.enabled)
		{
			return;
		}
		MessageBox _messageBox = MenuHelpers.Instance.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("ConfirmSkipText"), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, false);
		_messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += delegate(object e, EventArgs obj)
		{
			_messageBox.Close();
		};
		_messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += delegate(object e, EventArgs obj)
		{
			_messageBox.Close();
			this.Invoke();
		};
	}

	private float firstClickTime;

	private const float deltaDoubleClick = 0.3f;
}
