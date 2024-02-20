using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using ObjectModel;

namespace BiteEditor.ObjectModel
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Attractor : IAttractor
	{
		public Attractor(float attractionMinValue, float attractionMaxValue, int initialPhase, int fillTime, int maxPowerDuration, int maxPowerRandomDuration, int switchDelay, Vector2f position2D, float attractionR, float randomR, ushort[] linkedZones)
		{
			this = default(Attractor);
			this.AttractionMinValue = attractionMinValue;
			this.AttractionMaxValue = attractionMaxValue;
			this.InitialPhase = initialPhase;
			this.FillTime = fillTime;
			this.MaxPowerDuration = maxPowerDuration;
			this.MaxPowerRandomDuration = maxPowerRandomDuration;
			this.SwitchDelay = switchDelay;
			this.Position2D = position2D;
			this.AttractionR = attractionR;
			this.RandomR = randomR;
			if (linkedZones != null)
			{
				this.LinkedZones = new ushort[linkedZones.Length];
				linkedZones.CopyTo(this.LinkedZones, 0);
			}
			else
			{
				this.LinkedZones = new ushort[0];
			}
		}

		[JsonProperty]
		public float AttractionMinValue { get; private set; }

		[JsonProperty]
		public float AttractionMaxValue { get; private set; }

		[JsonProperty]
		public int InitialPhase { get; private set; }

		[JsonProperty]
		public int FillTime { get; private set; }

		[JsonProperty]
		public int MaxPowerDuration { get; private set; }

		[JsonProperty]
		public int MaxPowerRandomDuration { get; private set; }

		[JsonProperty]
		public int SwitchDelay { get; private set; }

		[JsonProperty]
		public Vector2f Position2D { get; private set; }

		[JsonProperty]
		public float AttractionR { get; private set; }

		[JsonProperty]
		public float RandomR { get; private set; }

		[JsonProperty]
		public ushort[] LinkedZones { get; private set; }
	}
}
