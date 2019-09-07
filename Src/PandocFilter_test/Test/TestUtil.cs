using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using Utf8Json;
using PandocUtil.PandocFilter.Filters;

namespace PandocUtil.PandocFilter.Test {
	public static class TestUtil {
		#region types

		// used in EqualJson() method.
		public class Missing {
			#region data

			public static readonly Missing Instance = new Missing();

			#endregion


			#region constructors

			private Missing() {
			}

			#endregion


			#region overrides

			public override string ToString() {
				return "(missing)";
			}

			#endregion
		}

		#endregion


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
			Dictionary<string, object> actual;
			using (MemoryStream outputStream = new MemoryStream()) {
				using (Stream inputStream = sample.OpenInput()) {
					targetFilter(inputStream, outputStream);
				}
				outputStream.Position = 0;
				actual = JsonSerializer.Deserialize <Dictionary<string, object>>(outputStream);
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


		#region methods - asserts

		public static void EqualJson(object expected, object actual, Stack<object> path = null) {
			// internal functions
			void pushPath(object value) {
				if (path == null) {
					path = new Stack<object>(4);
				}
				path.Push(value);
			}

			void popPath() {
				Debug.Assert(path != null);
				path.Pop();
			}

			string getTypeText(object value) {
				return (value == null) ? "null" : value.GetType().ToString();
			}

			IEnumerable<object> getPath() {
				// change the path enumeration from LIFO to FIFO.
				return (path == null) ? null : path.Reverse();
			}

			EqualJsonException createTypeFailure(string expectedType) {
				return new EqualJsonException(getPath(), EqualJsonException.TypePoint, expectedType, getTypeText(actual));
			}

			EqualJsonException createValueFailure(object expectedValue, object actualValue) {
				return new EqualJsonException(getPath(), EqualJsonException.ValuePoint, expectedValue, actualValue);
			}

			EqualJsonException createCountFailure(int expectedCount, int actualCount) {
				return new EqualJsonException(getPath(), EqualJsonException.CountPoint, expectedCount, actualCount);
			}

			switch (expected) {
				case IReadOnlyDictionary<string, object> expectedObj:
					// expected is a JSON object

					// check type
					IReadOnlyDictionary<string, object> actualObj = actual as IReadOnlyDictionary<string, object>;
					if (actualObj == null) {
						throw createTypeFailure("(compatible with JSON object)");
					}

					// check contents
					HashSet<string> actualKeys = new HashSet<string>(actualObj.Keys);
					foreach(KeyValuePair<string, object> pair in expectedObj) {
						string expectedKey = pair.Key;
						pushPath(expectedKey);
						try {
							object actualValue;
							if (actualObj.TryGetValue(expectedKey, out actualValue) == false) {
								// an expected item is missing
								throw createValueFailure(pair.Value, Missing.Instance);
							}

							EqualJson(pair.Value, actualValue, path);
							actualKeys.Remove(expectedKey);
						} finally {
							popPath();
						}
					}
					if (0 < actualKeys.Count) {
						// There is an unexpected item in actualObj.
						string key = actualKeys.First();
						pushPath(key);
						throw createValueFailure(Missing.Instance, actualObj[key]);
					}
					break;
				case IReadOnlyList<object> expectedArray:
					// expected is a JSON array

					// check type
					IReadOnlyList<object> actualArray = actual as IReadOnlyList<object>;
					if (actualArray == null) {
						throw createTypeFailure("(compatible with JSON array)");
					}

					// check count
					int expectedCount = expectedArray.Count;
					int actualCount = actualArray.Count;
					if (expectedCount != actualCount) {
						throw createCountFailure(expectedCount, actualCount);
					}

					// check contents
					for (int i = 0; i < expectedCount; ++i) {
						pushPath(i);
						try {
							EqualJson(expectedArray[i], actualArray[i], path);
						} finally {
							popPath();
						}
					}
					break;
				default:
					if (object.Equals(expected, actual) == false) {
						throw createValueFailure(expected, actual);
					}
					break;
			}

			return;
		}

		public static void EqualJson(string expectedJSONString, object actual) {
			EqualJson(JsonSerializer.Deserialize<object>(expectedJSONString), actual);
		}

		#endregion
	}
}
