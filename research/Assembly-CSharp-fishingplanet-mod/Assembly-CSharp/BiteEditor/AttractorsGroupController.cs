using System;
using System.Collections.Generic;
using System.Linq;
using BiteEditor.ObjectModel;
using ObjectModel;

namespace BiteEditor
{
	public class AttractorsGroupController
	{
		public AttractorsGroupController(IAttractorGroup context, int? seed = null)
		{
			this._context = context;
			this._rndSeed = ((seed == null) ? ((int)(DateTime.Now.Ticks % 2147483647L)) : seed.Value);
			this._simulator = new AttractorsGroupController.ChartSimulator();
		}

		public AttractorsGroupController.ChartObj[] FindAttractors(int progress, FishName fish)
		{
			this.UpdateChartSimulator(progress, null);
			return this._simulator.GetAffectedAttractors(fish);
		}

		public void FindClosestAttractors(Vector2f pos, int progress, List<AttractorsGroupController.ChartObj.AttractionData> result, int? rndSeed = null)
		{
			this.UpdateChartSimulator(progress, rndSeed);
			this._simulator.GetAttractorsAtPoint(pos, progress, result);
		}

		public void FindActiveFishZones(Vector2f playerPos, int progress, int rndSeed, HashSet<ushort> result)
		{
			this.UpdateChartSimulator(progress, new int?(rndSeed));
			this._simulator.FindActiveFishZones(playerPos, progress, result);
		}

		private void PushAttractorToSimulator(int i, int time)
		{
			byte b = this._freeIndices[i];
			this._freeIndices.RemoveAt(i);
			int num = this._rnd.Next(this._context.FishGroups.Length);
			this._simulator.Push(new AttractorsGroupController.ChartObj(b, this._context.Attractors[(int)b], this._rnd, time, this._context.FishGroups[num], this._context));
		}

		private int NextFreeIndex
		{
			get
			{
				AttractorsMovement movementType = this._context.MovementType;
				if (movementType == AttractorsMovement.Random)
				{
					return this._rnd.Next(this._freeIndices.Count);
				}
				if (movementType != AttractorsMovement.Circle && movementType != AttractorsMovement.BackCircle)
				{
					return -1;
				}
				return 0;
			}
		}

		private void UpdateChartSimulator(int minutes, int? rndSeed = null)
		{
			this._simulator.Restart();
			this._rnd = new Random((rndSeed == null) ? this._rndSeed : rndSeed.Value);
			this._activeCount = (byte)Math.Min(this._rnd.Next((int)this._context.MinActiveCount, (int)(this._context.MaxActiveCount + 1)), this._context.Attractors.Count - 1);
			this._freeIndices.Clear();
			if (this._context.MovementType == AttractorsMovement.Random)
			{
				byte b = 0;
				while ((int)b < this._context.Attractors.Count)
				{
					this._freeIndices.Add(b);
					b += 1;
				}
			}
			else
			{
				byte b2 = (byte)this._rnd.Next(this._context.Attractors.Count);
				b2 = 0;
				this._freeIndices.Add(b2);
				if (this._context.MovementType == AttractorsMovement.Circle)
				{
					byte b3 = b2 + 1;
					while ((int)b3 < this._context.Attractors.Count)
					{
						this._freeIndices.Add(b3);
						b3 += 1;
					}
					for (byte b4 = 0; b4 < b2; b4 += 1)
					{
						this._freeIndices.Add(b4);
					}
				}
				else if (this._context.MovementType == AttractorsMovement.BackCircle)
				{
					for (short num = (short)(b2 - 1); num >= 0; num -= 1)
					{
						this._freeIndices.Add((byte)num);
					}
					for (byte b5 = (byte)(this._context.Attractors.Count - 1); b5 > b2; b5 -= 1)
					{
						this._freeIndices.Add(b5);
					}
				}
			}
			for (int i = 0; i < (int)this._activeCount; i++)
			{
				int nextFreeIndex = this.NextFreeIndex;
				this.PushAttractorToSimulator(nextFreeIndex, 0);
			}
			while (this._simulator.HasOutdatedObj(minutes))
			{
				AttractorsGroupController.ChartObj chartObj = this._simulator.Pop();
				int nextFreeIndex2 = this.NextFreeIndex;
				this.PushAttractorToSimulator(nextFreeIndex2, chartObj.ToTime + 1);
				this._freeIndices.Add(chartObj.AttractorIndex);
			}
		}

		private byte _activeCount;

		private IAttractorGroup _context;

		private int _rndSeed;

		private Random _rnd;

		private List<byte> _freeIndices = new List<byte>();

		private AttractorsGroupController.ChartSimulator _simulator;

		public struct ChartObj
		{
			public ChartObj(byte attractorIndex, IAttractor attractor, Random rnd, int initialTime, IFishGroup fishGroup, IAttractorGroup context)
			{
				this = default(AttractorsGroupController.ChartObj);
				this.Attractor = new AttractorData(attractor, rnd);
				this.FishGroup = fishGroup;
				this.ResultType = context.ResultType;
				this._attractionShape = context.AttractionShape;
				this.AttractorIndex = attractorIndex;
				this._fromTime = ((initialTime != 0) ? initialTime : (initialTime - rnd.Next(attractor.InitialPhase + 1)));
				this._maxPowerRndDuration = rnd.Next(attractor.MaxPowerRandomDuration + 1);
				this.ToTime = this._fromTime + attractor.FillTime * 2 + attractor.MaxPowerDuration + this._maxPowerRndDuration + attractor.SwitchDelay;
				this._linkedZones = new ushort[attractor.LinkedZones.Length];
				attractor.LinkedZones.CopyTo(this._linkedZones, 0);
			}

			public float Attraction { get; private set; }

			public AttractorsGroupController.ChartObj.AttractionData EvaluateMaxAttraction(int time)
			{
				int num = time - this._fromTime;
				if (num < this.Attractor.FillTime)
				{
					float num2 = this._attractionShape.Evaluate((float)num / (float)this.Attractor.FillTime);
					return new AttractorsGroupController.ChartObj.AttractionData(AttractorsGroupController.ChartObj.Phase.Inc, this.ResultType, this.FishGroup, this._linkedZones, this.Attractor.AttractionMinValue * (1f - num2) + this.Attractor.AttractionMaxValue * num2);
				}
				if (num < this.Attractor.FillTime + this.Attractor.MaxPowerDuration + this._maxPowerRndDuration)
				{
					return new AttractorsGroupController.ChartObj.AttractionData(AttractorsGroupController.ChartObj.Phase.Plato, this.ResultType, this.FishGroup, this._linkedZones, this.Attractor.AttractionMaxValue);
				}
				if (num < 2 * this.Attractor.FillTime + this.Attractor.MaxPowerDuration + this._maxPowerRndDuration)
				{
					num -= this.Attractor.FillTime + this.Attractor.MaxPowerDuration + this._maxPowerRndDuration;
					float num3 = this._attractionShape.Evaluate(1f - (float)num / (float)this.Attractor.FillTime);
					return new AttractorsGroupController.ChartObj.AttractionData(AttractorsGroupController.ChartObj.Phase.Dec, this.ResultType, this.FishGroup, this._linkedZones, this.Attractor.AttractionMinValue * (1f - num3) + this.Attractor.AttractionMaxValue * num3);
				}
				return new AttractorsGroupController.ChartObj.AttractionData(AttractorsGroupController.ChartObj.Phase.Delay, this.ResultType, this.FishGroup, this._linkedZones, 0f);
			}

			public AttractorsGroupController.ChartObj.AttractionData UpdateAttraction(Vector2f pos, int time)
			{
				AttractorsGroupController.ChartObj.AttractionData attractionData = this.EvaluateMaxAttraction(time);
				if (attractionData.Phase != AttractorsGroupController.ChartObj.Phase.Delay)
				{
					float magnitude = (this.Attractor.Position2D - pos).Magnitude;
					float num = magnitude / this.Attractor.AttractionR;
					return attractionData.Update(this.Attractor.AttractionMinValue * num + attractionData.Value * (1f - num));
				}
				return attractionData;
			}

			public readonly int ToTime;

			public readonly byte AttractorIndex;

			public readonly IFishGroup FishGroup;

			public readonly AttractorData Attractor;

			public AttractorResultType ResultType;

			private SimpleCurve _attractionShape;

			private readonly int _fromTime;

			private readonly int _maxPowerRndDuration;

			private ushort[] _linkedZones;

			public enum Phase
			{
				Inc,
				Plato,
				Dec,
				Delay
			}

			public struct AttractionData
			{
				public AttractionData(AttractorsGroupController.ChartObj.Phase phase, AttractorResultType resultType, IFishGroup fishGroup, ushort[] linkedZones, float value = 0f)
				{
					this = default(AttractorsGroupController.ChartObj.AttractionData);
					this.Phase = phase;
					this.Value = value;
					this.ResultType = resultType;
					this.FishGroup = fishGroup;
					this.LinkedZones = new ushort[linkedZones.Length];
					linkedZones.CopyTo(this.LinkedZones, 0);
				}

				public AttractorsGroupController.ChartObj.AttractionData Update(float value)
				{
					return new AttractorsGroupController.ChartObj.AttractionData(this.Phase, this.ResultType, this.FishGroup, this.LinkedZones, value);
				}

				public readonly AttractorsGroupController.ChartObj.Phase Phase;

				public readonly float Value;

				public readonly AttractorResultType ResultType;

				public readonly IFishGroup FishGroup;

				public readonly ushort[] LinkedZones;
			}
		}

		private class ChartSimulator
		{
			public AttractorsGroupController.ChartObj[] GetAffectedAttractors(FishName fish)
			{
				return this._objs.Where((AttractorsGroupController.ChartObj o) => o.FishGroup.Fish.Any((FishGroup.Record r) => r.FishName == fish)).ToArray<AttractorsGroupController.ChartObj>();
			}

			public void Restart()
			{
				this._objs.Clear();
			}

			public void Push(AttractorsGroupController.ChartObj obj)
			{
				if (this._objs.Count == 0)
				{
					this._objs.Add(obj);
				}
				else
				{
					int num = this._objs.FindIndex((AttractorsGroupController.ChartObj o) => o.ToTime > obj.ToTime);
					if (num == -1)
					{
						this._objs.Add(obj);
					}
					else
					{
						this._objs.Insert(num, obj);
					}
				}
			}

			public bool HasOutdatedObj(int time)
			{
				return this._objs[0].ToTime < time;
			}

			public AttractorsGroupController.ChartObj Pop()
			{
				AttractorsGroupController.ChartObj chartObj = this._objs[0];
				this._objs.RemoveAt(0);
				return chartObj;
			}

			public void GetAttractorsAtPoint(Vector2f pos, int progress, List<AttractorsGroupController.ChartObj.AttractionData> result)
			{
				for (int i = 0; i < this._objs.Count; i++)
				{
					float magnitude = (this._objs[i].Attractor.Position2D - pos).Magnitude;
					if (magnitude < this._objs[i].Attractor.AttractionR)
					{
						AttractorsGroupController.ChartObj.AttractionData attractionData = this._objs[i].UpdateAttraction(pos, progress);
						if (attractionData.Phase != AttractorsGroupController.ChartObj.Phase.Delay)
						{
							result.Add(attractionData);
						}
					}
				}
			}

			public void FindActiveFishZones(Vector2f pos, int progress, HashSet<ushort> result)
			{
				for (int i = 0; i < this._objs.Count; i++)
				{
					float magnitude = (this._objs[i].Attractor.Position2D - pos).Magnitude;
					if (magnitude < 100f)
					{
						AttractorsGroupController.ChartObj.AttractionData attractionData = this._objs[i].UpdateAttraction(pos, progress);
						if (attractionData.Phase == AttractorsGroupController.ChartObj.Phase.Plato)
						{
							foreach (ushort num in attractionData.LinkedZones)
							{
								result.Add(num);
							}
						}
					}
				}
			}

			private List<AttractorsGroupController.ChartObj> _objs = new List<AttractorsGroupController.ChartObj>();
		}
	}
}
