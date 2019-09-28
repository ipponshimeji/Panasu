using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Panasu {
	public class Parameters: ConfigurationsBase {
		#region constants

		public const string ParameterTerm = "parameter";

		#endregion


		#region data

		// The prefix of the parameter names in metadata in AST.
		public static readonly string ParamPrefix = $"{Schema.ExtendedNames.Param}.";

		#endregion


		#region creation

		protected Parameters() {
		}

		public Parameters(IReadOnlyDictionary<string, object> jsonObj, Parameters overwriteParams) {
			// argument checks
			if (jsonObj == null) {
				throw new ArgumentNullException(nameof(jsonObj));
			}
			if (overwriteParams == null) {
				throw new ArgumentNullException(nameof(overwriteParams));
			}

			// nothing to do, only checks arguments
		}

		// copy the contents of src except the freeze state.
		protected Parameters(Parameters src) {
			// argument checks
			if (src == null) {
				throw new ArgumentNullException(nameof(src));
			}

			// nothing to do, only checks arguments
		}


		public override ConfigurationsBase Clone() {
			return new Parameters(this);
		}

		#endregion


		#region methods

		public static new InvalidOperationException CreateMissingConfigurationException(string name, string term = ParameterTerm) {
			return ConfigurationsBase.CreateMissingConfigurationException(name, term);
		}

		public static new InvalidOperationException CreateInvalidConfigurationException(string name, string reason, string term = ParameterTerm) {
			return ConfigurationsBase.CreateInvalidConfigurationException(name, reason, term);
		}

		#endregion
	}
}
