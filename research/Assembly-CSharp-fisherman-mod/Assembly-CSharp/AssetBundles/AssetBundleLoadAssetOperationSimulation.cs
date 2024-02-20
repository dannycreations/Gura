using System;
using UnityEngine;

namespace AssetBundles
{
	public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
	{
		public AssetBundleLoadAssetOperationSimulation(Object simulatedObject)
		{
			this.m_SimulatedObject = simulatedObject;
		}

		public override T GetAsset<T>()
		{
			return this.m_SimulatedObject as T;
		}

		public override bool Update()
		{
			return false;
		}

		public override bool IsDone()
		{
			return true;
		}

		private Object m_SimulatedObject;
	}
}
