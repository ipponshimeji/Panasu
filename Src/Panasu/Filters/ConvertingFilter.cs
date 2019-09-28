using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Panasu.Filters {
	public abstract class ConvertingFilter: Filter {
		#region types

		public new abstract class Parameters: Filter.Parameters {
			#region types

			public new class Names: Filter.Parameters.Names {
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

			private string fromBaseDirPath = null;

			private Uri fromBaseDirUri = null;

			private string fromFileRelPath = null;

			private Uri fromFileUri = null;

			private string toBaseDirPath = null;

			private Uri toBaseDirUri = null;

			private string toFileRelPath = null;

			private Uri toFileUri = null;

			#endregion


			#region properties

			/// <summary>
			/// The path of the directory which is the base of the files to be converted by pandoc.
			/// </summary>
			public string FromBaseDirPath {
				get {
					return this.fromBaseDirPath;
				}
				set {
					if (this.fromFileRelPath == null) {
						EnsureNotFreezed();
						this.fromBaseDirPath = value;
					} else {
						SetFromFile(value, this.fromFileRelPath);
					}
				}
			}

			public Uri FromBaseDirUri {
				get {
					return this.fromBaseDirUri;
				}
			}

			/// <summary>
			/// The relative path of the file which pandoc converts from.
			/// </summary>
			/// <remarks>
			/// The path is relative from FromBaseDirPath.
			/// Note that this file is not the direct input of this filter.
			/// The direct input is the AST data converted from the file. 
			/// </remarks>
			public string FromFileRelPath {
				get {
					return this.fromFileRelPath;
				}
				set {
					if (this.fromBaseDirPath == null) {
						EnsureNotFreezed();
						this.fromFileRelPath = value;
					} else {
						SetFromFile(this.fromBaseDirPath, value);
					}
				}
			}

			public Uri FromFileUri {
				get {
					return this.fromFileUri;
				}
			}

			/// <summary>
			/// The path of the directory which is the base of the files converted by pandoc.
			/// </summary>
			public string ToBaseDirPath {
				get {
					return this.toBaseDirPath;
				}
				set {
					if (this.toFileRelPath == null) {
						EnsureNotFreezed();
						this.toBaseDirPath = value;
					} else {
						SetToFile(value, this.toFileRelPath);
					}
				}
			}

			public Uri ToBaseDirUri {
				get {
					return this.toBaseDirUri;
				}
			}

			/// <summary>
			/// The relative path of the file which pandoc converts to.
			/// </summary>
			/// <remarks>
			/// The path is relative from ToBaseDirUri.
			/// Note that this file is not the direct output of this filter.
			/// The direct output is the AST data to be converted to the file. 
			/// </remarks>
			public string ToFileRelPath {
				get {
					return this.toFileRelPath;
				}
				set {
					if (this.toBaseDirPath == null) {
						EnsureNotFreezed();
						this.toFileRelPath = value;
					} else {
						SetToFile(this.toBaseDirPath, value);
					}
				}
			}

			public Uri ToFileUri {
				get {
					return this.toFileUri;
				}
			}

			#endregion


			#region creation

			protected Parameters(): base() {
			}

			protected Parameters(IReadOnlyDictionary<string, object> jsonObj, Parameters overwriteParams): base(jsonObj, overwriteParams) {
				// argument checks
				// arguments were checked by the base class at this point
				Debug.Assert(jsonObj != null);
				Debug.Assert(overwriteParams != null);

				// initialize members
				this.FromBaseDirPath = ReadValueFrom(jsonObj, Names.FromBaseDirPath, overwriteParams.FromBaseDirPath);
				this.FromFileRelPath = ReadValueFrom(jsonObj, Names.FromFileRelPath, overwriteParams.FromFileRelPath);

				this.ToBaseDirPath = ReadValueFrom(jsonObj, Names.ToBaseDirPath, overwriteParams.ToBaseDirPath);
				this.ToFileRelPath = ReadValueFrom(jsonObj, Names.ToFileRelPath, overwriteParams.ToFileRelPath);
			}

			protected Parameters(Parameters src) : base(src) {
				// argument checks
				// arguments were checked by the base class at this point
				Debug.Assert(src != null);

				// copy the src's contents
				this.fromBaseDirPath = src.fromBaseDirPath;
				this.fromBaseDirUri = src.fromBaseDirUri;
				this.fromFileRelPath = src.fromFileRelPath;
				this.fromFileUri = src.fromFileUri;
				this.toBaseDirPath = src.toBaseDirPath;
				this.toBaseDirUri = src.toBaseDirUri;
				this.toFileRelPath = src.toFileRelPath;
				this.toFileUri = src.toBaseDirUri;
			}

			#endregion


			#region methods

			private void SetFromFile(string fromBaseDirPath, string fromFileRelPath) {
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
				this.fromBaseDirPath = Path.GetFullPath(fromBaseDirPath);
				this.fromBaseDirUri = GetDirectoryUri(this.fromBaseDirPath);
				this.fromFileRelPath = NormalizeDirectorySeparator(fromFileRelPath);
				this.fromFileUri = new Uri(this.fromBaseDirUri, this.fromFileRelPath);
			}

			private void SetToFile(string toBaseDirPath, string toFileRelPath) {
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
				this.toBaseDirPath = Path.GetFullPath(toBaseDirPath);
				this.toBaseDirUri = GetDirectoryUri(this.toBaseDirPath);
				this.toFileRelPath = NormalizeDirectorySeparator(toFileRelPath);
				this.toFileUri = new Uri(this.toBaseDirUri, this.toFileRelPath);
			}

			private Uri GetDirectoryUri(string dirPath) {
				// argument checks
				Debug.Assert(string.IsNullOrEmpty(dirPath) == false);

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


			#region overrides

			public override void CompleteContents() {
				// complete the base class level contents
				base.CompleteContents();

				// complete this class level contents
				if (this.FromBaseDirPath == null) {
					throw CreateMissingConfigurationException(Names.FromBaseDirPath);
				}
				if (this.FromFileRelPath == null) {
					throw CreateMissingConfigurationException(Names.FromFileRelPath);
				}
				if (this.ToBaseDirPath == null) {
					throw CreateMissingConfigurationException(Names.ToBaseDirPath);
				}
				if (this.ToFileRelPath == null) {
					throw CreateMissingConfigurationException(Names.ToFileRelPath);
				}
				Debug.Assert(this.FromBaseDirUri != null);
				Debug.Assert(this.FromFileUri != null);
				Debug.Assert(this.ToBaseDirUri != null);
				Debug.Assert(this.ToFileUri != null);
			}

			#endregion
		}

		public abstract new class Configurations: Filter.Configurations {
			#region properties

			public new Parameters Parameters {
				get {
					return GetParameters<Parameters>();
				}
			}

			#endregion


			#region creation

			protected Configurations(Func<IReadOnlyDictionary<string, object>, Parameters> createParams): base(createParams) {
			}

			protected Configurations(Func<IReadOnlyDictionary<string, object>, Parameters> createParams, IReadOnlyDictionary<string, object> jsonObj): base(createParams, jsonObj) {
			}

			protected Configurations(Configurations src): base(src) {
			}

			#endregion
		}

		#endregion


		#region properties

		public new Configurations Config {
			get {
				return GetConfiguration<Configurations>();
			}
		}

		#endregion


		#region constructors

		protected ConvertingFilter(Configurations config): base(config) {
		}

		#endregion


		#region overrides

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
