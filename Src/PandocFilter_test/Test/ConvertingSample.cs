using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PandocUtil.PandocFilter.Filters;

namespace PandocUtil.PandocFilter.Test {
	public class ConvertingSample: FilteringSample {
		#region properties

		public new ConvertingFilter.Configurations Config {
			get {
				return GetConfig<ConvertingFilter.Configurations>();
			}
		}

		#endregion


		#region creation

		public ConvertingSample(string description, string inputFilePath, string answerFilePath, ConvertingFilter.Configurations config): base(description, inputFilePath, answerFilePath, config) {
		}

		protected ConvertingSample(IReadOnlyDictionary<string, object> jsonObj, string basePath, Func<IReadOnlyDictionary<string, object>, ConvertingFilter.Configurations> createConfigurations): base(jsonObj, basePath, createConfigurations) {
			// modify configurations
			string tempDirPath = Path.GetTempPath();
			string getFullPath(string path) {
				return Path.Combine(tempDirPath, path);
			}

			string dirPath = this.Config.Parameters.FromBaseDirPath;
			if (dirPath != null) {
				this.Config.Parameters.FromBaseDirPath = getFullPath(dirPath);
			}
			dirPath = this.Config.Parameters.ToBaseDirPath;
			if (dirPath != null) {
				this.Config.Parameters.ToBaseDirPath = getFullPath(dirPath);
			}
		}

		#endregion
	}
}
