using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ToggleGroupHandler<T, T2> where T : struct, IConvertible where T2 : ToggleGroupHandlerRecord<T>
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event ToggleGroupHandler<T, T2>.ToggleSelectedDelegate EToggleSelected = delegate
	{
	};

	public void Init(GameObject owner, bool autoSelectFirstItem = true)
	{
		bool allowSwitchOff = this._group.allowSwitchOff;
		this._group.allowSwitchOff = true;
		for (int i = 0; i < this._toggles.Length; i++)
		{
			Toggle toggle = this._toggles[i].Toggle;
			if (toggle != null)
			{
				toggle.group = this._group;
				toggle.isOn = false;
				int toggleIndex = i;
				toggle.onValueChanged.AddListener(delegate(bool tg)
				{
					if (tg)
					{
						this.SelectToggle(toggleIndex, false);
					}
				});
			}
			else
			{
				LogHelper.Error("{0} has no toggle at {1} position", new object[] { owner, i });
			}
		}
		this._group.allowSwitchOff = allowSwitchOff;
		if (autoSelectFirstItem)
		{
			this._toggles[0].Toggle.isOn = true;
		}
	}

	public T2 CurrentRecord
	{
		get
		{
			return this._toggles[this._latsIndex];
		}
	}

	public void SetActive(bool flag)
	{
		for (int i = 0; i < this._toggles.Length; i++)
		{
			this._toggles[i].Toggle.gameObject.SetActive(flag);
		}
		if (flag)
		{
			for (int j = 0; j < this._toggles.Length; j++)
			{
				if (this._toggles[j].Toggle.isOn)
				{
					this.SelectToggle(j, true);
				}
			}
		}
	}

	public void SetInitialValue(T type, bool doNotGenerateEvent)
	{
		if (doNotGenerateEvent)
		{
			this._isNoEventsMode = true;
			for (int i = 0; i < this._toggles.Length; i++)
			{
				if (this._toggles[i].Type.Equals(type))
				{
					if (this._toggles[i].Toggle.isOn)
					{
						this._latsIndex = i;
						this.EToggleSelected(this._toggles[i]);
					}
					else
					{
						this._toggles[i].Toggle.isOn = true;
					}
					break;
				}
			}
			this._isNoEventsMode = false;
		}
		else
		{
			this.SelectToggle(type);
		}
	}

	public T2 GetToggleByValue()
	{
		return this._toggles.FirstOrDefault((T2 t) => t.Toggle.isOn);
	}

	public void SelectToggle(T type)
	{
		for (int i = 0; i < this._toggles.Length; i++)
		{
			if (this._toggles[i].Type.Equals(type))
			{
				if (this._toggles[i].Toggle.isOn)
				{
					this._latsIndex = i;
					this.EToggleSelected(this._toggles[i]);
				}
				else
				{
					this._toggles[i].Toggle.isOn = true;
				}
				break;
			}
		}
	}

	private void SelectToggle(int toggleIndex, bool force = false)
	{
		if (force || this._latsIndex != toggleIndex)
		{
			this._latsIndex = toggleIndex;
			if (!this._isNoEventsMode)
			{
				this.EToggleSelected(this._toggles[toggleIndex]);
			}
		}
	}

	public virtual void Destroy()
	{
		for (int i = 0; i < this._toggles.Length; i++)
		{
			this._toggles[i].Toggle.onValueChanged.RemoveAllListeners();
		}
	}

	[SerializeField]
	private ToggleGroup _group;

	[SerializeField]
	private T2[] _toggles;

	private int _latsIndex = -1;

	private bool _isNoEventsMode;

	public delegate void ToggleSelectedDelegate(T2 toggleType);
}
