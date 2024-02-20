using System;

namespace TPM
{
	[Serializable]
	public struct BytePair
	{
		public TPMMecanimIParameter Parameter;

		public byte Value;
	}
}
