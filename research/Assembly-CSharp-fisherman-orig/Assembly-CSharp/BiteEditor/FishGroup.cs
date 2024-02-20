using System;
using BiteEditor.ObjectModel;
using UnityEngine;

namespace BiteEditor
{
	public class FishGroup : MonoBehaviour, IFishGroup
	{
		public FishGroup.Record[] Fish
		{
			get
			{
				return this._fish;
			}
		}

		public int ExportId;

		[SerializeField]
		private FishGroup.Record[] _fish;
	}
}
