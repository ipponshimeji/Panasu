using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Utf8Json;
using Xunit;

namespace PandocUtil.PandocFilter.Test {
	public class FilteringSample {
		#region data

		public readonly string Description;

		public readonly string InputFilePath;

		public readonly string AnswerFilePath;

		#endregion


		#region creation

		public FilteringSample(string description, string inputFilePath, string answerFilePath) {
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

			// initialize members
			this.Description = description;
			this.InputFilePath = inputFilePath;
			this.AnswerFilePath = answerFilePath;
		}

		protected FilteringSample(IReadOnlyDictionary<string, object> config, string basePath) {
			// argument checks
			if (config == null) {
				throw new ArgumentNullException(nameof(config));
			}
			if (string.IsNullOrEmpty(basePath)) {
				throw new ArgumentNullException(nameof(basePath));
			}

			// initialize members
			string getFullPath(string path) {
				return Path.Combine(basePath, path);
			}

			try {
				this.Description = config.GetOptionalValue<string>("Description", string.Empty);
				this.InputFilePath = getFullPath(config.GetIndispensableValue<string>("InputFilePath"));
				this.AnswerFilePath = getFullPath(config.GetIndispensableValue<string>("AnswerFilePath"));
			} catch (KeyNotFoundException exception) {
				throw new ArgumentException(exception.Message, nameof(config));
			}
		}

		#endregion


		#region methods

		protected static Dictionary<string, object> LoadConfigFile(string configFilePath) {
			// argument checks
			if (string.IsNullOrEmpty(configFilePath)) {
				throw new ArgumentNullException(nameof(configFilePath));
			}

			// read a JSON object from the config file
			using (Stream stream = File.OpenRead(configFilePath)) {
				return JsonSerializer.Deserialize<Dictionary<string, object>>(stream);
			}
		}

		public Stream OpenInput() {
			return File.OpenRead(this.InputFilePath);
		}

		public string GetExpected() {
			return File.ReadAllText(this.AnswerFilePath);
		}

		public void AssertEqual(string actual) {
			Assert.Equal(GetExpected(), actual);
		}

		#endregion


		#region overrides

		public override string ToString() {
			return this.Description;
		}

		#endregion
	}
}
