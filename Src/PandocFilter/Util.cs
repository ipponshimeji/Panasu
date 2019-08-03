using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PandocUtil.PandocFilter {
	public static class Util {
		#region methods

		public static void ClearDisposable<T>(ref T target) where T: class, IDisposable {
			if (target != null) {
				IDisposable temp = target;
				target = null;
				temp.Dispose();
			}
		}

		private static bool CheckValue<T>(bool keyExist, object originalValue, out T value) {
			if (keyExist) {
				if (originalValue == null) {
					// the key exists, but its value is null
					if (typeof(T).IsValueType == false) {
						value = default(T); // actually it is null
						return true;
					}
				} else if (originalValue is T) {
					value = (T)originalValue;
					return true;
				}
			}

			value = default(T);
			return false;
		}

		public static bool TryGetValue<T>(this IReadOnlyDictionary<string, object> dictionary, string key, out T value) {
			// argument checks
			if (dictionary == null) {
				throw new ArgumentNullException(nameof(dictionary));
			}
			if (key == null) {
				throw new ArgumentNullException(nameof(key));
			}
			// key can be empty

			// try to get value
			object originalValue;
			return CheckValue<T>(dictionary.TryGetValue(key, out originalValue), originalValue, out value);
		}

		public static bool TryGetValue<T>(this IDictionary<string, object> dictionary, string key, out T value) {
			// argument checks
			if (dictionary == null) {
				throw new ArgumentNullException(nameof(dictionary));
			}
			if (key == null) {
				throw new ArgumentNullException(nameof(key));
			}
			// key can be empty

			// try to get value
			object originalValue;
			return CheckValue<T>(dictionary.TryGetValue(key, out originalValue), originalValue, out value);
		}

		public static (bool, T) GetOptionalValue<T>(this IReadOnlyDictionary<string, object> dictionary, string key) {
			T value;
			bool exist = TryGetValue(dictionary, key, out value);
			return (exist, value);
		}

		public static (bool, T) GetOptionalValue<T>(this IDictionary<string, object> dictionary, string key) {
			T value;
			bool exist = TryGetValue(dictionary, key, out value);
			return (exist, value);
		}

		public static T GetOptionalValue<T>(this IReadOnlyDictionary<string, object> dictionary, string key, T defaultValue) {
			T value;
			return TryGetValue(dictionary, key, out value) ? value : defaultValue;
		}

		public static T GetOptionalValue<T>(this IDictionary<string, object> dictionary, string key, T defaultValue) {
			T value;
			return TryGetValue(dictionary, key, out value) ? value : defaultValue;
		}

		public static T GetIndispensableValue<T>(this IReadOnlyDictionary<string, object> dictionary, string key) {
			T value;
			if (TryGetValue(dictionary, key, out value)) {
				return value;
			} else {
				throw new KeyNotFoundException($"The indispensable key '{key}' was not present in the dictionary.");
			}
		}

		public static T GetIndispensableValue<T>(this IDictionary<string, object> dictionary, string key) {
			T value;
			if (TryGetValue(dictionary, key, out value)) {
				return value;
			} else {
				throw new KeyNotFoundException($"The indispensable key '{key}' was not present in the dictionary.");
			}
		}

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

		#endregion
	}
}
