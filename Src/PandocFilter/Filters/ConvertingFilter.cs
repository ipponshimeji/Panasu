using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter.Filters {
	public abstract class ConvertingFilter: Filter {
		#region types

		public new class Parameters: Filter.Parameters {
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

			public Parameters(): base() {
			}

			public Parameters(Parameters src): base(src) {
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

			public Parameters(IReadOnlyDictionary<string, object> jsonObj, Parameters overwriteParams, bool complete = false): base(jsonObj, overwriteParams, complete) {
				// argument checks
				// arguments are checked by the base class at this point
				Debug.Assert(jsonObj != null);
				Debug.Assert(overwriteParams != null);

				// initialize members
				this.FromBaseDirPath = GetOptionalReferenceTypeParameter(jsonObj, Names.FromBaseDirPath, overwriteParams.FromBaseDirPath, null);
				this.FromFileRelPath = GetOptionalReferenceTypeParameter(jsonObj, Names.FromFileRelPath, overwriteParams.FromFileRelPath, null);

				this.ToBaseDirPath = GetOptionalReferenceTypeParameter(jsonObj, Names.ToBaseDirPath, overwriteParams.ToBaseDirPath, null);
				this.ToFileRelPath = GetOptionalReferenceTypeParameter(jsonObj, Names.ToFileRelPath, overwriteParams.ToFileRelPath, null);

				if (complete) {
					// check whether the indispensable parameters are set or not
					if (this.FromBaseDirPath == null) {
						throw CreateMissingParameterException(Names.FromBaseDirPath);
					}
					if (this.FromFileRelPath == null) {
						throw CreateMissingParameterException(Names.FromFileRelPath);
					}
					if (this.ToBaseDirPath == null) {
						throw CreateMissingParameterException(Names.ToBaseDirPath);
					}
					if (this.ToFileRelPath == null) {
						throw CreateMissingParameterException(Names.ToFileRelPath);
					}
					Debug.Assert(this.FromBaseDirUri != null);
					Debug.Assert(this.FromFileUri != null);
					Debug.Assert(this.ToBaseDirUri != null);
					Debug.Assert(this.ToFileUri != null);
				}
			}

			public override Filter.Parameters Clone() {
				return new Parameters(this);
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

			protected override void ReportInvalidParameters(Action<string, string> report) {
				// argument checks
				Debug.Assert(report != null);

				// check the base class lelvel
				base.ReportInvalidParameters(report);

				// check this class lelvel
				if (this.fromBaseDirPath == null) {
					report(Names.FromBaseDirPath, null);
				}
				if (this.fromFileRelPath == null) {
					report(Names.FromFileRelPath, null);
				}
				if (this.toBaseDirPath == null) {
					report(Names.ToBaseDirPath, null);
				}
				if (this.toFileRelPath == null) {
					report(Names.ToFileRelPath, null);
				}
			}

			#endregion
		}

		public new class Configuration: Filter.Configuration {
			#region properties

			public new Parameters Parameters {
				get {
					return GetParameters<Parameters>();
				}
			}

			#endregion


			#region creation

			protected Configuration(Parameters parameters, IReadOnlyDictionary<string, object> jsonObj = null): base(parameters, jsonObj) {
			}

			public Configuration(IReadOnlyDictionary<string, object> jsonObj = null): this(CreateParameters(jsonObj), jsonObj) {
			}

			private static Parameters CreateParameters(IReadOnlyDictionary<string, object> jsonObj) {
				return (jsonObj == null) ? new Parameters() : new Parameters(GetParametersObj(jsonObj), new Parameters(), false);
			}

			public Configuration(Configuration src): base(src) {
			}

			public override Filter.Configuration Clone() {
				return new Configuration(this);
			}

			#endregion
		}

		#endregion


		#region properties

		public new Configuration Config {
			get {
				return GetConfiguration<Configuration>();
			}
		}

		#endregion


		#region constructors

		protected ConvertingFilter(Configuration config): base(config) {
		}

		#endregion


		#region overrides

		protected override Filter.Parameters CreateParameters(IReadOnlyDictionary<string, object> jsonObj, Filter.Parameters overwiteParams, bool complete) {
			return new Parameters(jsonObj, (Parameters)overwiteParams, complete);
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
