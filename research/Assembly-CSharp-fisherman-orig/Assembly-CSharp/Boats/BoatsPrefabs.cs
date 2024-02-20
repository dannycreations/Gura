using System;
using UnityEngine;

namespace Boats
{
	public class BoatsPrefabs : MonoBehaviour
	{
		public BoatsPrefabs.BoatRecord[] Boats
		{
			get
			{
				return this._boats;
			}
		}

		public BoatsFactory Factory
		{
			get
			{
				return this._factory;
			}
		}

		[SerializeField]
		private BoatsPrefabs.BoatRecord[] _boats;

		[SerializeField]
		private BoatsFactory _factory;

		[Serializable]
		public class SubTypeRecord
		{
			public string SubType
			{
				get
				{
					return this._subType;
				}
			}

			public Material[] Materials
			{
				get
				{
					return this._materials;
				}
			}

			[SerializeField]
			private string _subType;

			[SerializeField]
			private Material[] _materials;
		}

		[Serializable]
		public class BoatRecord
		{
			public string Type
			{
				get
				{
					return this._type;
				}
			}

			public BoatsPrefabs.SubTypeRecord[] SubTypes
			{
				get
				{
					return this._subTypes;
				}
			}

			public GameObject Prefab1p
			{
				get
				{
					return this._prefab1p;
				}
			}

			public GameObject Prefab3P
			{
				get
				{
					return this._prefab3p;
				}
			}

			[SerializeField]
			private string _type;

			[SerializeField]
			private BoatsPrefabs.SubTypeRecord[] _subTypes;

			[SerializeField]
			private GameObject _prefab1p;

			[SerializeField]
			private GameObject _prefab3p;
		}
	}
}
