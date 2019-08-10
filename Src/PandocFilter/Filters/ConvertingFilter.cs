using System;
using System.Diagnostics;
using System.IO;
using PandocUtil.PandocFilter.Utils;

namespace PandocUtil.PandocFilter.Filters {
	public abstract class ConvertingFilter: Filter {
		#region data

		/// <summary>
		/// The URI of the file which pandoc converts from.
		/// </summary>
		/// <remarks>
		/// Note that this file is not the direct input of this filter.
		/// The direct input is the AST data converted from the file. 
		/// </remarks>
		public readonly Uri FromFileUri;

		/// <summary>
		/// The URI of the file which pandoc converts to.
		/// </summary>
		/// <remarks>
		/// Note that this file is not the direct output of this filter.
		/// The direct output is the AST data to be converted to the file. 
		/// </remarks>
		public readonly Uri ToFileUri;

		#endregion


		#region constructors

		protected ConvertingFilter(string fromFilePath, string toFilePath): base() {
			// argument checks
			if (string.IsNullOrEmpty(fromFilePath)) {
				throw new ArgumentNullException(nameof(fromFilePath));
			}
			if (string.IsNullOrEmpty(toFilePath)) {
				throw new ArgumentNullException(nameof(toFilePath));
			}

			// initialize members
			this.FromFileUri = new Uri(Path.GetFullPath(fromFilePath));
			this.ToFileUri = new Uri(Path.GetFullPath(toFilePath));
		}

		#endregion


		#region methods

		public string RebaseRelativeUri(string relativeUriString) {
			return Util.RebaseRelativeUri(this.FromFileUri, relativeUriString, this.ToFileUri);
		}

		#endregion
	}
}
