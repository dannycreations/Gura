using System;
using UnityEngine;

public class ServerTimeCache
{
	public DateTime ServerUtcNow
	{
		get
		{
			DateTime localUtcNow = this.GetLocalUtcNow();
			DateTime? dateTime = this.serverTime;
			if (dateTime == null)
			{
				return localUtcNow;
			}
			DateTime? dateTime2 = this.serverTimeTimestamp;
			if (dateTime2 == null)
			{
				return this.serverTime.Value;
			}
			return this.serverTime.Value.Add(localUtcNow.Subtract(this.serverTimeTimestamp.Value));
		}
	}

	public void Update()
	{
		DateTime localUtcNow = this.GetLocalUtcNow();
		DateTime? dateTime = this.serverTimeTimestamp;
		if (dateTime != null)
		{
			DateTime? dateTime2 = this.serverTimeTimestamp;
			if (localUtcNow < dateTime2)
			{
				this.serverTimeTimestamp = null;
			}
			else if (localUtcNow.Subtract(this.serverTimeTimestamp.Value).TotalSeconds > 600.0)
			{
				this.serverTimeTimestamp = null;
			}
		}
		if (!this.isServerTimeBeingRefreshed)
		{
			DateTime? dateTime3 = this.serverTimeTimestamp;
			if (dateTime3 == null)
			{
				this.RefreshTimeFromServer();
			}
		}
		if (this.isServerTimeBeingRefreshed)
		{
			float? num = this.serverTimeRefreshTimeout;
			if (num != null)
			{
				float? num2 = this.serverTimeRefreshTimeout;
				this.serverTimeRefreshTimeout = ((num2 == null) ? null : new float?(num2.GetValueOrDefault() + Mathf.Abs(Time.deltaTime)));
				if (this.serverTimeRefreshTimeout > 30f)
				{
					this.ResetRefresh();
					this.RefreshTimeFromServer();
				}
			}
		}
		if (!this.isServerTimeBeingRefreshed)
		{
			DateTime? dateTime4 = this.serverTimeTimestamp;
			if (dateTime4 != null && localUtcNow.Subtract(this.serverTimeTimestamp.Value).TotalSeconds > 60.0)
			{
				this.RefreshTimeFromServer();
			}
		}
	}

	private void RefreshTimeFromServer()
	{
		if (this.isServerTimeBeingRefreshed)
		{
			return;
		}
		if (!PhotonConnectionFactory.Instance.IsConnectedToGameServer || !PhotonConnectionFactory.Instance.IsAuthenticated)
		{
			return;
		}
		if (!this.signedForEvents)
		{
			PhotonConnectionFactory.Instance.OnGotServerTime += this.HandleGotServerTime;
			this.signedForEvents = true;
		}
		this.serverTimeRefreshTimeout = new float?(0f);
		this.isServerTimeBeingRefreshed = true;
		PhotonConnectionFactory.Instance.GetServerTime(int.MinValue);
	}

	private void ResetRefresh()
	{
		this.serverTimeRefreshTimeout = null;
		this.isServerTimeBeingRefreshed = false;
	}

	private void HandleGotServerTime(int callerid, DateTime time)
	{
		if (callerid == -2147483648)
		{
			this.serverTime = new DateTime?(time);
			this.serverTimeTimestamp = new DateTime?(this.GetLocalUtcNow());
			this.ResetRefresh();
		}
	}

	private DateTime GetLocalUtcNow()
	{
		return TimeHelper.LocalUtcTime();
	}

	private bool signedForEvents;

	private DateTime? serverTime;

	private DateTime? serverTimeTimestamp;

	private float? serverTimeRefreshTimeout;

	private bool isServerTimeBeingRefreshed;

	public const int CallerId = -2147483648;

	private const int RefreshTimeout = 60;

	private const int RefreshTimeoutX10 = 600;

	private const int RefreshRestartTimeout = 30;
}
