using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ObjectModel;
using UnityEngine;

namespace TPM
{
	[Serializable]
	public class TPMAssembledRod : IAssembledRod
	{
		public Rod Rod
		{
			get
			{
				return null;
			}
		}

		public IRod RodInterface
		{
			get
			{
				return this._rod;
			}
		}

		public IReel ReelInterface
		{
			get
			{
				return this._reel;
			}
		}

		public IBell BellInterface
		{
			get
			{
				return this._bell;
			}
		}

		public ILine LineInterface
		{
			get
			{
				return this._line;
			}
		}

		public ILeader LeaderInterface
		{
			get
			{
				return this._leader;
			}
		}

		public IBobber BobberInterface
		{
			get
			{
				return this._bobber;
			}
		}

		public ISinker SinkerInterface
		{
			get
			{
				return this._sinker;
			}
		}

		public IFeeder FeederInterface
		{
			get
			{
				return this._feeder;
			}
		}

		public IHook HookInterface
		{
			get
			{
				return this._hook;
			}
		}

		public IBait BaitInterface
		{
			get
			{
				return this._bait;
			}
		}

		public IChum ChumInterface
		{
			get
			{
				return this._chum;
			}
		}

		public IChum[] ChumInterfaceAll
		{
			get
			{
				return this._chumAll;
			}
		}

		public IQuiverTip QuiverTipInterface
		{
			get
			{
				return this._quiver;
			}
		}

		public ReelTypes ReelType
		{
			get
			{
				return this._reel.ReelType;
			}
		}

		public RodTemplate RodTemplate { get; private set; }

		public int Slot
		{
			get
			{
				return 0;
			}
		}

		public bool IsRodDisassembled
		{
			get
			{
				return false;
			}
		}

		public void ReplaceData(IAssembledRod rodAssembly)
		{
			this._rod.ReplaceData(rodAssembly.RodInterface);
			this._reel.ReplaceData(rodAssembly.ReelInterface);
			this._line.ReplaceData(rodAssembly.LineInterface);
			this._bell = ((rodAssembly.BellInterface == null) ? null : new TPMBell(rodAssembly.BellInterface));
			this._bobber = ((rodAssembly.BobberInterface == null) ? null : new TPMBobber(rodAssembly.BobberInterface));
			this._sinker = ((rodAssembly.SinkerInterface == null) ? null : new TPMSinker(rodAssembly.SinkerInterface));
			this._feeder = ((rodAssembly.FeederInterface == null) ? null : new TPMFeeder(rodAssembly.FeederInterface));
			this._hook = ((rodAssembly.HookInterface == null) ? null : new TPMHook(rodAssembly.HookInterface));
			this._bait = ((rodAssembly.BaitInterface == null) ? null : new TPMBait(rodAssembly.BaitInterface));
			this._chum = ((rodAssembly.ChumInterface == null) ? null : new TPMChum(rodAssembly.ChumInterface));
			TPMChum[] array;
			if (rodAssembly.ChumInterfaceAll != null)
			{
				array = rodAssembly.ChumInterfaceAll.Select((IChum c) => new TPMChum(c)).ToArray<TPMChum>();
			}
			else
			{
				array = null;
			}
			this._chumAll = array;
			this._quiver = ((rodAssembly.QuiverTipInterface == null) ? null : new TPMQuiverTip(rodAssembly.QuiverTipInterface));
			this._leader = ((rodAssembly.LeaderInterface == null) ? null : new TPMLeader(rodAssembly.LeaderInterface));
			this.RodTemplate = rodAssembly.RodTemplate;
		}

		public void Clone(TPMAssembledRod srcRod)
		{
			this._rod.ReplaceData(srcRod.RodInterface);
			this._reel.ReplaceData(srcRod.ReelInterface);
			this._line.ReplaceData(srcRod.LineInterface);
			if (srcRod.BellInterface == null)
			{
				this._bell = null;
			}
			else if (this._bell == null)
			{
				this._bell = new TPMBell(srcRod.BellInterface);
			}
			else
			{
				this._bell.ReplaceData(srcRod.BellInterface);
			}
			if (srcRod.BobberInterface == null)
			{
				this._bobber = null;
			}
			else if (this._bobber == null)
			{
				this._bobber = new TPMBobber(srcRod.BobberInterface);
			}
			else
			{
				this._bobber.ReplaceData(srcRod.BobberInterface);
			}
			if (srcRod.SinkerInterface == null)
			{
				this._sinker = null;
			}
			else if (this._sinker == null)
			{
				this._sinker = new TPMSinker(srcRod.SinkerInterface);
			}
			else
			{
				this._sinker.ReplaceData(srcRod.SinkerInterface);
			}
			if (srcRod.FeederInterface == null)
			{
				this._feeder = null;
			}
			else if (this._feeder == null)
			{
				this._feeder = new TPMFeeder(srcRod.FeederInterface);
			}
			else
			{
				this._feeder.ReplaceData(srcRod.FeederInterface);
			}
			if (srcRod.HookInterface == null)
			{
				this._hook = null;
			}
			else if (this._hook == null)
			{
				this._hook = new TPMHook(srcRod.HookInterface);
			}
			else
			{
				this._hook.ReplaceData(srcRod.HookInterface);
			}
			if (srcRod.BaitInterface == null)
			{
				this._bait = null;
			}
			else if (this._bait == null)
			{
				this._bait = new TPMBait(srcRod.BaitInterface);
			}
			else
			{
				this._bait.ReplaceData(srcRod.BaitInterface);
			}
			if (srcRod.ChumInterface == null)
			{
				this._chum = null;
			}
			else if (this._chum == null)
			{
				this._chum = new TPMChum(srcRod.ChumInterface);
			}
			else
			{
				this._chum.ReplaceData(srcRod.ChumInterface);
			}
			if (srcRod.ChumInterfaceAll == null)
			{
				this._chumAll = null;
			}
			else if (this._chumAll == null || this._chumAll.Length != srcRod.ChumInterfaceAll.Length)
			{
				this._chumAll = srcRod.ChumInterfaceAll.Select((IChum c) => new TPMChum(c)).ToArray<TPMChum>();
			}
			else
			{
				for (int i = 0; i < this._chumAll.Length; i++)
				{
					this._chumAll[i].ReplaceData(srcRod.ChumInterfaceAll[i]);
				}
			}
			if (srcRod.QuiverTipInterface == null)
			{
				this._quiver = null;
			}
			else if (this._quiver == null)
			{
				this._quiver = new TPMQuiverTip(srcRod.QuiverTipInterface);
			}
			else
			{
				this._quiver.ReplaceData(srcRod.QuiverTipInterface);
			}
			if (srcRod.LeaderInterface == null)
			{
				this._leader = null;
			}
			else if (this._leader == null)
			{
				this._leader = new TPMLeader(srcRod.LeaderInterface);
			}
			else
			{
				this._leader.ReplaceData(srcRod.LeaderInterface);
			}
			this.RodTemplate = srcRod.RodTemplate;
			this.RodTransformData = srcRod.RodTransformData;
			this.RodPoints = new List<Vector3>(srcRod.RodPoints);
			this.lineData.ReplaceData(srcRod.lineData);
			this.tackleData.ReplaceData(srcRod.tackleData);
		}

		public bool IsDifferent(TPMAssembledRod obj)
		{
			return this.RodInterface.Asset != obj.RodInterface.Asset || this.ReelInterface.Asset != obj.ReelInterface.Asset || this.LineInterface.Asset != obj.LineInterface.Asset || (this.BellInterface != null && obj.BellInterface != null && this.BellInterface.Asset != obj.BellInterface.Asset) || (this.BobberInterface != null && obj.BobberInterface != null && this.BobberInterface.Asset != obj.BobberInterface.Asset) || (this.SinkerInterface != null && obj.SinkerInterface != null && this.SinkerInterface.Asset != obj.SinkerInterface.Asset) || (this.FeederInterface != null && obj.FeederInterface != null && this.FeederInterface.Asset != obj.FeederInterface.Asset) || (this.HookInterface != null && obj.HookInterface != null && this.HookInterface.Asset != obj.HookInterface.Asset) || (this.BaitInterface != null && obj.BaitInterface != null && this.BaitInterface.Asset != obj.BaitInterface.Asset) || (this.QuiverTipInterface != null && obj.QuiverTipInterface != null && this.QuiverTipInterface.Color != obj.QuiverTipInterface.Color) || (this.LeaderInterface != null && obj.LeaderInterface != null && this.LeaderInterface.Color != obj.LeaderInterface.Color);
		}

		public void UpdateRodData(Transform rodTransform, IList<Vector3> rodPoints)
		{
			if (rodPoints.Count != this.RodPoints.Count)
			{
				this.RodPoints = new List<Vector3>(rodPoints);
			}
			else
			{
				for (int i = 0; i < rodPoints.Count; i++)
				{
					this.RodPoints[i] = rodPoints[i];
				}
			}
			this.RodTransformData.position = rodTransform.position;
			this.RodTransformData.rotation = rodTransform.rotation;
		}

		public void ReadFromStream(Stream stream)
		{
			this._rod.ReadFromStream(stream);
			this._reel.ReadFromStream(stream);
			this._line.ReadFromStream(stream);
			this._bell = TPMBell.ReadFromStream(stream);
			this._bobber = TPMBobber.ReadFromStream(stream);
			this._sinker = TPMSinker.ReadFromStream(stream);
			this._feeder = TPMFeeder.ReadFromStream(stream);
			this._hook = TPMHook.ReadFromStream(stream);
			this._bait = TPMBait.ReadFromStream(stream);
			this._chum = TPMChum.ReadFromStream(stream);
			this._quiver = TPMQuiverTip.ReadFromStream(stream);
			this._leader = TPMLeader.ReadFromStream(stream);
			this.RodTemplate = (RodTemplate)Serializer.ReadByte(stream);
			Serializer.ReadVectorsList(stream, this.RodPoints);
			this.RodTransformData.ReadFromStream(stream);
			this.lineData.ReadFromStream(stream);
			this.tackleData.ReadFromStream(stream);
		}

		public void WriteToStream(Stream stream)
		{
			this._rod.WriteToStream(stream);
			this._reel.WriteToStream(stream);
			this._line.WriteToStream(stream);
			TPMBell.WriteToStream(stream, this._bell);
			TPMBobber.WriteToStream(stream, this._bobber);
			TPMSinker.WriteToStream(stream, this._sinker);
			TPMFeeder.WriteToStream(stream, this._feeder);
			TPMHook.WriteToStream(stream, this._hook);
			TPMBait.WriteToStream(stream, this._bait);
			TPMChum.WriteToStream(stream, this._chum);
			TPMQuiverTip.WriteToStream(stream, this._quiver);
			TPMLeader.WriteToStream(stream, this._leader);
			Serializer.WriteAsByte(stream, (int)((byte)this.RodTemplate));
			Serializer.WriteVectors(stream, this.RodPoints);
			this.RodTransformData.WriteToStream(stream);
			this.lineData.WriteToStream(stream);
			this.tackleData.WriteToStream(stream);
		}

		public LineData lineData = new LineData();

		public TackleData tackleData = new TackleData();

		private TPMRod _rod;

		private TPMReel _reel;

		private TPMLine _line;

		private TPMBell _bell;

		private TPMBobber _bobber;

		private TPMSinker _sinker;

		private TPMFeeder _feeder;

		private TPMHook _hook;

		private TPMBait _bait;

		private TPMChum _chum;

		private TPMChum[] _chumAll;

		private TPMQuiverTip _quiver;

		private TPMLeader _leader;

		public List<Vector3> RodPoints = new List<Vector3>();

		public TransformData RodTransformData;
	}
}
