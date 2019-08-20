using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter.Filters {
	public abstract class ConvertingFilter: Filter {
		#region data

		/// <summary>
		/// The URI of the directory which is the base of the files to be converted by pandoc.
		/// </summary>
		public readonly Uri FromBaseDirUri;

		/// <summary>
		/// The relative path of the file which pandoc converts from.
		/// </summary>
		/// <remarks>
		/// The path is relative from FromBaseDirUri.
		/// Note that this file is not the direct input of this filter.
		/// The direct input is the AST data converted from the file. 
		/// </remarks>
		public readonly string FromFileRelPath;

		/// <summary>
		/// The URI of the directory which is the base of the converted files by pandoc.
		/// </summary>
		public readonly Uri ToBaseDirUri;

		/// <summary>
		/// The relative path of the file which pandoc converts to.
		/// </summary>
		/// <remarks>
		/// The path is relative from ToBaseDirUri.
		/// Note that this file is not the direct output of this filter.
		/// The direct output is the AST data to be converted to the file. 
		/// </remarks>
		public readonly string ToFileRelPath;

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

		protected ConvertingFilter(string fromBaseDirPath, string fromFileRelPath, string toBaseDirPath, string toFileRelPath): base() {
			// argument checks
			if (string.IsNullOrEmpty(fromBaseDirPath)) {
				throw new ArgumentNullException(nameof(fromBaseDirPath));
			}
			if (string.IsNullOrEmpty(fromFileRelPath)) {
				throw new ArgumentNullException(nameof(fromFileRelPath));
			}
			if (string.IsNullOrEmpty(toBaseDirPath)) {
				throw new ArgumentNullException(nameof(toBaseDirPath));
			}
			if (string.IsNullOrEmpty(toFileRelPath)) {
				throw new ArgumentNullException(nameof(toFileRelPath));
			}

			string ensureEndWithDirectorySeparator(string path) {
				Debug.Assert(string.IsNullOrEmpty(path) == false);

				char lastChar = path[path.Length - 1];
				if (lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar) {
					return path;
				} else {
					return string.Concat(path, Path.DirectorySeparatorChar.ToString());
				}
			}

			// initialize members
			this.FromBaseDirUri = new Uri(ensureEndWithDirectorySeparator(Path.GetFullPath(fromBaseDirPath)));
			this.FromFileRelPath = fromFileRelPath;
			this.ToBaseDirUri = new Uri(ensureEndWithDirectorySeparator(Path.GetFullPath(toBaseDirPath)));
			this.ToFileRelPath = toFileRelPath;
			this.FromFileUri = new Uri(this.FromBaseDirUri, fromFileRelPath);
			this.ToFileUri = new Uri(this.ToBaseDirUri, toFileRelPath);
		}

		#endregion


		#region methods

		public string RebaseRelativeUri(string relativeUriString) {
			return Util.RebaseRelativeUri(this.FromFileUri, relativeUriString, this.ToFileUri);
		}

		#endregion


		#region overrides

		protected override void ModifyMacro(ModifyingContext context, string name, IReadOnlyDictionary<string, object> macro) {
			switch (name) {
				case StandardMacros.Names.Rebase:
					object evaluated = StandardMacros.Rebase(macro, false, this.ToBaseDirUri, this.ToFileUri);
					context.ReplaceValue(evaluated);
					return;
				default:
					base.ModifyMacro(context, name, macro);
					return;
			}
		}

		#endregion
	}
}
