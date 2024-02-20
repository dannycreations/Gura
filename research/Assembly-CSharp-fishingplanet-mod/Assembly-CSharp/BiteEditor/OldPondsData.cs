using System;
using System.Collections.Generic;
using System.Linq;
using BiteEditor.ObjectModel;
using ObjectModel;
using UnityEngine;

namespace BiteEditor
{
	public class OldPondsData
	{
		public OldPondsData.ImportedData Import(HeightMap heightMap)
		{
			return new OldPondsData.ImportedData(this, heightMap);
		}

		public OldPondsData.Box[] FishBoxes;

		public class Point3
		{
			public float X;

			public float Y;

			public float Z;
		}

		public class Box
		{
			public string Name;

			public string InheritedFrom;

			public OldPondsData.Point3 Position;

			public OldPondsData.Point3 Scale;

			public OldPondsData.Point3 Rotation;

			public OldPondsData.Box.Condition[] Conditions;

			public class Fish
			{
				public string FishCode;

				public int Quantity;

				public float MinWeight;

				public float MaxWeight;
			}

			public class Condition
			{
				public string[] Weather;

				public OldPondsData.Box.Fish[] Fish;
			}
		}

		public class ImportedData
		{
			public ImportedData(OldPondsData data, HeightMap heightMap)
			{
				Dictionary<string, Settings.FishCodeNameExport> dictionary = Settings.ParseFishTable();
				Transform transform = new GameObject("helper").transform;
				Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
				Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
				for (int i = 0; i < data.FishBoxes.Length; i++)
				{
					dictionary3[data.FishBoxes[i].Name] = i;
				}
				this._baseWeather = new OldPondsData.ImportedData.Weather(heightMap.Height, heightMap.Width);
				for (int j = 0; j < data.FishBoxes.Length; j++)
				{
					OldPondsData.Box box = data.FishBoxes[j];
					OldPondsData.Box box2 = box;
					if (!string.IsNullOrEmpty(box.InheritedFrom) && dictionary3.ContainsKey(box.InheritedFrom))
					{
						box2 = data.FishBoxes[dictionary3[box.InheritedFrom]];
					}
					if (box2.Conditions != null)
					{
						dictionary2.Clear();
						for (int k = 0; k < box2.Conditions.Length; k++)
						{
							OldPondsData.Box.Condition condition = box2.Conditions[k];
							for (int l = 0; l < condition.Fish.Length; l++)
							{
								OldPondsData.Box.Fish fish = condition.Fish[l];
								if (!dictionary2.ContainsKey(fish.FishCode) || dictionary2[fish.FishCode] < fish.Quantity)
								{
									dictionary2[fish.FishCode] = fish.Quantity;
								}
								Settings.FishCodeNameExport fishCodeNameExport = dictionary[fish.FishCode];
								if (!this._fish.ContainsKey(fish.FishCode))
								{
									this._fish[fish.FishCode] = new OldPondsData.ImportedData.FishDescription(fishCodeNameExport.FishName, fishCodeNameExport.FishForm, fish.MinWeight, fish.MaxWeight);
								}
								else
								{
									OldPondsData.ImportedData.FishDescription fishDescription = this._fish[fish.FishCode];
									fishDescription.MinWeight = Mathf.Min(fishDescription.MinWeight, fish.MinWeight);
									fishDescription.MaxWeight = Mathf.Max(fishDescription.MaxWeight, fish.MaxWeight);
								}
							}
							for (int m = 0; m < condition.Weather.Length; m++)
							{
								string text = condition.Weather[m];
								if (!this._weatherNames.Contains(text))
								{
									this._weatherNames.Add(text);
								}
							}
						}
						transform.position = new Vector3(box.Position.X, box.Position.Y, box.Position.Z);
						transform.rotation = Quaternion.AngleAxis(box.Rotation.Y, Vector3.up);
						float num = box.Scale.X * 0.5f;
						float num2 = box.Scale.Z * 0.5f;
						float num3 = num * 1.5f;
						float num4 = num2 * 1.5f;
						Vector3f vector3f = new Vector3f(box.Position.X, box.Position.Y, box.Position.Z);
						Vector3f vector3f2 = vector3f + new Vector3f(-box.Scale.X, 0f, box.Scale.Z) * 0.75f;
						Vector3f vector3f3 = vector3f2 + new Vector3f(box.Scale.X, 0f, -box.Scale.Z) * 1.5f;
						Vector2f vector2f = heightMap.CellSize * 0.5f;
						Depth waterDepth = heightMap.GetWaterDepth(vector3f + new Vector3f(0f, box.Scale.Y * 0.5f, 0f));
						if (waterDepth != Depth.Invalid)
						{
							Depth waterDepth2 = heightMap.GetWaterDepth(vector3f + new Vector3f(0f, -box.Scale.Y * 0.5f, 0f));
							List<Depth> list = new List<Depth> { waterDepth };
							if (waterDepth == Depth.Top && waterDepth2 == Depth.Bottom)
							{
								list.Add(Depth.Middle);
							}
							if (waterDepth2 != waterDepth)
							{
								list.Add(waterDepth2);
							}
							foreach (string text2 in dictionary2.Keys)
							{
								Settings.FishCodeNameExport fishCodeNameExport2 = dictionary[text2];
								OldPondsData.ImportedData.Weather.Fish fish2 = this._baseWeather.FindFish(fishCodeNameExport2.FishName);
								float num5 = Mathf.Lerp(0f, 0.4f, Mathf.Clamp01((float)dictionary2[text2] / 8f));
								LogHelper.Log("{0} {1} ({2}) = {3}", new object[] { fishCodeNameExport2.FishName, fishCodeNameExport2.FishForm, text2, num5 });
								for (int n = 0; n < list.Count; n++)
								{
									Depth depth = list[n];
									OldPondsData.ImportedData.Weather.Fish.Layer layer = fish2.FindLayer(depth, fishCodeNameExport2.FishForm);
									for (float num6 = vector3f2.z; num6 >= vector3f3.z; num6 -= vector2f.y)
									{
										for (float num7 = vector3f2.x; num7 <= vector3f3.x; num7 += vector2f.x)
										{
											Vector3f vector3f4 = new Vector3f(num7, 0f, num6) - vector3f;
											float num8 = Mathf.Sqrt(vector3f4.x * vector3f4.x / (num * num) + vector3f4.z * vector3f4.z / (num2 * num2));
											if (num8 <= 1f)
											{
												Vector3 vector = transform.TransformPoint(vector3f4.ToVector3());
												Vector2i? vector2i = heightMap.Vector3fToMatrix(new Vector3f(vector));
												if (vector2i != null)
												{
													Vector2i value = vector2i.Value;
													layer.Density[(int)value.y, (int)value.x] = num5;
												}
											}
											else if (num8 <= 1.5f)
											{
												Vector3 vector2 = transform.TransformPoint(vector3f4.ToVector3());
												Vector2i? vector2i2 = heightMap.Vector3fToMatrix(new Vector3f(vector2));
												if (vector2i2 != null)
												{
													Vector2i value2 = vector2i2.Value;
													float num9 = Mathf.Clamp01((1.5f - num8) / 0.5f);
													float num10 = Mathf.Lerp(0f, num5, num9);
													layer.Density[(int)value2.y, (int)value2.x] = num10;
												}
											}
										}
									}
								}
							}
						}
					}
				}
				Object.DestroyImmediate(transform.gameObject);
			}

			public Dictionary<string, OldPondsData.ImportedData.FishDescription> Fish
			{
				get
				{
					return this._fish;
				}
			}

			public List<string> WeatherNames
			{
				get
				{
					return this._weatherNames;
				}
			}

			public OldPondsData.ImportedData.Weather BaseWeather
			{
				get
				{
					return this._baseWeather;
				}
			}

			private Dictionary<string, OldPondsData.ImportedData.FishDescription> _fish = new Dictionary<string, OldPondsData.ImportedData.FishDescription>();

			private List<string> _weatherNames = new List<string>();

			private OldPondsData.ImportedData.Weather _baseWeather;

			private const float _SCALE = 1.5f;

			public class FishDescription
			{
				public FishDescription(FishName fishName, FishForm fishForm, float minWeight, float maxWeight)
				{
					this.FishName = fishName;
					this.FishForm = fishForm;
					this.MinWeight = minWeight;
					this.MaxWeight = maxWeight;
				}

				public readonly FishName FishName;

				public readonly FishForm FishForm;

				public float MinWeight;

				public float MaxWeight;
			}

			public class Weather
			{
				public Weather(int mapHeight, int mapWidth)
				{
					this._mapHeight = mapHeight;
					this._mapWidth = mapWidth;
				}

				public OldPondsData.ImportedData.Weather.Fish FindFish(FishName name)
				{
					OldPondsData.ImportedData.Weather.Fish fish2 = this.AllFish.FirstOrDefault((OldPondsData.ImportedData.Weather.Fish fish) => fish.Name == name);
					if (fish2 == null)
					{
						fish2 = new OldPondsData.ImportedData.Weather.Fish(name, this._mapHeight, this._mapWidth);
						this.AllFish.Add(fish2);
					}
					return fish2;
				}

				public readonly List<OldPondsData.ImportedData.Weather.Fish> AllFish = new List<OldPondsData.ImportedData.Weather.Fish>();

				private int _mapHeight;

				private int _mapWidth;

				public class Fish
				{
					public Fish(FishName name, int mapHeight, int mapWidth)
					{
						this.Name = name;
						this._mapHeight = mapHeight;
						this._mapWidth = mapWidth;
					}

					public OldPondsData.ImportedData.Weather.Fish.Layer FindLayer(Depth depth, FishForm form)
					{
						OldPondsData.ImportedData.Weather.Fish.Layer layer = this.Layers.FirstOrDefault((OldPondsData.ImportedData.Weather.Fish.Layer l) => l.FishForm == form && l.Depth == depth);
						if (layer == null)
						{
							layer = new OldPondsData.ImportedData.Weather.Fish.Layer(depth, form, this._mapHeight, this._mapWidth);
							this.Layers.Add(layer);
						}
						return layer;
					}

					public readonly List<OldPondsData.ImportedData.Weather.Fish.Layer> Layers = new List<OldPondsData.ImportedData.Weather.Fish.Layer>();

					public readonly FishName Name;

					private int _mapHeight;

					private int _mapWidth;

					public class Layer
					{
						public Layer(Depth depth, FishForm fishForm, int mapHeight, int mapWidth)
						{
							this.Depth = depth;
							this.FishForm = fishForm;
							this.Density = new float[mapHeight, mapWidth];
						}

						public readonly Depth Depth;

						public readonly FishForm FishForm;

						public readonly float[,] Density;
					}
				}
			}
		}
	}
}
