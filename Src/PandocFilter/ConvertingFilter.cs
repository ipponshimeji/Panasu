using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter {
	public abstract class ConvertingFilter: Filter {
		public Uri InputFileUri { get; private set; }
		public Uri OutputFileUri { get; private set; }

		protected ConvertingFilter(string inputFilePath, string outputFilePath) {
			// argument checks
			if (string.IsNullOrEmpty(inputFilePath)) {
				throw new ArgumentNullException(nameof(inputFilePath));
			}
			if (string.IsNullOrEmpty(outputFilePath)) {
				throw new ArgumentNullException(nameof(outputFilePath));
			}

			this.InputFileUri = new Uri(Path.GetFullPath(inputFilePath));
			this.OutputFileUri = new Uri(Path.GetFullPath(outputFilePath));
		}

		public string RebaseRelativeUri(string relativeUriString) {
			return Util.RebaseRelativeUri(this.InputFileUri, relativeUriString, this.OutputFileUri);
		}
	}
}
