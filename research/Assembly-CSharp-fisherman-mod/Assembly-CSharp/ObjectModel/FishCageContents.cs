using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public class FishCageContents
	{
		public FishCageContents()
		{
			this.fishList = new List<CaughtFish>();
		}

		[JsonProperty]
		public List<CaughtFish> Fish
		{
			get
			{
				return this.fishList;
			}
		}

		public FishCage Cage { get; set; }

		public bool CanAdd(Fish fish)
		{
			this.LastVerificationError = null;
			if (this.Cage == null)
			{
				this.LastVerificationError = "No fish cage";
				return false;
			}
			if (fish.Weight > this.Cage.MaxFishWeight)
			{
				this.LastVerificationError = "Max weight exceeded";
				return false;
			}
			float? weight = this.Weight;
			if (((weight == null) ? 0f : weight.Value) > this.Cage.TotalWeight)
			{
				this.LastVerificationError = "Cage is full";
				return false;
			}
			if (this.Cage.Durability <= 0)
			{
				this.LastVerificationError = "Cage is fully weared";
				return false;
			}
			return true;
		}

		public int Count
		{
			get
			{
				return this.fishList.Count;
			}
		}

		public void Clear()
		{
			this.fishList.Clear();
		}

		public CaughtFish TakeFish(CaughtFish caughtFish)
		{
			if (this.CanAdd(caughtFish.Fish))
			{
				this.fishList.Add(caughtFish);
				return caughtFish;
			}
			return null;
		}

		public bool ReleaseFish(Guid fishId)
		{
			if (this.Cage.Safety)
			{
				return true;
			}
			this.LastVerificationError = "Cage damages fish. No release possible.";
			return false;
		}

		public void RemoveFish(CaughtFish fish)
		{
			if (fish != null)
			{
				this.fishList.Remove(fish);
			}
		}

		public string LastVerificationError { get; private set; }

		public int? SilverCost
		{
			get
			{
				return this.GetSilverCost();
			}
		}

		public int? GoldCost
		{
			get
			{
				return this.GetGoldCost();
			}
		}

		public float? Weight
		{
			get
			{
				return this.GetWeight();
			}
		}

		public CaughtFish FindFishById(Guid fishId)
		{
			foreach (CaughtFish caughtFish in this.fishList)
			{
				if (caughtFish.Fish != null && caughtFish.Fish.InstanceId == fishId)
				{
					return caughtFish;
				}
			}
			throw new InvalidOperationException("Fish with Id " + fishId + " not found in cage!");
		}

		private int? GetSilverCost()
		{
			int? num = null;
			for (int i = 0; i < this.fishList.Count; i++)
			{
				CaughtFish caughtFish = this.fishList[i];
				if (caughtFish.Fish != null && caughtFish.Fish.SilverCost != null)
				{
					num = new int?(((num == null) ? 0 : num.Value) + (int)caughtFish.Fish.SilverCost.Value);
				}
			}
			return num;
		}

		private int? GetGoldCost()
		{
			int? num = null;
			for (int i = 0; i < this.fishList.Count; i++)
			{
				CaughtFish caughtFish = this.fishList[i];
				if (caughtFish.Fish != null && caughtFish.Fish.GoldCost != null)
				{
					num = new int?(((num == null) ? 0 : num.Value) + (int)caughtFish.Fish.GoldCost.Value);
				}
			}
			return num;
		}

		private float? GetWeight()
		{
			float? num = null;
			for (int i = 0; i < this.fishList.Count; i++)
			{
				CaughtFish caughtFish = this.fishList[i];
				if (caughtFish.Fish != null)
				{
					num = new float?(((num == null) ? 0f : num.Value) + caughtFish.Fish.Weight);
				}
			}
			return num;
		}

		public float SumFishExperience()
		{
			float num = 0f;
			foreach (CaughtFish caughtFish in this.Fish)
			{
				if (caughtFish.Fish.Experience != null)
				{
					num += caughtFish.Fish.Experience.Value;
				}
			}
			return num;
		}

		private readonly List<CaughtFish> fishList;
	}
}
