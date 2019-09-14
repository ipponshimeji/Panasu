using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PandocUtil.PandocFilter.Filters {
	public class Parameters {
		#region data

		// The prefix of the parameter names in metadata of AST.
		public static readonly string ParamPrefix = $"{Schema.ExtendedNames.Param}.";


		// parameters read from the metadata in the AST
		private readonly Dictionary<string, object> metadataParameters;

		private bool freezed = false;

		#endregion


		#region properties

		public IReadOnlyDictionary<string, object> MetadataParameters {
			get {
				return this.metadataParameters;
			}
		}

		#endregion


		#region creation

		public Parameters(Dictionary<string, object> dictionary, bool ast) {
			// argument checks
			if (dictionary == null) {
				throw new ArgumentNullException(nameof(dictionary));
			}
			if (ast) {
				dictionary = GetParametersFromAST(dictionary);
			}

			// initialize members
			this.metadataParameters = dictionary;
		}


		private static Dictionary<string, object> GetParametersFromAST(IReadOnlyDictionary<string, object> ast) {
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

		public T GetMetadataParameter<T>(string name) {
			// The entry for the name should exist.
			// A KeyNotFoundException will be thrown if the parameter does not exist.
			// An InvalidCastException will be thrown if the value is not type T. 
			return (T)this.metadataParameters[name];
		}

		#endregion


		#region methods - setup

		public static InvalidOperationException CreateMissingParameterException(string name) {
			// argument checks
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}

			return new InvalidOperationException($"An indispensable parameter '{name}' is missing.");
		}

		public static InvalidOperationException CreateInvalidParameterException(string name, string reason) {
			// argument checks
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}
			if (reason == null) {
				reason = "(unknown)";
			}

			return new InvalidOperationException($"The parameter '{name}' is invalid: {reason}");
		}

		public static InvalidOperationException CreateNotSetupException(string name) {
			// argument checks
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}

			return new InvalidOperationException($"The parameter '{name}' is not set up.");
		}

		public void Freeze() {
			this.freezed = true;
		}

		public void EnsureNotFreezed() {
			if (this.freezed) {
				throw new InvalidOperationException("It cannot be changed because it is freezed.");
			}
		}

		public T GetOptionalMetadataParameter<T>(string name, T overwriteValue, bool overwrite, T defaultValue) {
			// argument checks
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}
			if (overwrite) {
				return overwriteValue;
			}

			return this.MetadataParameters.GetOptionalValue(name, defaultValue);
		}

		public T GetIndispensableMetadataParameter<T>(string name, T overwriteValue, bool overwrite) {
			// argument checks
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}
			if (overwrite) {
				return overwriteValue;
			}

			T value;
			if (this.MetadataParameters.TryGetValue(name, out value)) {
				return value;
			} else {
				throw CreateInvalidParameterException(name, "missing or invalid type");
			}
		}

		public T GetOptionalReferenceTypeMetadataParameter<T>(string name, T overwriteValue, T defaultValue) where T: class {
			return GetOptionalMetadataParameter(name, overwriteValue, overwriteValue != null, defaultValue);
		}

		public T GetIndispensableReferenceTypeMetadataParameter<T>(string name, T overwriteValue) where T : class {
			return GetIndispensableMetadataParameter(name, overwriteValue, overwriteValue != null);
		}

		public T GetOptionalValueTypeMetadataParameter<T>(string name, T? overwriteValue, T defaultValue) where T : struct {
			return GetOptionalMetadataParameter(name, overwriteValue ?? default(T), overwriteValue.HasValue, defaultValue);
		}

		public T GetIndispensableValueTypeMetadataParameter<T>(string name, T? overwriteValue) where T : struct {
			return GetIndispensableMetadataParameter(name, overwriteValue ?? default(T), overwriteValue.HasValue);
		}

		public void SetMetadataParameter(string name, object value) {
			this.metadataParameters[name] = value;
		}

		#endregion
	}
}
