using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ObjectModel;

namespace BiteEditor.ObjectModel
{
	public class AttractorsGroup : IAttractorGroup
	{
		public AttractorsGroup(AttractorsMovement movementType, byte minActiveCount, byte maxActiveCount, int attractionShapeId, int[] fishGroupsIds, AttractorResultType resultType)
		{
			this.MovementType = movementType;
			this.MinActiveCount = minActiveCount;
			this.MaxActiveCount = maxActiveCount;
			this.AttractionShapeId = attractionShapeId;
			this.ResultType = resultType;
			this._fishGroupsIds = new int[fishGroupsIds.Length];
			fishGroupsIds.CopyTo(this._fishGroupsIds, 0);
			this.Attractors = new List<IAttractor>();
		}

		public AttractorsGroup()
		{
		}

		[JsonProperty]
		public AttractorsMovement MovementType { get; private set; }

		[JsonProperty]
		public byte MinActiveCount { get; private set; }

		[JsonProperty]
		public byte MaxActiveCount { get; private set; }

		[JsonProperty]
		public int AttractionShapeId { get; private set; }

		[JsonProperty]
		public List<IAttractor> Attractors { get; private set; }

		[JsonProperty]
		public AttractorResultType ResultType { get; private set; }

		[JsonIgnore]
		public int[] FishGroupIds
		{
			get
			{
				return this._fishGroupsIds;
			}
		}

		[JsonIgnore]
		public IFishGroup[] FishGroups { get; private set; }

		[JsonIgnore]
		public SimpleCurve AttractionShape { get; private set; }

		public AttractorsGroup Copy()
		{
			AttractorsGroup attractorsGroup = new AttractorsGroup(this.MovementType, this.MinActiveCount, this.MaxActiveCount, this.AttractionShapeId, this.FishGroupIds, this.ResultType);
			foreach (IAttractor attractor in this.Attractors)
			{
				attractorsGroup.Attractors.Add(attractor);
			}
			return attractorsGroup;
		}

		public void FinishInitialization(Pond pond)
		{
			this.AttractionShape = pond.FindCurve(this.AttractionShapeId).CurveShape;
			this.FishGroups = new IFishGroup[this._fishGroupsIds.Length];
			for (int i = 0; i < this._fishGroupsIds.Length; i++)
			{
				this.FishGroups[i] = pond.FindFishGroup(this._fishGroupsIds[i]);
			}
			this._groupController = new AttractorsGroupController(this, null);
		}

		public void FindClosestAttractors(Vector2f pos, int progress, int rndSeed, List<AttractorsGroupController.ChartObj.AttractionData> result)
		{
			this._groupController.FindClosestAttractors(pos, progress, result, new int?(rndSeed));
		}

		public void FindActiveFishZones(Vector2f playerPos, int progress, int rndSeed, HashSet<ushort> result)
		{
			this._groupController.FindActiveFishZones(playerPos, progress, rndSeed, result);
		}

		public void AddAttractor(Attractor attractor)
		{
			this.Attractors.Add(attractor);
		}

		[JsonProperty]
		private int[] _fishGroupsIds;

		[JsonIgnore]
		private AttractorsGroupController _groupController;
	}
}
