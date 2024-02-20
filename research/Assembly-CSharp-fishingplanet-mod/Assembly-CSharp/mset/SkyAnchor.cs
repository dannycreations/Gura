using System;
using UnityEngine;

namespace mset
{
	public class SkyAnchor : MonoBehaviour
	{
		public Sky CurrentSky
		{
			get
			{
				return this.Blender.CurrentSky;
			}
		}

		public Sky PreviousSky
		{
			get
			{
				return this.Blender.PreviousSky;
			}
		}

		public float BlendTime
		{
			get
			{
				return this.Blender.BlendTime;
			}
			set
			{
				this.Blender.BlendTime = value;
			}
		}

		public bool IsStatic
		{
			get
			{
				return this.isStatic;
			}
		}

		private void Start()
		{
			if (this.BindType != SkyAnchor.AnchorBindType.TargetSky)
			{
				base.GetComponent<Renderer>().SetPropertyBlock(new MaterialPropertyBlock());
				SkyManager skyManager = SkyManager.Get();
				skyManager.RegisterNewRenderer(base.GetComponent<Renderer>());
				skyManager.ApplyCorrectSky(base.GetComponent<Renderer>());
				this.BlendTime = skyManager.LocalBlendTime;
				if (this.Blender.CurrentSky)
				{
					this.Blender.SnapToSky(this.Blender.CurrentSky);
				}
				else
				{
					this.Blender.SnapToSky(skyManager.GlobalSky);
				}
			}
			this.materials = base.GetComponent<Renderer>().materials;
			this.LastPosition = base.transform.position;
			this.HasChanged = true;
		}

		private void OnEnable()
		{
			this.isStatic = base.gameObject.isStatic;
			this.ComputeCenter(ref this.CachedCenter);
			this.firstFrame = true;
		}

		private void LateUpdate()
		{
			if (this.BindType == SkyAnchor.AnchorBindType.TargetSky)
			{
				this.HasChanged = this.AnchorSky != this.Blender.CurrentSky;
				if (this.AnchorSky != null)
				{
					this.CachedCenter = this.AnchorSky.transform.position;
				}
			}
			else if (this.BindType == SkyAnchor.AnchorBindType.TargetTransform)
			{
				if (this.AnchorTransform && (this.AnchorTransform.position.x != this.LastPosition.x || this.AnchorTransform.position.y != this.LastPosition.y || this.AnchorTransform.position.z != this.LastPosition.z))
				{
					this.HasChanged = true;
					this.LastPosition = this.AnchorTransform.position;
					this.CachedCenter.x = this.LastPosition.x;
					this.CachedCenter.y = this.LastPosition.y;
					this.CachedCenter.z = this.LastPosition.z;
				}
			}
			else if (!this.isStatic)
			{
				if (this.LastPosition.x != base.transform.position.x || this.LastPosition.y != base.transform.position.y || this.LastPosition.z != base.transform.position.z)
				{
					this.HasChanged = true;
					this.LastPosition = base.transform.position;
					this.ComputeCenter(ref this.CachedCenter);
				}
			}
			else
			{
				this.HasChanged = false;
			}
			this.HasChanged |= this.firstFrame;
			this.firstFrame = false;
			bool flag = this.Blender.IsBlending || this.Blender.WasBlending(Time.deltaTime);
			if (flag)
			{
				this.Apply();
			}
			else if (this.BindType == SkyAnchor.AnchorBindType.TargetSky)
			{
				if (this.HasChanged || this.Blender.CurrentSky.Dirty)
				{
					this.Apply();
				}
			}
			else if (this.HasLocalSky && (this.HasChanged || this.Blender.CurrentSky.Dirty))
			{
				this.Apply();
			}
		}

		public void UpdateMaterials()
		{
			this.materials = base.GetComponent<Renderer>().materials;
		}

		public void CleanUpMaterials()
		{
			if (this.materials != null)
			{
				foreach (Material material in this.materials)
				{
					Object.Destroy(material);
				}
				this.materials = new Material[0];
			}
		}

		public void SnapToSky(Sky nusky)
		{
			if (nusky == null)
			{
				return;
			}
			if (this.BindType == SkyAnchor.AnchorBindType.TargetSky)
			{
				return;
			}
			this.Blender.SnapToSky(nusky);
			this.HasLocalSky = true;
		}

		public void BlendToSky(Sky nusky)
		{
			if (nusky == null)
			{
				return;
			}
			if (this.BindType == SkyAnchor.AnchorBindType.TargetSky)
			{
				return;
			}
			this.Blender.BlendToSky(nusky);
			this.HasLocalSky = true;
		}

		public void SnapToGlobalSky(Sky nusky)
		{
			this.SnapToSky(nusky);
			this.HasLocalSky = false;
		}

		public void BlendToGlobalSky(Sky nusky)
		{
			if (this.HasLocalSky)
			{
				this.BlendToSky(nusky);
			}
			this.HasLocalSky = false;
		}

		public void Apply()
		{
			if (this.BindType == SkyAnchor.AnchorBindType.TargetSky)
			{
				if (this.AnchorSky)
				{
					this.Blender.SnapToSky(this.AnchorSky);
				}
				else
				{
					this.Blender.SnapToSky(SkyManager.Get().GlobalSky);
				}
			}
			this.Blender.Apply(base.GetComponent<Renderer>(), this.materials);
		}

		public void GetCenter(ref Vector3 _center)
		{
			_center.x = this.CachedCenter.x;
			_center.y = this.CachedCenter.y;
			_center.z = this.CachedCenter.z;
		}

		private void ComputeCenter(ref Vector3 _center)
		{
			_center.x = base.transform.position.x;
			_center.y = base.transform.position.y;
			_center.z = base.transform.position.z;
			switch (this.BindType)
			{
			case SkyAnchor.AnchorBindType.Center:
				_center.x = base.GetComponent<Renderer>().bounds.center.x;
				_center.y = base.GetComponent<Renderer>().bounds.center.y;
				_center.z = base.GetComponent<Renderer>().bounds.center.z;
				break;
			case SkyAnchor.AnchorBindType.Offset:
			{
				Vector3 vector = base.transform.localToWorldMatrix.MultiplyPoint3x4(this.AnchorOffset);
				_center.x = vector.x;
				_center.y = vector.y;
				_center.z = vector.z;
				break;
			}
			case SkyAnchor.AnchorBindType.TargetTransform:
				if (this.AnchorTransform)
				{
					_center.x = this.AnchorTransform.position.x;
					_center.y = this.AnchorTransform.position.y;
					_center.z = this.AnchorTransform.position.z;
				}
				break;
			case SkyAnchor.AnchorBindType.TargetSky:
				if (this.AnchorSky)
				{
					_center.x = this.AnchorSky.transform.position.x;
					_center.y = this.AnchorSky.transform.position.y;
					_center.z = this.AnchorSky.transform.position.z;
				}
				break;
			}
		}

		private void OnDestroy()
		{
			this.CleanUpMaterials();
		}

		private void OnApplicationQuit()
		{
			this.CleanUpMaterials();
		}

		public SkyAnchor.AnchorBindType BindType;

		public Transform AnchorTransform;

		public Vector3 AnchorOffset = Vector3.zero;

		public Sky AnchorSky;

		public Vector3 CachedCenter = Vector3.zero;

		public SkyApplicator CurrentApplicator;

		private bool isStatic;

		public bool HasLocalSky;

		public bool HasChanged = true;

		[SerializeField]
		private SkyBlender Blender = new SkyBlender();

		private Vector3 LastPosition = Vector3.zero;

		[NonSerialized]
		public Material[] materials;

		private bool firstFrame;

		public enum AnchorBindType
		{
			Center,
			Offset,
			TargetTransform,
			TargetSky
		}
	}
}
