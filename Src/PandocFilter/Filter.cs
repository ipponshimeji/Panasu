using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PandocUtil.PandocFilter {
	public class Filter {
		public virtual void Handle(Dictionary<string, object> doc) {
			// argument checks
			if (doc == null) {
				throw new ArgumentNullException(nameof(doc));
			}

			// handle the 'blocks' value
			object value;
			if (doc.TryGetValue(Schema.Names.Blocks, out value)) {
				HandleValue(value);
			}
		}

		protected virtual void HandleValue(object value) {
			// value may be null

			switch (value) {
				// Check IDictionary<string, object> prior to IList<object> to detect object,
				// because IDictionary<string, object> implements IList<object>.
				case IDictionary<string, object> obj:
					// value is an object
					(bool _, string type) = GetOptionalValue<string>(obj, Schema.Names.T);
					(bool _, object content) = GetOptionalValue<object>(obj, Schema.Names.C);
					if (!string.IsNullOrEmpty(type)) {
						// value is an element
						// Note that content may be null.
						HandleElement(obj, type, content);
					}
					break;
				case IList<object> array:
					// value is an array
					foreach (object item in array) {
						HandleValue(item);
					}
					break;
			}

			return;
		}

		protected virtual void HandleElement(IDictionary<string, object> element, string type, object contents) {
			if (element == null) {
				throw new ArgumentNullException(nameof(element));
			}
			if (string.IsNullOrEmpty(type)) {
				throw new ArgumentNullException(nameof(type));
			}

			if (contents != null) {
				HandleValue(contents);
			}
		}


		public static (bool, T) GetOptionalValue<T>(IDictionary<string, object> dic, string key) where T : class {
			// argument checks
			Debug.Assert(dic != null);
			Debug.Assert(key != null);

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
