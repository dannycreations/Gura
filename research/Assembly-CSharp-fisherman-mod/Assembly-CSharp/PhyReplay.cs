using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Mono.Simd.Math;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class PhyReplay
{
	public PhyReplay(ConnectedBodiesSystem targetSim, string fileName, bool read = false)
	{
		this.ProtocolVersion = 0;
		this.ProtocolVersion = 1;
		this.ProtocolVersion = 2;
		this.ProtocolVersion = 3;
		this.ProtocolVersion = 4;
		this.ProtocolVersion = 5;
		this.targetSim = targetSim;
		this.fileName = fileName;
		this.read = read;
		this.pspool = new PhyReplay.ReplayParticleStatePool(604000);
	}

	public ushort ProtocolVersion { get; private set; }

	public int ReadSegmentsCount
	{
		get
		{
			return (this.readSegments == null) ? 0 : this.readSegments.Length;
		}
	}

	public int CurrentReadSegmentIndex
	{
		get
		{
			return this.currentReadSegmentIndex;
		}
		set
		{
			if (value != this.currentReadSegmentIndex && value >= 0 && value < this.ReadSegmentsCount)
			{
				this.currentFrameIndex = 0;
				this.ReadSeekSegment(value);
			}
		}
	}

	public PhyReplay.ReplaySegment CurrentReadSegment
	{
		get
		{
			return this.readSegments[this.currentReadSegmentIndex];
		}
	}

	public int CurrentFrameIndex
	{
		get
		{
			return this.currentFrameIndex;
		}
		set
		{
			this.currentFrameIndex = value;
			if (value < 0)
			{
				this.CurrentReadSegmentIndex--;
				this.currentFrameIndex = this.CurrentReadSegment.FramesCount - 1;
			}
			if (value >= this.CurrentReadSegment.FramesCount)
			{
				this.CurrentReadSegmentIndex++;
				this.currentFrameIndex = 0;
			}
		}
	}

	public int PoolFreeParticles
	{
		get
		{
			return this.pspool.FreeParticles;
		}
	}

	public int PoolSize
	{
		get
		{
			return this.pspool.Size;
		}
	}

	public int RWQueueSize
	{
		get
		{
			return this.rwSegmentsQueue.Count;
		}
	}

	public void UpdateFrame(float frameProgress)
	{
		if (this.currentSegment == null)
		{
			this.currentSegment = new PhyReplay.ReplaySegment(this.targetSim, this.pspool);
		}
		this.currentSegment.ScanNextFrame(frameProgress);
		if (this.currentSegment.FramesCount >= 151)
		{
			this.StartNewSegment();
		}
	}

	public void StartNewSegment()
	{
		if (this.rwThread != null && (this.rwThread.ThreadState & ThreadState.Unstarted) > ThreadState.Running)
		{
			this.rwThread.Start();
		}
		if (this.currentSegment != null && this.currentSegment.FramesCount > 0)
		{
			object obj = this.rwSegmentsQueue;
			lock (obj)
			{
				this.rwSegmentsQueue.Enqueue(this.currentSegment);
			}
			this.currentSegment = null;
		}
	}

	public void WriteSegment(PhyReplay.ReplaySegment segment)
	{
	}

	private void WriterThreadProc()
	{
		while (!this.rwThreadAbort)
		{
			object obj = this.rwSegmentsQueue;
			lock (obj)
			{
				if (this.rwSegmentsQueue.Count > 0)
				{
					PhyReplay.ReplaySegment replaySegment = this.rwSegmentsQueue.Dequeue();
					this.WriteSegment(replaySegment);
					replaySegment.Release();
				}
			}
		}
	}

	public void Close()
	{
		this.rwThreadAbort = true;
		this.rwThread.Abort();
		Debug.LogWarning("Signalling rwThread to stop");
	}

	private void readTOC(FileStream stream, BinaryReader binreader)
	{
	}

	private void rebuildTOC(FileStream stream, BinaryReader binreader)
	{
		List<PhyReplay.ReplaySegment> list = new List<PhyReplay.ReplaySegment>();
		int num = 0;
		while (stream.Position < stream.Length)
		{
			list.Add(new PhyReplay.ReplaySegment(stream.Position, num, this.pspool));
			list[num].ReadFrom(binreader);
			num++;
		}
	}

	public void ReadSegment(PhyReplay.ReplaySegment segment)
	{
	}

	public void ReadSeekSegment(int index)
	{
		object obj = this.rwSegmentsQueue;
		lock (obj)
		{
			this.rwSegmentsQueue.Enqueue(this.readSegments[index]);
		}
		this.currentReadSegmentIndex = index;
	}

	public PhyReplay.ReplayParticleState[] ReadFrame()
	{
		return this.CurrentReadSegment.ReadFrame(this.CurrentFrameIndex);
	}

	public Vector3[] ReadParticleTrace(int UID, int firstSegment, int segmentsCount, out Vector3[] vel, int downsampleRate = 1)
	{
		Vector3[] array = new Vector3[segmentsCount];
		vel = new Vector3[segmentsCount];
		for (int i = 0; i < segmentsCount; i++)
		{
			PhyReplay.ReplaySegment replaySegment = this.readSegments[firstSegment + i];
			bool flag = false;
			if (replaySegment.Frames == null)
			{
				flag = true;
				this.ReadSegment(replaySegment);
			}
			List<int> list = new List<int>(replaySegment.Masses);
			int num = list.IndexOf(UID);
			if (num >= 0)
			{
				array[i] = this.pspool.particles[replaySegment.Frames[0][num]].position;
				vel[i] = this.pspool.particles[replaySegment.Frames[0][num]].velocity;
			}
			if (flag)
			{
				replaySegment.Release();
			}
		}
		int num2 = array.Length;
		Vector3[] array2 = new Vector3[num2 / downsampleRate];
		int num3 = 0;
		for (int j = 0; j < array2.Length; j++)
		{
			Vector3 vector = Vector3.zero;
			for (int k = 0; k < downsampleRate; k++)
			{
				vector += array[num3];
				num3++;
				if (num3 >= array.Length)
				{
					num3 = array.Length - 1;
				}
			}
			vector /= (float)downsampleRate;
			array2[j] = vector;
		}
		return array2;
	}

	private void ReaderThreadProc()
	{
		try
		{
			while (!this.rwThreadAbort)
			{
				object obj = this.rwSegmentsQueue;
				lock (obj)
				{
					if (this.rwSegmentsQueue.Count > 0)
					{
						if (this.loadedSegments.Count > 10)
						{
							int num = -1;
							PhyReplay.ReplaySegment replaySegment = null;
							foreach (PhyReplay.ReplaySegment replaySegment2 in this.loadedSegments)
							{
								int num2 = Mathf.Abs(replaySegment2.Index - this.currentReadSegmentIndex);
								if (num2 > num || num == -1)
								{
									num = num2;
									replaySegment = replaySegment2;
								}
							}
							if (replaySegment != null)
							{
								replaySegment.Release();
								this.loadedSegments.Remove(replaySegment);
							}
						}
						PhyReplay.ReplaySegment replaySegment3 = this.rwSegmentsQueue.Dequeue();
						this.ReadSegment(replaySegment3);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Concat(new object[] { "ReaderThreadProc exception: ", ex, "\n", ex.StackTrace }));
		}
	}

	~PhyReplay()
	{
		this.Close();
	}

	public static readonly char[] BinTag = new char[] { 'P', 'H', 'Y' };

	public static readonly char[] TOCTag = new char[] { 'T', 'O', 'C' };

	public const int MaxLoadedSegments = 10;

	private string fileName;

	private bool read;

	private ConnectedBodiesSystem targetSim;

	private PhyReplay.ReplaySegment currentSegment;

	private Queue<PhyReplay.ReplaySegment> rwSegmentsQueue;

	private PhyReplay.ReplaySegment writtenSegment;

	private Thread rwThread;

	private PhyReplay.ReplayParticleStatePool pspool;

	private volatile bool rwThreadAbort;

	private List<long> toc;

	private PhyReplay.ReplaySegment[] readSegments;

	private HashSet<PhyReplay.ReplaySegment> loadedSegments;

	private int currentReadSegmentIndex;

	private int currentFrameIndex;

	[Flags]
	public enum NonVolatile
	{
		None = 0,
		MassValue = 1,
		VisualPositionOffet = 2,
		IsKinematic = 4,
		WaterMotor = 8,
		GroundHeight = 16,
		GroundQuad = 32,
		All = -1
	}

	public struct ReplayParticleState
	{
		public void ReadMass(Mass target, float frameProgress)
		{
			this.nonvolatiles = PhyReplay.NonVolatile.None;
			this.position = target.Position4f.AsVector3();
			this.velocity = target.Velocity4f.AsVector3();
			this.force = target.Force;
			this.massvalue = target.MassValue;
			this.visualpositionoffset = target.VisualPositionOffset;
			this.iskinematic = target.IsKinematic;
			this.watermotor = target.WaterMotor4f.AsVector3();
			this.groundheight = target.GroundHeight;
		}

		public void DetectChanges(PhyReplay.ReplayParticleState previous)
		{
			if (!Mathf.Approximately(this.massvalue, previous.massvalue))
			{
				this.nonvolatiles |= PhyReplay.NonVolatile.MassValue;
			}
			if (!Mathf.Approximately(this.groundheight, previous.groundheight))
			{
				this.nonvolatiles |= PhyReplay.NonVolatile.GroundHeight;
			}
			if (this.iskinematic != previous.iskinematic)
			{
				this.nonvolatiles |= PhyReplay.NonVolatile.IsKinematic;
			}
			if (!Vector3Extension.Approximately(this.visualpositionoffset, previous.visualpositionoffset))
			{
				this.nonvolatiles |= PhyReplay.NonVolatile.VisualPositionOffet;
			}
			if (!Vector3Extension.Approximately(this.watermotor, previous.watermotor))
			{
				this.nonvolatiles |= PhyReplay.NonVolatile.WaterMotor;
			}
			if (this.quadMinXZ.x != previous.quadMinXZ.x || this.quadMinXZ.y != previous.quadMinXZ.y || this.quadMaxXZ.x != previous.quadMaxXZ.x || this.quadMaxXZ.y != previous.quadMaxXZ.y || this.quadY.x != previous.quadY.x || this.quadY.y != previous.quadY.y || this.quadY.z != previous.quadY.z || this.quadY.w != previous.quadY.w)
			{
				this.nonvolatiles |= PhyReplay.NonVolatile.GroundQuad;
			}
		}

		public void WriteTo(BinaryWriter bin)
		{
		}

		public void ExtrapolateNonvolatiles(PhyReplay.ReplayParticleState previous)
		{
			if ((this.nonvolatiles & PhyReplay.NonVolatile.MassValue) == PhyReplay.NonVolatile.None)
			{
				this.massvalue = previous.massvalue;
			}
			if ((this.nonvolatiles & PhyReplay.NonVolatile.GroundHeight) == PhyReplay.NonVolatile.None)
			{
				this.groundheight = previous.groundheight;
			}
			if ((this.nonvolatiles & PhyReplay.NonVolatile.IsKinematic) == PhyReplay.NonVolatile.None)
			{
				this.iskinematic = previous.iskinematic;
			}
			if ((this.nonvolatiles & PhyReplay.NonVolatile.VisualPositionOffet) == PhyReplay.NonVolatile.None)
			{
				this.visualpositionoffset = previous.visualpositionoffset;
			}
			if ((this.nonvolatiles & PhyReplay.NonVolatile.WaterMotor) == PhyReplay.NonVolatile.None)
			{
				this.watermotor = previous.watermotor;
			}
			if ((this.nonvolatiles & PhyReplay.NonVolatile.GroundQuad) == PhyReplay.NonVolatile.None)
			{
				this.quadMinXZ = previous.quadMinXZ;
				this.quadMaxXZ = previous.quadMaxXZ;
				this.quadY = previous.quadY;
			}
		}

		public void ReadFrom(BinaryReader bin)
		{
		}

		public Vector3 position;

		public Vector3 velocity;

		public Vector3 force;

		public Vector2 quadMinXZ;

		public Vector2 quadMaxXZ;

		public Vector4 quadY;

		public PhyReplay.NonVolatile nonvolatiles;

		public float massvalue;

		public Vector3 visualpositionoffset;

		public bool iskinematic;

		public Vector3 watermotor;

		public float groundheight;
	}

	public class ReplayParticleStatePool
	{
		public ReplayParticleStatePool(int size)
		{
			this.particles = new PhyReplay.ReplayParticleState[size];
			this.occupied = new bool[size];
			this._freeParticles = size;
		}

		public int FreeParticles
		{
			get
			{
				return this._freeParticles;
			}
		}

		public int Size
		{
			get
			{
				return this.particles.Length;
			}
		}

		public int Reserve()
		{
			int num = -1;
			for (int i = 0; i < this.occupied.Length; i++)
			{
				if (!this.occupied[this.pos])
				{
					num = this.pos;
					this.occupied[this.pos] = true;
					this._freeParticles--;
					break;
				}
				this.pos++;
				if (this.pos >= this.occupied.Length)
				{
					this.pos = 0;
				}
			}
			return num;
		}

		public void Release(int index)
		{
			if (this.occupied[index])
			{
				this.occupied[index] = false;
				this.pos = index;
				this._freeParticles++;
			}
			else
			{
				Debug.LogError("ReplayParticleStatePool: cannot release unoccupied particle " + index);
			}
		}

		public void Release(int[] indices)
		{
			if (indices.Length > 0)
			{
				for (int i = 0; i < indices.Length; i++)
				{
					if (this.occupied[indices[i]])
					{
						this.occupied[indices[i]] = false;
						this._freeParticles++;
					}
					else
					{
						Debug.LogError("ReplayParticleStatePool: cannot release unoccupied particle " + indices[i]);
					}
				}
				this.pos = indices[0];
			}
		}

		public void ReleaseThreadSafe(int[] indices)
		{
			if (indices.Length > 0)
			{
				int num = 0;
				for (int i = 0; i < indices.Length; i++)
				{
					if (this.occupied[indices[i]])
					{
						this.occupied[indices[i]] = false;
						num++;
					}
					else
					{
						Debug.LogError("ReplayParticleStatePool.ReleaseThreadSafe: cannot release unoccupied particle " + indices[i]);
					}
				}
				Interlocked.Add(ref this._freeParticles, num);
			}
		}

		public PhyReplay.ReplayParticleState[] particles;

		private bool[] occupied;

		private int pos;

		private int _freeParticles;
	}

	public class ReplaySegment
	{
		public ReplaySegment(ConnectedBodiesSystem sim, PhyReplay.ReplayParticleStatePool pspool)
		{
			this._isReady = false;
			this.pspool = pspool;
			this.sim = sim;
			this.TimeStamp = sim.IterationsCounter;
			this.Masses = new int[sim.Masses.Count];
			for (int i = 0; i < sim.Masses.Count; i++)
			{
				this.Masses[i] = sim.Masses[i].UID;
			}
			this.Connections = new int[sim.Connections.Count][];
			for (int j = 0; j < sim.Connections.Count; j++)
			{
				this.Connections[j] = new int[]
				{
					sim.Connections[j].UID,
					sim.Connections[j].Mass1.UID,
					(sim.Connections[j].Mass2 == null) ? (-1) : sim.Connections[j].Mass2.UID
				};
			}
			this.FramesCount = 0;
			this.Frames = new int[151][];
			this.VerletMasses = new List<int>();
			for (int k = 0; k < sim.Masses.Count; k++)
			{
				if (sim.Masses[k] is VerletMass)
				{
					this.VerletMasses.Add(sim.Masses[k].UID);
				}
			}
			this.Springs = new List<PhyReplay.ReplaySegment.SpringData>();
			for (int l = 0; l < sim.Connections.Count; l++)
			{
				Spring spring = sim.Connections[l] as Spring;
				if (spring != null)
				{
					this.Springs.Add(new PhyReplay.ReplaySegment.SpringData
					{
						Constant = spring.SpringConstant,
						Friction = spring.FrictionConstant,
						Length = ((!spring.IsRepulsive) ? spring.SpringLength : (-spring.SpringLength))
					});
				}
				else
				{
					VerletSpring verletSpring = sim.Connections[l] as VerletSpring;
					if (verletSpring != null)
					{
						this.Springs.Add(new PhyReplay.ReplaySegment.SpringData
						{
							Friction = verletSpring.friction.X,
							Length = ((!verletSpring.IsRepulsive) ? verletSpring.length : (-verletSpring.length)),
							Constant = -1f
						});
					}
					else
					{
						Bend bend = sim.Connections[l] as Bend;
						if (bend != null)
						{
							this.Springs.Add(new PhyReplay.ReplaySegment.SpringData
							{
								Constant = bend.SpringConstant,
								Friction = bend.FrictionConstant,
								Length = bend.SpringLength
							});
						}
						else
						{
							this.Springs.Add(default(PhyReplay.ReplaySegment.SpringData));
						}
					}
				}
			}
			this.MassConnectionsUIDs = new List<int[]>();
			for (int m = 0; m < sim.Masses.Count; m++)
			{
				Mass mass = sim.Masses[m];
				this.MassConnectionsUIDs.Add(new int[]
				{
					(mass.PriorSpring == null) ? (-1) : mass.PriorSpring.UID,
					(mass.NextSpring == null) ? (-1) : mass.NextSpring.UID
				});
			}
		}

		public ReplaySegment(long Offset, int Index, PhyReplay.ReplayParticleStatePool pspool)
		{
			this.pspool = pspool;
			this.sim = null;
			this.Frames = null;
			this.TimeStamp = 0U;
			this.Offset = Offset;
			this.Index = Index;
		}

		public bool IsReady
		{
			get
			{
				return this._isReady;
			}
		}

		public bool ScanNextFrame(float frameProgress)
		{
			if (this.FramesCount >= 151)
			{
				return false;
			}
			int num = 0;
			this.Frames[this.FramesCount] = new int[this.sim.Masses.Count];
			if (this.Frames[0].Length != this.Frames[this.FramesCount].Length)
			{
				Debug.LogError("Masses count changed during one ReplaySegment");
			}
			for (int i = 0; i < this.sim.Masses.Count; i++)
			{
				this.Frames[this.FramesCount][i] = this.pspool.Reserve();
				int num2 = this.Frames[this.FramesCount][i];
				if (num2 >= 0)
				{
					this.pspool.particles[num2].ReadMass(this.sim.Masses[i], frameProgress);
					if (this.FramesCount > 0)
					{
						this.pspool.particles[num2].DetectChanges(this.pspool.particles[this.Frames[this.FramesCount - 1][i]]);
					}
					else
					{
						this.pspool.particles[num2].nonvolatiles = PhyReplay.NonVolatile.All;
					}
				}
				else
				{
					num++;
				}
			}
			if (num > 0)
			{
				Debug.LogError(string.Concat(new object[] { "ParticleStatePool is depleted. FrameIndex = ", this.FramesCount, " states skipped = ", num }));
			}
			this.FramesCount++;
			return true;
		}

		public PhyReplay.ReplayParticleState[] ReadFrame(int frameIndex)
		{
			if (this.Masses == null)
			{
				return null;
			}
			if (frameIndex >= 0 && frameIndex < this.FramesCount)
			{
				PhyReplay.ReplayParticleState[] array = new PhyReplay.ReplayParticleState[this.Masses.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = this.pspool.particles[this.Frames[frameIndex][i]];
					if (frameIndex > 0)
					{
						array[i].ExtrapolateNonvolatiles(this.pspool.particles[this.Frames[0][i]]);
					}
				}
				return array;
			}
			return null;
		}

		public void WriteTo(BinaryWriter bin)
		{
			this._isReady = true;
		}

		public void ReadFrom(BinaryReader bin)
		{
			this._isReady = true;
		}

		public void Release()
		{
			for (int i = 0; i < this.FramesCount; i++)
			{
				this.pspool.ReleaseThreadSafe(this.Frames[i]);
			}
			this.Frames = null;
			this.Masses = null;
			this.Connections = null;
			this.FramesCount = 0;
		}

		public const int MaxFramesPerSegment = 151;

		public uint TimeStamp;

		public int[] Masses;

		public int[][] Connections;

		public int[][] Frames;

		public int FramesCount;

		private volatile bool _isReady;

		public List<int> VerletMasses;

		public List<PhyReplay.ReplaySegment.SpringData> Springs;

		public List<int[]> MassConnectionsUIDs;

		public long Offset;

		public int Index;

		private PhyReplay.ReplayParticleStatePool pspool;

		private ConnectedBodiesSystem sim;

		public struct SpringData
		{
			public float Length;

			public float Constant;

			public float Friction;
		}
	}
}
