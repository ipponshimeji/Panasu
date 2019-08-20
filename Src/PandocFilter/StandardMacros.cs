using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PandocUtil.PandocFilter {
	public static class StandardMacros {
		#region types

		public class Names {
			public const string Rebase = "rebase";
		}

		#endregion


		#region methods

		public static object Rebase(IReadOnlyDictionary<string, object> macro, bool generate, Uri oldBaseUri, Uri newBaseUri) {
			// argument checks
			if (macro == null) {
				throw new ArgumentNullException(nameof(macro));
			}

			// get 'value' value
			IDictionary<string, object> value = macro.GetOptionalValue<IDictionary<string, object>>("value", null);
			if (value == null) {
				return null;
			}
			if (generate) {
				value = (IDictionary<string, object>)Util.CloneJsonObject(value);
			}
			(string type, object contents) = Schema.IsElement(value);
			IList<object> arrayContents = contents as IList<object>;
			if (type == null || arrayContents == null) {
				return null;
			}

			// rebase the 'value' contents
			for (int i = 0; i < arrayContents.Count; ++i) {
				IDictionary<string, object> content = arrayContents[i] as IDictionary<string, object>;
				if (content != null) {
					(string t, object c) = Schema.IsElement(content);
					switch (t) {
						case Schema.TypeNames.Str:
							string oldLink = c as string;
							if (oldLink != null) {
								content[Schema.Names.C] = Util.RebaseRelativeUri(oldBaseUri, oldLink, newBaseUri);
								return value;
							}
							break;
					}
				}
			}

			return value;	// not converted
		}

		#endregion
	}
}
