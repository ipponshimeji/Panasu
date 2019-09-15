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

			public Parameters(IReadOnlyDictionary<string, object> metadataParams, Parameters overwriteParams): base(metadataParams, overwriteParams) {
				// argument checks
				// arguments are checked by the base class at this point
				Debug.Assert(metadataParams != null);
				Debug.Assert(overwriteParams != null);

				// initialize members
				string fromBaseDirPath = GetIndispensableReferenceTypeParameter(metadataParams, Names.FromBaseDirPath, overwriteParams.FromBaseDirPath);
				string fromFileRelPath = GetIndispensableReferenceTypeParameter(metadataParams, Names.FromFileRelPath, overwriteParams.FromFileRelPath);
				SetFromFile(fromBaseDirPath, fromFileRelPath);

				string toBaseDirPath = GetIndispensableReferenceTypeParameter(metadataParams, Names.ToBaseDirPath, overwriteParams.ToBaseDirPath);
				string toFileRelPath = GetIndispensableReferenceTypeParameter(metadataParams, Names.ToFileRelPath, overwriteParams.ToFileRelPath);
				SetToFile(toBaseDirPath, toFileRelPath);
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

		public new class Config: Filter.Config {
			#region properties

			public new Parameters Parameters {
				get {
					return GetParameters<Parameters>();
				}
			}

			#endregion


			#region creation

			protected Config(Parameters parameters): base(parameters) {
			}

			public Config(): this(new Parameters()) {
			}

			#endregion
		}



		protected new class Old_Parameters: Filter.Old_Parameters {
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

			public Old_Parameters(Dictionary<string, object> dictionary, bool ast) : base(dictionary, ast) {
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

		protected override Filter.Old_Parameters NewParameters(Dictionary<string, object> ast) {
			return new Old_Parameters(ast, ast: true);
		}

		protected override void SetupParameters(Filter.Old_Parameters parameters) {
			// argument checks
			Debug.Assert(parameters != null);
			Old_Parameters actualParams = parameters as Old_Parameters;
			Debug.Assert(actualParams != null);

			// setup the base class level
			base.SetupParameters(parameters);

			// setup parameters
			string fromBaseDirPath = actualParams.GetIndispensableReferenceTypeMetadataParameter(Old_Parameters.Names.FromBaseDirPath, this.FromBaseDirPath);
			string fromFileRelPath = actualParams.GetIndispensableReferenceTypeMetadataParameter(Old_Parameters.Names.FromFileRelPath, this.FromFileRelPath);
			actualParams.SetFromFile(fromBaseDirPath, fromFileRelPath);

			string toBaseDirPath = actualParams.GetIndispensableReferenceTypeMetadataParameter(Old_Parameters.Names.ToBaseDirPath, this.ToBaseDirPath);
			string toFileRelPath = actualParams.GetIndispensableReferenceTypeMetadataParameter(Old_Parameters.Names.ToFileRelPath, this.ToFileRelPath);
			actualParams.SetToFile(toBaseDirPath, toFileRelPath);
		}

		protected override object ExpandMacro<ActualContext>(ActualContext context, string macroName) {
			// argument checks
			if (macroName == null) {
				throw new ArgumentNullException(nameof(macroName));
			}

			switch (macroName) {
				case StandardMacros.Names.Rebase: {
					Old_Parameters parameters = context.GetParameters<Old_Parameters>();
					return StandardMacros.Rebase(context, ExpandMacroParameter<ActualContext>, parameters.ToBaseDirUri, parameters.ToFileUri);
				}
				case StandardMacros.Names.Condition: {
					Old_Parameters parameters = context.GetParameters<Old_Parameters>();
					return StandardMacros.Condition(context, ExpandMacroParameter<ActualContext>, parameters.FromFileRelPath);
				}
				default:
					return base.ExpandMacro<ActualContext>(context, macroName);
			}
		}

		#endregion
	}
}
