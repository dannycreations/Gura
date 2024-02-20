using System;

public interface IAudioController
{
	void InGameOffVolume();

	void InGameOnVolume();

	void RegisterSound(IEnvironmentSound sound);

	void OnEnvironmentVolumeChanged(float volume);

	void UnRegisterSound(IEnvironmentSound sound);
}
