using System;
using UnityEngine;

namespace AssetBundles
{
	public class AssetBundleLoadManifestOperation : AssetBundleLoadAssetOperationFull
	{
		public AssetBundleLoadManifestOperation(string bundleName, string assetName, Type type)
			: base(bundleName, assetName, type)
		{
		}

		public override bool Update()
		{
			base.Update();
			if (this.m_Request != null && this.m_Request.isDone)
			{
				AssetBundleManager.AssetBundleManifestObject = this.GetAsset<AssetBundleManifest>();
				return false;
			}
			return true;
		}
	}
}
