using System;
using UnityEngine;

namespace AssetBundles
{
	public class LoadedAssetBundle
	{
		public LoadedAssetBundle(AssetBundle assetBundle)
		{
			this.m_AssetBundle = assetBundle;
			this.m_ReferencedCount = 1;
		}

		public AssetBundle m_AssetBundle;

		public int m_ReferencedCount;
	}
}
