using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Panasu.Filters {
	public class ReadOnlyAST {
		#region data

		// The prefix of the parameter names in metadata of AST.
		public static readonly string ParamPrefix = $"{Schema.ExtendedNames.Param}.";


		protected readonly Dictionary<string, object> jsonValue;

		public readonly Version PandocAPIVersion;

		public readonly IReadOnlyDictionary<string, object> MetadataParameters;

		#endregion


		#region properties

		public IReadOnlyDictionary<string, object> JsonValue {
			get {
				return this.jsonValue;
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
			this.PandocAPIVersion = ReadPandocAPIVersion(jsonValue);
			this.MetadataParameters = ReadMetadataParameters(jsonValue);
		}

		private static Dictionary<string, object> ReadMetadataParameters(IReadOnlyDictionary<string, object> ast) {
			// argument checks
			Debug.Assert(ast != null);

			// get metadata
			IReadOnlyDictionary<string, object> metadata = ast.GetOptionalValue<IReadOnlyDictionary<string, object>>(Schema.Names.Meta, null);
			if (metadata == null) {
				return new Dictionary<string, object>();
			}

			// collect parameters
			// A parameter is a metadata whose name starts with "_Param.".
			int prefixLen = ParamPrefix.Length;
			return metadata
			.Where(pair => pair.Key.StartsWith(ParamPrefix))
			.ToDictionary(pair => pair.Key.Substring(prefixLen), pair => Schema.RestoreMetadata(pair.Value));
		}

		#endregion


		#region methods

		/// <summary>
		/// Get the metadata of the AST.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Note that the metadata may be modified if the AST is not read only.
		/// </remarks>
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
