using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter.Filters {
	public class FormattingFilter: ConvertingFilter {
		#region types

		public new class Parameters: ConvertingFilter.Parameters {
			#region types

			public new class Names: ConvertingFilter.Parameters.Names {
				#region constants

				public const string RebaseOtherRelativeLinks = "RebaseOtherRelativeLinks";
				public const string ExtensionMap = "ExtensionMap";

				#endregion
			}

			#endregion


			#region data

			private bool? rebaseOtherRelativeLinks = null;

			private IReadOnlyDictionary<string, string> extensionMap = null;

			#endregion


			#region properties

			public bool? RebaseOtherRelativeLinks {
				get {
					return this.rebaseOtherRelativeLinks;
				}
				set {
					EnsureNotFreezed();
					this.rebaseOtherRelativeLinks = value;
				}
			}

			public IReadOnlyDictionary<string, string> ExtensionMap {
				get {
					return this.extensionMap;
				}
				set {
					EnsureNotFreezed();
					this.extensionMap = value;
				}
			}

			#endregion


			#region creation

			public Parameters(): base() {
			}

			public Parameters(IReadOnlyDictionary<string, object> metadataParams, Parameters overwriteParams) : base(metadataParams, overwriteParams) {
				// argument checks
				// arguments are checked by the base class at this point
				Debug.Assert(metadataParams != null);
				Debug.Assert(overwriteParams != null);

				// state checks
				Debug.Assert(this.IsFreezed == false);

				// initialize members
				this.rebaseOtherRelativeLinks = GetOptionalValueTypeParameter<bool>(metadataParams, Names.RebaseOtherRelativeLinks, overwriteParams.RebaseOtherRelativeLinks, false);
				this.extensionMap = GetOptionalReferenceTypeParameter(metadataParams, Names.ExtensionMap, overwriteParams.ExtensionMap, null);
				if (this.extensionMap == null) {
					this.extensionMap = new Dictionary<string, string>() {
						{ Path.GetExtension(this.FromFileRelPath), Path.GetExtension(this.ToFileRelPath) }
					};
				}
			}

			#endregion


			#region overrides

			protected override void ReportInvalidParameters(Action<string, string> report) {
				// argument checks
				Debug.Assert(report != null);

				// check the base class lelvel
				base.ReportInvalidParameters(report);

				// check this class lelvel
				if (this.extensionMap == null) {
					report(Names.ExtensionMap, null);
				}
				if (this.extensionMap == null) {
					report(Names.ExtensionMap, null);
				}
			}

			#endregion
		}

		public new class Config: ConvertingFilter.Config {
			#region properties

			public new Parameters Parameters {
				get {
					return GetParameters<Parameters>();
				}
			}

			#endregion


			#region creation

			protected Config(Parameters parameters) : base(parameters) {
			}

			public Config() : this(new Parameters()) {
			}

			#endregion
		}



		protected new class Old_Parameters: ConvertingFilter.Old_Parameters {
			#region types

			public new class Names: ConvertingFilter.Old_Parameters.Names {
				#region constants

				public const string RebaseOtherRelativeLinks = "RebaseOtherRelativeLinks";
				public const string ExtensionMap = "ExtensionMap";

				#endregion

			}

			#endregion


			#region data

			private bool? rebaseOtherRelativeLinks = null;

			private IReadOnlyDictionary<string, string> extensionMap = null;

			#endregion


			#region properties

			public bool RebaseOtherRelativeLinks {
				get {
					if (this.rebaseOtherRelativeLinks.HasValue == false) {
						throw CreateNotSetupException(nameof(RebaseOtherRelativeLinks));
					}

					return this.rebaseOtherRelativeLinks.Value;
				}
				set {
					EnsureNotFreezed();
					this.rebaseOtherRelativeLinks = value;
				}
			}

			public IReadOnlyDictionary<string, string> ExtensionMap {
				get {
					IReadOnlyDictionary<string, string> value = this.extensionMap;
					if (value == null) {
						throw CreateNotSetupException(nameof(ExtensionMap));
					}

					return value;
				}
				set {
					EnsureNotFreezed();
					this.extensionMap = value;
				}
			}

			#endregion


			#region creation

			public Old_Parameters(Dictionary<string, object> dictionary, bool ast) : base(dictionary, ast) {
			}

			#endregion
		}

		#endregion


		#region data

		public bool? RebaseOtherRelativeLinks { get; set; } = null;

		public IReadOnlyDictionary<string, string> ExtensionMap { get; set; } = null;

		#endregion


		#region constructors

		public FormattingFilter(): base() {
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

			// do the base class level arrange
			base.SetupParameters(parameters);

			// RebaseOtherRelativeLinks
			{
				bool value = actualParams.GetOptionalValueTypeMetadataParameter(Old_Parameters.Names.RebaseOtherRelativeLinks, this.RebaseOtherRelativeLinks, true);
				actualParams.RebaseOtherRelativeLinks = value;
			}

			// ExtensionMap
			{
				IReadOnlyDictionary<string, string> value = actualParams.GetOptionalReferenceTypeMetadataParameter(Old_Parameters.Names.ExtensionMap, this.ExtensionMap, null);
				if (value == null) {
					value = new Dictionary<string, string>() {
						{ Path.GetExtension(actualParams.FromFileRelPath), Path.GetExtension(actualParams.ToFileRelPath) }
					};
				}
				actualParams.ExtensionMap = value;
			}
		}

		protected override void ModifyElement(ModifyingContext context, string type, object contents) {
			// argument checks
			Debug.Assert(context != null);
			Debug.Assert(type != null);

			switch (type) {
				case Schema.TypeNames.Image:
				case Schema.TypeNames.Link: {
					IList<object> array = contents as IList<object>;
					if (array != null && 2 < array.Count) {
						IList<object> val = array[2] as IList<object>;
						if (val != null && 0 < val.Count) {
							string target = val[0] as string;
							if (target != null) {
								val[0] = ConvertLink(context, target);
							}
						}
					}
					break;
				}
				case Schema.TypeNames.Header: {
					IList<object> array = contents as IList<object>;
					if (array != null && 1 < array.Count) {
						// get heading level
						object level = array[0];
						if (level is double && (double)level == 1.0) {
							// level-1 header
							AdjustHeader1(context, type, array);
						}
					}
					break;
				}
			}

			base.ModifyElement(context, type, contents);
		}

		#endregion


		#region overridables

		protected virtual string ConvertLink(ModifyingContext context, string target) {
			// argument checks
			if (string.IsNullOrEmpty(target)) {
				throw new ArgumentNullException(nameof(target));
			}
			if (Util.IsRelativeUri(target) == false) {
				// an absolute path is not changed
				return target;
			}

			Old_Parameters parameters = context.GetParameters<Old_Parameters>();
			(string unescapedPath, string fragment) = Util.DecomposeRelativeUri(target);
			string newTarget;
			string outputExtension;
			if (parameters.ExtensionMap.TryGetValue(Path.GetExtension(unescapedPath), out outputExtension)) {
				// target to change extension
				newTarget = Path.ChangeExtension(unescapedPath, outputExtension);
				if (0 < fragment.Length) {
					newTarget = string.Concat(newTarget, fragment);
				}
			} else {
				// not a target to change extension
				if (!parameters.RebaseOtherRelativeLinks) {
					newTarget = target;
				} else {
					Uri newUri = parameters.ToFileUri.MakeRelativeUri(new Uri(parameters.FromFileUri, unescapedPath));
					newTarget = newUri.ToString();
					if (0 < fragment.Length) {
						newTarget = string.Concat(newTarget, fragment);
					}
				}
			}

			return newTarget;
		}

		protected virtual void AdjustHeader1(ModifyingContext context, string type, IList<object> contents) {
			// argument checks
			Debug.Assert(context != null);
			Debug.Assert(type == Schema.TypeNames.Header);
			Debug.Assert(contents != null);

			IDictionary<string, object> metadata = context.AST.GetMetadata(true);
			IDictionary<string, object> title;
			if (metadata.TryGetValue(Schema.Names.Title, out title) == false) {
				// the ast has no title metadata
				// set the header contents to the title
				title = new Dictionary<string, object>();
				title[Schema.Names.T] = Schema.TypeNames.MetaInlines;
				if (3 <= contents.Count) {
					title[Schema.Names.C] = contents[2];
				}
				metadata[Schema.Names.Title] = title;
			}

			// remove the header element
			// Many writers such as HTML writer generate H1 element from the title metadata.
			context.RemoveValue();
		}

		#endregion
	}
}
