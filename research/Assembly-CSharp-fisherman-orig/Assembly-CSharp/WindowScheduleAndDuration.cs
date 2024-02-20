using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowScheduleAndDuration : WindowBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int, int, DateTime?> OnSelectedData = delegate(int i, int j, DateTime? dt)
	{
	};

	public void Init(List<WindowList.WindowListElem> durationData, int durationIndex, List<WindowList.WindowListElem> stData, int stIndex, DateTime? dt, int overlayIndex)
	{
		this._overlayIndex = overlayIndex;
		this._isImperial = MeasuringSystemManager.CurrentMeasuringSystem == MeasuringSystem.Imperial;
		this.PmAmGo.SetActive(this._isImperial);
		for (int j = 0; j < this.DtInputFields.Length; j++)
		{
			WindowScheduleAndDuration.DtComponents type = this.DtInputFields[j].Type;
			this.DtInputFieldsCached[type] = this.DtInputFields[j];
			this.DtInputFields[j].Tgl.group = this.Tg;
			HotkeyPressRedirect hkpr = this.DtInputFields[j].Tgl.GetComponent<HotkeyPressRedirect>();
			this.DtInputFields[j].Tgl.GetComponent<ToggleColorTransitionChanges>().OnStateChanged += delegate(ToggleColorTransitionChanges.SelectionState s)
			{
				hkpr.SetPausedFromScript(s != ToggleColorTransitionChanges.SelectionState.Highlighted);
			};
			if (type != WindowScheduleAndDuration.DtComponents.PmAm)
			{
				this.DtInputFields[j].If.onValueChanged.AddListener(delegate(string v)
				{
					int num;
					if (int.TryParse(v, out num))
					{
						this.SetIfValue(type, num);
					}
				});
			}
		}
		this.Dt = WindowScheduleAndDuration.InitDt((dt == null) ? dt : new DateTime?(dt.Value.ToLocalTime()));
		this.Dt0 = WindowScheduleAndDuration.InitDt(null);
		this.DtMax = this.Dt0.AddDays((double)UserCompetitionPublic.UgcMaxDaysScheduledFuture);
		this.UpdateDt();
		this.DataDuration = durationData;
		this.DataSt = stData;
		this.SelectedIndexDuration = durationIndex;
		this.SelectedIndexSt = stIndex;
		this.UpdateOverlay();
		WindowList.CreateList(this.DataDuration, this.ItemsRootDuration, this.ItemDurationPrefab, this.ItemsDuration, this.Tg, this.SelectedIndexDuration, delegate(int i)
		{
			if (SettingsManager.InputType == InputModuleManager.InputType.Mouse)
			{
				this.SetActiveItem(i, this.ItemsDuration, ref this.SelectedIndexDuration);
			}
		}, delegate(int i)
		{
			this.SetActiveItem(i, this.ItemsDuration, ref this.SelectedIndexDuration);
		}, null, null);
		WindowList.CreateList(this.DataSt, this.ItemsRootSt, this.ItemStPrefab, this.ItemsSt, this.Tg, this.SelectedIndexSt, delegate(int i)
		{
			if (SettingsManager.InputType == InputModuleManager.InputType.Mouse)
			{
				this.SetActiveItem(i, this.ItemsSt, ref this.SelectedIndexSt);
				this.UpdateOverlay();
			}
		}, delegate(int i)
		{
			this.SetActiveItem(i, this.ItemsSt, ref this.SelectedIndexSt);
			this.UpdateOverlay();
		}, null, null);
	}

	public void IncUp(int v)
	{
		this.Inc((WindowScheduleAndDuration.DtComponents)v, 1);
	}

	public void IncDown(int v)
	{
		this.Inc((WindowScheduleAndDuration.DtComponents)v, -1);
	}

	public static void SetUpDownColor(Button b)
	{
		if (b != null)
		{
			b.GetComponent<Image>().color = ((!b.interactable) ? UgcConsts.UpDownDisabled : UgcConsts.UpDownEnabled);
			b.GetComponentInChildren<TextMeshProUGUI>().color = ((!b.interactable) ? UgcConsts.UpDownArrowDisabled : Color.white);
		}
	}

	protected void SetActiveItem(int i, List<IWindowListItem> items, ref int selectedIndex)
	{
		items[selectedIndex].SetActive(false);
		selectedIndex = i;
		items[selectedIndex].SetActive(true);
	}

	protected override void AcceptActionCalled()
	{
		this.OnSelectedData(this.SelectedIndexDuration, this.SelectedIndexSt, this.FillDt());
	}

	private DateTime? FillDt()
	{
		return (this.SelectedIndexSt != this._overlayIndex) ? new DateTime?(this.Dt) : null;
	}

	private void UpdateDt()
	{
		this.SetIfValue(WindowScheduleAndDuration.DtComponents.Yy, this.Dt.Year);
		this.SetIfValue(WindowScheduleAndDuration.DtComponents.Mm, this.Dt.Month);
		this.SetIfValue(WindowScheduleAndDuration.DtComponents.Dd, this.Dt.Day);
		int num = this.Dt.Hour;
		if (this._isImperial)
		{
			if (num > 11)
			{
				this.SetIfValue(WindowScheduleAndDuration.DtComponents.PmAm, "PM");
				if (num > 12)
				{
					num -= 12;
				}
			}
			else
			{
				this.SetIfValue(WindowScheduleAndDuration.DtComponents.PmAm, "AM");
				if (num == 0)
				{
					num = 12;
				}
			}
		}
		this.SetIfValue(WindowScheduleAndDuration.DtComponents.Hh, num);
		this.SetIfValue(WindowScheduleAndDuration.DtComponents.Min, this.Dt.Minute);
		this.CheckUpDownEnabled();
	}

	private void Inc(WindowScheduleAndDuration.DtComponents c, int v)
	{
		DateTime dtMax = this.DtMax;
		DateTime dateTime = this.Dt;
		switch (c)
		{
		case WindowScheduleAndDuration.DtComponents.Dd:
			dateTime = dateTime.AddDays((double)v);
			break;
		case WindowScheduleAndDuration.DtComponents.Hh:
			dateTime = dateTime.AddHours((double)v);
			break;
		case WindowScheduleAndDuration.DtComponents.Min:
			dateTime = dateTime.AddMinutes((double)(15 * v));
			break;
		case WindowScheduleAndDuration.DtComponents.PmAm:
		{
			int num = this.PmAmPref.IndexOf(this.DtInputFieldsCached[c].If.text);
			this.SetIfValue(c, this.PmAmPref[num + v]);
			break;
		}
		}
		this.Dt = dateTime;
		this.UpdateDt();
	}

	private void SetIfValue(WindowScheduleAndDuration.DtComponents c, int v)
	{
		this.DtInputFieldsValues[c] = v;
		this.DtInputFieldsCached[c].If.text = ((c != WindowScheduleAndDuration.DtComponents.Mm) ? v.ToString() : this.GetMonthValue(v));
	}

	private void SetIfValue(WindowScheduleAndDuration.DtComponents c, string v)
	{
		this.DtInputFieldsCached[c].If.text = v;
	}

	private int GetIfValue(WindowScheduleAndDuration.DtComponents c)
	{
		return int.Parse(this.DtInputFieldsCached[c].If.text);
	}

	private string GetMonthValue(int v)
	{
		return ScriptLocalization.Get(WindowScheduleAndDuration.MonthsLocIds[v - 1]);
	}

	private int GetIfValueHour()
	{
		int num = this.GetIfValue(WindowScheduleAndDuration.DtComponents.Hh);
		if (this._isImperial)
		{
			if (this.DtInputFieldsCached[WindowScheduleAndDuration.DtComponents.PmAm].If.text == "PM")
			{
				if (num != 11)
				{
					num += 12;
				}
			}
			else if (num == 11)
			{
				num = 0;
			}
		}
		return num;
	}

	private void UpdateOverlay()
	{
		this.Overlay.SetActive(this.SelectedIndexSt == this._overlayIndex);
		this.DtInputFieldsCached.Values.ToList<WindowScheduleAndDuration.DtComponent>().ForEach(delegate(WindowScheduleAndDuration.DtComponent p)
		{
			p.Tgl.interactable = !this.Overlay.activeSelf;
		});
	}

	public static DateTime InitDt(DateTime? dt)
	{
		DateTime now = DateTime.Now;
		if (dt != null && dt.Value > now)
		{
			return dt.Value;
		}
		int num = -1;
		for (int i = 0; i < 4; i++)
		{
			int num2 = i * 15;
			if (now.Minute < num2)
			{
				num = num2;
				break;
			}
		}
		if (num == -1)
		{
			DateTime dateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
			return dateTime.AddHours(1.0);
		}
		return new DateTime(now.Year, now.Month, now.Day, now.Hour, num, 0);
	}

	private void CheckUpDownEnabled()
	{
		DateTime dtMax = this.DtMax;
		DateTime dt = this.Dt;
		if (this._isImperial)
		{
			WindowScheduleAndDuration.DtComponent dtComponent = this.DtInputFieldsCached[WindowScheduleAndDuration.DtComponents.PmAm];
			int num = this.PmAmPref.IndexOf(dtComponent.If.text);
			dtComponent.Up.interactable = num < this.PmAmPref.Count - 1;
			dtComponent.Down.interactable = num > 0;
		}
		WindowScheduleAndDuration.DtComponent dtComponent2 = this.DtInputFieldsCached[WindowScheduleAndDuration.DtComponents.Min];
		dtComponent2.Up.interactable = dt.AddMinutes(15.0) < this.DtMax;
		dtComponent2.Down.interactable = dt.AddMinutes(-15.0) >= this.Dt0;
		WindowScheduleAndDuration.DtComponent dtComponent3 = this.DtInputFieldsCached[WindowScheduleAndDuration.DtComponents.Hh];
		dtComponent3.Up.interactable = dt.AddHours(1.0) < this.DtMax;
		dtComponent3.Down.interactable = dt.AddHours(-1.0) >= this.Dt0;
		WindowScheduleAndDuration.DtComponent dtComponent4 = this.DtInputFieldsCached[WindowScheduleAndDuration.DtComponents.Dd];
		dtComponent4.Up.interactable = dt.AddDays(1.0) < this.DtMax;
		dtComponent4.Down.interactable = dt.AddDays(-1.0) >= this.Dt0;
		foreach (KeyValuePair<WindowScheduleAndDuration.DtComponents, WindowScheduleAndDuration.DtComponent> keyValuePair in this.DtInputFieldsCached)
		{
			WindowScheduleAndDuration.SetUpDownColor(keyValuePair.Value.Up);
			WindowScheduleAndDuration.SetUpDownColor(keyValuePair.Value.Down);
		}
	}

	[SerializeField]
	protected WindowScheduleAndDuration.DtComponent[] DtInputFields;

	[SerializeField]
	protected GameObject PmAmGo;

	[SerializeField]
	protected GameObject Overlay;

	[SerializeField]
	protected ToggleGroup Tg;

	[SerializeField]
	protected GameObject ItemsRootDuration;

	[SerializeField]
	protected GameObject ItemsRootSt;

	[SerializeField]
	protected GameObject ItemDurationPrefab;

	[SerializeField]
	protected GameObject ItemStPrefab;

	public static readonly IList<string> MonthsLocIds = new ReadOnlyCollection<string>(new List<string>
	{
		"January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
		"November", "December"
	});

	public const string Am = "AM";

	public const string Pm = "PM";

	private const int MinutesStep = 15;

	protected readonly List<string> PmAmPref = new List<string> { "AM", "PM" };

	protected int SelectedIndexDuration;

	protected List<IWindowListItem> ItemsDuration = new List<IWindowListItem>();

	protected List<WindowList.WindowListElem> DataDuration;

	protected int SelectedIndexSt;

	protected List<IWindowListItem> ItemsSt = new List<IWindowListItem>();

	protected List<WindowList.WindowListElem> DataSt;

	protected DateTime Dt0;

	protected DateTime Dt;

	protected DateTime DtMax;

	protected Dictionary<WindowScheduleAndDuration.DtComponents, WindowScheduleAndDuration.DtComponent> DtInputFieldsCached = new Dictionary<WindowScheduleAndDuration.DtComponents, WindowScheduleAndDuration.DtComponent>();

	protected Dictionary<WindowScheduleAndDuration.DtComponents, int> DtInputFieldsValues = new Dictionary<WindowScheduleAndDuration.DtComponents, int>();

	protected bool _isImperial;

	private int _overlayIndex;

	[Serializable]
	protected class DtComponent
	{
		[SerializeField]
		public WindowScheduleAndDuration.DtComponents Type;

		[SerializeField]
		public InputField If;

		[SerializeField]
		public Toggle Tgl;

		[SerializeField]
		public Button Up;

		[SerializeField]
		public Button Down;
	}

	protected enum DtComponents
	{
		Mm,
		Dd,
		Yy,
		Hh,
		Min,
		PmAm
	}
}
