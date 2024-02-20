using System;
using System.Collections.Generic;

namespace DigitalOpus.MB.Lod
{
	public class MB2_LODClusterComparer : IComparer<LODCombinedMesh>
	{
		int IComparer<LODCombinedMesh>.Compare(LODCombinedMesh a, LODCombinedMesh b)
		{
			int num = a.NumBakeImmediately() - b.NumBakeImmediately();
			if (num != 0)
			{
				return num;
			}
			int num2;
			if (b.IsVisible() && !a.IsVisible())
			{
				num2 = -1;
			}
			else if (b.IsVisible() == a.IsVisible())
			{
				num2 = 0;
			}
			else
			{
				num2 = 1;
			}
			if (num2 != 0)
			{
				return num2;
			}
			return a.NumDirty() - b.NumDirty();
		}
	}
}
