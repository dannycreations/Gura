using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPM
{
	public class BakedObjectController : MonoBehaviour
	{
		public void SetupBaker(MB3_MeshBaker meshBaker)
		{
			this._meshBaker = meshBaker;
		}

		private void Start()
		{
			this._groups = new Dictionary<BakedObjectController.PartType, BakedObjectController.Group>
			{
				{
					BakedObjectController.PartType.HEAD,
					new BakedObjectController.Group
					{
						items = this._heads
					}
				},
				{
					BakedObjectController.PartType.BOOTS,
					new BakedObjectController.Group
					{
						items = this._boots
					}
				}
			};
		}

		private void SwitchToNextItem(BakedObjectController.PartType groupID)
		{
			List<GameObject> objectsInCombined = this._meshBaker.meshCombiner.GetObjectsInCombined();
			List<GameObject> list = new List<GameObject>();
			BakedObjectController.Group group = this._groups[groupID];
			if (group.curIndex != -1)
			{
				for (int i = 0; i < objectsInCombined.Count; i++)
				{
					Renderer component = objectsInCombined[i].GetComponent<Renderer>();
					if (component == group.items[group.curIndex])
					{
						list.Add(component.gameObject);
						break;
					}
				}
			}
			if (++group.curIndex >= group.items.Length)
			{
				group.curIndex = 0;
			}
			List<GameObject> list2 = new List<GameObject> { group.items[group.curIndex].gameObject };
			this._meshBaker.AddDeleteGameObjects(list2.ToArray(), list.ToArray(), true);
			this._meshBaker.Apply(null);
		}

		private void Update()
		{
			if (Input.GetKeyUp(104))
			{
				this.SwitchToNextItem(BakedObjectController.PartType.HEAD);
			}
			if (Input.GetKeyUp(98))
			{
				this.SwitchToNextItem(BakedObjectController.PartType.BOOTS);
			}
		}

		public Renderer[] _heads;

		public Renderer[] _boots;

		private MB3_MeshBaker _meshBaker;

		private Dictionary<BakedObjectController.PartType, BakedObjectController.Group> _groups;

		private enum PartType
		{
			HEAD,
			BOOTS
		}

		private class Group
		{
			public int curIndex = -1;

			public Renderer[] items;
		}
	}
}
