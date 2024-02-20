using System;

namespace TPM
{
	public class Package
	{
		public Package()
		{
			for (int i = 0; i < 3; i++)
			{
				this.Data[i] = new ThirdPersonData();
			}
		}

		public int Length
		{
			get
			{
				return this._length;
			}
		}

		public void SetPackageLength(int length)
		{
			this._length = length;
		}

		public ThirdPersonData this[int i]
		{
			get
			{
				return this.Data[i];
			}
		}

		public ThirdPersonData Last
		{
			get
			{
				return this.Data[this._length - 1];
			}
		}

		private readonly ThirdPersonData[] Data = new ThirdPersonData[3];

		private int _length;
	}
}
