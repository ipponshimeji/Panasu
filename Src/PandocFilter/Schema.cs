using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace PandocUtil.PandocFilter {
	public static class Schema {
		#region types

		public class Names {
			public const string Blocks = "blocks";
			public const string C = "c";
			public const string Meta = "meta";
			public const string PandocApiVersion = "pandoc-api-version";
			public const string T = "t";
			public const string Title = "title";
		}

		public class TypeNames {
			public const string Header = "Header";
			public const string Image = "Image";
			public const string MetaBlocks = "MetaBlocks";
			public const string MetaInlines = "MetaInlines";
			public const string MetaList = "MetaList";
			public const string MetaMap = "MetaMap";
			public const string Link = "Link";
			public const string RawInline = "RawInline";
			public const string Space = "Space";
			public const string Str = "Str";
		}

		public class ExtendedNames {
			public const string Macro = "_macro";
		}

		#endregion


		#region methods

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

		public static (string name, IReadOnlyDictionary<string, object> obj) IsMacro(object contents) {
			// Is 'contents' a JSON object? 
			IReadOnlyDictionary<string, object> obj = contents as IReadOnlyDictionary<string, object>;
			if (obj != null) {
				// Does 'contents' have '_macro' key?
				IReadOnlyDictionary<string, object> macro;
				if (obj.TryGetValue(ExtendedNames.Macro, out macro)) {
					(string type, object c) = IsElement(macro);
					switch (type) {
						case TypeNames.MetaInlines:
						case TypeNames.MetaBlocks:
							string name = GetStringValue(c, ExtendedNames.Macro);
							if (string.IsNullOrEmpty(name) == false) {
								return (name, obj);
							}
							break;
					}
				}
			}

			return (null, null);
		}

		#endregion
	}
}
