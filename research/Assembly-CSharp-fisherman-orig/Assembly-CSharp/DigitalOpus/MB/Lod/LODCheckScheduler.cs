using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

namespace DigitalOpus.MB.Lod
{
	public class LODCheckScheduler
	{
		public int GetNextFrameCheckOffset()
		{
			if (this.nextFrameCheckOffset >= 1000)
			{
				this.nextFrameCheckOffset = 0;
			}
			return this.nextFrameCheckOffset++;
		}

		public void Init(MB2_LODManager m)
		{
			this.manager = m;
			if (this.manager.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log("Init called for LODCheckScheduler.");
			}
			this.containsMultipleCells = false;
			this.containsMovingClusters = false;
			this.minGridSize = float.PositiveInfinity;
			for (int i = 0; i < this.manager.bakers.Length; i++)
			{
				if (this.manager.bakers[i].clusterType != MB2_LODManager.BakerPrototype.CombinerType.simple)
				{
					this.containsMultipleCells = true;
					if (this.manager.bakers[i].gridSize < this.minGridSize)
					{
						this.minGridSize = this.manager.bakers[i].gridSize;
					}
				}
				if (this.manager.bakers[i].clusterType == MB2_LODManager.BakerPrototype.CombinerType.moving)
				{
					this.containsMovingClusters = true;
				}
			}
			if (this.containsMultipleCells)
			{
				this.sqrDistThreashold = this.minGridSize / 1.5f * (this.minGridSize / 1.5f);
				this.InitializeLastCameraPositions(this.manager.GetCameras());
			}
		}

		private void InitializeLastCameraPositions(MB2_LODCamera[] cams)
		{
			this.lastCameraPositions = new Vector3[cams.Length];
			for (int i = 0; i < this.lastCameraPositions.Length; i++)
			{
				this.lastCameraPositions[i] = new Vector3(1E+16f, 1E+16f, 1E+16f);
			}
		}

		private void UpdateClusterSchedules()
		{
			if (this.manager.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				Debug.Log("Updating cluster lodcheck schedules.");
			}
			for (int i = 0; i < this.manager.bakers.Length; i++)
			{
				LODClusterManager baker = this.manager.bakers[i].baker;
				for (int j = 0; j < baker.clusters.Count; j++)
				{
					LODCluster lodcluster = baker.clusters[j];
					int numFramesBetweenChecks = this.GetNumFramesBetweenChecks(lodcluster);
					int num = int.MaxValue;
					List<LODCombinedMesh> combiners = lodcluster.GetCombiners();
					for (int k = 0; k < combiners.Count; k++)
					{
						if (combiners[k].numFramesBetweenChecksOffset == -1)
						{
							if (this.manager.LOG_LEVEL >= MB2_LogLevel.trace)
							{
								Debug.Log("fm=" + Time.frameCount + " calling cluster Update");
							}
							combiners[k].Update();
						}
						combiners[k].numFramesBetweenChecks = numFramesBetweenChecks;
						combiners[k].numFramesBetweenChecksOffset = this.GetNextFrameCheckOffset();
						int num2 = combiners[k].numFramesBetweenChecks - (Time.frameCount + combiners[k].numFramesBetweenChecksOffset) % combiners[k].numFramesBetweenChecks;
						if (num2 < num)
						{
							num = num2;
						}
					}
					lodcluster.nextCheckFrame = Time.frameCount + num;
				}
			}
			this.lastSheduleUpdateTime = Time.time;
		}

		public int GetNumFramesBetweenChecks(LODCluster cell)
		{
			int num = -1;
			if (cell is LODClusterGrid || cell is LODClusterMoving)
			{
				float num2 = Mathf.Sqrt(this.manager.GetDistanceSqrToClosestPerspectiveCamera(cell.Center()));
				MB2_LODManager.BakerPrototype bakerPrototype = cell.GetClusterManager().GetBakerPrototype();
				num = bakerPrototype.numFramesBetweenLODChecks;
				int num3 = Mathf.FloorToInt(num2 / (bakerPrototype.gridSize * 0.5f)) + 1;
				num *= num3;
			}
			else if (cell is LODClusterSimple)
			{
				MB2_LODManager.BakerPrototype bakerPrototype2 = cell.GetClusterManager().GetBakerPrototype();
				num = bakerPrototype2.numFramesBetweenLODChecks;
			}
			else
			{
				Debug.LogError("Should never get here.");
			}
			return num;
		}

		private void _UpdateClusterSchedulesIfCameraHasMoved()
		{
			MB2_LODCamera[] cameras = this.manager.GetCameras();
			if (cameras.Length != this.lastCameraPositions.Length)
			{
				this.InitializeLastCameraPositions(cameras);
			}
			bool flag = false;
			for (int i = 0; i < this.lastCameraPositions.Length; i++)
			{
				Vector3 position = cameras[i].transform.position;
				Vector3 vector = position - this.lastCameraPositions[i];
				if (Vector3.Dot(vector, vector) > this.sqrDistThreashold)
				{
					this.UpdateClusterSchedules();
					flag = true;
					for (int j = 0; j < cameras.Length; j++)
					{
						this.lastCameraPositions[j] = cameras[j].transform.position;
					}
					break;
				}
			}
			if (this.containsMovingClusters && !flag && Time.time - this.lastSheduleUpdateTime > 1f)
			{
				this.UpdateClusterSchedules();
				for (int k = 0; k < cameras.Length; k++)
				{
					this.lastCameraPositions[k] = cameras[k].transform.position;
				}
			}
		}

		public void CheckIfLODsNeedToChange()
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (this.containsMultipleCells)
			{
				this._UpdateClusterSchedulesIfCameraHasMoved();
			}
			for (int i = 0; i < this.manager.bakers.Length; i++)
			{
				LODClusterManager baker = this.manager.bakers[i].baker;
				if (!(baker is LODClusterManagerSimple))
				{
					this.containsMultipleCells = true;
				}
				if (baker is LODClusterManagerMoving)
				{
					this.containsMovingClusters = true;
				}
				for (int j = 0; j < baker.clusters.Count; j++)
				{
					LODCluster lodcluster = baker.clusters[j];
					if (this.FORCE_CHECK_EVERY_FRAME || lodcluster.nextCheckFrame == Time.frameCount)
					{
						if (lodcluster is LODClusterMoving)
						{
							((LODClusterMoving)lodcluster).UpdateBounds();
						}
						int num = int.MaxValue;
						List<LODCombinedMesh> combiners = lodcluster.GetCombiners();
						for (int k = 0; k < combiners.Count; k++)
						{
							LODCombinedMesh lodcombinedMesh = combiners[k];
							bool flag = false;
							if (lodcombinedMesh.numFramesBetweenChecks == -1)
							{
								lodcombinedMesh.numFramesBetweenChecks = this.GetNumFramesBetweenChecks(lodcluster);
								lodcombinedMesh.numFramesBetweenChecksOffset = this.GetNextFrameCheckOffset();
								flag = true;
							}
							int num2 = lodcombinedMesh.numFramesBetweenChecks - (Time.frameCount + lodcombinedMesh.numFramesBetweenChecksOffset) % lodcombinedMesh.numFramesBetweenChecks;
							if (this.FORCE_CHECK_EVERY_FRAME || flag || num2 == lodcombinedMesh.numFramesBetweenChecks)
							{
								if (this.manager.LOG_LEVEL >= MB2_LogLevel.trace)
								{
									Debug.Log("fm=" + Time.frameCount + " calling cluster Update");
								}
								lodcombinedMesh.Update();
							}
							if (num2 < num)
							{
								num = num2;
							}
						}
						lodcluster.nextCheckFrame = Time.frameCount + num;
					}
					if (lodcluster.nextCheckFrame < Time.frameCount)
					{
						Debug.LogError(Time.frameCount + " Error somehow bypassed a frame when checking. " + lodcluster.nextCheckFrame);
					}
				}
			}
			this.manager.statLastCheckLODNeedToChangeTime = Time.realtimeSinceStartup - realtimeSinceStartup;
			this.manager.statTotalCheckLODNeedToChangeTime += this.manager.statLastCheckLODNeedToChangeTime;
		}

		public void ForceCheckIfLODsNeedToChange()
		{
			if (this.containsMultipleCells)
			{
				this._UpdateClusterSchedulesIfCameraHasMoved();
			}
			for (int i = 0; i < this.manager.bakers.Length; i++)
			{
				LODClusterManager baker = this.manager.bakers[i].baker;
				for (int j = 0; j < baker.clusters.Count; j++)
				{
					LODCluster lodcluster = baker.clusters[j];
					List<LODCombinedMesh> combiners = lodcluster.GetCombiners();
					for (int k = 0; k < combiners.Count; k++)
					{
						LODCombinedMesh lodcombinedMesh = combiners[k];
						lodcombinedMesh.Update();
					}
				}
			}
		}

		public bool FORCE_CHECK_EVERY_FRAME;

		private Vector3[] lastCameraPositions;

		private bool containsMultipleCells;

		private bool containsMovingClusters;

		private float sqrDistThreashold;

		private float minGridSize;

		private MB2_LODManager manager;

		private int nextFrameCheckOffset;

		private float lastSheduleUpdateTime;
	}
}
