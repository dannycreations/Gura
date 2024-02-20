using System;
using System.Linq;
using UnityEngine;

namespace Boats
{
	public class BoatsFactory : MonoBehaviour
	{
		public BoatsFactory.BoatRecord[] Boats
		{
			get
			{
				return this._boats;
			}
			set
			{
				this._boats = value;
			}
		}

		public ushort GetID(string boatType, string materialType)
		{
			BoatsFactory.BoatRecord boatRecord = this._boats.FirstOrDefault((BoatsFactory.BoatRecord r) => r.Type == boatType);
			if (boatRecord == null)
			{
				return ushort.MaxValue;
			}
			BoatsFactory.SubTypeRecord subTypeRecord = boatRecord.SubTypes.FirstOrDefault((BoatsFactory.SubTypeRecord m) => m.SubType == materialType);
			byte b = (byte)(Array.IndexOf<BoatsFactory.SubTypeRecord>(boatRecord.SubTypes, subTypeRecord) + 1);
			return (ushort)(((int)b << 8) | Array.IndexOf<BoatsFactory.BoatRecord>(this._boats, boatRecord));
		}

		public BoatFactoryData GetData(ushort factoryID)
		{
			int num = (factoryID >> 8) - 1;
			int num2 = (int)(factoryID & 255);
			BoatsFactory.BoatRecord boatRecord = this._boats[num2];
			return new BoatFactoryData
			{
				Prefab1p = boatRecord.Prefab1p,
				Prefab3p = boatRecord.Prefab3P,
				Materials = ((num >= 0) ? boatRecord.SubTypes[num].Materials : null)
			};
		}

		[SerializeField]
		private BoatsFactory.BoatRecord[] _boats;

		[Serializable]
		public class SubTypeRecord
		{
			public string SubType;

			public string[] Materials;
		}

		[Serializable]
		public class BoatRecord
		{
			public string Type;

			public BoatsFactory.SubTypeRecord[] SubTypes;

			public string Prefab1p;

			public string Prefab3P;
		}
	}
}
