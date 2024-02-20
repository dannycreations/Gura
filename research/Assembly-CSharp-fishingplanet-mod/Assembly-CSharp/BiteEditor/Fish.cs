using System;
using System.Collections.Generic;
using System.Linq;
using BiteEditor.ObjectModel;
using UnityEngine;

namespace BiteEditor
{
	public class Fish : MonoBehaviour
	{
		public override string ToString()
		{
			return (!(this._group != null)) ? this._name.ToString() : this._group.name;
		}

		public bool IsAffectFish(FishName name)
		{
			return (!(this._group != null)) ? (this._name == name) : this._group.Fish.Any((FishGroup.Record r) => r.FishName == name);
		}

		public FishGroup.Record[] GetAffectedFish()
		{
			if (this._group == null)
			{
				List<FishForm> list = new List<FishForm>();
				List<FishLayer> layers = this.Layers;
				for (int i = 0; i < layers.Count; i++)
				{
					FishLayer fishLayer = layers[i];
					for (int j = 0; j < fishLayer.Forms.Count; j++)
					{
						if (!list.Contains(fishLayer.Forms[j]))
						{
							list.Add(fishLayer.Forms[j]);
						}
					}
				}
				return new FishGroup.Record[]
				{
					new FishGroup.Record
					{
						FishName = this._name,
						FishForms = list
					}
				};
			}
			return this._group.Fish;
		}

		public List<FishLayer> Layers
		{
			get
			{
				List<FishLayer> list = new List<FishLayer>();
				for (int i = 0; i < base.transform.childCount; i++)
				{
					FishLayer component = base.transform.GetChild(i).GetComponent<FishLayer>();
					if (component != null)
					{
						list.Add(component);
					}
				}
				return list;
			}
		}

		[SerializeField]
		private FishName _name;

		[SerializeField]
		private FishGroup _group;
	}
}
