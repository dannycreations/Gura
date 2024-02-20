using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

public class ActionDragStyleAnalyzer
{
	public DragStyle Style
	{
		get
		{
			return this.style;
		}
	}

	public float Quality
	{
		get
		{
			return this.quality;
		}
	}

	public bool IsBeingPulled
	{
		get
		{
			return this.isBeingPulled || this.style == DragStyle.Simple || this.style == DragStyle.SlowSimple || this.style == DragStyle.Twitch;
		}
	}

	public void Reset(DragStyleSet dragSet)
	{
		this.style = DragStyle.Undefined;
		this.quality = 0f;
		this.dragSet = dragSet;
		this.LastIdlePeriodLength = 0f;
		this.LastMovingPeriodLength = 0f;
		this.priorPosition = null;
		this.history.Clear();
		this.vVelocityAverager.Clear();
		this.ResetPeriod();
	}

	public void FinishPeriod()
	{
		ActionDragStyleAnalyzer.Period period = new ActionDragStyleAnalyzer.Period
		{
			IsIdle = !this.isBeingPulled,
			TimeLength = this.currentPeriodLength,
			IsStriking = this.isStriking,
			HasFullStrike = this.hasFullStrike,
			IsLying = this.isLying,
			IsReeling = this.isReeling,
			ReelingSpeed = this.reelingSpeed,
			VerticalAmplitude = this.minDepth - this.maxDepth
		};
		this.AddPeriod(period);
		this.ResetPeriod();
	}

	private void ResetPeriod()
	{
		this.lastStrikeTimeStamp = -1f;
		this.strikeCounter = 0;
		this.currentPeriodLength = 0f;
		this.isReeling = false;
		this.isStriking = false;
		this.hasFullStrike = false;
		this.reelingSpeed = 0;
		this.isLying = false;
		this.minDepth = float.NegativeInfinity;
		this.maxDepth = 0f;
	}

	public void Update(ActionDragStyleAnalyzer.ActionStatus status)
	{
		Vector3? vector = this.priorPosition;
		if (vector == null)
		{
			this.priorPosition = new Vector3?(status.TacklePosition);
			this.priorTime = new float?(Time.deltaTime);
			return;
		}
		if (this.priorPosition == status.TacklePosition && this.priorTime == Time.deltaTime)
		{
			return;
		}
		Vector3 vector2 = (status.TacklePosition - this.priorPosition.Value) / Time.deltaTime;
		float magnitude = vector2.magnitude;
		if (Mathf.Abs(magnitude) < 0.01f)
		{
		}
		bool flag;
		if (this.dragSet == DragStyleSet.Popper || this.dragSet == DragStyleSet.Walker)
		{
			flag = status.IsStriking;
		}
		else
		{
			flag = !GameFactory.Player.Reel.IsReeling;
		}
		Vector2 vector3;
		vector3..ctor(vector2.x, vector2.z);
		this.currentHorizontalVelocity = vector3.magnitude;
		if (this.currentHorizontalVelocity < 0.01f)
		{
			this.currentHorizontalVelocity = 0f;
		}
		this.currentHorizontalVelocity = this.vVelocityAverager.UpdateAndGet(this.currentHorizontalVelocity);
		this.currentPeriodLength += Time.deltaTime;
		if (this.minDepth < status.TacklePosition.y)
		{
			this.minDepth = status.TacklePosition.y;
		}
		if (this.maxDepth > status.TacklePosition.y)
		{
			this.maxDepth = status.TacklePosition.y;
		}
		if (status.IsReeling)
		{
			this.isReeling = true;
			this.reelingSpeed = status.ReelingSpeed;
		}
		if (status.IsStriking && this.isBeingPulled == flag)
		{
			this.isStriking = true;
		}
		if (status.HasFullStrike)
		{
			this.hasFullStrike = true;
		}
		if (!this.isBeingPulled && status.IsLying)
		{
			this.isLying = true;
		}
		if (status.IsStriking)
		{
			this.lastStrikeTimeStamp = Time.time;
			if (!this.strikePeriod)
			{
				this.strikeCounter++;
			}
			this.strikePeriod = true;
		}
		else
		{
			this.strikePeriod = false;
		}
		if (this.isBeingPulled != flag)
		{
			this.FinishPeriod();
		}
		else
		{
			if (!flag)
			{
				float num = Mathf.Min(this.currentPeriodLength, 2f);
				this.AvgSpeed = (this.AvgSpeed * (num - Time.deltaTime) + this.currentHorizontalVelocity * Time.deltaTime) / num;
			}
			this.PerformCurrentPeriodAnalysis(this.currentPeriodLength, !flag);
		}
		this.isBeingPulled = flag;
		this.priorPosition = new Vector3?(status.TacklePosition);
		this.priorTime = new float?(Time.deltaTime);
		if (this.priorStyle != this.style)
		{
			GameFactory.Player.HudFishingHandler.LurePositionHandlerContinuous.ShowDragStyleText(ActionDragStyleAnalyzer.GetFineDragStyleName(this.style), this.quality);
		}
		this.priorStyle = this.style;
	}

	public static string GetFineDragStyleName(DragStyle dragStyle)
	{
		switch (dragStyle)
		{
		case DragStyle.SlowSimple:
			return "Straight Slow";
		case DragStyle.Simple:
			return "Straight";
		case DragStyle.StopNGo:
			return "Stop&Go";
		case DragStyle.Rise:
			return "Lift&Drop";
		case DragStyle.Twitch:
			return "Twitching";
		case DragStyle.Popping:
			return "Popping";
		case DragStyle.Walking:
			return "Walking";
		default:
			return string.Empty;
		}
	}

	private int historyCountIsIdle(bool isIdle)
	{
		int num = 0;
		for (int i = 0; i < this.history.Count; i++)
		{
			if (this.history[i].IsIdle == isIdle)
			{
				num++;
			}
		}
		return num;
	}

	private float historyAverageVerticalAmplitude()
	{
		float num = 0f;
		for (int i = 0; i < this.history.Count; i++)
		{
			num += this.history[i].VerticalAmplitude;
		}
		return num / (float)this.history.Count;
	}

	private float historyAverageTimeLength(bool isIdle)
	{
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < this.history.Count; i++)
		{
			if (this.history[i].IsIdle == isIdle)
			{
				num += this.history[i].TimeLength;
				num2++;
			}
		}
		return num / (float)num2;
	}

	private void AddPeriod(ActionDragStyleAnalyzer.Period period)
	{
		this.PeriodNumber++;
		if (this.history.Count == 4)
		{
			this.history.RemoveAt(0);
		}
		this.history.Add(period);
		if (this.historyCountIsIdle(true) != 2 || this.historyCountIsIdle(false) != 2)
		{
			return;
		}
		this.AvgAmplitude = this.historyAverageVerticalAmplitude();
		float num = this.historyAverageTimeLength(true);
		float num2 = this.historyAverageTimeLength(false);
		this.AvgMoveVsIdle = num2 / num;
		if (period.IsIdle)
		{
			this.LastIdlePeriodLength = period.TimeLength;
		}
		else
		{
			this.LastMovingPeriodLength = period.TimeLength;
		}
		this.PerformPeriodsAnalysis();
	}

	private void PerformPeriodsAnalysis()
	{
		if (this.history.Count < 4)
		{
			return;
		}
		ActionDragStyleAnalyzer.ComplexDrag complexDrag = null;
		if (this.dragSet == DragStyleSet.NormalLure)
		{
			complexDrag = this.MatchComplexDrag(ActionDragStyleAnalyzer.ComplexDrags);
		}
		else if (this.dragSet == DragStyleSet.Popper)
		{
			complexDrag = this.MatchComplexDrag(ActionDragStyleAnalyzer.TopWaterPopperDrags);
		}
		else if (this.dragSet == DragStyleSet.Walker)
		{
			complexDrag = this.MatchComplexDrag(ActionDragStyleAnalyzer.TopWaterWalkerDrags);
		}
		this.quality = this.CalculateQuality(complexDrag);
		this.style = ((complexDrag == null) ? DragStyle.Undefined : complexDrag.Style);
	}

	private int checkPeriods(bool isIdle, bool? isLying, bool? isStriking, bool? hasFullStrike, bool? isReeling, ActionDragStyleAnalyzer.RangeInt reelingSpeed)
	{
		for (int i = 0; i < this.history.Count; i++)
		{
			if (this.history[i].IsIdle == isIdle)
			{
				if (isLying != null && this.history[i].IsLying != isLying.Value)
				{
					return 1;
				}
				if (isStriking != null && this.history[i].IsStriking != isStriking.Value)
				{
					return 2;
				}
				if (hasFullStrike != null && this.history[i].HasFullStrike != hasFullStrike.Value)
				{
					return 3;
				}
				if (isReeling != null && this.history[i].IsReeling != isReeling.Value)
				{
					return 4;
				}
				if (reelingSpeed != null && (this.history[i].ReelingSpeed < reelingSpeed.Min || this.history[i].ReelingSpeed > reelingSpeed.Max))
				{
					return 5;
				}
			}
		}
		return 0;
	}

	private ActionDragStyleAnalyzer.ComplexDrag MatchComplexDrag(ActionDragStyleAnalyzer.ComplexDrag[] candidates)
	{
		foreach (ActionDragStyleAnalyzer.ComplexDrag complexDrag in candidates)
		{
			if (this.AvgMoveVsIdle >= complexDrag.PeriodRate.Min || this.AvgMoveVsIdle <= complexDrag.PeriodRate.Max)
			{
				int num = this.checkPeriods(false, complexDrag.ActiveChars.IsLying, complexDrag.ActiveChars.IsStriking, complexDrag.ActiveChars.HasFullStrike, complexDrag.ActiveChars.IsReeling, complexDrag.ActiveChars.ReelingSpeed);
				if (num == 0)
				{
					num = this.checkPeriods(true, complexDrag.IdleChars.IsLying, complexDrag.IdleChars.IsStriking, complexDrag.IdleChars.HasFullStrike, complexDrag.IdleChars.IsReeling, complexDrag.IdleChars.ReelingSpeed);
					if (num == 0)
					{
						return complexDrag;
					}
				}
			}
		}
		return null;
	}

	private ActionDragStyleAnalyzer.ComplexDrag findComplexDrag(DragStyle dragStyle)
	{
		for (int i = 0; i < ActionDragStyleAnalyzer.ComplexDrags.Length; i++)
		{
			if (ActionDragStyleAnalyzer.ComplexDrags[i].Style == dragStyle)
			{
				return ActionDragStyleAnalyzer.ComplexDrags[i];
			}
		}
		return null;
	}

	public float CalculateQuality(float amplitude, float ratio, DragStyle dragStyle)
	{
		ActionDragStyleAnalyzer.ComplexDrag complexDrag = this.findComplexDrag(dragStyle);
		this.AvgAmplitude = amplitude;
		this.AvgMoveVsIdle = ratio;
		return this.CalculateQuality(complexDrag);
	}

	private float CalculateQuality(ActionDragStyleAnalyzer.ComplexDrag matchedComplexStyle)
	{
		if (matchedComplexStyle == null)
		{
			return 0f;
		}
		float num = (matchedComplexStyle.Amplitude.Max - matchedComplexStyle.Amplitude.Min) / 2f;
		float num2 = matchedComplexStyle.Amplitude.Max - num;
		float num3 = (matchedComplexStyle.PeriodRate.Max - matchedComplexStyle.PeriodRate.Min) / 2f;
		float num4 = matchedComplexStyle.PeriodRate.Max - num3;
		float num5 = (Mathf.Abs(this.AvgAmplitude - num) / num2 + Mathf.Abs(this.AvgMoveVsIdle - num3) / num4) / 2f;
		return Mathf.Clamp01(1f - num5);
	}

	private ActionDragStyleAnalyzer.SimpleDrag findSimpleDrag(float avgSpeed)
	{
		for (int i = 0; i < ActionDragStyleAnalyzer.SimpleDrags.Length; i++)
		{
			if (avgSpeed > ActionDragStyleAnalyzer.SimpleDrags[i].Speed.Min && avgSpeed < ActionDragStyleAnalyzer.SimpleDrags[i].Speed.Max)
			{
				return ActionDragStyleAnalyzer.SimpleDrags[i];
			}
		}
		return null;
	}

	private void PerformCurrentPeriodAnalysis(float periodLength, bool isMoving)
	{
		if (isMoving && periodLength > 3f && this.dragSet == DragStyleSet.NormalLure)
		{
			ActionDragStyleAnalyzer.SimpleDrag simpleDrag = this.findSimpleDrag(this.AvgSpeed);
			this.quality = (float)((simpleDrag == null) ? 0 : 1);
			this.style = ((simpleDrag == null) ? DragStyle.Undefined : simpleDrag.Style);
			if (this.strikeCounter > 0)
			{
				this.style = DragStyle.Undefined;
			}
			if (this.dragSet == DragStyleSet.NormalLure && this.isStriking && Time.time - this.lastStrikeTimeStamp <= 1.3f)
			{
				if (this.strikeCounter > 2)
				{
					this.style = DragStyle.Twitch;
				}
			}
			else
			{
				this.strikeCounter = 0;
			}
		}
		else if (isMoving && this.LastIdlePeriodLength > 0f && periodLength > this.LastIdlePeriodLength * 2f)
		{
			this.style = DragStyle.Undefined;
			this.lastStrikeTimeStamp = -1f;
			this.strikeCounter = 0;
		}
		else if (!isMoving && this.LastMovingPeriodLength > 0f && periodLength > this.LastMovingPeriodLength * 2f)
		{
			this.style = DragStyle.Undefined;
			this.lastStrikeTimeStamp = -1f;
			this.strikeCounter = 0;
		}
	}

	private DragStyle style;

	private DragStyle priorStyle;

	private float quality;

	public int PeriodNumber;

	private static readonly ActionDragStyleAnalyzer.SimpleDrag[] SimpleDrags = new ActionDragStyleAnalyzer.SimpleDrag[]
	{
		new ActionDragStyleAnalyzer.SimpleDrag
		{
			Style = DragStyle.SlowSimple,
			Speed = new ActionDragStyleAnalyzer.Range(0.5f, 1f)
		},
		new ActionDragStyleAnalyzer.SimpleDrag
		{
			Style = DragStyle.Simple,
			Speed = new ActionDragStyleAnalyzer.Range(1f, 2f)
		}
	};

	private const float TwitchMaxPeriodRate = 1.3f;

	private static readonly ActionDragStyleAnalyzer.ComplexDrag[] ComplexDrags = new ActionDragStyleAnalyzer.ComplexDrag[]
	{
		new ActionDragStyleAnalyzer.ComplexDrag
		{
			Style = DragStyle.StopNGo,
			Amplitude = new ActionDragStyleAnalyzer.Range(0.15f, 0.65f),
			PeriodRate = new ActionDragStyleAnalyzer.Range(0.2f, 1f),
			ActiveChars = new ActionDragStyleAnalyzer.PeriodChars
			{
				IsReeling = new bool?(true),
				ReelingSpeed = new ActionDragStyleAnalyzer.RangeInt(1, 3),
				IsStriking = new bool?(false)
			},
			IdleChars = new ActionDragStyleAnalyzer.PeriodChars
			{
				IsStriking = new bool?(false)
			}
		},
		new ActionDragStyleAnalyzer.ComplexDrag
		{
			Style = DragStyle.Rise,
			Amplitude = new ActionDragStyleAnalyzer.Range(0.1f, 0.8f),
			PeriodRate = new ActionDragStyleAnalyzer.Range(0.05f, 0.6f),
			ActiveChars = new ActionDragStyleAnalyzer.PeriodChars
			{
				IsStriking = new bool?(true)
			},
			IdleChars = new ActionDragStyleAnalyzer.PeriodChars
			{
				IsReeling = new bool?(true),
				ReelingSpeed = new ActionDragStyleAnalyzer.RangeInt(1, 3),
				IsLying = new bool?(true)
			}
		},
		new ActionDragStyleAnalyzer.ComplexDrag
		{
			Style = DragStyle.Twitch,
			Amplitude = new ActionDragStyleAnalyzer.Range(0.05f, 0.3f),
			PeriodRate = new ActionDragStyleAnalyzer.Range(0.1f, 1.3f),
			ActiveChars = new ActionDragStyleAnalyzer.PeriodChars
			{
				IsStriking = new bool?(true)
			},
			IdleChars = new ActionDragStyleAnalyzer.PeriodChars
			{
				IsReeling = new bool?(true),
				ReelingSpeed = new ActionDragStyleAnalyzer.RangeInt(1, 3),
				IsLying = new bool?(false)
			}
		}
	};

	private static readonly ActionDragStyleAnalyzer.ComplexDrag[] TopWaterPopperDrags = new ActionDragStyleAnalyzer.ComplexDrag[]
	{
		new ActionDragStyleAnalyzer.ComplexDrag
		{
			Style = DragStyle.Popping,
			Amplitude = new ActionDragStyleAnalyzer.Range(0f, 0.3f),
			PeriodRate = new ActionDragStyleAnalyzer.Range(0.1f, 1.5f),
			ActiveChars = new ActionDragStyleAnalyzer.PeriodChars
			{
				IsStriking = new bool?(true)
			},
			IdleChars = new ActionDragStyleAnalyzer.PeriodChars
			{
				IsReeling = new bool?(true),
				ReelingSpeed = new ActionDragStyleAnalyzer.RangeInt(1, 3),
				IsLying = new bool?(false)
			}
		}
	};

	private static readonly ActionDragStyleAnalyzer.ComplexDrag[] TopWaterWalkerDrags = new ActionDragStyleAnalyzer.ComplexDrag[]
	{
		new ActionDragStyleAnalyzer.ComplexDrag
		{
			Style = DragStyle.Walking,
			Amplitude = new ActionDragStyleAnalyzer.Range(0f, 0.3f),
			PeriodRate = new ActionDragStyleAnalyzer.Range(0.1f, 2f),
			ActiveChars = new ActionDragStyleAnalyzer.PeriodChars
			{
				IsStriking = new bool?(true)
			},
			IdleChars = new ActionDragStyleAnalyzer.PeriodChars
			{
				IsReeling = new bool?(true),
				ReelingSpeed = new ActionDragStyleAnalyzer.RangeInt(1, 3),
				IsLying = new bool?(false)
			}
		}
	};

	private DragStyleSet dragSet;

	private bool isBeingPulled;

	private float currentPeriodLength;

	private Vector3? priorPosition;

	private float? priorTime;

	public float AvgAmplitude;

	public float AvgSpeed;

	private float minDepth;

	private float maxDepth;

	private float currentHorizontalVelocity;

	private bool isReeling;

	private bool isStriking;

	private float lastStrikeTimeStamp;

	private int strikeCounter;

	private bool strikePeriod;

	private bool hasFullStrike;

	private int reelingSpeed;

	private bool isLying;

	private readonly List<ActionDragStyleAnalyzer.Period> history = new List<ActionDragStyleAnalyzer.Period>();

	public float AvgMoveVsIdle;

	public float LastIdlePeriodLength;

	public float LastMovingPeriodLength;

	private readonly TendencyTracker hVelocityAverager = new TendencyTracker(3, 0.15f);

	private readonly Averager vVelocityAverager = new Averager(5);

	private const float MinSimplePeriodLength = 3f;

	private const float MinVelocity = 0.01f;

	public class ActionStatus
	{
		public Vector3 TacklePosition;

		public bool IsStriking;

		public bool HasFullStrike;

		public bool IsLying;

		public bool IsReeling;

		public int ReelingSpeed;
	}

	private class Period
	{
		public bool IsIdle;

		public float VerticalAmplitude;

		public float TimeLength;

		public bool IsStriking;

		public bool HasFullStrike;

		public bool IsLying;

		public bool IsReeling;

		public int ReelingSpeed;
	}

	private class SimpleDrag
	{
		public DragStyle Style;

		public ActionDragStyleAnalyzer.Range Speed;
	}

	private class ComplexDrag
	{
		public DragStyle Style;

		public ActionDragStyleAnalyzer.Range PeriodRate;

		public ActionDragStyleAnalyzer.Range Amplitude;

		public ActionDragStyleAnalyzer.PeriodChars ActiveChars;

		public ActionDragStyleAnalyzer.PeriodChars IdleChars;
	}

	private class PeriodChars
	{
		public bool? IsStriking;

		public bool? HasFullStrike;

		public bool? IsLying;

		public bool? IsReeling;

		public ActionDragStyleAnalyzer.RangeInt ReelingSpeed;
	}

	private class Range
	{
		public Range(float min, float max)
		{
			this.Min = min;
			this.Max = max;
		}

		public readonly float Min;

		public readonly float Max;
	}

	private class RangeInt
	{
		public RangeInt(int min, int max)
		{
			this.Min = min;
			this.Max = max;
		}

		public readonly int Min;

		public readonly int Max;
	}
}
