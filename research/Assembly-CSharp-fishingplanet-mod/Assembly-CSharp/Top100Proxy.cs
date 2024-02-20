using System;
using System.Diagnostics;

public class Top100Proxy : ServerProxy<TopLeadersResult>
{
	public Top100Proxy(string controllerName, Action requestMethod, ServerProxy<TopLeadersResult>.IsMyRespond respondChecker, int[] invalidationMinutes)
		: base(controllerName, null, null, requestMethod, respondChecker, invalidationMinutes)
	{
		this._subscribeMethod = new Action(this.Subscribe);
		this._unsubscribeMethod = new Action(this.UnSubscribe);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action ERequestFailure = delegate
	{
	};

	private void Subscribe()
	{
		PhotonConnectionFactory.Instance.OnGotLeaderboards += base.OnRespond;
		PhotonConnectionFactory.Instance.OnGettingLeaderboardsFailed += this.OnFail;
	}

	private void UnSubscribe()
	{
		PhotonConnectionFactory.Instance.OnGotLeaderboards -= base.OnRespond;
		PhotonConnectionFactory.Instance.OnGettingLeaderboardsFailed -= this.OnFail;
	}

	private void OnFail(Failure failure)
	{
		LogHelper.Log(failure.FullErrorInfo);
		this.ERequestFailure();
	}
}
