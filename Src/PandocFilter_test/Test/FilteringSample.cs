using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using Utf8Json;
using Panasu.Filters;

namespace Panasu.Test {
	public class FilteringSample {
		#region data

		public readonly string Description;

		public readonly string InputFilePath;

		public readonly string AnswerFilePath;

		private readonly Filter.Configurations config;

		#endregion


		#region properties

		public Filter.Configurations Config {
			get {
				return this.config;
			}
		}

		#endregion


		#region creation

		public FilteringSample(string description, string inputFilePath, string answerFilePath, Filter.Configurations config) {
			// argument checks
			if (description == null) {
				description = string.Empty;
			}
			if (string.IsNullOrEmpty(inputFilePath)) {
				throw new ArgumentNullException(nameof(inputFilePath));
			}
			if (string.IsNullOrEmpty(answerFilePath)) {
				throw new ArgumentNullException(nameof(answerFilePath));
			}
			if (config == null) {
				throw new ArgumentNullException(nameof(config));
			}

			// initialize members
			this.Description = description;
			this.InputFilePath = inputFilePath;
			this.AnswerFilePath = answerFilePath;
			this.config = config;
		}

		protected FilteringSample(IReadOnlyDictionary<string, object> jsonObj, string basePath, Func<IReadOnlyDictionary<string, object>, Filter.Configurations> createConfigurations) {
			// argument checks
			if (jsonObj == null) {
				throw new ArgumentNullException(nameof(jsonObj));
			}
			if (string.IsNullOrEmpty(basePath)) {
				throw new ArgumentNullException(nameof(basePath));
			}
			if (createConfigurations == null) {
				throw new ArgumentNullException(nameof(jsonObj));
			}

			// initialize members
			string getFullPath(string path) {
				return Path.Combine(basePath, path);
			}

			try {
				this.Description = jsonObj.GetOptionalValue<string>("Description", string.Empty);
				this.InputFilePath = getFullPath(jsonObj.GetIndispensableValue<string>("InputFilePath"));
				this.AnswerFilePath = getFullPath(jsonObj.GetIndispensableValue<string>("AnswerFilePath"));

				IReadOnlyDictionary<string, object> configJsonObj = jsonObj.GetOptionalValue<IReadOnlyDictionary<string, object>>("Config", null);
				if (configJsonObj == null) {
					configJsonObj = ImmutableDictionary<string, object>.Empty;
				}
				this.config = createConfigurations(configJsonObj);
			} catch (KeyNotFoundException exception) {
				throw new ArgumentException(exception.Message, nameof(jsonObj));
			}
		}

		#endregion


		#region methods

		protected static Dictionary<string, object> LoadDefinitionFile(string defFilePath) {
			// argument checks
			if (string.IsNullOrEmpty(defFilePath)) {
				throw new ArgumentNullException(nameof(defFilePath));
			}

			// read a JSON object from the definition file
			using (Stream stream = File.OpenRead(defFilePath)) {
				return JsonSerializer.Deserialize<Dictionary<string, object>>(stream);
			}
		}

		public Stream OpenInput() {
			return File.OpenRead(this.InputFilePath);
		}

		public Dictionary<string, object> GetExpected() {
			using (Stream stream = File.OpenRead(this.AnswerFilePath)) {
				return JsonSerializer.Deserialize<Dictionary<string, object>>(stream);
			}
		}

		public void AssertEqual(IReadOnlyDictionary<string, object> actual) {
			TestUtil.EqualJson(GetExpected(), actual);
		}

		protected TConfiguration GetConfig<TConfiguration>() where TConfiguration: Filter.Configurations {
			return (TConfiguration)this.config;
		}	

		#endregion


		#region overrides

		public override string ToString() {
			return this.Description;
		}

		#endregion
	}
}
