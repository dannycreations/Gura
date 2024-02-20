using System;
using BiteEditor.ObjectModel;

namespace BiteEditor
{
	public interface IFishGroup
	{
		FishGroup.Record[] Fish { get; }
	}
}
