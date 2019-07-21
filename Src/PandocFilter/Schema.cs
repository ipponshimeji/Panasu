using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PandocUtil.PandocFilter {
	public class Schema {
		public class Names {
			public const string C = "c";
			public const string Blocks = "blocks";
			public const string T = "t";
		}

		public class TypeNames {
			public const string Header = "Header";
			public const string Image = "Image";
			public const string Link = "Link";
		}

		public static (bool, T) GetOptionalValue<T>(IDictionary<string, object> dic, string key) where T : class {
			// argument checks
			if (dic == null) {
				throw new ArgumentNullException(nameof(dic));
			}
			if (key == null) {
				throw new ArgumentNullException(nameof(key));
			}

			bool exist = false;
			T value = null;
			object originalValue = null;
			if (dic.TryGetValue(key, out originalValue)) {
				if (originalValue == null) {
					exist = true;
				} else {
					value = originalValue as T;
					exist = (value != null);
				}
			}

			return (exist, value);
		}
	}
}
