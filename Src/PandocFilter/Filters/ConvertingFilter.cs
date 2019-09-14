using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter.Filters {
	public abstract class ConvertingFilter: Filter {
		#region types

		protected new class Parameters: Filter.Parameters {
			#region types

			public class Names{
				#region constants

				public const string FromBaseDirPath = "FromBaseDirPath";
				public const string FromBaseDirUri = "FromBaseDirUri";
				public const string FromFileRelPath = "FromFileRelPath";
				public const string FromFileUri = "FromFileUri";
				public const string ToBaseDirPath = "ToBaseDirPath";
				public const string ToBaseDirUri = "ToBaseDirUri";
				public const string ToFileRelPath = "ToFileRelPath";
				public const string ToFileUri = "ToFileUri";

				#endregion

			}

			#endregion


			#region data

			private Uri fromBaseDirUri = null;

			private string fromFileRelPath = null;

			private Uri fromFileUri = null;

			private Uri toBaseDirUri = null;

			private string toFileRelPath = null;

			private Uri toFileUri = null;

			#endregion


			#region properties

			public Uri FromBaseDirUri {
				get {
					Uri value = this.fromBaseDirUri;
					if (value == null) {
						throw CreateNotSetupException(nameof(FromBaseDirUri));
					}

					return value;
				}
			}

			public string FromFileRelPath {
				get {
					string value = this.fromFileRelPath;
					if (value == null) {
						throw CreateNotSetupException(nameof(FromFileRelPath));
					}

					return value;
				}
			}

			public Uri FromFileUri {
				get {
					Uri value = this.fromFileUri;
					if (value == null) {
						throw CreateNotSetupException(nameof(FromFileUri));
					}

					return value;
				}
			}

			public Uri ToBaseDirUri {
				get {
					Uri value = this.toBaseDirUri;
					if (value == null) {
						throw CreateNotSetupException(nameof(ToBaseDirUri));
					}

					return value;
				}
			}

			public string ToFileRelPath {
				get {
					string value = this.toFileRelPath;
					if (value == null) {
						throw CreateNotSetupException(nameof(ToFileRelPath));
					}

					return value;
				}
			}

			public Uri ToFileUri {
				get {
					Uri value = this.toFileUri;
					if (value == null) {
						throw CreateNotSetupException(nameof(ToFileUri));
					}

					return value;
				}
			}

			#endregion


			#region creation

			public Parameters(Dictionary<string, object> dictionary, bool ast) : base(dictionary, ast) {
			}

			#endregion


			#region methods

			public void SetFromFile(string fromBaseDirPath, string fromFileRelPath) {
				// argument checks
				if (string.IsNullOrEmpty(fromBaseDirPath)) {
					throw new ArgumentNullException(nameof(fromBaseDirPath));
				}
				if (string.IsNullOrEmpty(fromFileRelPath)) {
					throw new ArgumentNullException(nameof(fromFileRelPath));
				}

				// state checks
				EnsureNotFreezed();

				// set from file values
				this.fromBaseDirUri = GetDirectoryUri(fromBaseDirPath);
				this.fromFileRelPath = NormalizeDirectorySeparator(fromFileRelPath);
				this.fromFileUri = new Uri(this.fromBaseDirUri, this.fromFileRelPath);
			}

			public void SetToFile(string toBaseDirPath, string toFileRelPath) {
				// argument checks
				if (string.IsNullOrEmpty(toBaseDirPath)) {
					throw new ArgumentNullException(nameof(toBaseDirPath));
				}
				if (string.IsNullOrEmpty(toFileRelPath)) {
					throw new ArgumentNullException(nameof(toFileRelPath));
				}

				// state checks
				EnsureNotFreezed();

				// set to file values
				this.toBaseDirUri = GetDirectoryUri(toBaseDirPath);
				this.toFileRelPath = NormalizeDirectorySeparator(toFileRelPath);
				this.toFileUri = new Uri(this.toBaseDirUri, this.toFileRelPath);
			}

			private Uri GetDirectoryUri(string dirPath) {
				// argument checks
				Debug.Assert(string.IsNullOrEmpty(dirPath) == false);
				dirPath = Path.GetFullPath(dirPath);

				// ensure the path ends with a directory separator
				char lastChar = dirPath[dirPath.Length - 1];
				if (lastChar != Path.DirectorySeparatorChar && lastChar != Path.AltDirectorySeparatorChar) {
					dirPath = string.Concat(dirPath, Path.DirectorySeparatorChar.ToString());
				}

				// create a uri for the directory
				return new Uri(dirPath);
			}

			private string NormalizeDirectorySeparator(string path) {
				// argument checks
				Debug.Assert(string.IsNullOrEmpty(path) == false);

				return path.Replace('\\', '/');   // normalize dir separators to '/'
			}

			#endregion
		}

		#endregion


		#region data

		/// <summary>
		/// The path of the directory which is the base of the files to be converted by pandoc.
		/// </summary>
		public string FromBaseDirPath { get; set; } = null;

		/// <summary>
		/// The relative path of the file which pandoc converts from.
		/// </summary>
		/// <remarks>
		/// The path is relative from FromBaseDirPath.
		/// Note that this file is not the direct input of this filter.
		/// The direct input is the AST data converted from the file. 
		/// </remarks>
		public string FromFileRelPath { get; set; } = null;

		/// <summary>
		/// The path of the directory which is the base of the files converted by pandoc.
		/// </summary>
		public string ToBaseDirPath { get; set; } = null;

		/// <summary>
		/// The relative path of the file which pandoc converts to.
		/// </summary>
		/// <remarks>
		/// The path is relative from ToBaseDirUri.
		/// Note that this file is not the direct output of this filter.
		/// The direct output is the AST data to be converted to the file. 
		/// </remarks>
		public string ToFileRelPath { get; set; } = null;

		#endregion


		#region constructors

		protected ConvertingFilter(): base() {
		}

		#endregion


		#region overrides

		protected override Filter.Parameters NewParameters(Dictionary<string, object> ast) {
			return new Parameters(ast, ast: true);
		}

		protected override void SetupParameters(Filter.Parameters parameters) {
			// argument checks
			Debug.Assert(parameters != null);
			Parameters actualParams = parameters as Parameters;
			Debug.Assert(actualParams != null);

			// setup the base class level
			base.SetupParameters(parameters);

			// setup parameters
			string fromBaseDirPath = actualParams.GetIndispensableReferenceTypeMetadataParameter(Parameters.Names.FromBaseDirPath, this.FromBaseDirPath);
			string fromFileRelPath = actualParams.GetIndispensableReferenceTypeMetadataParameter(Parameters.Names.FromFileRelPath, this.FromFileRelPath);
			actualParams.SetFromFile(fromBaseDirPath, fromFileRelPath);

			string toBaseDirPath = actualParams.GetIndispensableReferenceTypeMetadataParameter(Parameters.Names.ToBaseDirPath, this.ToBaseDirPath);
			string toFileRelPath = actualParams.GetIndispensableReferenceTypeMetadataParameter(Parameters.Names.ToFileRelPath, this.ToFileRelPath);
			actualParams.SetToFile(toBaseDirPath, toFileRelPath);
		}

		protected override object ExpandMacro<ActualContext>(ActualContext context, string macroName) {
			// argument checks
			if (macroName == null) {
				throw new ArgumentNullException(nameof(macroName));
			}

			switch (macroName) {
				case StandardMacros.Names.Rebase: {
					Parameters parameters = context.GetParameters<Parameters>();
					return StandardMacros.Rebase(context, ExpandMacroParameter<ActualContext>, parameters.ToBaseDirUri, parameters.ToFileUri);
				}
				case StandardMacros.Names.Condition: {
					Parameters parameters = context.GetParameters<Parameters>();
					return StandardMacros.Condition(context, ExpandMacroParameter<ActualContext>, parameters.FromFileRelPath);
				}
				default:
					return base.ExpandMacro<ActualContext>(context, macroName);
			}
		}

		#endregion
	}
}
