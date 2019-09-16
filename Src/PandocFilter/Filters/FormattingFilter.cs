using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

			private Dictionary<string, string> extensionMap = null;

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

			// Use GetExtensionMap() or SetExtentionMap() to get/set writable interface.
			public IReadOnlyDictionary<string, string> ExtensionMap {
				get {
					return this.extensionMap;
				}
			}

			#endregion


			#region creation

			public Parameters(): base() {
			}

			public Parameters(Parameters src) : base(src) {
				// copy the src's contents
				this.rebaseOtherRelativeLinks = src.rebaseOtherRelativeLinks;
				this.extensionMap = (src.extensionMap == null)? null: new Dictionary<string, string>(src.extensionMap);
			}

			public Parameters(IReadOnlyDictionary<string, object> jsonObj, Parameters overwriteParams, bool complete = false): base(jsonObj, overwriteParams, complete) {
				// argument checks
				// arguments are checked by the base class at this point
				Debug.Assert(jsonObj != null);
				Debug.Assert(overwriteParams != null);

				// state checks
				Debug.Assert(this.IsFreezed == false);

				// initialize members
				this.rebaseOtherRelativeLinks = GetOptionalValueTypeParameter<bool>(jsonObj, Names.RebaseOtherRelativeLinks, overwriteParams.RebaseOtherRelativeLinks, null);
				if (overwriteParams.ExtensionMap != null) {
					this.extensionMap = new Dictionary<string, string>(overwriteParams.ExtensionMap);
				} else {
					IReadOnlyDictionary<string, object> extensionMap = jsonObj.GetOptionalValue<IReadOnlyDictionary<string, object>>(Names.ExtensionMap, null);
					if (extensionMap == null) {
						this.extensionMap = null;
					} else {
						this.extensionMap = extensionMap.ToDictionary(pair => pair.Key, pair => pair.Value.ToString());
					}
				}

				if (complete) {
					// set default value if the value is not set
					if (this.rebaseOtherRelativeLinks.HasValue == false) {
						this.rebaseOtherRelativeLinks = false;
					}
					Dictionary<string, string> extensionMap = this.extensionMap;
					if (extensionMap == null) {
						extensionMap = new Dictionary<string, string>();
						this.extensionMap = extensionMap;
					}
					string fromExt = Path.GetExtension(this.FromFileRelPath);
					string toExt;
					if (extensionMap.TryGetValue(fromExt, out toExt) == false) {
						extensionMap[fromExt] = Path.GetExtension(this.ToFileRelPath);
					}
				}
			}

			public Parameters(IReadOnlyDictionary<string, object> jsonObj): this(jsonObj, new Parameters()) {
			}


			public override Filter.Parameters Clone() {
				return new Parameters(this);
			}

			#endregion


			#region methods

			public Dictionary<string, string> GetExtensionMap() {
				// state checks
				EnsureNotFreezed();

				return this.extensionMap;
			}

			public void SetExtensionMap(Dictionary<string, string> value) {
				// state checks
				EnsureNotFreezed();

				this.extensionMap = value;
			}

			public void AddExtensionMap(string fromExt, string toExt) {
				// argument checks
				if (fromExt == null) {
					throw new ArgumentNullException(nameof(fromExt));
				}

				// state checks
				Dictionary<string, string> map = this.extensionMap;
				if (map == null) {
					map = new Dictionary<string, string>();
					this.extensionMap = map;
				}

				map[fromExt] = toExt;
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

		public new class Configuration: ConvertingFilter.Configuration {
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

		public FormattingFilter(Configuration config): base(config) {
		}

		public FormattingFilter(): base(new Configuration()) {
		}

		#endregion


		#region overrides

		protected override Filter.Parameters CreateParameters(IReadOnlyDictionary<string, object> jsonObj, Filter.Parameters overwiteParams, bool complete) {
			return new Parameters(jsonObj, (Parameters)overwiteParams, complete);
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

			Parameters parameters = context.GetParameters<Parameters>();
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
				if (parameters.RebaseOtherRelativeLinks ?? false) {
					// rebase
					Uri newUri = parameters.ToFileUri.MakeRelativeUri(new Uri(parameters.FromFileUri, unescapedPath));
					newTarget = newUri.ToString();
					if (0 < fragment.Length) {
						newTarget = string.Concat(newTarget, fragment);
					}
				} else {
					// no rebase
					newTarget = target;
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
