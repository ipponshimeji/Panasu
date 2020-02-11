using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Panasu {
	public static class Util {
		#region methods

		public static void ClearDisposable<T>(ref T? target) where T: class, IDisposable {
			if (target != null) {
				IDisposable temp = target;
				target = null;
				temp.Dispose();
			}
		}


		private static bool CheckValue<T>(bool valueExists, object? originalValue, [MaybeNullWhen(false)] out T value) where T: notnull {
			if (valueExists == false) {
				// the value does not exist
				value = default(T)!;
				return false;
			} else if (originalValue == null) {
				// the value exists, but it is null
				throw new InvalidCastException("Its value is a null.");
			} else {
				// the value exists and it is not null
				// The cast below may throw an InvalidCastException if originalValue is unconformable to T.
				value = (T)originalValue;
				return true;
			}
		}

		private static bool CheckNullableValue<T>(bool valueExists, object? originalValue, out T? value) where T: class {
			if (valueExists == false) {
				// the value does not exist
				value = null;
				return false;
			} else {
				// the value exists
				// The cast below may throw an InvalidCastException if originalValue is unconformable to T.
				value = (T?)originalValue;
				return true;
			}
		}

		public static bool TryGetValue<T>(this IReadOnlyDictionary<string, object?> dictionary, string key, [MaybeNullWhen(false)] out T value) where T: notnull {
			// argument checks
			if (dictionary == null) {
				throw new ArgumentNullException(nameof(dictionary));
			}

			object? originalValue;
			return CheckValue<T>(dictionary.TryGetValue(key, out originalValue), originalValue, out value);
		}

		public static bool TryGetValue<T>(this IDictionary<string, object?> dictionary, string key, [MaybeNullWhen(false)] out T value) where T: notnull {
			// argument checks
			if (dictionary == null) {
				throw new ArgumentNullException(nameof(dictionary));
			}

			object? originalValue;
			return CheckValue<T>(dictionary.TryGetValue(key, out originalValue), originalValue, out value);
		}

		public static bool TryGetValue<T>(this Dictionary<string, object?> dictionary, string key, [MaybeNullWhen(false)] out T value) where T: notnull {
			return TryGetValue<T>((IDictionary<string, object?>)dictionary, key, out value);
		}

		public static bool TryGetNullableValue<T>(this IReadOnlyDictionary<string, object?> dictionary, string key, out T? value) where T: class {
			// argument checks
			if (dictionary == null) {
				throw new ArgumentNullException(nameof(dictionary));
			}

			object? originalValue;
			return CheckNullableValue<T>(dictionary.TryGetValue(key, out originalValue), originalValue, out value);
		}

		public static bool TryGetNullableValue<T>(this IDictionary<string, object?> dictionary, string key, out T? value) where T: class {
			// argument checks
			if (dictionary == null) {
				throw new ArgumentNullException(nameof(dictionary));
			}

			object? originalValue;
			return CheckNullableValue<T>(dictionary.TryGetValue(key, out originalValue), originalValue, out value);
		}

		public static bool TryGetNullableValue<T>(this Dictionary<string, object?> dictionary, string key, out T? value) where T: class {
			return TryGetNullableValue<T>((IDictionary<string, object?>)dictionary, key, out value);
		}

		public static T GetOptionalValue<T>(this IReadOnlyDictionary<string, object?> dictionary, string key, T defaultValue) where T: notnull {
			T value;
			return TryGetValue<T>(dictionary, key, out value)? value: defaultValue;
		}

		public static T GetOptionalValue<T>(this IDictionary<string, object?> dictionary, string key, T defaultValue) where T: notnull {
			T value;
			return TryGetValue<T>(dictionary, key, out value)? value: defaultValue;
		}

		public static T GetOptionalValue<T>(this Dictionary<string, object?> dictionary, string key, T defaultValue) where T: notnull {
			return GetOptionalValue<T>((IDictionary<string, object?>)dictionary, key, defaultValue);
		}

		public static T? GetOptionalNullableValue<T>(this IReadOnlyDictionary<string, object?> dictionary, string key, T? defaultValue) where T: class {
			T? value;
			return TryGetNullableValue<T>(dictionary, key, out value)? value: defaultValue;
		}

		public static T? GetOptionalNullableValue<T>(this IDictionary<string, object?> dictionary, string key, T? defaultValue) where T: class {
			T? value;
			return TryGetNullableValue<T>(dictionary, key, out value) ? value : defaultValue;
		}

		public static T? GetOptionalNullableValue<T>(this Dictionary<string, object?> dictionary, string key, T? defaultValue) where T: class {
			return GetOptionalNullableValue<T>((IDictionary<string, object?>)dictionary, key, defaultValue);
		}

		public static Exception CreateMissingKeyException(string key) {
			return new KeyNotFoundException($"The indispensable key '{key}' is missing in the dictionary.");
		}

		public static T GetIndispensableValue<T>(this IReadOnlyDictionary<string, object?> dictionary, string key) where T: notnull {
			T value;
			if (TryGetValue<T>(dictionary, key, out value)) {
				return value;
			} else {
				throw CreateMissingKeyException(key);
			}
		}

		public static T GetIndispensableValue<T>(this IDictionary<string, object?> dictionary, string key) where T: notnull {
			T value;
			if (TryGetValue<T>(dictionary, key, out value)) {
				return value;
			} else {
				throw CreateMissingKeyException(key);
			}
		}

		public static T GetIndispensableValue<T>(this Dictionary<string, object?> dictionary, string key) where T: notnull {
			return GetIndispensableValue<T>((IDictionary<string, object?>)dictionary, key);
		}

		public static T? GetIndispensableNullableValue<T>(this IReadOnlyDictionary<string, object?> dictionary, string key) where T: class {
			T? value;
			if (TryGetNullableValue<T>(dictionary, key, out value)) {
				return value;
			} else {
				throw CreateMissingKeyException(key);
			}
		}

		public static T? GetIndispensableNullableValue<T>(this IDictionary<string, object?> dictionary, string key) where T: class {
			T? value;
			if (TryGetNullableValue<T>(dictionary, key, out value)) {
				return value;
			} else {
				throw CreateMissingKeyException(key);
			}
		}

		public static T? GetIndispensableNullableValue<T>(this Dictionary<string, object?> dictionary, string key) where T : class {
			return GetIndispensableNullableValue<T>((IDictionary<string, object?>)dictionary, key);
		}


		public static bool IsRelativeUri(string uriString) {
			// argument checks
			if (string.IsNullOrEmpty(uriString)) {
				throw new ArgumentNullException(nameof(uriString));
			}

			// The Uri.TryCreate() call below returns true for uriString "/a/b/c".
			// So Path.IsPathRooted() is called to remove such rooted path.
			return Uri.TryCreate(uriString, UriKind.Relative, out Uri? uri) && !Path.IsPathRooted(uriString);
		}

		public static (string path, string fragment) DecomposeRelativeUri(string relativeUriString, bool unescapePath = true) {
			// argument checks
			if (relativeUriString == null) {
				throw new ArgumentNullException(nameof(relativeUriString));
			}

			// '#' in the uriString is the separator
			// because '#' in the path or fragment should be escaped to "%23"
			string path;
			string fragment;
			int index = relativeUriString.IndexOf('#');
			if (0 <= index) {
				// uriString has a fragment
				path = relativeUriString.Substring(0, index);
				fragment = relativeUriString.Substring(index);	// include '#'
			} else {
				// uriString has no fragment
				path = relativeUriString;
				fragment = string.Empty;
			}

			// Note that fragment contains the separator '#' if it exists.
			if (unescapePath) {
				path = Uri.UnescapeDataString(path);
			}
			return (path, fragment);
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
			(string path, string fragment) = DecomposeRelativeUri(relativeUriString, false);

			// rebase the relative path
			Uri newUri = newBaseUri.MakeRelativeUri(new Uri(oldBaseUri, path));
			string newRelativeUriString = newUri.ToString();

			// compose the fragment
			if (0 < fragment.Length) {
				newRelativeUriString = string.Concat(newRelativeUriString, fragment);
			}

			return newRelativeUriString;
		}

		#endregion
	}
}
