using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter.Test {
	public class ConvertingSample: FilteringSample {
		#region data

		public readonly string SupposedFromBaseDirUri;

		public readonly string SupposedFromFileRelPath;

		public readonly string SupposedToBaseDirUri;

		public readonly string SupposedToFileRelPath;

		#endregion


		#region creation

		public ConvertingSample(string description, string inputFilePath, string answerFilePath, string supposedFromBaseDirUri, string supposedFromFileRelPath, string supposedToBaseDirUri, string supposedToFileRelPath):
		base(description, inputFilePath, answerFilePath) {
			// argument checks
			if (string.IsNullOrEmpty(supposedFromBaseDirUri)) {
				throw new ArgumentNullException(nameof(supposedFromBaseDirUri));
			}
			if (string.IsNullOrEmpty(supposedFromFileRelPath)) {
				throw new ArgumentNullException(nameof(supposedFromFileRelPath));
			}
			if (string.IsNullOrEmpty(supposedToBaseDirUri)) {
				throw new ArgumentNullException(nameof(supposedToBaseDirUri));
			}
			if (string.IsNullOrEmpty(supposedToFileRelPath)) {
				throw new ArgumentNullException(nameof(supposedToFileRelPath));
			}

			// initialize members
			this.SupposedFromBaseDirUri = supposedFromBaseDirUri;
			this.SupposedFromFileRelPath = supposedFromFileRelPath;
			this.SupposedToBaseDirUri = supposedToBaseDirUri;
			this.SupposedToFileRelPath = supposedToFileRelPath;
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
				this.SupposedFromBaseDirUri = getFullPath(config.GetIndispensableValue<string>("SupposedFromBaseDirUri"));
				this.SupposedFromFileRelPath = config.GetIndispensableValue<string>("SupposedFromFileRelPath");
				this.SupposedToBaseDirUri = getFullPath(config.GetIndispensableValue<string>("SupposedToBaseDirUri"));
				this.SupposedToFileRelPath = config.GetIndispensableValue<string>("SupposedToFileRelPath");
			} catch (KeyNotFoundException exception) {
				throw new ArgumentException(exception.Message, nameof(config));
			}
		}

		#endregion
	}
}
