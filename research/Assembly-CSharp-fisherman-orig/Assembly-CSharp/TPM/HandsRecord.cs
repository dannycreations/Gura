using System;
using UnityEngine;

namespace TPM
{
	[Serializable]
	public class HandsRecord
	{
		public Gender gender;

		[Tooltip("Use the same size of array as number of values in HandLength enum and fill all records please")]
		public HandLengthRecord[] hands = new HandLengthRecord[2];
	}
}
