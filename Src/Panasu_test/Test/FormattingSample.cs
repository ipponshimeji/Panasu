using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Panasu.Filters;

namespace Panasu.Test {
	public class FormattingSample: ConvertingSample {
		#region types

		public class StandardSampleProvider: IEnumerable<object[]> {
			#region constants

			public const string SampleGroup = "Formatting";

			#endregion


			#region data

			private static readonly FormattingSample[] samples = {
				GetSample(SampleGroup, "link_absolute"),
				GetSample(SampleGroup, "link_mapped"),
				GetSample(SampleGroup, "link_nonmapped_norebase"),
				GetSample(SampleGroup, "link_nonmapped_rebase"),
				GetSample(SampleGroup, "header_only_h1"),
				GetSample(SampleGroup, "header_title_h1"),
				GetSample(SampleGroup, "macro_rebase"),
				GetSample(SampleGroup, "macro_condition_from-file")
			};

			#endregion


			#region properties

			public static IReadOnlyList<FormattingSample> Samples {
				get {
					return samples;
				}
			}

			#endregion


			#region IEnumerable

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			#endregion


			#region IEnumerator<object[]>

			public IEnumerator<object[]> GetEnumerator() {
				foreach (FormattingSample sample in samples) {
					yield return new object[] { sample };
				}
			}

			#endregion
		}

		#endregion


		#region properties

		public new FormattingFilter.Configurations Config {
			get {
				return GetConfig<FormattingFilter.Configurations>();
			}
		}

		#endregion


		#region creation

		public FormattingSample(string description, string inputFilePath, string answerFilePath, FormattingFilter.Configurations config): base(description, inputFilePath, answerFilePath, config) {
		}

		protected FormattingSample(IReadOnlyDictionary<string, object> jsonObj, string basePath, Func<IReadOnlyDictionary<string, object>, FormattingFilter.Configurations> createConfigurations): base(jsonObj, basePath, createConfigurations) {
		}

		public FormattingSample(IReadOnlyDictionary<string, object> jsonObj, string basePath) : base(jsonObj, basePath, CreateConfigurations) {
		}

		protected static FormattingFilter.Configurations CreateConfigurations(IReadOnlyDictionary<string, object> jsonObj) {
			return new FormattingFilter.Configurations(jsonObj);
		}


		public static FormattingSample GetSample(string defFilePath) {
			// argument checks
			if (string.IsNullOrEmpty(defFilePath)) {
				throw new ArgumentNullException(nameof(defFilePath));
			}

			// read a JSON object from the config file
			string basePath = Path.GetDirectoryName(defFilePath);
			Dictionary<string, object> jsonObj = LoadDefinitionFile(defFilePath);
			return new FormattingSample(jsonObj, basePath);
		}

		public static FormattingSample GetSample(string group, string name) {
			// argument checks
			if (string.IsNullOrEmpty(group)) {
				throw new ArgumentNullException(nameof(group));
			}
			if (string.IsNullOrEmpty(name)) {
				throw new ArgumentNullException(nameof(name));
			}

			// create a instance
			string resourceDirPath = TestUtil.GetFilteringResourceDir(group);
			string defFilePath = Path.Combine(resourceDirPath, $"{name}.def.json");
			return GetSample(defFilePath);
		}

		#endregion
	}
}
