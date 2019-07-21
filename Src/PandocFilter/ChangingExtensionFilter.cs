using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter {
	public class ChangingExtensionFilter: ConvertingFilter {
		public readonly bool RebaseOtherRelativeLink;
		private readonly Dictionary<string, string> extensionMap;

		public IReadOnlyDictionary<string, string> ExtensionMap {
			get {
				return this.extensionMap;
			}
		}


		public ChangingExtensionFilter(string inputFilePath, string outputFilePath, bool rebaseOtherRelativeLink, IDictionary<string, string> extensionMap) : base(inputFilePath, outputFilePath) {
			// argument checks

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

		protected override void HandleElement(IDictionary<string, object> element, string type, object contents) {
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

			base.HandleElement(element, type, contents);
		}

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
				newTarget = Path.ChangeExtension(unescapedPath, outputExtension);
				if (0 < fragment.Length) {
					newTarget = string.Concat(newTarget, fragment);
				}
			} else {
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
	}
}
