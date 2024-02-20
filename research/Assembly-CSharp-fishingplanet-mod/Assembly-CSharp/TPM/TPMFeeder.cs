using System;
using System.IO;
using ObjectModel;

namespace TPM
{
	[Serializable]
	public class TPMFeeder : IFeeder
	{
		public TPMFeeder(IFeeder feeder)
		{
			this._itemId = feeder.ItemId;
			if (feeder is PvaFeeder)
			{
				this.PvaForm = new PvaFeederForm?((feeder as PvaFeeder).Form);
			}
			else if (feeder is TPMFeeder)
			{
				TPMFeeder tpmfeeder = feeder as TPMFeeder;
				if (tpmfeeder.PvaForm != null)
				{
					this.PvaForm = new PvaFeederForm?(tpmfeeder.PvaForm.Value);
				}
				else
				{
					this.PvaForm = null;
				}
			}
		}

		public TPMFeeder(int itemId, int pvaForm)
		{
			this._itemId = itemId;
			if (pvaForm != 0)
			{
				this.PvaForm = new PvaFeederForm?((PvaFeederForm)pvaForm);
			}
		}

		public int ItemId
		{
			get
			{
				return this._itemId;
			}
		}

		public string Asset
		{
			get
			{
				return CacheLibrary.AssetsCache.GetItemAssetPath(this._itemId).Asset;
			}
		}

		public PvaFeederForm? PvaForm { get; private set; }

		public void ReplaceData(IFeeder feeder)
		{
			this._itemId = feeder.ItemId;
			if (feeder is PvaFeeder)
			{
				this.PvaForm = new PvaFeederForm?((feeder as PvaFeeder).Form);
			}
			else if (feeder is TPMFeeder)
			{
				TPMFeeder tpmfeeder = feeder as TPMFeeder;
				if (tpmfeeder.PvaForm != null)
				{
					this.PvaForm = new PvaFeederForm?(tpmfeeder.PvaForm.Value);
				}
				else
				{
					this.PvaForm = null;
				}
			}
		}

		public static void WriteToStream(Stream stream, TPMFeeder feeder)
		{
			Serializer.WriteBool(stream, feeder != null);
			if (feeder != null)
			{
				Serializer.WriteInt(stream, feeder._itemId);
				if (feeder.PvaForm != null)
				{
					Serializer.WriteInt(stream, (int)feeder.PvaForm.Value);
				}
				else
				{
					Serializer.WriteInt(stream, 0);
				}
			}
		}

		public static TPMFeeder ReadFromStream(Stream stream)
		{
			return (!Serializer.ReadBool(stream)) ? null : new TPMFeeder(Serializer.ReadInt(stream), Serializer.ReadInt(stream));
		}

		private int _itemId;
	}
}
