using System;
using System.Collections.Generic;
using System.Diagnostics;
using PandocUtil.PandocFilter.Filters;

namespace PandocUtil.PandocFilter.Test {
	public class ConvertingSample: FilteringSample {
		#region properties

		public new ConvertingFilter.Configuration Config {
			get {
				return GetConfig<ConvertingFilter.Configuration>();
			}
		}

		#endregion


		#region creation

		public ConvertingSample(string description, string inputFilePath, string answerFilePath, ConvertingFilter.Configuration config): base(description, inputFilePath, answerFilePath, config) {
		}

		protected ConvertingSample(IReadOnlyDictionary<string, object> jsonObj, string basePath, Func<IReadOnlyDictionary<string, object>, ConvertingFilter.Configuration> configCreator): base(jsonObj, basePath, configCreator) {
		}

		#endregion
	}
}
