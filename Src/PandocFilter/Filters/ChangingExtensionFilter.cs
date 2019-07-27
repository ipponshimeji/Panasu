using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter.Filters {
	public class ChangingExtensionFilter: ConvertingFilter {
		#region data

		public readonly bool RebaseOtherRelativeLink;

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

		public ChangingExtensionFilter(string inputFilePath, string outputFilePath, bool rebaseOtherRelativeLink, IDictionary<string, string> extensionMap) : base(inputFilePath, outputFilePath) {
			// argument checks
			// extensionMap can be null

			// initialize membera
			this.RebaseOtherRelativeLink = rebaseOtherRelativeLink;
			this.extensionMap = (extensionMap == null) ? new Dictionary<string, string>() : new Dictionary<string, string>(extensionMap);
		}

		public ChangingExtensionFilter(string inputFilePath, string outputFilePath, bool rebaseOtherRelativeLink, string inputExtension, string outputExtension) : base(inputFilePath, outputFilePath) {
			// argument checks
			if (inputExtension == null) {
				inputExtension = string.Empty;
			}
			if (outputExtension == null) {
				outputExtension = string.Empty;
			}

			// initialize membera
			this.RebaseOtherRelativeLink = rebaseOtherRelativeLink;
			this.extensionMap = new Dictionary<string, string>(1);
			this.extensionMap.Add(inputExtension, outputExtension);
		}

		#endregion


		#region overrides

		protected override void ModifyElement(Context context, IDictionary<string, object> element, string type, object contents) {
			// argument checks
			Debug.Assert(element != null);
			Debug.Assert(type != null);

			switch (type) {
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
				case Schema.TypeNames.Image: {
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
			}

			base.ModifyElement(context, element, type, contents);
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
				if (!this.RebaseOtherRelativeLink) {
					newTarget = target;
				} else {
					Uri newUri = this.OutputFileUri.MakeRelativeUri(new Uri(this.InputFileUri, unescapedPath));
					newTarget = newUri.ToString();
					if (0 < fragment.Length) {
						newTarget = string.Concat(newTarget, fragment);
					}
				}
			}

			return newTarget;
		}

		#endregion
	}
}
