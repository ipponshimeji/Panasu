using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;


namespace PandocUtil.PandocFilter {
	public abstract class Configurations: ConfigurationsBase {
		#region data

		private readonly Parameters parameters;

		#endregion


		#region properties

		public Parameters Parameters {
			get {
				return this.parameters;
			}
		}

		#endregion


		#region creation

		protected Configurations(Func<IReadOnlyDictionary<string, object>, Parameters> createParams) {
			// argument checks
			if (createParams == null) {
				throw new ArgumentNullException(nameof(createParams));
			}

			// initialize members
			this.parameters = createParams(ImmutableDictionary<string, object>.Empty);
		}

		public Configurations(Func<IReadOnlyDictionary<string, object>, Parameters> createParams, IReadOnlyDictionary<string, object> jsonObj) {
			// argument checks
			if (createParams == null) {
				throw new ArgumentNullException(nameof(createParams));
			}
			if (jsonObj == null) {
				throw new ArgumentNullException(nameof(jsonObj));				
			}

			// initialize members
			this.parameters = createParams(GetParametersObj(jsonObj));
		}

		private static IReadOnlyDictionary<string, object> GetParametersObj(IReadOnlyDictionary<string, object> jsonObj) {
			// argument checks
			Debug.Assert(jsonObj != null);

			IReadOnlyDictionary<string, object> paramsJsonObj = jsonObj.GetOptionalValue<IReadOnlyDictionary<string, object>>("Parameters", null);
			return paramsJsonObj ?? ImmutableDictionary<string, object>.Empty;
		}

		protected Configurations(Configurations src) {
			// argument checks
			if (src == null) {
				throw new ArgumentNullException(nameof(src));
			}

			// initialize members
			this.parameters = src.parameters.Clone<Parameters>();
		}

		#endregion


		#region methods

		protected TParameters GetParameters<TParameters>() where TParameters: Parameters {
			return (TParameters)this.parameters;
		}

		#endregion


		#region overrides

		protected override void OnFreezed() {
			base.OnFreezed();
			this.Parameters.Freeze();
		}

		public override void CompleteContents() {
			base.CompleteContents();
			// Note that this.Parameters should not be completed at this point.
		}

		#endregion
	}
}
