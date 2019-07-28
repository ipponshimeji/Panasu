using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PandocUtil.PandocFilter.Test {
	public static class TestUtil {
		#region data

		private static readonly object classLock = new object();

		private static string resourceDirPath = null;

		#endregion


		#region properties

		public static string ResourceDirPath {
			get {
				string value = resourceDirPath;
				if (value == null) {
					lock (classLock) {
						value = resourceDirPath;
						if (value == null) {
							string moduleDir = Path.GetDirectoryName(typeof(TestUtil).Assembly.ManifestModule.FullyQualifiedName);
							value = Path.GetFullPath("../../../_Resources", moduleDir);
							resourceDirPath = value;
						}
					}
				}

				Debug.Assert(value != null);
				return value;
			}
		}

		#endregion


		#region methods

		public static string GetResourceDir(string subDirName) {
			return Path.Combine(ResourceDirPath, subDirName);
		}

		#endregion
	}
}
