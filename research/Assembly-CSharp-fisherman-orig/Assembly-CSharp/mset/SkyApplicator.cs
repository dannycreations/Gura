using System;
using System.Collections.Generic;
using UnityEngine;

namespace mset
{
	[RequireComponent(typeof(Sky))]
	public class SkyApplicator : MonoBehaviour
	{
		public Bounds TriggerDimensions
		{
			get
			{
				return this.triggerDimensions;
			}
			set
			{
				this.HasChanged = true;
				this.triggerDimensions = value;
			}
		}

		private void Awake()
		{
			this.TargetSky = base.GetComponent<Sky>();
		}

		private void Start()
		{
		}

		private void OnEnable()
		{
			base.gameObject.isStatic = true;
			base.transform.root.gameObject.isStatic = true;
			this.LastPosition = base.transform.position;
			if (this.ParentApplicator == null && base.transform.parent != null && base.transform.parent.GetComponent<SkyApplicator>() != null)
			{
				this.ParentApplicator = base.transform.parent.GetComponent<SkyApplicator>();
			}
			if (this.ParentApplicator != null)
			{
				this.ParentApplicator.Children.Add(this);
			}
			else
			{
				SkyManager skyManager = SkyManager.Get();
				if (skyManager != null)
				{
					skyManager.RegisterApplicator(this);
				}
			}
		}

		private void OnDisable()
		{
			if (this.ParentApplicator != null)
			{
				this.ParentApplicator.Children.Remove(this);
			}
			SkyManager skyManager = SkyManager.Get();
			if (skyManager)
			{
				skyManager.UnregisterApplicator(this, this.AffectedRenderers);
				this.AffectedRenderers.Clear();
			}
		}

		public void RemoveRenderer(Renderer rend)
		{
			if (this.AffectedRenderers.Contains(rend))
			{
				this.AffectedRenderers.Remove(rend);
				SkyAnchor component = rend.GetComponent<SkyAnchor>();
				if (component && component.CurrentApplicator == this)
				{
					component.CurrentApplicator = null;
				}
			}
		}

		public void AddRenderer(Renderer rend)
		{
			SkyAnchor component = rend.GetComponent<SkyAnchor>();
			if (component != null)
			{
				if (component.CurrentApplicator != null)
				{
					component.CurrentApplicator.RemoveRenderer(rend);
				}
				component.CurrentApplicator = this;
			}
			this.AffectedRenderers.Add(rend);
		}

		public bool ApplyInside(Renderer rend)
		{
			if (this.TargetSky == null || !this.TriggerIsActive)
			{
				return false;
			}
			SkyAnchor component = rend.gameObject.GetComponent<SkyAnchor>();
			if (component && component.BindType == SkyAnchor.AnchorBindType.TargetSky && component.AnchorSky == this.TargetSky)
			{
				this.TargetSky.Apply(rend, 0);
				component.Apply();
				return true;
			}
			foreach (SkyApplicator skyApplicator in this.Children)
			{
				if (skyApplicator.ApplyInside(rend))
				{
					return true;
				}
			}
			Vector3 vector = rend.bounds.center;
			if (component)
			{
				component.GetCenter(ref vector);
			}
			vector = base.transform.worldToLocalMatrix.MultiplyPoint(vector);
			if (this.TriggerDimensions.Contains(vector))
			{
				this.TargetSky.Apply(rend, 0);
				return true;
			}
			return false;
		}

		public bool RendererInside(Renderer rend)
		{
			SkyAnchor skyAnchor = rend.gameObject.GetComponent<SkyAnchor>();
			if (skyAnchor && skyAnchor.BindType == SkyAnchor.AnchorBindType.TargetSky && skyAnchor.AnchorSky == this.TargetSky)
			{
				this.AddRenderer(rend);
				skyAnchor.Apply();
				return true;
			}
			if (!this.TriggerIsActive)
			{
				return false;
			}
			foreach (SkyApplicator skyApplicator in this.Children)
			{
				if (skyApplicator.RendererInside(rend))
				{
					return true;
				}
			}
			if (skyAnchor == null)
			{
				skyAnchor = rend.gameObject.AddComponent(typeof(SkyAnchor)) as SkyAnchor;
			}
			skyAnchor.GetCenter(ref this._center);
			this._center = base.transform.worldToLocalMatrix.MultiplyPoint(this._center);
			if (this.TriggerDimensions.Contains(this._center))
			{
				if (!this.AffectedRenderers.Contains(rend))
				{
					this.AddRenderer(rend);
					if (!skyAnchor.HasLocalSky)
					{
						skyAnchor.SnapToSky(SkyManager.Get().GlobalSky);
					}
					skyAnchor.BlendToSky(this.TargetSky);
				}
				return true;
			}
			this.RemoveRenderer(rend);
			return false;
		}

		private void LateUpdate()
		{
			if (this.TargetSky.Dirty)
			{
				foreach (Renderer renderer in this.AffectedRenderers)
				{
					if (!(renderer == null))
					{
						this.TargetSky.Apply(renderer, 0);
					}
				}
				this.TargetSky.Dirty = false;
			}
			if (base.transform.position != this.LastPosition)
			{
				this.HasChanged = true;
			}
		}

		public Sky TargetSky;

		public bool TriggerIsActive = true;

		[SerializeField]
		private Bounds triggerDimensions = new Bounds(Vector3.zero, Vector3.one);

		public bool HasChanged = true;

		public SkyApplicator ParentApplicator;

		public List<SkyApplicator> Children = new List<SkyApplicator>();

		private HashSet<Renderer> AffectedRenderers = new HashSet<Renderer>();

		private Vector3 LastPosition = Vector3.zero;

		private Vector3 _center;
	}
}
