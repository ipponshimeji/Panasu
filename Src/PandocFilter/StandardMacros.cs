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

		public static Exception CreateIndispensableParameterMissingException(string macroName, string paramName) {
			return new ApplicationException($"The indispensable parameter '{paramName}' for '{macroName}' macro is missing.");
		}

		public static Exception CreateInvalidParameterException(string macroName, string paramName, string reason) {
			return new ApplicationException($"The parameter '{paramName}' for '{macroName}' is invalid: {reason}");
		}

		public static object Rebase(IReadOnlyDictionary<string, object> macro, bool generate, Uri oldBaseUri, Uri newBaseUri) {
			// argument checks
			if (macro == null) {
				throw new ArgumentNullException(nameof(macro));
			}

			const string macroName = "rebase";

			// get 'target' value
			const string targetParamName = "target";
			IDictionary<string, object> value = macro.GetOptionalValue<IDictionary<string, object>>(targetParamName, null);
			if (value == null) {
				throw CreateIndispensableParameterMissingException(macroName, targetParamName);
			}
			if (generate) {
				value = (IDictionary<string, object>)Util.CloneJsonObject(value);
			}

			(string type, object contents) = Schema.IsElement(value);
			IList<object> arrayContents = contents as IList<object>;
			if (type == null || arrayContents == null) {
				throw CreateInvalidParameterException(macroName, targetParamName, "unexpected format.");
			}

			// rebase the 'target' contents
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
