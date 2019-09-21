using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace PandocUtil.PandocFilter {
	public abstract class ConfigurationsBase: FreezableObject {
		#region constants

		public const string ConfigurationTerm = "configuration";

		#endregion


		#region creation

		protected ConfigurationsBase() {
		}


		public abstract ConfigurationsBase Clone();

		public TConfiguration Clone<TConfiguration>() where TConfiguration: ConfigurationsBase {
			return (TConfiguration)this.Clone();
		}

		#endregion


		#region methods

		public static InvalidOperationException CreateMissingConfigurationException(string name, string term = ConfigurationTerm) {
			// argument checks
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}
			if (term == null) {
				term = ConfigurationTerm;
			}

			return new InvalidOperationException($"The indispensable {term} '{name}' is missing.");
		}

		public static InvalidOperationException CreateInvalidConfigurationException(string name, string reason, string term = ConfigurationTerm) {
			// argument checks
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}
			if (reason == null) {
				reason = "(unknown)";
			}
			if (term == null) {
				term = ConfigurationTerm;
			}

			return new InvalidOperationException($"The {term} '{name}' is invalid: {reason}");
		}

		public T ReadValueFrom<T>(IReadOnlyDictionary<string, object> json, string name, T overwriteValue, bool overwrite, T defaultValue) {
			// argument checks
			if (json == null) {
				throw new ArgumentNullException(nameof(json));
			}
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}
			if (overwrite) {
				return overwriteValue;
			}

			return json.GetOptionalValue(name, defaultValue);
		}

		public T ReadValueFrom<T>(IReadOnlyDictionary<string, object> json, string name, T overwriteValue) where T: class {
			// argument checks
			if (json == null) {
				throw new ArgumentNullException(nameof(json));
			}
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}
			if (overwriteValue != null) {
				return overwriteValue;
			}

			return json.GetOptionalValue<T>(name, null);
		}

		public Nullable<T> ReadValueFrom<T>(IReadOnlyDictionary<string, object> json, string name, Nullable<T> overwriteValue) where T: struct {
			// argument checks
			if (json == null) {
				throw new ArgumentNullException(nameof(json));
			}
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}
			if (overwriteValue.HasValue) {
				return overwriteValue;
			}

			return json.GetOptionalValue<Nullable<T>>(name, null);
		}

		public List<T> ReadArrayFrom<T>(IReadOnlyDictionary<string, object> json, string name, IReadOnlyList<T> overwriteValue) {
			// argument checks
			if (json == null) {
				throw new ArgumentNullException(nameof(json));
			}
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}
			if (overwriteValue != null) {
				return new List<T>(overwriteValue);
			}

			IReadOnlyList<object> originalArray = json.GetOptionalValue<IReadOnlyList<object>>(name, null);
			return (originalArray == null) ? null : originalArray.Select(item => (T)item).ToList();
		}

		public Dictionary<string, T> ReadObjectFrom<T>(IReadOnlyDictionary<string, object> json, string name, IReadOnlyDictionary<string, T> overwriteValue) {
			// argument checks
			if (json == null) {
				throw new ArgumentNullException(nameof(json));
			}
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}
			if (overwriteValue != null) {
				return new Dictionary<string, T>(overwriteValue);
			}

			IReadOnlyDictionary<string, object> originalObj = json.GetOptionalValue<IReadOnlyDictionary<string, object>>(name, null);
			return (originalObj == null) ? null : originalObj.ToDictionary(pair => pair.Key, pair => (T)pair.Value);
		}

		#endregion


		#region overridables

		public virtual void CompleteContents() {
		}

		#endregion
	}
}
