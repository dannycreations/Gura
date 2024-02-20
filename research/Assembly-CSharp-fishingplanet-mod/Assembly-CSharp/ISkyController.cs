using System;
using System.Collections;

public interface ISkyController
{
	void ForceChangeSky();

	void ChangeAdditionalParams();

	IEnumerator SetFirstSky();

	bool IsFirstSkyInited();

	event EventHandler<EventArgs> ChangedSky;

	event EventHandler<EventArgs> UnnecessaryChangeSky;

	SkyInfo CurrentSky { get; set; }
}
