using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class TrajectoryDragStyleAnalyzer
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

	public void Reset()
	{
		this.style = DragStyle.Undefined;
		this.quality = 0f;
		this.LastIdlePeriodLength = 0f;
		this.LastMovingPeriodLength = 0f;
		this.priorPosition = null;
		this.history.Clear();
		this.vVelocityAverager.Clear();
		this.ResetPeriod();
	}

	public void FinishPeriod()
	{
		TrajectoryDragStyleAnalyzer.Period period = new TrajectoryDragStyleAnalyzer.Period
		{
			IsIdle = !this.isBeingPolled,
			TimeLength = this.currentPeriodLength,
			VerticalAmplitude = this.minDepth - this.maxDepth
		};
		this.AddPeriod(period);
		this.ResetPeriod();
	}

	private void ResetPeriod()
	{
		this.currentPeriodLength = 0f;
		this.minDepth = float.NegativeInfinity;
		this.maxDepth = 0f;
		this.avgSpeed = 0f;
	}

	public void Update(Vector3 tacklePosition)
	{
		Vector3? vector = this.priorPosition;
		if (vector == null)
		{
			this.priorPosition = new Vector3?(tacklePosition);
			this.priorTime = new float?(Time.deltaTime);
			return;
		}
		if (this.priorPosition == tacklePosition && this.priorTime == Time.deltaTime)
		{
			return;
		}
		Vector3 vector2 = (tacklePosition - this.priorPosition.Value) / Time.deltaTime;
		float num = vector2.y;
		if (Mathf.Abs(num) < 0.01f)
		{
			num = 0f;
		}
		bool flag = this.hVelocityAverager.UpdateAndGet(num, tacklePosition.y);
		Vector2 vector3;
		vector3..ctor(vector2.x, vector2.z);
		float num2 = vector3.magnitude;
		if (num2 < 0.01f)
		{
			num2 = 0f;
		}
		num2 = this.vVelocityAverager.UpdateAndGet(num2);
		this.currentPeriodLength += Time.deltaTime;
		if (this.minDepth < tacklePosition.y)
		{
			this.minDepth = tacklePosition.y;
		}
		if (this.maxDepth > tacklePosition.y)
		{
			this.maxDepth = tacklePosition.y;
		}
		if (this.isBeingPolled != flag)
		{
			this.FinishPeriod();
		}
		else
		{
			if (flag)
			{
				this.avgSpeed = (this.avgSpeed * (this.currentPeriodLength - Time.deltaTime) + num2 * Time.deltaTime) / this.currentPeriodLength;
			}
			this.PerformCurrentPeriodAnalysis(this.currentPeriodLength, flag);
		}
		this.isBeingPolled = flag;
		this.priorPosition = new Vector3?(tacklePosition);
		this.priorTime = new float?(Time.deltaTime);
		if (this.priorStyle != this.style)
		{
			GameFactory.Player.HudFishingHandler.LurePositionHandlerContinuous.ShowDragStyleText(this.GetFineDragStyleName(this.style), this.quality);
		}
		this.priorStyle = this.style;
	}

	private string GetFineDragStyleName(DragStyle dragStyle)
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
		default:
			return string.Empty;
		}
	}

	private void AddPeriod(TrajectoryDragStyleAnalyzer.Period period)
	{
		this.PeriodNumber++;
		if (this.history.Count == 4)
		{
			this.history.RemoveAt(0);
		}
		this.history.Add(period);
		if (this.history.Count((TrajectoryDragStyleAnalyzer.Period i) => i.IsIdle) == 2)
		{
			if (this.history.Count((TrajectoryDragStyleAnalyzer.Period i) => !i.IsIdle) == 2)
			{
				this.AvgAmplitude = this.history.Average((TrajectoryDragStyleAnalyzer.Period i) => i.VerticalAmplitude);
				float num = this.history.Where((TrajectoryDragStyleAnalyzer.Period i) => i.IsIdle).Average((TrajectoryDragStyleAnalyzer.Period i) => i.TimeLength);
				float num2 = this.history.Where((TrajectoryDragStyleAnalyzer.Period i) => !i.IsIdle).Average((TrajectoryDragStyleAnalyzer.Period i) => i.TimeLength);
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
				return;
			}
		}
	}

	private void PerformPeriodsAnalysis()
	{
		if (this.history.Count < 4)
		{
			return;
		}
		TrajectoryDragStyleAnalyzer.ComplexDrag complexDrag = TrajectoryDragStyleAnalyzer.ComplexDrags.FirstOrDefault((TrajectoryDragStyleAnalyzer.ComplexDrag i) => this.AvgAmplitude > i.Amplitude.Min && this.AvgAmplitude < i.Amplitude.Max && this.AvgMoveVsIdle > i.PeriodRate.Min && this.AvgMoveVsIdle < i.PeriodRate.Max);
		this.quality = this.CalculateQuality(complexDrag);
		this.style = ((complexDrag == null) ? DragStyle.Undefined : complexDrag.Style);
	}

	private float CalculateQuality(TrajectoryDragStyleAnalyzer.ComplexDrag matchedComplexStyle)
	{
		if (matchedComplexStyle == null)
		{
			return 0f;
		}
		float num = matchedComplexStyle.Amplitude.Max - matchedComplexStyle.Amplitude.Min;
		float num2 = matchedComplexStyle.Amplitude.Max - num;
		float num3 = matchedComplexStyle.PeriodRate.Max - matchedComplexStyle.PeriodRate.Min;
		float num4 = matchedComplexStyle.PeriodRate.Max - num3;
		float num5 = (Mathf.Abs(this.AvgAmplitude - num) / num2 + Mathf.Abs(this.AvgMoveVsIdle - num3) / num4) / 2f;
		return Mathf.Clamp01((num5 >= 1f) ? (2f - num5) : num5);
	}

	private void PerformCurrentPeriodAnalysis(float periodLength, bool isMoving)
	{
		if (isMoving && periodLength > 3f)
		{
			TrajectoryDragStyleAnalyzer.SimpleDrag simpleDrag = TrajectoryDragStyleAnalyzer.SimpleDrags.FirstOrDefault((TrajectoryDragStyleAnalyzer.SimpleDrag i) => this.avgSpeed > i.Speed.Min && this.avgSpeed < i.Speed.Max);
			this.quality = (float)((simpleDrag == null) ? 0 : 1);
			this.style = ((simpleDrag == null) ? DragStyle.Undefined : simpleDrag.Style);
		}
		else if (!isMoving && this.LastIdlePeriodLength > 0f && periodLength > this.LastIdlePeriodLength * 2f)
		{
			this.style = DragStyle.Undefined;
		}
		else if (isMoving && this.LastMovingPeriodLength > 0f && periodLength > this.LastMovingPeriodLength * 2f)
		{
			this.style = DragStyle.Undefined;
		}
	}

	private DragStyle style;

	private DragStyle priorStyle;

	private float quality;

	private bool isBeingPolled;

	private float currentPeriodLength;

	private Vector3? priorPosition;

	private float? priorTime;

	private float minDepth;

	private float maxDepth;

	private float avgSpeed;

	private readonly List<TrajectoryDragStyleAnalyzer.Period> history = new List<TrajectoryDragStyleAnalyzer.Period>();

	public float AvgAmplitude;

	public float AvgMoveVsIdle;

	public float LastIdlePeriodLength;

	public float LastMovingPeriodLength;

	public int PeriodNumber;

	private readonly TendencyTracker hVelocityAverager = new TendencyTracker(3, 0.15f);

	private readonly Averager vVelocityAverager = new Averager(5);

	private const float MinSimplePeriodLength = 3f;

	private const float MinVelocity = 0.01f;

	private static readonly TrajectoryDragStyleAnalyzer.SimpleDrag[] SimpleDrags = new TrajectoryDragStyleAnalyzer.SimpleDrag[]
	{
		new TrajectoryDragStyleAnalyzer.SimpleDrag
		{
			Style = DragStyle.SlowSimple,
			Speed = new TrajectoryDragStyleAnalyzer.Range(0.5f, 1f)
		},
		new TrajectoryDragStyleAnalyzer.SimpleDrag
		{
			Style = DragStyle.Simple,
			Speed = new TrajectoryDragStyleAnalyzer.Range(1f, 2f)
		}
	};

	private static readonly TrajectoryDragStyleAnalyzer.ComplexDrag[] ComplexDrags = new TrajectoryDragStyleAnalyzer.ComplexDrag[]
	{
		new TrajectoryDragStyleAnalyzer.ComplexDrag
		{
			Style = DragStyle.StopNGo,
			PeriodRate = new TrajectoryDragStyleAnalyzer.Range(0.8f, 1.2f),
			Amplitude = new TrajectoryDragStyleAnalyzer.Range(0.25f, 1f)
		},
		new TrajectoryDragStyleAnalyzer.ComplexDrag
		{
			Style = DragStyle.Rise,
			PeriodRate = new TrajectoryDragStyleAnalyzer.Range(0.2f, 0.8f),
			Amplitude = new TrajectoryDragStyleAnalyzer.Range(0.2f, 0.5f)
		},
		new TrajectoryDragStyleAnalyzer.ComplexDrag
		{
			Style = DragStyle.Twitch,
			PeriodRate = new TrajectoryDragStyleAnalyzer.Range(0.7f, 1.1f),
			Amplitude = new TrajectoryDragStyleAnalyzer.Range(0.05f, 0.2f)
		}
	};

	private class Period
	{
		public bool IsIdle;

		public float VerticalAmplitude;

		public float TimeLength;
	}

	private class SimpleDrag
	{
		public DragStyle Style;

		public TrajectoryDragStyleAnalyzer.Range Speed;
	}

	private class ComplexDrag
	{
		public DragStyle Style;

		public TrajectoryDragStyleAnalyzer.Range PeriodRate;

		public TrajectoryDragStyleAnalyzer.Range Amplitude;
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
}
