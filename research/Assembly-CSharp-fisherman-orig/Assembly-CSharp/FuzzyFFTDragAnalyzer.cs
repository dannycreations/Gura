using System;
using FuzzyLogic;
using ObjectModel;
using UnityEngine;

public class FuzzyFFTDragAnalyzer
{
	public FuzzyFFTDragAnalyzer()
	{
		if (GameFactory.Player.RodSlot.SimThread != null)
		{
			GameFactory.Player.RodSlot.SimThread.EnableMassTracer(100, new int[]
			{
				GameFactory.Player.RodSlot.Sim.RodTipMass.UID,
				GameFactory.Player.RodSlot.Sim.TackleTipMass.UID
			});
		}
		else
		{
			GameFactory.Player.RodSlot.Sim.EnableMassTracer(100, new int[]
			{
				GameFactory.Player.RodSlot.Sim.RodTipMass.UID,
				GameFactory.Player.RodSlot.Sim.TackleTipMass.UID
			});
		}
		this.rodTipFFT = new FFTAnalyzer(256, 32, 2, 4);
		this.tackleFFT = new FFTAnalyzer(256, 32, 2, 3);
		this.tackleVelocityAverager = new Averager(100);
		this.tackleGroundDistanceAverager = new TimedAverager(4f, 200);
	}

	public FuzzyValue Straight { get; private set; }

	public FuzzyValue StopAndGo { get; private set; }

	public FuzzyValue LiftAndDrop { get; private set; }

	public FuzzyValue Twitching { get; private set; }

	public DragStyle DragStyle { get; private set; }

	public float DragQuality { get; private set; }

	public float BufferDurtaion
	{
		get
		{
			return 10.24f;
		}
	}

	public void ResetBuffers()
	{
		this.rodTipFFT.FillBuffer(GameFactory.Player.RodSlot.Sim.Traces[0][0].y);
		this.tackleFFT.FillBuffer(GameFactory.Player.RodSlot.Sim.Traces[1][0].y);
	}

	public bool Update()
	{
		this.tackleVelocityAverager.UpdateAndGet(GameFactory.Player.RodSlot.Sim.TackleTipMass.Velocity.magnitude);
		this.scoreGround = this.tackleGroundDistanceAverager.UpdateAndGet(FuzzyValue.NumberIsBig(GameFactory.Player.RodSlot.Sim.TackleTipMass.Position.y - GameFactory.Player.RodSlot.Sim.TackleTipMass.GroundHeight, 0.05f), Time.deltaTime);
		float[] array = new float[GameFactory.Player.RodSlot.Sim.TraceScanCounter];
		float[] array2 = new float[GameFactory.Player.RodSlot.Sim.TraceScanCounter];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GameFactory.Player.RodSlot.Sim.Traces[0][i].y;
			array2[i] = GameFactory.Player.RodSlot.Sim.Traces[1][i].y;
		}
		if (this.rodTipFFT.Update(array))
		{
			this.scoreRodLow = this.rodTipFFT.LowOscillationsScore;
			this.scoreRodHigh = this.rodTipFFT.HighOscillationsScore;
		}
		if (this.tackleFFT.Update(array2))
		{
			this.scoreTackleLow = this.tackleFFT.LowOscillationsScore;
			this.scoreTackleHigh = this.tackleFFT.HighOscillationsScore;
			if (HUDLinesDrawer.Instance != null)
			{
				HUDLinesDrawer.Instance.Clear();
				float[] array3 = new float[32];
				Array.Copy(this.rodTipFFT.AmplitudesArray(), array3, array3.Length);
				HUDLinesDrawer.Instance.DrawGizmoPlotBars(array3, 0f, 10f, new Vector2(600f, 100f), new Vector2(1000f, 300f), Color.magenta, Color.white, true);
				HUDLinesDrawer.Instance.DrawGizmoPlot(this.rodTipFFT.SignalArray(), -1f, 1f, new Vector2(600f, 100f), new Vector2(1000f, 300f), Color.yellow, Color.white, true);
				array3 = new float[32];
				Array.Copy(this.tackleFFT.AmplitudesArray(), array3, array3.Length);
				HUDLinesDrawer.Instance.DrawGizmoPlotBars(array3, 0f, 10f, new Vector2(100f, 100f), new Vector2(500f, 300f), Color.red, Color.white, true);
				HUDLinesDrawer.Instance.DrawGizmoPlot(this.tackleFFT.SignalArray(), -1f, 1f, new Vector2(100f, 100f), new Vector2(500f, 300f), Color.green, Color.white, true);
			}
			this.updateFuzzyConditions();
			return true;
		}
		return false;
	}

	private void updateFuzzyConditions()
	{
		this.tackleLow = FuzzyValue.NumberIsBig(this.scoreTackleLow, 4f);
		this.tackleHigh = FuzzyValue.NumberIsBig(this.scoreTackleHigh, 4f);
		this.rodLow = FuzzyValue.NumberIsBig(this.scoreRodLow, 3f);
		this.rodHigh = FuzzyValue.NumberIsBig(this.scoreRodHigh, 3f);
		this.tackleVelocity = FuzzyValue.NumberIsBig(this.tackleVelocityAverager.Current, 0.2f);
		this.tackleIsNotOnGround = FuzzyValue.NumberIsBig(this.scoreGround, 0.75f);
		this.LiftAndDrop = (this.tackleLow | this.tackleHigh) & this.tackleVelocity & (this.rodLow | this.rodHigh) & ~this.tackleIsNotOnGround;
		this.StopAndGo = (this.tackleLow | this.tackleHigh) & this.tackleVelocity & ~(this.rodLow | this.rodHigh);
		this.Twitching = (this.tackleLow | this.tackleHigh) & this.tackleVelocity & (this.rodLow | this.rodHigh) & this.tackleIsNotOnGround;
		this.Straight = this.tackleVelocity & ~(this.tackleLow | this.tackleHigh | this.rodLow | this.rodHigh) & this.tackleIsNotOnGround;
		FuzzyValue[] array = new FuzzyValue[] { this.Straight, this.StopAndGo, this.LiftAndDrop, this.Twitching };
		int max = FuzzyValue.GetMax(array);
		this.DragQuality = array[max] * 1.25f;
		if (this.DragQuality > 0.25f)
		{
			switch (max)
			{
			case 0:
				this.DragStyle = DragStyle.Simple;
				break;
			case 1:
				this.DragStyle = DragStyle.StopNGo;
				break;
			case 2:
				this.DragStyle = DragStyle.Rise;
				break;
			case 3:
				this.DragStyle = DragStyle.Twitch;
				break;
			}
		}
		else
		{
			this.DragStyle = DragStyle.Undefined;
			this.DragQuality = 0f;
		}
	}

	public const int BufferSize = 256;

	public const int InputUpdateRate = 100;

	public const int FFTUpdateRate = 32;

	public const int TackleFirstHarmonicLow = 2;

	public const int TackleHarmonicsHalfInterval = 3;

	public const int RodFirstHarmonicLow = 2;

	public const int RodHarmonicsHalfInterval = 4;

	public const float TackleAmpThreshold = 4f;

	public const float RodAmpThreshold = 3f;

	public const float TackleVelocityThreshold = 0.2f;

	public const float TackleGroundDistanceThreshold = 0.05f;

	private FFTAnalyzer rodTipFFT;

	private FFTAnalyzer tackleFFT;

	private Averager tackleVelocityAverager;

	private TimedAverager tackleGroundDistanceAverager;

	private float scoreRodLow;

	private float scoreRodHigh;

	private float scoreTackleLow;

	private float scoreTackleHigh;

	private float scoreGround;

	private FuzzyValue tackleLow;

	private FuzzyValue tackleHigh;

	private FuzzyValue rodLow;

	private FuzzyValue rodHigh;

	private FuzzyValue tackleVelocity;

	private FuzzyValue tackleIsNotOnGround;
}
