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

			public Parameters(IReadOnlyDictionary<string, object> jsonObj, Parameters overwriteParams): base(jsonObj, overwriteParams) {
				// argument checks
				// arguments were checked by the base class at this point
				Debug.Assert(jsonObj != null);
				Debug.Assert(overwriteParams != null);

				// initialize members
				this.rebaseOtherRelativeLinks = ReadValueFrom(jsonObj, Names.RebaseOtherRelativeLinks, overwriteParams.RebaseOtherRelativeLinks);
				this.extensionMap = ReadObjectFrom<string>(jsonObj, Names.ExtensionMap, overwriteParams.ExtensionMap);
			}

			public Parameters(IReadOnlyDictionary<string, object> jsonObj): this(jsonObj, new Parameters()) {
			}

			public Parameters(Parameters src) : base(src) {
				// argument checks
				// arguments were checked by the base class at this point
				Debug.Assert(src != null);

				// copy the src's contents
				this.rebaseOtherRelativeLinks = src.rebaseOtherRelativeLinks;
				this.extensionMap = (src.extensionMap == null) ? null : new Dictionary<string, string>(src.extensionMap);
			}


			public override ConfigurationsBase Clone() {
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

			public override void CompleteContents() {
				// complete the base class level contents
				base.CompleteContents();

				// complete this class level contents

				// RebaseOtherRelativeLinks
				if (this.rebaseOtherRelativeLinks.HasValue == false) {
					// set default value
					this.rebaseOtherRelativeLinks = false;
				}

				// ExtensionMap
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

			#endregion
		}

		public new class Configurations: ConvertingFilter.Configurations {
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

			public Configurations(): this(CreateParameters) {
			}

			public Configurations(IReadOnlyDictionary<string, object> jsonObj) : this(CreateParameters, jsonObj) {
			}

			private static Parameters CreateParameters(IReadOnlyDictionary<string, object> jsonObj) {
				Debug.Assert(jsonObj != null);
				return new Parameters(jsonObj);
			}

			public Configurations(Configurations src): base(src) {
			}


			public override ConfigurationsBase Clone() {
				return new Configurations(this);
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

		public FormattingFilter(Configurations config): base(config) {
		}

		public FormattingFilter(): base(new Configurations()) {
		}

		#endregion


		#region overrides

		protected override Filter.Parameters CreateParameters(IReadOnlyDictionary<string, object> jsonObj, Filter.Parameters overwiteParams) {
			return new Parameters(jsonObj, (Parameters)overwiteParams);
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
