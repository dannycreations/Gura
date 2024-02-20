using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA
{
	internal class Layer : MonoBehaviour
	{
		protected void RegisterAction()
		{
			this._actions.Add(delegate
			{
			});
		}

		protected void RegisterAction(Dictionary<string, object> action, string id)
		{
			object valueObj;
			action.TryGetValue("value", out valueObj);
			object obj;
			if (action.TryGetValue("type", out obj))
			{
				PopupEventArgs eventArgs = new PopupEventArgs(id, (string)obj, (string)valueObj);
				string text = (string)obj;
				if (text != null)
				{
					if (text == "none")
					{
						this._actions.Add(delegate
						{
						});
						return;
					}
					if (text == "action")
					{
						this._actions.Add(delegate
						{
							if (valueObj != null)
							{
								this._popup.OnAction(eventArgs);
							}
							this._popup.Close();
						});
						return;
					}
					if (text == "link")
					{
						this._actions.Add(delegate
						{
							this._popup.OnAction(eventArgs);
							if (valueObj != null)
							{
								Application.OpenURL((string)valueObj);
							}
							this._popup.Close();
						});
						return;
					}
				}
				this._actions.Add(delegate
				{
					this._popup.OnDismiss(eventArgs);
					this._popup.Close();
				});
			}
		}

		protected IPopup _popup;

		protected List<Action> _actions = new List<Action>();

		protected int _depth;
	}
}
