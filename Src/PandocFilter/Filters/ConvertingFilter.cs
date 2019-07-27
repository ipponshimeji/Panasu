using System;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter.Filters {
	public abstract class ConvertingFilter: Filter {
		#region data

		public readonly Uri InputFileUri;

		public readonly Uri OutputFileUri;

		#endregion


		#region constructors

		protected ConvertingFilter(string inputFilePath, string outputFilePath): base() {
			// argument checks
			if (string.IsNullOrEmpty(inputFilePath)) {
				throw new ArgumentNullException(nameof(inputFilePath));
			}
			if (string.IsNullOrEmpty(outputFilePath)) {
				throw new ArgumentNullException(nameof(outputFilePath));
			}

			// initialize members
			this.InputFileUri = new Uri(Path.GetFullPath(inputFilePath));
			this.OutputFileUri = new Uri(Path.GetFullPath(outputFilePath));
		}

		#endregion


		#region methods

		public string RebaseRelativeUri(string relativeUriString) {
			return Util.RebaseRelativeUri(this.InputFileUri, relativeUriString, this.OutputFileUri);
		}

		#endregion
	}
}
