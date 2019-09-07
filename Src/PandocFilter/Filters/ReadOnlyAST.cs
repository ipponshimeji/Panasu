using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PandocUtil.PandocFilter.Filters {
	public class ReadOnlyAST {
		#region data

		protected readonly Dictionary<string, object> jsonValue;

		private readonly Version pandocAPIVersion;

		private readonly Dictionary<string, object> parameters;

		#endregion


		#region properties

		public IReadOnlyDictionary<string, object> JsonValue {
			get {
				return this.jsonValue;
			}
		}

		public Version PandocAPIVersion {
			get {
				return this.pandocAPIVersion;
			}
		}

		public IReadOnlyDictionary<string, object> Parameters {
			get {
				return this.parameters;
			}
		}

		#endregion


		#region creation

		public ReadOnlyAST(Dictionary<string, object> jsonValue) {
			// argument checks
			if (jsonValue == null) {
				throw new ArgumentNullException(nameof(jsonValue));
			}

			// initialize members
			this.jsonValue = jsonValue;
			this.pandocAPIVersion = ReadPandocAPIVersion(jsonValue);
			this.parameters = GetParameters(jsonValue);
		}

		private Dictionary<string, object> GetParameters(IReadOnlyDictionary<string, object> ast) {
			// argument checks
			Debug.Assert(jsonValue != null);

			// get metadata
			IReadOnlyDictionary<string, object> metadata = ast.GetOptionalValue<IReadOnlyDictionary<string, object>>(Schema.Names.Meta, null);
			if (metadata == null) {
				return new Dictionary<string, object>();
			}

			// collect parameters
			// A parameter is a metadata whose name starts with "_Param.".
			string prefix = $"{Schema.ExtendedNames.Param}.";
			int prefixLen = prefix.Length;
			return metadata
			.Where(pair => pair.Key.StartsWith(prefix))
			.ToDictionary(pair => pair.Key.Substring(prefix.Length), pair => Schema.RestoreMetadata(pair.Value));
		}

		#endregion


		#region methods

		public IReadOnlyDictionary<string, object> GetMetadata() {
			IDictionary<string, object> ast = this.jsonValue;
			Debug.Assert(ast != null);

			return ast.GetOptionalValue<IReadOnlyDictionary<string, object>>(Schema.Names.Meta, null);
		}

		#endregion


		#region overridables

		protected virtual Version ReadPandocAPIVersion(IReadOnlyDictionary<string, object> ast) {
			// argument checks
			if (ast == null) {
				throw new ArgumentNullException(nameof(ast));
			}

			// read the 'pandoc-api-version' key
			Version version = new Version();    // version 0.0, by default
			IReadOnlyList<object> apiVersion;
			if (ast.TryGetValue<IReadOnlyList<object>>(Schema.Names.PandocApiVersion, out apiVersion)) {
				try {
					int getNumber(int index) {
						// An InvalidCastException will be thrown if list[index] is not an integer.
						return (int)apiVersion[index];
					}

					// The following code is redundant,
					// but only 'case 4:' is run in the most case.
					int major = 0, minor = 0, buildNo = -1, revision = -1;
					switch (apiVersion.Count) {
						case 1:
							major = getNumber(0);
							break;
						case 2:
							major = getNumber(0);
							minor = getNumber(1);
							break;
						case 3:
							major = getNumber(0);
							minor = getNumber(1);
							buildNo = getNumber(2);
							break;
						case 4:
							major = getNumber(0);
							minor = getNumber(1);
							buildNo = getNumber(2);
							revision = getNumber(3);
							break;
					}
					version = new Version(major, minor, buildNo, revision);
				} catch (InvalidCastException) {
					;   // continue with the default version (0.0)
				}
			}

			return version;
		}

		#endregion
	}
}
