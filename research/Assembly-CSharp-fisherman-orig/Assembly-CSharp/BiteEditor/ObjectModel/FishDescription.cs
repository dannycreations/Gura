using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiteEditor.ObjectModel
{
	public class FishDescription
	{
		public FishDescription()
		{
			Dictionary<FishForm, Func<double, double>> dictionary = new Dictionary<FishForm, Func<double, double>>();
			dictionary.Add(FishForm.Young, (double x) => -0.0135 * x * x * x - 0.9727 * x * x + 1.9829 * x + 0.0032);
			dictionary.Add(FishForm.Common, (double x) => x);
			dictionary.Add(FishForm.Trophy, (double x) => x);
			dictionary.Add(FishForm.Unique, (double x) => 1.079 * x * x * x - 0.2853 * x * x + 0.1849 * x - 0.0086);
			this._formToNorm = dictionary;
			base..ctor();
		}

		public FishDescription(FishName name)
		{
			Dictionary<FishForm, Func<double, double>> dictionary = new Dictionary<FishForm, Func<double, double>>();
			dictionary.Add(FishForm.Young, (double x) => -0.0135 * x * x * x - 0.9727 * x * x + 1.9829 * x + 0.0032);
			dictionary.Add(FishForm.Common, (double x) => x);
			dictionary.Add(FishForm.Trophy, (double x) => x);
			dictionary.Add(FishForm.Unique, (double x) => 1.079 * x * x * x - 0.2853 * x * x + 0.1849 * x - 0.0086);
			this._formToNorm = dictionary;
			base..ctor();
			this._name = name;
			this._forms = new Dictionary<FishForm, FishDescription.FormRecord>();
		}

		public void AddForm(FishForm form, float minWeight, float maxWeight, float attractorsModifier, bool isDetractorEnabled, float detractionDuration, DetractionType detractionType, float detraction, float r100, float r0)
		{
			this._forms[form] = new FishDescription.FormRecord(minWeight, maxWeight, attractorsModifier, isDetractorEnabled, detractionDuration, detractionType, detraction, r100, r0);
		}

		public FishDescription.FormRecord GetFormData(FishForm form)
		{
			if (this._forms.ContainsKey(form))
			{
				return this._forms[form];
			}
			throw new InvalidOperationException(string.Format("Fish {0} has no description for the {1} form", this._name, form));
		}

		public FishDescription.RandomWeight GenerateRandomWeight(Random rnd, FishForm form, float weightK)
		{
			if (this._forms.ContainsKey(form))
			{
				FishDescription.FormRecord formRecord = this._forms[form];
				double num = this._formToNorm[form](rnd.NextDouble());
				float num2 = (float)((1.0 - num) * (double)formRecord.MinWeight + num * (double)formRecord.MaxWeight) * weightK;
				if (num2 > formRecord.MaxWeight || num2 < formRecord.MinWeight)
				{
					foreach (KeyValuePair<FishForm, FishDescription.FormRecord> keyValuePair in this._forms)
					{
						if (keyValuePair.Value.MinWeight <= num2 && num2 <= keyValuePair.Value.MaxWeight)
						{
							return new FishDescription.RandomWeight(keyValuePair.Key, keyValuePair.Value.MinWeight, keyValuePair.Value.MinWeight, num2);
						}
					}
				}
				return new FishDescription.RandomWeight(form, formRecord.MinWeight, formRecord.MaxWeight, num2);
			}
			throw new InvalidOperationException(string.Format("Fish {0} has no description for the {1} form", this._name, form));
		}

		public bool TestForm(FishForm form)
		{
			return this._forms.ContainsKey(form);
		}

		[JsonProperty]
		private FishName _name;

		[JsonProperty]
		private Dictionary<FishForm, FishDescription.FormRecord> _forms;

		[JsonIgnore]
		private Dictionary<FishForm, Func<double, double>> _formToNorm;

		public struct FormRecord
		{
			public FormRecord(float minWeight, float maxWeight, float attractorsModifier, bool isDetractorEnabled, float detractionDuration, DetractionType detractionType, float detraction, float r100, float r0)
			{
				this = default(FishDescription.FormRecord);
				this.AttractorsModifier = attractorsModifier;
				this.MinWeight = minWeight;
				this.MaxWeight = maxWeight;
				this.IsDetractorEnabled = isDetractorEnabled;
				this.DetractionDuration = detractionDuration;
				this.DetractionType = detractionType;
				this.Detraction = detraction;
				this.R100 = r100;
				this.R0 = r0;
			}

			public readonly float MinWeight;

			public readonly float MaxWeight;

			public readonly bool IsDetractorEnabled;

			public readonly float DetractionDuration;

			public readonly DetractionType DetractionType;

			public readonly float Detraction;

			public readonly float R100;

			public readonly float R0;

			public readonly float AttractorsModifier;
		}

		public struct RandomWeight
		{
			public RandomWeight(FishForm form, float minWeight, float maxWeight, float weight)
			{
				this = default(FishDescription.RandomWeight);
				this.Form = form;
				this.MinWeight = minWeight;
				this.MaxWeight = maxWeight;
				this.Weight = weight;
			}

			public override string ToString()
			{
				return string.Format("({0:f1}kg - {1:f1}kg) => {2:f3}kg", this.MinWeight, this.MaxWeight, this.Weight);
			}

			public readonly FishForm Form;

			public readonly float MinWeight;

			public readonly float MaxWeight;

			public readonly float Weight;
		}
	}
}
