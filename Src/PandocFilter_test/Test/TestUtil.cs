using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Utf8Json;
using PandocUtil.PandocFilter.Filters;

namespace PandocUtil.PandocFilter.Test {
	public static class TestUtil {
		#region data

		public static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();


		private static readonly object classLock = new object();

		private static string resourceDirPath = null;

		private static string filteringResourceDirPath = null;

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

		public static string FilteringResourceDirPath {
			get {
				string value = filteringResourceDirPath;
				if (value == null) {
					lock (classLock) {
						value = filteringResourceDirPath;
						if (value == null) {
							value = GetResourceDir("Filtering");
							filteringResourceDirPath = value;
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

		public static string GetFilteringResourceDir(string subDirName) {
			return Path.Combine(FilteringResourceDirPath, subDirName);
		}

		public static void TestFiltering(Action<Stream, Stream> targetFilter, FilteringSample sample) {
			// argument checks
			if (targetFilter == null) {
				throw new ArgumentNullException(nameof(targetFilter));
			}

			// Act
			string actual;
			using (MemoryStream outputStream = new MemoryStream()) {
				using (Stream inputStream = sample.OpenInput()) {
					targetFilter(inputStream, outputStream);
				}
				actual = Encoding.UTF8.GetString(outputStream.ToArray());
			}

			// Assert
			sample.AssertEqual(actual);
		}

		public static void TestFiltering(Filter targetFilter, bool generate, FilteringSample sample) {
			// argument checks
			if (targetFilter ==  null) {
				throw new ArgumentNullException(nameof(targetFilter));
			}

			void filter(Stream inputStream, Stream outputStream) {
				// read input JSON
				Dictionary<string, object> ast;
				ast = JsonSerializer.Deserialize<Dictionary<string, object>>(inputStream);

				// filter
				IDictionary<string, object> filtered;
				if (generate) {
					filtered = targetFilter.Generate(ast);
				} else {
					targetFilter.Modify(ast);
					filtered = ast;
				}

				// write output JSON
				JsonSerializer.Serialize(outputStream, filtered);
			}
			TestFiltering(filter, sample);
		}

		#endregion
	}
}
