using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Boats;
using ObjectModel;
using UnityEngine;

namespace TPM
{
	public class ThirdPersonData : QueuePool<ThirdPersonData>.IItem
	{
		public ThirdPersonData()
		{
			this._byteParameters = new byte[4];
			this._floatParameters = new float[5];
			this._floatParameters[0] = 0f;
			this._floatParameters[1] = 0f;
			this._floatParameters[2] = 1f;
			this._floatParameters[3] = 1f;
			this._boolParameters = new bool[14];
		}

		public bool IsRodAssembled
		{
			get
			{
				return this._boolParameters[2];
			}
		}

		public TPMAssembledRod RodAssembly
		{
			get
			{
				return (!this.IsRodAssembled) ? null : this._rodAssembly;
			}
		}

		public ThirdPersonData.RodsPool RodsOnPods
		{
			get
			{
				return this._rodsOnPods;
			}
		}

		public ThirdPersonData.RodPodsPool RodPods
		{
			get
			{
				return this._rodPods;
			}
		}

		public ThirdPersonData.BoatData Boat
		{
			get
			{
				return (!this._boat.IsPresent) ? null : this._boat;
			}
		}

		public float Time
		{
			get
			{
				return this._time;
			}
		}

		public void SetSetverTime()
		{
			this._time = global::UnityEngine.Time.realtimeSinceStartup;
		}

		public ThirdPersonData.FishPool FishesAndItems
		{
			get
			{
				return this._fishAndItems;
			}
		}

		public byte[] ByteParameters
		{
			get
			{
				return this._byteParameters;
			}
		}

		public float[] FloatParameters
		{
			get
			{
				return this._floatParameters;
			}
		}

		public bool[] BoolParameters
		{
			get
			{
				return this._boolParameters;
			}
		}

		public bool IsPhotoMode
		{
			get
			{
				return this._isPhotoMode;
			}
		}

		public void ReplacePhotoModeData(bool isHoldFishInHands)
		{
			this._isPhotoMode = true;
			if (isHoldFishInHands)
			{
				this._boolParameters[2] = false;
				this._byteParameters[3] = 9;
				this._fishAndItems.Clear();
			}
			this._floatParameters[3] = 0f;
			this._rodPods.Clear();
			this._rodsOnPods.Clear();
		}

		public void UpdateBoat(IBoatController boatController)
		{
			this._boat.Clone(boatController, boatController != null);
			if (boatController == null)
			{
				float num = ((this._floatParameters[0] <= 0f) ? this._floatParameters[1] : this._floatParameters[0]);
				this._floatParameters[3] = 1f - num;
			}
		}

		public void Clone(ThirdPersonData data)
		{
			this._time = data.Time;
			this.isPaused = data.isPaused;
			this.isLeftHandRod = data.isLeftHandRod;
			this.isTackleThrown = data.isTackleThrown;
			this.playerPosition = data.playerPosition;
			this.playerRotation = data.playerRotation;
			this._boat.Clone(data._boat, data._boat.IsPresent);
			this.isBaitVisibility = data.isBaitVisibility;
			this.fireworkID = data.fireworkID;
			this.currentClipHash = data.currentClipHash;
			for (int i = 0; i < data._byteParameters.Length; i++)
			{
				this._byteParameters[i] = data._byteParameters[i];
			}
			for (int j = 0; j < data._floatParameters.Length; j++)
			{
				this._floatParameters[j] = data._floatParameters[j];
			}
			for (int k = 0; k < data._boolParameters.Length; k++)
			{
				this._boolParameters[k] = data._boolParameters[k];
			}
			this._fishAndItems.Sync(data._fishAndItems);
			this._rodsOnPods.Sync(data._rodsOnPods);
			if (this.IsRodAssembled)
			{
				this._rodAssembly.Clone(data._rodAssembly);
			}
			this._rodPods.Sync(data._rodPods);
		}

		public void SerializeToStream(Stream stream)
		{
			Serializer.WriteData(stream, this._byteParameters);
			Serializer.Write01Floats(stream, this._floatParameters);
			if (this.isPaused && !this._boolParameters[7])
			{
				this._boolParameters[2] = false;
			}
			Serializer.WriteBoolsAsUShort(stream, this._boolParameters);
			Serializer.WriteBools(stream, new bool[]
			{
				this.isPaused,
				this.isBaitVisibility,
				this.isLeftHandRod,
				this.isTackleThrown,
				this._boat.IsPresent
			});
			Serializer.WriteVector3(stream, this.playerPosition);
			Serializer.WriteQuaternion(stream, this.playerRotation);
			if (this.RodAssembly != null)
			{
				this.RodAssembly.WriteToStream(stream);
			}
			if (this._boat.IsPresent)
			{
				this._boat.WriteToStream(stream);
			}
			Serializer.WriteFloat(stream, this._time);
			Serializer.WriteInt(stream, this.currentClipHash);
			Serializer.WriteInt(stream, this.fireworkID);
			this._fishAndItems.Serialize(stream);
			this._rodsOnPods.Serialize(stream);
			this._rodPods.Serialize(stream);
		}

		public static ThirdPersonData DeserializePortion(Stream stream)
		{
			ThirdPersonData thirdPersonData = new ThirdPersonData();
			ThirdPersonData.DeserializePortion(stream, thirdPersonData);
			return thirdPersonData;
		}

		public static void WriteHeader(Stream stream)
		{
			Serializer.WriteData(stream, ThirdPersonData.HEADER);
			Serializer.WriteAsByte(stream, 6);
		}

		public static bool ReadHeader(Stream stream, bool isLogEnabled = false)
		{
			byte[] array = Serializer.ReadBytes(stream, ThirdPersonData.HEADER.Length);
			for (int i = 0; i < ThirdPersonData.HEADER.Length; i++)
			{
				if (array[i] != ThirdPersonData.HEADER[i])
				{
					if (isLogEnabled)
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int j = 0; j < array.Length; j++)
						{
							stringBuilder.Append(array[j]);
						}
						LogHelper.Error("Invalid header {0}", new object[] { stringBuilder });
					}
					return false;
				}
			}
			byte b = Serializer.ReadByte(stream);
			if (b != 6)
			{
				if (isLogEnabled)
				{
					LogHelper.Error("Invalid protocol version {0} != {1}", new object[] { b, 6 });
				}
				return false;
			}
			return true;
		}

		public static Package DeserializePackage(byte[] streamData)
		{
			int num = 0;
			using (MemoryStream memoryStream = new MemoryStream(streamData))
			{
				if (!ThirdPersonData.ReadHeader(memoryStream, false))
				{
					return null;
				}
				while (memoryStream.Position < memoryStream.Length)
				{
					ThirdPersonData.DeserializePortion(memoryStream, ThirdPersonData._deserializerCache[num++]);
				}
			}
			ThirdPersonData._deserializerCache.SetPackageLength(num);
			return ThirdPersonData._deserializerCache;
		}

		private static void DeserializePortion(Stream stream, ThirdPersonData portion)
		{
			portion._byteParameters = Serializer.ReadBytes(stream, 4);
			portion._floatParameters = Serializer.Read01Floats(stream, 5);
			portion._floatParameters[2] *= 10f;
			portion._boolParameters = Serializer.ReadBoolsFromUShort(stream, 14);
			bool[] array = Serializer.ReadBools(stream, 5);
			portion.isPaused = array[0];
			portion.isBaitVisibility = array[1];
			portion.isLeftHandRod = array[2];
			portion.isTackleThrown = array[3];
			bool flag = array[4];
			portion.playerPosition = Serializer.ReadVector3(stream);
			portion.playerRotation = Serializer.ReadQuaternion(stream);
			if (portion.IsRodAssembled)
			{
				portion.RodAssembly.ReadFromStream(stream);
			}
			portion._boat.ReadFromStream(stream, flag);
			portion._time = Serializer.ReadFloat(stream);
			portion.currentClipHash = Serializer.ReadInt(stream);
			portion.fireworkID = Serializer.ReadInt(stream);
			portion._fishAndItems.Deserialize(stream);
			portion._rodsOnPods.Deserialize(stream);
			portion._rodPods.Deserialize(stream);
		}

		public void TakeRod(AssembledRod rod)
		{
			this._rodAssembly.ReplaceData(rod);
		}

		public int PutRodOnPod()
		{
			return this._rodsOnPods.Add(this._rodAssembly);
		}

		public void OnTakeRodFromPod(int id)
		{
			this._rodsOnPods.Remove(id);
		}

		public int OnPutPod(RodPodController pod)
		{
			return this._rodPods.Add(pod.ItemId, pod.transform.position, pod.transform.rotation, 0f);
		}

		public void UpdatePods(List<RodPodController> pods)
		{
			for (int i = 0; i < pods.Count; i++)
			{
				RodPodController rodPodController = pods[i];
				this._rodPods.Update(rodPodController.TpmId, rodPodController.transform.position, rodPodController.transform.rotation, 0f);
			}
		}

		public void TakePod(int id)
		{
			this._rodPods.Remove(id);
		}

		public void ClearPods()
		{
			this._rodPods.Clear();
		}

		public void UpdateFakeRod(int id, Transform rodTransform, IList<Vector3> points, Vector3[] mainAndLeaderPoints, Vector3 sinkersFirstPoint, bool isLeaderVisible, Transform tackleTransfrom, Transform hookTransform)
		{
			this._rodsOnPods.Update(id, rodTransform, points, mainAndLeaderPoints, sinkersFirstPoint, isLeaderVisible, tackleTransfrom, hookTransform);
		}

		public void UpdateRod(Transform rodTransform, List<Vector3> rodPoints)
		{
			if (this.IsRodAssembled)
			{
				this.RodAssembly.UpdateRodData(rodTransform, rodPoints);
			}
		}

		public void UpdateLinePoints(Vector3[] mainAndLeaderPoints, Vector3 sinkersFirstPoint, bool isLeaderVisible)
		{
			if (this.IsRodAssembled)
			{
				this.RodAssembly.lineData.UpdateLineData(mainAndLeaderPoints, sinkersFirstPoint, isLeaderVisible);
			}
		}

		public void UpdateMecanimParameter(TPMMecanimIParameter name, byte value)
		{
			this._byteParameters[(int)((byte)name)] = value;
		}

		public void UpdateMecanimParameter(TPMMecanimFParameter name, float value)
		{
			if (name == TPMMecanimFParameter.RollSpeed)
			{
				value /= 10f;
			}
			this._floatParameters[(int)((byte)name)] = value;
		}

		public void UpdateMecanimParameter(TPMMecanimBParameter name, bool value)
		{
			this._boolParameters[(int)((byte)name)] = value;
		}

		public void UpdateTackle(Transform tackleTransfrom, Transform hookTransform)
		{
			if (this.IsRodAssembled)
			{
				if (tackleTransfrom != null)
				{
					this.RodAssembly.tackleData.rotation = tackleTransfrom.rotation;
				}
				if (hookTransform != null)
				{
					this.RodAssembly.tackleData.hookRotation = hookTransform.rotation;
				}
			}
		}

		public void UpdateTackleThrowData(Vector3? position, float? startAngle)
		{
			if (this.IsRodAssembled)
			{
				this.RodAssembly.tackleData.throwTargetPosition = position;
				this.RodAssembly.tackleData.throwStartAngle = startAngle;
				if (position != null)
				{
					this.isTackleThrown = true;
				}
			}
		}

		public int AddFish(Fish fishTemplate, Vector3 fishStartPosition)
		{
			return this._fishAndItems.Add(new TPMFish(fishTemplate), fishStartPosition, TPMFishState.None);
		}

		public void UpdateFish(int instanceId, Vector3 position, Vector3 fishBackward, Vector3 fishBackward2, Vector3 fishRight, TPMFishState state)
		{
			this._fishAndItems.UpdateFish(instanceId, position, fishBackward, fishBackward2, fishRight, state);
		}

		public void DelFish(int id)
		{
			this._fishAndItems.Remove(id);
		}

		public int AddItem(int itemId, Vector3 position)
		{
			return this._fishAndItems.Add(new TPMFish(itemId, 0f), position, TPMFishState.UnderwaterItem);
		}

		public void UpdateItem(int itemId, Vector3 position, TPMFishState state = TPMFishState.None)
		{
			this._fishAndItems.UpdateItem(itemId, position, state);
		}

		public void DestroyItem(int id)
		{
			this._fishAndItems.Remove(id);
		}

		public const byte VERSION = 6;

		public static readonly byte[] HEADER = new byte[] { 120, 95, 99, 110 };

		public const int FISHES_MAX_COUNT = 7;

		public const int RODS_MAX_COUNT = 4;

		public const int RODS_PODS_MAX_COUNT = 4;

		public bool isPaused;

		public bool isTackleThrown;

		public Quaternion playerRotation;

		public Vector3 playerPosition;

		private TPMAssembledRod _rodAssembly = new TPMAssembledRod();

		private ThirdPersonData.RodsPool _rodsOnPods = new ThirdPersonData.RodsPool(5);

		private ThirdPersonData.RodPodsPool _rodPods = new ThirdPersonData.RodPodsPool(4);

		private ThirdPersonData.BoatData _boat = new ThirdPersonData.BoatData();

		public bool isBaitVisibility;

		public bool isLeftHandRod;

		private float _time;

		private ThirdPersonData.FishPool _fishAndItems = new ThirdPersonData.FishPool(7);

		private byte[] _byteParameters;

		private float[] _floatParameters;

		private bool[] _boolParameters;

		public int currentClipHash;

		public int fireworkID;

		private bool _isPhotoMode;

		private static Package _deserializerCache = new Package();

		public class BoatData : IBoatData
		{
			public bool IsPresent { get; private set; }

			public ushort FactoryID
			{
				get
				{
					return this._factoryID;
				}
			}

			public Vector3 Position
			{
				get
				{
					return this._position;
				}
			}

			public Quaternion Rotation
			{
				get
				{
					return this._rotation;
				}
			}

			public void ReadFromStream(Stream stream, bool isPresent)
			{
				this.IsPresent = isPresent;
				if (this.IsPresent)
				{
					this._factoryID = Serializer.ReadUShort(stream);
					this._position = Serializer.ReadVector3(stream);
					this._rotation = Serializer.ReadQuaternion(stream);
				}
			}

			public void WriteToStream(Stream stream)
			{
				if (this.IsPresent)
				{
					Serializer.WriteAsUShort(stream, this._factoryID);
					Serializer.WriteVector3(stream, this._position);
					Serializer.WriteQuaternion(stream, this._rotation);
				}
			}

			public void Clone(IBoatData srcBoatData, bool isPresent)
			{
				this.IsPresent = srcBoatData != null && isPresent;
				if (this.IsPresent)
				{
					this._factoryID = srcBoatData.FactoryID;
					this._position = srcBoatData.Position;
					this._rotation = srcBoatData.Rotation;
				}
			}

			private ushort _factoryID;

			private Vector3 _position;

			private Quaternion _rotation;
		}

		public interface IFish
		{
			int Id { get; }
		}

		public class FishData : ThirdPersonData.IFish, SerializableCleverPool<ThirdPersonData.FishData>.ISerializableItem, CleverPool<ThirdPersonData.FishData>.IItem
		{
			public int Id
			{
				get
				{
					return this._id;
				}
			}

			public void SetId(int id)
			{
				this._id = id;
			}

			private bool IsDetailedState
			{
				get
				{
					return this.state == TPMFishState.Hooked || this.state == TPMFishState.ShowBig || this.state == TPMFishState.ShowSmall;
				}
			}

			public void WriteToStream(Stream stream)
			{
				Serializer.WriteAsByte(stream, this._id);
				this.template.WriteToStream(stream);
				Serializer.WriteVector3(stream, this.position);
				Serializer.WriteAsByte(stream, (int)((byte)this.state));
				if (this.IsDetailedState)
				{
					Serializer.WriteVector3(stream, this.backward);
					Serializer.WriteVector3(stream, this.backward2);
					Serializer.WriteVector3(stream, this.right);
				}
			}

			public void ReadFromStream(Stream stream)
			{
				this._id = (int)Serializer.ReadByte(stream);
				this.template = TPMFish.ReadFromStream(stream);
				this.position = Serializer.ReadVector3(stream);
				this.state = (TPMFishState)Serializer.ReadByte(stream);
				if (this.IsDetailedState)
				{
					this.backward = Serializer.ReadVector3(stream);
					this.backward2 = Serializer.ReadVector3(stream);
					this.right = Serializer.ReadVector3(stream);
				}
			}

			public void Clone(ThirdPersonData.FishData fish)
			{
				this._id = fish._id;
				this.template = fish.template;
				this.position = fish.position;
				this.state = fish.state;
				if (this.IsDetailedState)
				{
					this.backward = fish.backward;
					this.backward2 = fish.backward2;
					this.right = fish.right;
				}
			}

			public void Replace(TPMFish template, Vector3 position, TPMFishState state)
			{
				this.template = template;
				this.position = position;
				this.state = state;
			}

			private int _id;

			public TPMFish template;

			public Vector3 position;

			public Vector3 backward;

			public Vector3 backward2;

			public Vector3 right;

			public TPMFishState state;
		}

		public class RodData : SerializableCleverPool<ThirdPersonData.RodData>.ISerializableItem, CleverPool<ThirdPersonData.RodData>.IItem
		{
			public int Id
			{
				get
				{
					return this._id;
				}
			}

			public void SetId(int id)
			{
				this._id = id;
			}

			public void WriteToStream(Stream stream)
			{
				Serializer.WriteAsByte(stream, this._id);
				this.rodAssembly.WriteToStream(stream);
			}

			public void ReadFromStream(Stream stream)
			{
				this._id = (int)Serializer.ReadByte(stream);
				this.rodAssembly.ReadFromStream(stream);
			}

			public void Clone(ThirdPersonData.RodData rodData)
			{
				this._id = rodData._id;
				this.rodAssembly.Clone(rodData.rodAssembly);
			}

			public void Replace(TPMAssembledRod newRod)
			{
				this.rodAssembly.Clone(newRod);
			}

			private int _id;

			public TPMAssembledRod rodAssembly = new TPMAssembledRod();
		}

		public class RodPodData : SerializableCleverPool<ThirdPersonData.RodPodData>.ISerializableItem, CleverPool<ThirdPersonData.RodPodData>.IItem
		{
			public int Id
			{
				get
				{
					return this._id;
				}
			}

			public void SetId(int id)
			{
				this._id = id;
			}

			public int AssetId
			{
				get
				{
					return this._assetId;
				}
			}

			public void WriteToStream(Stream stream)
			{
				Serializer.WriteAsByte(stream, this._id);
				Serializer.WriteInt(stream, this._assetId);
				Serializer.WriteVector3(stream, this.Position);
				Serializer.WriteFloat(stream, this.Yaw);
				Serializer.WriteFloat(stream, this.Incle);
			}

			public void ReadFromStream(Stream stream)
			{
				this._id = (int)Serializer.ReadByte(stream);
				this._assetId = Serializer.ReadInt(stream);
				this.Position = Serializer.ReadVector3(stream);
				this.Yaw = Serializer.ReadFloat(stream);
				this.Incle = Serializer.ReadFloat(stream);
			}

			public void Clone(ThirdPersonData.RodPodData target)
			{
				this._id = target.Id;
				this._assetId = target._assetId;
				this.Position = target.Position;
				this.Yaw = target.Yaw;
				this.Incle = target.Incle;
			}

			public void Replace(int assetId, Vector3 position, Quaternion rotation, float incle)
			{
				this._assetId = assetId;
				this.Position = position;
				this.Yaw = rotation.eulerAngles.y;
				this.Incle = incle;
			}

			private int _id;

			private int _assetId;

			public Vector3 Position;

			public float Yaw;

			public float Incle;
		}

		public class FishPool : SerializableCleverPool<ThirdPersonData.FishData>
		{
			public FishPool(int size)
				: base(size, 250)
			{
			}

			public int Add(TPMFish template, Vector3 position, TPMFishState state = TPMFishState.None)
			{
				ThirdPersonData.FishData objectToAdd = base.GetObjectToAdd();
				objectToAdd.Replace(template, position, state);
				return objectToAdd.Id;
			}

			public void UpdateFish(int id, Vector3 position, Vector3 backward, Vector3 backward2, Vector3 right, TPMFishState state)
			{
				for (int i = 0; i < this._curCount; i++)
				{
					if (this._pool[i].Id == id)
					{
						ThirdPersonData.FishData fishData = this._pool[i];
						fishData.position = position;
						fishData.backward = backward;
						fishData.backward2 = backward2;
						fishData.right = right;
						fishData.state = state;
						break;
					}
				}
			}

			public void UpdateItem(int id, Vector3 position, TPMFishState state)
			{
				for (int i = 0; i < this._curCount; i++)
				{
					if (this._pool[i].Id == id)
					{
						ThirdPersonData.FishData fishData = this._pool[i];
						fishData.position = position;
						fishData.state = state;
						break;
					}
				}
			}
		}

		public class RodsPool : SerializableCleverPool<ThirdPersonData.RodData>
		{
			public RodsPool(int size)
				: base(size, 250)
			{
			}

			public int Add(TPMAssembledRod rodAssembly)
			{
				ThirdPersonData.RodData objectToAdd = base.GetObjectToAdd();
				objectToAdd.Replace(rodAssembly);
				return objectToAdd.Id;
			}

			public void Update(int id, Transform rodTransform, IList<Vector3> rodPoints, Vector3[] mainAndLeaderPoints, Vector3 sinkersFirstPoint, bool isLeaderVisible, Transform tackleTransfrom, Transform hookTransform)
			{
				int num = base.FindIndex((ThirdPersonData.RodData r) => r.Id == id);
				if (num != -1)
				{
					ThirdPersonData.RodData rodData = this._pool[num];
					rodData.rodAssembly.UpdateRodData(rodTransform, rodPoints);
					rodData.rodAssembly.lineData.UpdateLineData(mainAndLeaderPoints, sinkersFirstPoint, isLeaderVisible);
					if (tackleTransfrom != null)
					{
						rodData.rodAssembly.tackleData.rotation = tackleTransfrom.rotation;
					}
					if (hookTransform != null)
					{
						rodData.rodAssembly.tackleData.hookRotation = hookTransform.rotation;
					}
				}
			}
		}

		public class RodPodsPool : SerializableCleverPool<ThirdPersonData.RodPodData>
		{
			public RodPodsPool(int size)
				: base(size, 250)
			{
			}

			public int Add(int assetId, Vector3 position, Quaternion rotation, float incle)
			{
				ThirdPersonData.RodPodData objectToAdd = base.GetObjectToAdd();
				objectToAdd.Replace(assetId, position, rotation, incle);
				return objectToAdd.Id;
			}

			public void Update(int id, Vector3 position, Quaternion rotation, float incle)
			{
				int num = base.FindIndex((ThirdPersonData.RodPodData r) => r.Id == id);
				if (num != -1)
				{
					ThirdPersonData.RodPodData rodPodData = this._pool[num];
					rodPodData.Position = position;
					rodPodData.Yaw = rotation.eulerAngles.y;
					rodPodData.Incle = incle;
				}
			}
		}
	}
}
