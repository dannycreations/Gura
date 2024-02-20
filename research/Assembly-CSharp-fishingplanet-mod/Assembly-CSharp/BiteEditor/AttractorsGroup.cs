using System;
using System.Collections.Generic;
using UnityEngine;

namespace BiteEditor
{
	public class AttractorsGroup : MonoBehaviour, IAttractorGroup
	{
		public IFishGroup[] FishGroups
		{
			get
			{
				return this._groups;
			}
		}

		public AttractorsMovement MovementType
		{
			get
			{
				return this._movementType;
			}
		}

		public byte MinActiveCount
		{
			get
			{
				return this._minActiveCount;
			}
		}

		public byte MaxActiveCount
		{
			get
			{
				return this._maxActiveCount;
			}
		}

		public SimpleCurve AttractionShape
		{
			get
			{
				return (!(this._attractionShape != null)) ? new SimpleCurve(new float[2], 0f, 1f) : this._attractionShape.Shape;
			}
		}

		public List<IAttractor> Attractors
		{
			get
			{
				return this._attractors;
			}
		}

		public AttractorResultType ResultType
		{
			get
			{
				return this._resultType;
			}
		}

		[SerializeField]
		private FishGroup[] _groups;

		[SerializeField]
		private AttractorsMovement _movementType;

		[SerializeField]
		private byte _minActiveCount = 1;

		[SerializeField]
		private byte _maxActiveCount = 1;

		[SerializeField]
		private Curve _attractionShape;

		[SerializeField]
		private AttractorResultType _resultType;

		private readonly List<IAttractor> _attractors = new List<IAttractor>();

		private AttractorsGroupController _controller;
	}
}
