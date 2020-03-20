using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace Panasu {
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
			#region constants

			public const string Header = "Header";
			public const string Image = "Image";
			public const string MetaBlocks = "MetaBlocks";
			public const string MetaBool = "MetaBool";
			public const string MetaInlines = "MetaInlines";
			public const string MetaList = "MetaList";
			public const string MetaMap = "MetaMap";
			public const string Link = "Link";
			public const string Para = "Para";
			public const string RawBlock = "RawBlock";
			public const string RawInline = "RawInline";
			public const string Space = "Space";
			public const string Str = "Str";

			#endregion


			#region properties

			public static StringComparer Comparer {
				get {
					return StringComparer.Ordinal;
				}
			}

			#endregion


			#region methods

			public static bool Equals(string? name1, string? name2) {
				return Comparer.Equals(name1, name2);
			}

			#endregion
		}

		public static class ExtendedNames {
			public const string Macro = "_macro";
			public const string Param = "_Param";
		}

		public static class JSONTypeNames {
			public const string Array = "array";
			public const string Boolean = "boolean";
			public const string Number = "number";
			public const string Null = "null";
			public const string Object = "object";
			public const string String = "string";

			public const string Unknown = "(unknown)";
		}

		#endregion


		#region methods

		public static FormatException CreateInvalidContentsFormatException(string typeName) {
			if (typeName == null) {
				throw new ArgumentNullException(nameof(typeName));
			}

			return new FormatException($"Invalid contents format as a '{typeName}' element.");
		}

		public static FormatException CreateInvalidContentsFormatException(string typeName, string expectedContentsType, object actualContents) {
			return new FormatException($"Invalid AST format: The contents in a '{typeName}' element is not '{expectedContentsType}' but '{GetJSONTypeName(actualContents)}'.");
		}

		public static string GetJSONTypeName(object? value) {
			switch (value) {
				case bool boolValue:
					return JSONTypeNames.Boolean;
				case string strValue:
					return JSONTypeNames.String;
				case double numValue:
					return JSONTypeNames.Number;
				case IDictionary<string, object> objValue:
				case IReadOnlyDictionary<string, object> roObjValue:
					return JSONTypeNames.Object;
				case IList<object> arrayValue:
				case IReadOnlyList<object> roArrayValue:
					return JSONTypeNames.Array;
				default:
					return (value == null) ? JSONTypeNames.Null : JSONTypeNames.Unknown;
			}
		}

		public static (string? type, object? contents) IsElement(IReadOnlyDictionary<string, object?>? obj) {
			if (obj != null) {
				// check existence of 't' and 'c' key
				string? type = obj.GetOptionalNullableValue<string>(Schema.Names.T, null);
				object? contents = obj.GetOptionalNullableValue<object>(Schema.Names.C, null);
				if (!string.IsNullOrEmpty(type)) {
					// value is an element
					// Note that content may be null.
					return (type, contents);
				}
			}

			return (null, null);    // not an element
		}

		public static (string? type, object? contents) IsElement(IDictionary<string, object?>? obj) {
			if (obj != null) {
				// check existence of 't' and 'c' key
				string? type = obj.GetOptionalNullableValue<string>(Schema.Names.T, null);
				object? contents = obj.GetOptionalNullableValue<object>(Schema.Names.C, null);
				if (!string.IsNullOrEmpty(type)) {
					// value is an element
					// Note that content may be null.
					return (type, contents);
				}
			}

			return (null, null);    // not an element
		}

		public static (string? type, object? contents) IsElement(Dictionary<string, object?>? obj) {
			return IsElement((IReadOnlyDictionary<string, object?>?)obj);
		}

		public static (string? type, object? contents) IsElement(object? value) {
			switch (value) {
				case IDictionary<string, object?> dic:
					return IsElement(dic);
				case IReadOnlyDictionary<string, object?> roDic:
					return IsElement(roDic);
				default:
					return (null, null);
			}
		}

		public static (string? macroName, object? contents) IsValueMacro(object? value) {
			// argument checks
			// value can be null

			(string? type, object? contents) = IsElement(value);
			if (TypeNames.Equals(type, TypeNames.MetaMap)) {
				// value is a 'MetaMap' element
				string macroName = IsContentsMacro(contents);
				if (macroName != null) {
					return (macroName, contents);
				}
			}

			return (null, null);	// not a macro
		}

		public static string? IsContentsMacro(object? contents) {
			// argument checks
			// contents can be null

			// Is 'contents' a JSON object? 
			IReadOnlyDictionary<string, object?>? obj = contents as IReadOnlyDictionary<string, object?>;
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

		public static FormatException CreateInvalidMetadataFormatException(string type, string contentsType) {
			return new FormatException($"Invalid metadata format: The '{Names.C}' in 'type' ");			
		}


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
						throw CreateInvalidContentsFormatException(type, Schema.JSONTypeNames.Array, contents);
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
					case Schema.TypeNames.Para:
						IReadOnlyList<object> arrayContents = contents as IReadOnlyList<object>;
						if (arrayContents == null) {
							throw CreateInvalidContentsFormatException(Schema.TypeNames.Para, Schema.JSONTypeNames.Array, contents);
						}
						str = GetMetadataStringValue(arrayContents, formatName) + Environment.NewLine;
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

		public static object RestoreMetadata(object value, string formatName = null) {
			(string type, object contents) = IsElement(value);
			switch (type) {
				case TypeNames.MetaInlines:
				case TypeNames.MetaBlocks:
					IReadOnlyList<object> array = contents as IReadOnlyList<object>;
					if (array == null) {
						throw CreateInvalidContentsFormatException(type, JSONTypeNames.Array, contents);
					}
					return GetMetadataStringValue(array, formatName);
				case TypeNames.MetaBool:
					if (!(contents is bool)) {
						throw CreateInvalidContentsFormatException(type, JSONTypeNames.Boolean, contents);
					}
					return (bool)contents;
				case TypeNames.MetaList:
					IReadOnlyList<object> fromArray = contents as IReadOnlyList<object>;
					if (fromArray == null) {
						throw CreateInvalidContentsFormatException(type, JSONTypeNames.Array, contents);
					}
					List<object> toArray = new List<object>();
					foreach (object item in fromArray) {
						toArray.Add(RestoreMetadata(item, formatName));
					}
					return toArray;
				case TypeNames.MetaMap:
					IReadOnlyDictionary<string, object> fromObj = contents as IReadOnlyDictionary<string, object>;
					if (fromObj == null) {
						throw CreateInvalidContentsFormatException(type, JSONTypeNames.Object, contents);
					}
					Dictionary<string, object> toObject = new Dictionary<string, object>();
					foreach (KeyValuePair<string, object> pair in fromObj) {
						toObject.Add(pair.Key, RestoreMetadata(pair.Value, formatName));
					}
					return toObject;
				default:
					throw new FormatException();	// TODO: message
			}
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
