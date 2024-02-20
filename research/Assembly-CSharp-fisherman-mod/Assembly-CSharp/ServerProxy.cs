using System;
using System.Diagnostics;
using UnityEngine;

public class ServerProxy<T> where T : class
{
	public ServerProxy(string controllerName, Action subscribeMethod, Action unsubscribeMethod, Action requestMethod, ServerProxy<T>.IsMyRespond respondChecker, int[] invalidationMinutes)
	{
		this._controllerName = controllerName;
		this._subscribeMethod = subscribeMethod;
		this._unsubscribeMethod = unsubscribeMethod;
		this._requestMethod = requestMethod;
		this._respondChecker = respondChecker;
		this._invalidationMinutes = invalidationMinutes;
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<T> ERespond = delegate
	{
	};

	public bool IsLoading
	{
		get
		{
			return this._isLoading;
		}
	}

	public void SendRequest()
	{
		DateTime now = DateTime.Now;
		if (Time.time > this._nextInvalidationTime)
		{
			this._nextInvalidationIndex = this.GetClosestMinuteIndex(now.Minute);
			int num = ((this._invalidationMinutes[this._nextInvalidationIndex] <= now.Minute) ? (60 - now.Minute + this._invalidationMinutes[0]) : (this._invalidationMinutes[this._nextInvalidationIndex] - now.Minute));
			this._nextInvalidationTime = Time.time + (float)((num + 1) * 60);
			if (!this._isLoading)
			{
				this._subscribeMethod();
				this._isLoading = true;
			}
			this._requestMethod();
		}
		else
		{
			this.ERespond(this._serverData);
		}
	}

	public void CancelRequest()
	{
		if (this._isLoading)
		{
			this._isLoading = false;
			this._unsubscribeMethod();
		}
	}

	public void ClearCache()
	{
		this._nextInvalidationTime = -1f;
	}

	public void OnRespond(T data)
	{
		if (this._respondChecker(data))
		{
			this._isLoading = false;
			this._unsubscribeMethod();
			this._serverData = data;
			this.ERespond(data);
		}
	}

	private int GetClosestMinuteIndex(int m)
	{
		for (int i = 0; i < this._invalidationMinutes.Length; i++)
		{
			if (this._invalidationMinutes[i] > m)
			{
				return i;
			}
		}
		return this._invalidationMinutes.Length - 1;
	}

	protected Action _subscribeMethod;

	protected Action _unsubscribeMethod;

	protected Action _requestMethod;

	protected ServerProxy<T>.IsMyRespond _respondChecker;

	protected float _nextRequestFrom = -1f;

	protected bool _isLoading;

	protected string _controllerName;

	private T _serverData;

	private int[] _invalidationMinutes;

	private int _nextInvalidationIndex;

	private float _nextInvalidationTime = -1f;

	public delegate bool IsMyRespond(T data);
}
