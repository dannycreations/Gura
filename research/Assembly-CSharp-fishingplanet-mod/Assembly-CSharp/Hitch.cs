using System;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class Hitch : HitchBase
{
	public bool IsEmptyMesh
	{
		get
		{
			return this._isEmptyMesh;
		}
	}

	public override float HitchProbability
	{
		get
		{
			return (!(this.Group != null)) ? this._hitchProbability : this.Group.HitchProbability;
		}
	}

	public override bool CanBreak
	{
		get
		{
			return (!(this.Group != null)) ? this._canBreak : this.Group.CanBreak;
		}
	}

	public override float MaxLoad
	{
		get
		{
			return (!(this.Group != null)) ? this._maxLoad : this.Group.MaxLoad;
		}
	}

	public override HitchBase.GeneratedItem[] GeneratedItems
	{
		get
		{
			return (!(this.Group != null)) ? this._generatedItems : this.Group.GeneratedItems;
		}
	}

	public override Vector3 Scale
	{
		get
		{
			return (!(this.Group != null)) ? this._scale : this.Group.Scale;
		}
	}

	public void AddBox(HitchView boxPrefab)
	{
		if (!this._isEmptyMesh)
		{
			Renderer rendererForObject = RenderersHelper.GetRendererForObject<Renderer>(base.transform);
			this._meshSource = rendererForObject.gameObject.GetComponent<MeshFilter>();
		}
		Transform transform = base.transform.Find("__box__");
		if (transform == null)
		{
			HitchView hitchView = Object.Instantiate<HitchView>(boxPrefab, base.transform);
			hitchView.name = "__box__";
			hitchView.transform.localPosition = Vector3.zero;
			this._view = hitchView;
			this._view.tag = "Untagged";
		}
		this.RecalculateView();
	}

	public void RemoveBox()
	{
		if (this._view != null)
		{
			Object.DestroyImmediate(this._view.gameObject);
			this._view = null;
		}
	}

	public void RecalculateView()
	{
		if (this._view != null)
		{
			if (this._isEmptyMesh)
			{
				this._view.transform.localPosition = Vector3.zero;
				this._view.transform.localScale = this.Scale;
			}
			else
			{
				Quaternion rotation = base.transform.rotation;
				base.transform.rotation = Quaternion.identity;
				Bounds bounds = this._meshSource.sharedMesh.bounds;
				base.transform.rotation = rotation;
				this._view.transform.localPosition = bounds.center;
				Vector3 scale = this.Scale;
				this._view.transform.localScale = new Vector3(bounds.size.x * scale.x, bounds.size.y * scale.y, bounds.size.z * scale.z);
			}
		}
	}

	public HitchBox ToServerBox()
	{
		Vector3 scale = this.Scale;
		Vector3 vector;
		vector..ctor(base.transform.localScale.x * scale.x, base.transform.localScale.y * scale.y, base.transform.localScale.z * scale.z);
		HitchBox hitchBox = new HitchBox();
		hitchBox.Id = (long)base.gameObject.GetInstanceID();
		hitchBox.Name = base.name;
		hitchBox.Position = new Point3(base.transform.position.x, base.transform.position.y, base.transform.position.z);
		hitchBox.Rotation = new Point3(base.transform.eulerAngles.x, base.transform.eulerAngles.y, base.transform.eulerAngles.z);
		hitchBox.Scale = new Point3(vector.x, vector.y, vector.z);
		hitchBox.CanBreak = this.CanBreak;
		hitchBox.MaxLoad = this.MaxLoad;
		hitchBox.HitchProbability = this.HitchProbability;
		hitchBox.GeneratedItems = this.GeneratedItems.Select((HitchBase.GeneratedItem i) => new global::ObjectModel.GeneratedItem
		{
			ItemId = i.ItemId,
			Probability = i.Probability
		}).ToArray<global::ObjectModel.GeneratedItem>();
		return hitchBox;
	}

	public override void SetWidth(float width)
	{
		if (this.Group == null)
		{
			base.SetWidth(width);
		}
		else
		{
			this.Group.SetWidth(width);
		}
		this.RecalculateView();
	}

	public override void SetDepth(float depth)
	{
		if (this.Group == null)
		{
			base.SetDepth(depth);
		}
		else
		{
			this.Group.SetDepth(depth);
		}
		this.RecalculateView();
	}

	private const string _HitchViewName = "__box__";

	public HitchGroup Group;

	public MeshFilter _meshSource;

	[SerializeField]
	public HitchView _view;

	[SerializeField]
	private bool _isEmptyMesh;
}
