using System;

namespace TPM
{
	public interface IDataCache
	{
		void AppendData(Package package);

		ThirdPersonData Update();

		bool IsPaused { get; }

		bool IsPlayer { get; }
	}
}
