using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter.Test {
	public class ExtensionChangingSample: ConvertingSample {
		#region types

		public class StandardSampleProvider: IEnumerable<object[]> {
			#region constants

			public const string SampleGroup = "ExtensionChanging";

			#endregion


			#region data

			private static readonly ExtensionChangingSample[] samples = {
				GetSample(SampleGroup, "link_absolute"),
				GetSample(SampleGroup, "link_mapped"),
				GetSample(SampleGroup, "link_nonmapped_norebase"),
				GetSample(SampleGroup, "link_nonmapped_rebase"),
				GetSample(SampleGroup, "header_only_h1"),
				GetSample(SampleGroup, "header_title_h1"),
				GetSample(SampleGroup, "macro_rebase")
			};

			#endregion


			#region properties

			public static IReadOnlyList<ExtensionChangingSample> Samples {
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


			#region IEnumerator<ExtensionChangingSample>

			public IEnumerator<object[]> GetEnumerator() {
				foreach (ExtensionChangingSample sample in samples) {
					yield return new object[] { sample };
				}
			}

			#endregion
		}

		#endregion


		#region data

		public readonly bool RebaseOtherRelativeLinks;

		private readonly Dictionary<string, string> extensionMap;

		#endregion


		#region properties

		public IReadOnlyDictionary<string, string> ExtensionMap {
			get {
				return this.extensionMap;
			}
		}

		#endregion


		#region creation

		public ExtensionChangingSample(string description, string inputFilePath, string answerFilePath, string supposedFromBaseDirUri, string supposedFromFileRelPath, string supposedToBaseDirUri, string supposedToFileRelPath, bool rebaseOtherRelativeLinks, Dictionary<string, string> extensionMap) :
		base(description, inputFilePath, answerFilePath, supposedFromBaseDirUri, supposedFromFileRelPath, supposedToBaseDirUri, supposedToFileRelPath) {
			// argument checks
			// extension can be null

			// initialize members
			this.RebaseOtherRelativeLinks = rebaseOtherRelativeLinks;
			this.extensionMap = extensionMap;
		}

		protected ExtensionChangingSample(IReadOnlyDictionary<string, object> config, string basePath) : base(config, basePath) {
			// argument checks
			Debug.Assert(config != null);

			// initialize members
			Dictionary<string, string> adapt(Dictionary<string, object> from) {
				if (from == null) {
					return null;
				} else {
					Dictionary<string, string> to = new Dictionary<string, string>(from.Count);
					foreach (KeyValuePair<string, object> pair in from) {
						string value = pair.Value as string;
						if (value == null) {
							string message = $"The value of '{pair.Key}' in 'ExtensionMap' must be a non-null string.";
							throw new ArgumentException(message, nameof(config));
						}
						to.Add(pair.Key, value);
					}
					return to;
				}
			}

			this.RebaseOtherRelativeLinks = config.GetOptionalValue<bool>("RebaseOtherRelativeLinks", false);
			this.extensionMap = adapt(config.GetOptionalValue<Dictionary<string, object>>("ExtensionMap", null));
		}

		public static ExtensionChangingSample GetSample(string configFilePath) {
			// argument checks
			if (string.IsNullOrEmpty(configFilePath)) {
				throw new ArgumentNullException(nameof(configFilePath));
			}

			// read a JSON object from the config file
			string basePath = Path.GetDirectoryName(configFilePath);
			Dictionary<string, object> config = LoadConfigFile(configFilePath);
			return new ExtensionChangingSample(config, basePath);
		}

		public static ExtensionChangingSample GetSample(string group, string name) {
			// argument checks
			if (string.IsNullOrEmpty(group)) {
				throw new ArgumentNullException(nameof(group));
			}
			if (string.IsNullOrEmpty(name)) {
				throw new ArgumentNullException(nameof(name));
			}

			// create a instance
			string resourceDirPath = TestUtil.GetFilteringResourceDir(group);
			string configFilePath = Path.Combine(resourceDirPath, $"{name}.config.json");
			return GetSample(configFilePath);
		}

		#endregion
	}
}
