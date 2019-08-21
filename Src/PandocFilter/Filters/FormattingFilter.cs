using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter.Filters {
	public class FormattingFilter: ConvertingFilter {
		#region data

		public readonly bool RebaseOtherRelativeLinks;

		private readonly Dictionary<string, string> extensionMap;

		#endregion


		#region properties

		public IReadOnlyDictionary<string, string> ExtensionMap {
			get {
				return this.extensionMap;
			}
		}

		#endregion


		#region constructors

		public FormattingFilter(string fromBaseDirPath, string fromFileRelPath, string toBaseDirPath, string toFileRelPath, bool rebaseOtherRelativeLinks, IReadOnlyDictionary<string, string> extensionMap) :
		base(fromBaseDirPath, fromFileRelPath, toBaseDirPath, toFileRelPath) {
			// argument checks
			// extensionMap can be null

			// initialize membera
			this.RebaseOtherRelativeLinks = rebaseOtherRelativeLinks;
			this.extensionMap = (extensionMap == null) ? new Dictionary<string, string>() : new Dictionary<string, string>(extensionMap);
		}

		public FormattingFilter(string fromBaseDirPath, string fromFileRelPath, string toBaseDirPath, string toFileRelPath, bool rebaseOtherRelativeLinks, string fromExtension, string toExtension) :
		base(fromBaseDirPath, fromFileRelPath, toBaseDirPath, toFileRelPath) {
			// argument checks
			if (fromExtension == null) {
				fromExtension = string.Empty;
			}
			if (toExtension == null) {
				toExtension = string.Empty;
			}

			// initialize membera
			this.RebaseOtherRelativeLinks = rebaseOtherRelativeLinks;
			this.extensionMap = new Dictionary<string, string>(1);
			this.extensionMap.Add(fromExtension, toExtension);
		}

		#endregion


		#region overrides

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
								val[0] = ConvertLink(target);
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

		protected virtual string ConvertLink(string target) {
			// argument checks
			if (string.IsNullOrEmpty(target)) {
				throw new ArgumentNullException(nameof(target));
			}
			if (Util.IsRelativeUri(target) == false) {
				// an absolute path is not changed
				return target;
			}

			(string unescapedPath, string fragment) = Util.DecomposeRelativeUri(target);
			string newTarget;
			string outputExtension;
			if (this.extensionMap.TryGetValue(Path.GetExtension(unescapedPath), out outputExtension)) {
				// target to change extension
				newTarget = Path.ChangeExtension(unescapedPath, outputExtension);
				if (0 < fragment.Length) {
					newTarget = string.Concat(newTarget, fragment);
				}
			} else {
				// not a target to change extension
				if (!this.RebaseOtherRelativeLinks) {
					newTarget = target;
				} else {
					Uri newUri = this.ToFileUri.MakeRelativeUri(new Uri(this.FromFileUri, unescapedPath));
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
