using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
	[Serializable]
	public abstract class MB3_MeshBakerGrouperCore
	{
		public abstract Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection);

		public abstract void DrawGizmos(Bounds sourceObjectBounds);

		public void DoClustering(MB3_TextureBaker tb, MB3_MeshBakerGrouper grouper)
		{
			Dictionary<string, List<Renderer>> dictionary = this.FilterIntoGroups(tb.GetObjectsToCombine());
			if (this.d.clusterOnLMIndex)
			{
				Dictionary<string, List<Renderer>> dictionary2 = new Dictionary<string, List<Renderer>>();
				foreach (string text in dictionary.Keys)
				{
					List<Renderer> list = dictionary[text];
					Dictionary<int, List<Renderer>> dictionary3 = this.GroupByLightmapIndex(list);
					foreach (int num in dictionary3.Keys)
					{
						string text2 = text + "-LM-" + num;
						dictionary2.Add(text2, dictionary3[num]);
					}
				}
				dictionary = dictionary2;
			}
			if (this.d.clusterByLODLevel)
			{
				Dictionary<string, List<Renderer>> dictionary4 = new Dictionary<string, List<Renderer>>();
				foreach (string text3 in dictionary.Keys)
				{
					List<Renderer> list2 = dictionary[text3];
					using (List<Renderer>.Enumerator enumerator4 = list2.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							Renderer r = enumerator4.Current;
							if (!(r == null))
							{
								bool flag = false;
								LODGroup componentInParent = r.GetComponentInParent<LODGroup>();
								if (componentInParent != null)
								{
									LOD[] lods = componentInParent.GetLODs();
									for (int i = 0; i < lods.Length; i++)
									{
										LOD lod = lods[i];
										if (Array.Find<Renderer>(lod.renderers, (Renderer x) => x == r) != null)
										{
											flag = true;
											string text4 = string.Format("{0}_LOD{1}", text3, i);
											List<Renderer> list3;
											if (!dictionary4.TryGetValue(text4, out list3))
											{
												list3 = new List<Renderer>();
												dictionary4.Add(text4, list3);
											}
											if (!list3.Contains(r))
											{
												list3.Add(r);
											}
										}
									}
								}
								if (!flag)
								{
									string text5 = string.Format("{0}_LOD0", text3);
									List<Renderer> list4;
									if (!dictionary4.TryGetValue(text5, out list4))
									{
										list4 = new List<Renderer>();
										dictionary4.Add(text5, list4);
									}
									if (!list4.Contains(r))
									{
										list4.Add(r);
									}
								}
							}
						}
					}
				}
				dictionary = dictionary4;
			}
			int num2 = 0;
			foreach (string text6 in dictionary.Keys)
			{
				List<Renderer> list5 = dictionary[text6];
				if (list5.Count > 1 || grouper.data.includeCellsWithOnlyOneRenderer)
				{
					this.AddMeshBaker(tb, text6, list5);
				}
				else
				{
					num2++;
				}
			}
			Debug.Log(string.Format("Found {0} cells with Renderers. Not creating bakers for {1} because there is only one mesh in the cell. Creating {2} bakers.", dictionary.Count, num2, dictionary.Count - num2));
		}

		private Dictionary<int, List<Renderer>> GroupByLightmapIndex(List<Renderer> gaws)
		{
			Dictionary<int, List<Renderer>> dictionary = new Dictionary<int, List<Renderer>>();
			for (int i = 0; i < gaws.Count; i++)
			{
				List<Renderer> list;
				if (dictionary.ContainsKey(gaws[i].lightmapIndex))
				{
					list = dictionary[gaws[i].lightmapIndex];
				}
				else
				{
					list = new List<Renderer>();
					dictionary.Add(gaws[i].lightmapIndex, list);
				}
				list.Add(gaws[i]);
			}
			return dictionary;
		}

		private void AddMeshBaker(MB3_TextureBaker tb, string key, List<Renderer> gaws)
		{
			int num = 0;
			for (int i = 0; i < gaws.Count; i++)
			{
				Mesh mesh = MB_Utility.GetMesh(gaws[i].gameObject);
				if (mesh != null)
				{
					num += mesh.vertexCount;
				}
			}
			GameObject gameObject = new GameObject("MeshBaker-" + key);
			gameObject.transform.position = Vector3.zero;
			MB3_MeshBakerCommon mb3_MeshBakerCommon;
			if (num >= 65535)
			{
				mb3_MeshBakerCommon = gameObject.AddComponent<MB3_MultiMeshBaker>();
				mb3_MeshBakerCommon.useObjsToMeshFromTexBaker = false;
			}
			else
			{
				mb3_MeshBakerCommon = gameObject.AddComponent<MB3_MeshBaker>();
				mb3_MeshBakerCommon.useObjsToMeshFromTexBaker = false;
			}
			mb3_MeshBakerCommon.textureBakeResults = tb.textureBakeResults;
			mb3_MeshBakerCommon.transform.parent = tb.transform;
			for (int j = 0; j < gaws.Count; j++)
			{
				mb3_MeshBakerCommon.GetObjectsToCombine().Add(gaws[j].gameObject);
			}
		}

		public GrouperData d;
	}
}
