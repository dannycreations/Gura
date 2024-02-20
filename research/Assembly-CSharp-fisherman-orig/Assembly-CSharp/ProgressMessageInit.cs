using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class ProgressMessageInit : WindowBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnFinish = delegate
	{
	};

	public void Init(float duration)
	{
		this._time0 = Time.realtimeSinceStartup;
		this._finishTime = new float?(duration);
		base.Open();
	}

	protected override void Update()
	{
		if (this._finishTime != null)
		{
			this._time = Time.realtimeSinceStartup - this._time0;
			float num = 1f - this._time / this._finishTime.Value;
			this._progress.fillAmount = Mathf.Min(num, 1f);
			float? finishTime = this._finishTime;
			int num2 = (int)((finishTime == null) ? null : new float?(finishTime.GetValueOrDefault() - this._time)).Value;
			this._progressValue.text = string.Format("{0}{1}", num2.ToString(), MeasuringSystemManager.Seconds());
			if (num <= 0f)
			{
				this._finishTime = null;
				this.OnFinish();
			}
		}
	}

	public override void Accept()
	{
	}

	[SerializeField]
	private Image _progress;

	[SerializeField]
	private Text _progressValue;

	private float? _finishTime;

	private float _time;

	private float _time0;
}
