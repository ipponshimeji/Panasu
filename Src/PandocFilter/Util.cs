using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PandocUtil.PandocFilter {
	public static class Util {
		public static object CloneJsonObject(object src) {
			// argument checks
			if (src == null) {
				// clone null
				return null;
			}

			// clone the json object
			switch (src) {
				case IDictionary<string, object> obj:
					// object
					return obj.ToDictionary(key => key, value => CloneJsonObject(value));
				case IList<object> array:
					// array
					return array.Select(child => CloneJsonObject(child)).ToList();
				default:
					// string or value type
					return src;
			}
		}

		public static bool IsRelativeUri(string uriString) {
			// argument checks
			if (string.IsNullOrEmpty(uriString)) {
				throw new ArgumentNullException(nameof(uriString));
			}

			Uri uri;
			return Uri.TryCreate(uriString, UriKind.Relative, out uri);
		}

		public static (string unescapedPath, string fragment) DecomposeRelativeUri(string relativeUriString) {
			// argument checks
			if (relativeUriString == null) {
				throw new ArgumentNullException(nameof(relativeUriString));
			}

			// '#' in the uriString is the separator
			// because '#' in the path or fragment should be escaped to "%23"
			string path;
			string fragment;
			int index = relativeUriString.IndexOf('#');
			if (0 < index) {
				// uriString has a fragment
				path = relativeUriString.Substring(0, index);
				fragment = relativeUriString.Substring(index);	// include '#'
			} else {
				// uriString has no fragment
				path = relativeUriString;
				fragment = string.Empty;
			}

			// Note that fragment contains the separator '#' if it exists.
			return (Uri.UnescapeDataString(path), fragment);
		}

		public static string RebaseRelativeUri(Uri oldBaseUri, string relativeUriString, Uri newBaseUri) {
			// argument checks
			if (oldBaseUri == null) {
				throw new ArgumentNullException(nameof(oldBaseUri));
			}
			if (relativeUriString == null) {
				throw new ArgumentNullException(nameof(relativeUriString));
			}
			if (newBaseUri == null) {
				throw new ArgumentNullException(nameof(newBaseUri));
			}

			// decompose fragment from the relative Uri
			(string unescapedPath, string fragment) = DecomposeRelativeUri(relativeUriString);

			// rebase the relative path
			Uri newUri = newBaseUri.MakeRelativeUri(new Uri(oldBaseUri, unescapedPath));
			string newRelativeUriString = newUri.ToString();

			// compose the fragment
			if (0 < fragment.Length) {
				newRelativeUriString = string.Concat(newRelativeUriString, fragment);
			}

			return newRelativeUriString;
		}

		public static string ChangeRelativeUriExtension(string relativeUriString, string extension) {
			// argument checks
			if (relativeUriString == null) {
				throw new ArgumentNullException(nameof(relativeUriString));
			}
			if (extension == null) {
				throw new ArgumentNullException(nameof(extension));
			}

			// decompose fragment from the relative Uri
			(string unescapedPath, string fragment) = DecomposeRelativeUri(relativeUriString);

			// change the extension of the relative path
			string newRelativeUriString = Path.ChangeExtension(unescapedPath, extension);

			// compose the fragment
			if (0 < fragment.Length) {
				newRelativeUriString = string.Concat(newRelativeUriString, fragment);
			}

			return newRelativeUriString;
		}
	}
}
