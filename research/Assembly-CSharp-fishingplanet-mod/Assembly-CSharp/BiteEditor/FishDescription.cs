using System;
using System.Collections.Generic;
using UnityEngine;

namespace BiteEditor
{
	public class FishDescription : MonoBehaviour
	{
		public FishName Name
		{
			get
			{
				return this._name;
			}
		}

		public IEnumerable<FishDescription.Record> Forms
		{
			get
			{
				return this._forms;
			}
		}

		public override string ToString()
		{
			return this._name.ToString();
		}

		[SerializeField]
		private FishName _name;

		[SerializeField]
		private FishDescription.Record[] _forms;

		[Serializable]
		public class Record
		{
			public FishForm Form;

			public float MinWeight;

			public float MaxWeight;

			[Tooltip("Enable|disable detraction when catch fish of this form")]
			public bool IsDetractorEnabled = true;

			[Tooltip("In gameplay minutes")]
			public float DetractionDuration = 90f;

			public DetractionType DetractionType = DetractionType.Add;

			public float Detraction = 0.025f;

			[Tooltip("Radius where 100% detraction work")]
			public float R100 = 2f;

			[Tooltip("Radius where detraction from R100 fall down to 0%")]
			public float R0 = 4f;

			[Tooltip("To modify value of attractors group")]
			public float AttractorsModifier = 1f;
		}
	}
}
