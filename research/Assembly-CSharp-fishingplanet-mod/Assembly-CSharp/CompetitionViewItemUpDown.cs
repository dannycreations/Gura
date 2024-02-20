using System;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using UnityEngine;
using UnityEngine.UI;

public class CompetitionViewItemUpDown : CompetitionViewItem
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> OnValueChanged = delegate(int v)
	{
	};

	private void Awake()
	{
		this._ifChecker.Init(this.If);
		this._ifChecker.OnValueChanged += delegate(int v)
		{
			this.OnValueChanged(v);
			this.CheckBtns();
		};
	}

	private void OnDestroy()
	{
		this._ifChecker.Dispose();
	}

	public void IncPlayers(int v)
	{
		this._ifChecker.Inc(v);
	}

	public void SetRange(Range range, int? value = null)
	{
		this._range = range;
		this._ifChecker.SetRange(range, value);
		this.CheckBtns();
	}

	public override void SetBlocked(bool flag)
	{
		base.SetBlocked(flag);
		Selectable btnUp = this.BtnUp;
		bool flag2 = !flag;
		this.If.interactable = flag2;
		flag2 = flag2;
		this.BtnDown.interactable = flag2;
		btnUp.interactable = flag2;
	}

	private void CheckBtns()
	{
		this.BtnUp.interactable = this._ifChecker.Value < this._range.Max;
		this.BtnDown.interactable = this._ifChecker.Value > this._range.Min;
		WindowScheduleAndDuration.SetUpDownColor(this.BtnUp);
		WindowScheduleAndDuration.SetUpDownColor(this.BtnDown);
	}

	[SerializeField]
	protected Button BtnUp;

	[SerializeField]
	protected Button BtnDown;

	[SerializeField]
	protected InputField If;

	private InputFieldMinMaxChecker _ifChecker = new InputFieldMinMaxChecker();

	private Range _range;
}
