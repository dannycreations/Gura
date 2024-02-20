using System;
using System.Collections;
using System.Diagnostics;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.Logic.Misc.Visual.UI.DateTimePicker
{
	public class DateTimePicker8 : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<DateTime> OnDateSelected;

		public DateTimePickerAdapter DayAdapter { get; private set; }

		public DateTimePickerAdapter MonthAdapter { get; private set; }

		public DateTimePickerAdapter YearAdapter { get; private set; }

		public DateTimePickerAdapter HourAdapter { get; private set; }

		public DateTimePickerAdapter MinuteAdapter { get; private set; }

		public DateTimePickerAdapter SecondAdapter { get; private set; }

		public DateTime SelectedValue
		{
			get
			{
				return new DateTime(this.YearAdapter.SelectedValue, this.MonthAdapter.SelectedValue, this.DayAdapter.SelectedValue, this.HourAdapter.SelectedValue, this.MinuteAdapter.SelectedValue, this.SecondAdapter.SelectedValue);
			}
		}

		private float AnimElapsedTime01
		{
			get
			{
				float num = Mathf.Clamp01((Time.time - this._AnimStartTime) / 0.25f);
				return num * num * num * num;
			}
		}

		private float AnimCurrentFloat
		{
			get
			{
				return Mathf.Lerp(this._AnimStart, this._AnimEnd, this.AnimElapsedTime01);
			}
		}

		public static DateTimePicker8 Show(Action<DateTime> onSelected)
		{
			return DateTimePicker8.Show(DateTime.Now, onSelected);
		}

		public static DateTimePicker8 Show(DateTime startingDate, Action<DateTime> onSelected)
		{
			return DateTimePicker8.Show(startingDate, onSelected, 660f, 330f);
		}

		public static DateTimePicker8 Show(DateTime startingDate, Action<DateTime> onSelected, float width, float height)
		{
			GameObject gameObject = Resources.Load<GameObject>("SRIA/DateTimePicker8");
			DateTimePicker8 component = Object.Instantiate<GameObject>(gameObject).GetComponent<DateTimePicker8>();
			Canvas canvas = Object.FindObjectOfType<Canvas>();
			if (!canvas)
			{
				throw new UnityException(typeof(DateTimePicker8).Name + ": no Canvas was found in the scene");
			}
			RectTransform rectTransform = canvas.transform as RectTransform;
			RectTransform rectTransform2 = component.transform as RectTransform;
			rectTransform2.SetParent(rectTransform, false);
			rectTransform2.SetAsLastSibling();
			component._DateToInitWith = new DateTime?(startingDate);
			if (onSelected != null)
			{
				component.OnDateSelected += onSelected;
			}
			rectTransform2.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(0, (rectTransform.rect.width - width) / 2f, width);
			rectTransform2.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(2, (rectTransform.rect.height - height) / 2f, height);
			return component;
		}

		private void Awake()
		{
			this._CanvasGroup = base.GetComponent<CanvasGroup>();
			this._CanvasGroup.alpha = 0f;
			this._AnimEnd = 1f;
			this._AnimStartTime = Time.time;
			this._Animating = true;
		}

		private void Start()
		{
			Transform transform = base.transform.Find("Adapters");
			this._DatePanel = transform.Find("Date");
			this._DatePanel.GetComponentAtPath("SelectedIndicatorText", out this._SelectedDateText);
			int num = 0;
			DateTimePickerAdapter[] allAdapters = this._AllAdapters;
			int num2 = num++;
			DateTimePickerAdapter dateTimePickerAdapter = this._DatePanel.GetComponentAtPath("Panel/Day");
			this.DayAdapter = dateTimePickerAdapter;
			allAdapters[num2] = dateTimePickerAdapter;
			DateTimePickerAdapter[] allAdapters2 = this._AllAdapters;
			int num3 = num++;
			dateTimePickerAdapter = this._DatePanel.GetComponentAtPath("Panel/Month");
			this.MonthAdapter = dateTimePickerAdapter;
			allAdapters2[num3] = dateTimePickerAdapter;
			DateTimePickerAdapter[] allAdapters3 = this._AllAdapters;
			int num4 = num++;
			dateTimePickerAdapter = this._DatePanel.GetComponentAtPath("Panel/Year");
			this.YearAdapter = dateTimePickerAdapter;
			allAdapters3[num4] = dateTimePickerAdapter;
			this._TimePanel = transform.Find("Time");
			this._TimePanel.GetComponentAtPath("SelectedIndicatorText", out this._SelectedTimeText);
			DateTimePickerAdapter[] allAdapters4 = this._AllAdapters;
			int num5 = num++;
			dateTimePickerAdapter = this._TimePanel.GetComponentAtPath("Panel/Hour");
			this.HourAdapter = dateTimePickerAdapter;
			allAdapters4[num5] = dateTimePickerAdapter;
			DateTimePickerAdapter[] allAdapters5 = this._AllAdapters;
			int num6 = num++;
			dateTimePickerAdapter = this._TimePanel.GetComponentAtPath("Panel/Minute");
			this.MinuteAdapter = dateTimePickerAdapter;
			allAdapters5[num6] = dateTimePickerAdapter;
			DateTimePickerAdapter[] allAdapters6 = this._AllAdapters;
			int num7 = num++;
			dateTimePickerAdapter = this._TimePanel.GetComponentAtPath("Panel/Second");
			this.SecondAdapter = dateTimePickerAdapter;
			allAdapters6[num7] = dateTimePickerAdapter;
			if (this._AutoInit)
			{
				this.ExecuteAfter(0.2f, new Action(this.AutoInit));
			}
		}

		private void Update()
		{
			if (this._Animating)
			{
				this._CanvasGroup.alpha = this.AnimCurrentFloat;
				if (this.AnimElapsedTime01 == 1f)
				{
					this._Animating = false;
					if (this._ActionOnAnimDone != null)
					{
						this._ActionOnAnimDone();
					}
				}
				return;
			}
			if (!this._Initialized)
			{
				return;
			}
			try
			{
				DateTime selectedValue = this.SelectedValue;
				this._SelectedDateText.text = ((!this._DisplaySelectedDateAsShort) ? selectedValue.ToLongDateString() : selectedValue.ToShortDateString());
				this._SelectedTimeText.text = ((!this._DisplaySelectedTimeAsShort) ? selectedValue.ToLongTimeString() : selectedValue.ToShortTimeString());
			}
			catch
			{
			}
		}

		public void InitWithNow()
		{
			this.InitWithDate(DateTime.Now);
		}

		public void InitWithDate(DateTime dateTime)
		{
			this.StopAnimations();
			this._DateToInitWith = null;
			this.UnregisterAutoCorrection();
			int doneNum = 0;
			int targetDone = 2;
			Func<float, bool> func = delegate(float p01)
			{
				if (p01 == 1f && ++doneNum == targetDone)
				{
					this._Initialized = true;
					this.RegisterAutoCorrection();
				}
				return true;
			};
			this.YearAdapter.ResetItems(3000, false, false);
			this.YearAdapter.SmoothScrollTo(dateTime.Year - 1, 0.5f, 0.5f, 0.5f, null, false);
			this.MonthAdapter.ResetItems(12, false, false);
			this.MonthAdapter.SmoothScrollTo(dateTime.Month - 1, 0.75f, 0.5f, 0.5f, null, false);
			this.DayAdapter.ResetItems(DateTime.DaysInMonth(dateTime.Year, dateTime.Month), false, false);
			this.DayAdapter.SmoothScrollTo(dateTime.Day - 1, 1f, 0.5f, 0.5f, func, true);
			this.SecondAdapter.ResetItems(60, false, false);
			this.SecondAdapter.SmoothScrollTo(dateTime.Second, 0.5f, 0.5f, 0.5f, null, false);
			this.MinuteAdapter.ResetItems(60, false, false);
			this.MinuteAdapter.SmoothScrollTo(dateTime.Minute, 0.75f, 0.5f, 0.5f, null, false);
			this.HourAdapter.ResetItems(24, false, false);
			this.HourAdapter.SmoothScrollTo(dateTime.Hour, 1f, 0.5f, 0.5f, func, true);
		}

		public void ReturnCurrent()
		{
			this._AnimStart = this._CanvasGroup.alpha;
			this._AnimEnd = 0f;
			this._AnimStartTime = Time.time;
			this._Animating = true;
			this._ActionOnAnimDone = delegate
			{
				this._ActionOnAnimDone = null;
				if (this.OnDateSelected != null)
				{
					this.OnDateSelected(this.SelectedValue);
				}
				Object.Destroy(base.gameObject);
			};
		}

		private void AutoInit()
		{
			DateTime? dateToInitWith = this._DateToInitWith;
			this._DateToInitWith = new DateTime?((dateToInitWith == null) ? DateTime.Now : dateToInitWith.Value);
			this.InitWithDate(this._DateToInitWith.Value);
		}

		private void UnregisterAutoCorrection()
		{
			this.YearAdapter.OnSelectedValueChanged -= this.OnYearChanged;
			this.MonthAdapter.OnSelectedValueChanged -= this.OnMonthChanged;
		}

		private void RegisterAutoCorrection()
		{
			this.YearAdapter.OnSelectedValueChanged += this.OnYearChanged;
			this.MonthAdapter.OnSelectedValueChanged += this.OnMonthChanged;
		}

		private void OnYearChanged(int year)
		{
			this.OnMonthChanged(this.MonthAdapter.SelectedValue);
		}

		private void OnMonthChanged(int month)
		{
			int selectedValue = this.DayAdapter.SelectedValue;
			int num = DateTime.DaysInMonth(this.YearAdapter.SelectedValue, month);
			if (num == this.DayAdapter.GetItemsCount())
			{
				return;
			}
			this.DayAdapter.ResetItems(num, false, false);
			this.DayAdapter.ScrollTo(Math.Min(num, selectedValue) - 1, 0.5f, 0.5f);
		}

		private void StopAnimations()
		{
			foreach (DateTimePickerAdapter dateTimePickerAdapter in this._AllAdapters)
			{
				if (dateTimePickerAdapter)
				{
					dateTimePickerAdapter.CancelAnimationsIfAny();
				}
			}
		}

		private void ExecuteAfter(float seconds, Action action)
		{
			base.StartCoroutine(this.ExecuteAfterCoroutine(seconds, action));
		}

		private IEnumerator ExecuteAfterCoroutine(float seconds, Action action)
		{
			if (seconds > 0f)
			{
				yield return null;
				yield return null;
			}
			yield return new WaitForSeconds(seconds);
			action();
			yield break;
		}

		[SerializeField]
		private bool _AutoInit;

		[SerializeField]
		private bool _DisplaySelectedDateAsShort;

		[SerializeField]
		private bool _DisplaySelectedTimeAsShort;

		private const float SCROLL_DURATION1 = 0.5f;

		private const float SCROLL_DURATION2 = 0.75f;

		private const float SCROLL_DURATION3 = 1f;

		private const float INIT_STEP_DURATION = 0.5f;

		private const float ANIM_DURATION = 0.25f;

		private const float DEFAULT_WIDTH = 660f;

		private const float DEFAULT_HEIGHT = 330f;

		private Transform _DatePanel;

		private Transform _TimePanel;

		private Text _SelectedDateText;

		private Text _SelectedTimeText;

		private bool _Initialized;

		private DateTime? _DateToInitWith;

		private bool _Animating;

		private float _AnimStart;

		private float _AnimEnd;

		private float _AnimStartTime;

		private Action _ActionOnAnimDone;

		private CanvasGroup _CanvasGroup;

		private DateTimePickerAdapter[] _AllAdapters = new DateTimePickerAdapter[6];
	}
}
