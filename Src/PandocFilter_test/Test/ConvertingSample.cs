using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PandocUtil.PandocFilter.Utils;

namespace PandocUtil.PandocFilter.Test {
	public class ConvertingSample: FilteringSample {
		#region data

		public readonly string SupposedFromFileUri;

		public readonly string SupposedToFileUri;

		#endregion


		#region creation

		public ConvertingSample(string description, string inputFilePath, string answerFilePath, string supposedFromFileUri, string supposedToFileUri):
		base(description, inputFilePath, answerFilePath) {
			// argument checks
			if (string.IsNullOrEmpty(supposedFromFileUri)) {
				throw new ArgumentNullException(nameof(supposedFromFileUri));
			}
			if (string.IsNullOrEmpty(supposedToFileUri)) {
				throw new ArgumentNullException(nameof(supposedToFileUri));
			}

			// initialize members
			this.SupposedFromFileUri = supposedFromFileUri;
			this.SupposedToFileUri = supposedToFileUri;
		}

		protected ConvertingSample(IReadOnlyDictionary<string, object> config, string basePath): base(config, basePath) {
			// argument checks
			Debug.Assert(config != null);

			// initialize members
			string tempDirPath = Path.GetTempPath();
			string getFullPath(string path) {
				return Path.Combine(tempDirPath, path);
			}

			try {
				this.SupposedFromFileUri = getFullPath(config.GetIndispensableValue<string>("SupposedFromFileUri"));
				this.SupposedToFileUri = getFullPath(config.GetIndispensableValue<string>("SupposedToFileUri"));
			} catch (KeyNotFoundException exception) {
				throw new ArgumentException(exception.Message, nameof(config));
			}
		}

		#endregion
	}
}
