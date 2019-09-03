using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace PandocUtil.PandocFilter {
	public static class Schema {
		#region types

		public static class Names {
			public const string Blocks = "blocks";
			public const string C = "c";
			public const string Meta = "meta";
			public const string PandocApiVersion = "pandoc-api-version";
			public const string T = "t";
			public const string Title = "title";
		}

		public static class TypeNames {
			public const string Header = "Header";
			public const string Image = "Image";
			public const string MetaBlocks = "MetaBlocks";
			public const string MetaInlines = "MetaInlines";
			public const string MetaList = "MetaList";
			public const string MetaMap = "MetaMap";
			public const string Link = "Link";
			public const string RawBlock = "RawBlock";
			public const string RawInline = "RawInline";
			public const string Space = "Space";
			public const string Str = "Str";
		}

		public static class ExtendedNames {
			public const string Macro = "_macro";
		}

		#endregion


		#region methods

		public static FormatException CreateInvalidContentsFormatException(string typeName) {
			return new FormatException($"Invalid contents format as '{typeName}' element.");
		}

		public static string GetStringValue(object contents, string formatName = null) {
			switch (contents) {
				case string str:
					return str;
				case IReadOnlyList<object> array:
					StringBuilder buf = new StringBuilder();
					foreach (object value in array) {
						(string t, object c) = IsElement(value);
						switch (t) {
							case TypeNames.Space:
								buf.Append(' ');
								break;
							case TypeNames.Str:
								buf.Append(c.ToString());
								break;
							case TypeNames.RawInline:
								if (formatName != null) {
									IReadOnlyList<object> l = c as IReadOnlyList<object>;
									if (l != null && 2 <= l.Count && formatName.Equals(l[0])) {
										buf.Append(l[1].ToString());
									}

								}
								break;
						}
					}
					return buf.ToString();
				default:
					return null;
			}
		}

		public static (string type, object contents) IsElement(IReadOnlyDictionary<string, object> obj) {
			if (obj != null) {
				// check existence of 't' and 'c' key
				string type = obj.GetOptionalValue<string>(Schema.Names.T, null);
				object contents = obj.GetOptionalValue<object>(Schema.Names.C, null);
				if (!string.IsNullOrEmpty(type)) {
					// value is an element
					// Note that content may be null.
					return (type, contents);
				}
			}

			return (null, null);    // not an element
		}

		public static (string type, object contents) IsElement(IDictionary<string, object> obj) {
			return IsElement((IReadOnlyDictionary<string, object>)new ReadOnlyDictionary<string, object>(obj));
		}

		public static (string type, object contents) IsElement(object value) {
			IReadOnlyDictionary<string, object> obj = value as IReadOnlyDictionary<string, object>;
			return (obj == null) ? (null, null) : IsElement(obj);
		}

		public static (string macroName, object contents) IsValueMacro(object value) {
			// argument checks
			// value can be null

			(string type, object contents) = IsElement(value);
			if (type == TypeNames.MetaMap) {
				string macroName = IsContentsMacro(contents);
				if (macroName != null) {
					return (macroName, contents);
				}
			}

			return (null, null);
		}

		public static string IsContentsMacro(object contents) {
			// argument checks
			// contents can be null

			// Is 'contents' a JSON object? 
			IReadOnlyDictionary<string, object> obj = contents as IReadOnlyDictionary<string, object>;
			if (obj != null) {
				// Does 'contents' have '_macro' key?
				object macroValue;
				if (obj.TryGetValue(ExtendedNames.Macro, out macroValue)) {
					// Then, obj is a macro.
					return GetMetadataStringValue(macroValue);
				}
			}

			return null;	// not a macro
		}

		#endregion


		#region methods - metadata

		public static string GetMetadataStringValue(object value, string formatName = null) {
			// argument checks
			if (value == null) {
				throw new ArgumentNullException(nameof(value));
			}
			// locationProvider can be null
			// formatName can be null

			// extract string value from its contents
			switch (value) {
				case IReadOnlyDictionary<string, object> obj:
					return GetMetadataStringValue(obj, formatName);
				case IReadOnlyList<object> array:
					return GetMetadataStringValue(array, formatName);
				default:
					return (value == null)? null: value.ToString();
			}
		}

		public static string GetMetadataStringValue(IReadOnlyDictionary<string, object> obj, string formatName = null) {
			// argument checks
			if (obj == null) {
				throw new ArgumentNullException(nameof(obj));
			}
			// formatName can be null

			// extract string value from its contents
			(string type, object contents) = Schema.IsElement(obj);
			switch (type) {
				case Schema.TypeNames.MetaInlines:
				case Schema.TypeNames.MetaBlocks:
					IReadOnlyList<object> arrayContents = contents as IReadOnlyList<object>;
					if (arrayContents == null) {
						throw new FormatException($"The '{Schema.Names.C}' value of a '{type}' element must be an array.");
					}
					return GetMetadataStringValue(arrayContents, formatName);
				default:
					throw new ArgumentException($"It must be a '{Schema.TypeNames.MetaInlines}' or '{Schema.TypeNames.MetaBlocks}' element", nameof(obj));
			}
		}

		public static string GetMetadataStringValue(IReadOnlyList<object> array, string formatName = null) {
			// argument checks
			if (array == null) {
				throw new ArgumentNullException(nameof(array));
			}
			// formatName can be null

			// connect each string value
			StringBuilder buf = new StringBuilder();
			foreach (object item in array) {
				(string type, object contents) = Schema.IsElement(item);
				string str;
				switch (type) {
					case Schema.TypeNames.Space:
						str = " ";
						break;
					case Schema.TypeNames.Str:
						str = contents.ToString();
						break;
					case Schema.TypeNames.RawInline:
						str = GetMetadataRawStringValue(false, contents, formatName);
						break;
					case Schema.TypeNames.RawBlock:
						str = GetMetadataRawStringValue(true, contents, formatName);
						break;
					default: // include null
						str = string.Empty;
						break;
				}
				buf.Append(str);
			}

			return buf.ToString();
		}

		public static string GetMetadataRawStringValue(bool block, object contents, string formatName = null) {
			// argument checks
			if (contents == null) {
				throw new ArgumentNullException(nameof(contents));
			}
			if (formatName == null) {
				return string.Empty;
			}

			// extract the value
			string value;
			IReadOnlyList<object> array = contents as IReadOnlyList<object>;
			if (array == null || array.Count < 2) {
				throw CreateInvalidContentsFormatException(block? Schema.TypeNames.RawBlock: Schema.TypeNames.RawInline);
			} else if (!formatName.Equals(array[0])) {
				value = string.Empty;
			} else {
				value = array[1].ToString();
			}

			return value;
		}

		public static Dictionary<string, object> CreateSimpleMetaInlinesElement(string str) {
			// return a MetaInlines element of single Str content.
			return new Dictionary<string, object>() {
				{ Schema.Names.T, Schema.TypeNames.MetaInlines },
				{ Schema.Names.C, new object[] {
					new Dictionary<string, object>() {
						{ Schema.Names.T, Schema.TypeNames.Str },
						{ Schema.Names.C, str }
					}
				} }
			};
		}

		#endregion
	}
}
