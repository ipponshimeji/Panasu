using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace Panasu.Test {
	public class UtilTest {
		#region types

		public class DisposableSample: IDisposable {
			#region data

			public int DisposeCount { get; private set; } = 0;

			#endregion


			#region IDisposable

			public void Dispose() {
				++this.DisposeCount;
			}

			#endregion
		}

		#endregion


		#region ClearDisposable

		public class ClearDisposable {
			[Fact(DisplayName = "general")]
			public void General() {
				// Arrange
				DisposableSample sample = new DisposableSample();
				Debug.Assert(sample.DisposeCount == 0);
				DisposableSample? value = sample;

				// Act
				Util.ClearDisposable(ref value);

				// Assert
				Assert.Null(value);
				Assert.Equal(1, sample.DisposeCount);
			}

			[Fact(DisplayName = "target: null")]
			public void target_null() {
				// Arrange
				DisposableSample? value = null;

				// Act
				Util.ClearDisposable(ref value);

				// Assert
				Assert.Null(value);
			}
		}

		#endregion


		#region TryGetValue

		public class TryGetValue {
			protected void TestNormal<T>(Dictionary<string, object?> sample, string key, bool expectedResult, T expectedValue) {
				// Arrange
				Debug.Assert(sample != null);
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;
				Debug.Assert(key != null);

				// Act
				T actualValue1;
				T actualValue2;
				T actualValue3;

				// overload 1: bool TryGetValue<T>(this IReadOnlyDictionary<string, object>, string, out T)
				bool actualResult1 = readOnlyDictionarySample.TryGetValue(key, out actualValue1);
				// overload 2: bool TryGetValue<T>(this IDictionary<string, object>, string, out T)
				bool actualResult2 = dictionarySample.TryGetValue(key, out actualValue2);
				// overload 3: bool TryGetValue<T>(this Dictionary<string, object>, string, out T)
				bool actualResult3 = sample.TryGetValue(key, out actualValue3);

				// Assert
				Assert.Equal(expectedResult, actualResult1);
				Assert.Equal(expectedResult, actualResult2);
				Assert.Equal(expectedResult, actualResult3);
				Assert.Equal(expectedValue, actualValue1);
				Assert.Equal(expectedValue, actualValue2);
				Assert.Equal(expectedValue, actualValue3);
			}

			protected void TestError<T, TException>(Dictionary<string, object?>? sample, string? key, string expectedMessage) where TException: Exception {
				// Arrange
				IReadOnlyDictionary<string, object?>? readOnlyDictionarySample = sample;
				IDictionary<string, object?>? dictionarySample = sample;

				// Act
				T dummy;

				// overload 1: bool TryGetValue<T>(this IReadOnlyDictionary<string, object>, string, out T)
				TException actualException1 = Assert.Throws<TException>(() => { readOnlyDictionarySample.TryGetValue(key, out dummy); });
				// overload 2: bool TryGetValue<T>(this IDictionary<string, object>, string, out T)
				TException actualException2 = Assert.Throws<TException>(() => { dictionarySample.TryGetValue(key, out dummy); });
				// overload 3: bool TryGetValue<T>(this Dictionary<string, object>, string, out T)
				TException actualException3 = Assert.Throws<TException>(() => { sample.TryGetValue(key, out dummy); });

				// Assert
				Assert.Equal(expectedMessage, actualException1.Message);
				Assert.Equal(expectedMessage, actualException2.Message);
				Assert.Equal(expectedMessage, actualException3.Message);
			}


			[Fact(DisplayName = "Reference Type, conformable")]
			public void ReferenceType_Conformable() {
				string key = "key";
				string expectedValue = "abc";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, expectedValue }
				};
				bool expectedResult = true;

				TestNormal(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "Value Type, conformable")]
			public void ValueType_Conformable() {
				string key = "key";
				int expectedValue = 7;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, expectedValue }
				};
				bool expectedResult = true;

				TestNormal(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "Reference Type, unconformable")]
			public void ReferenceType_Unconformable() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, 5 }
				};
				string expectedMessage = "A value of 'System.Int32' type cannot be casted to 'System.String' type.";

				TestError<string, InvalidCastException>(sample, key, expectedMessage);
			}

			[Fact(DisplayName = "Value Type, unconformable")]
			public void ValueType_Unconformable() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, "xyz" }
				};
				string expectedMessage = "A value of 'System.String' type cannot be casted to 'System.Int32' type.";

				TestError<int, InvalidCastException>(sample, key, expectedMessage);
			}

			[Fact(DisplayName = "Reference Type, null")]
			public void ReferenceType_Null() {
				string key = "key";
				string? expectedValue = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, null }
				};
				bool expectedResult = true;

				TestNormal(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "Value Type, null")]
			public void ValueType_Null() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, null }
				};
				string expectedMessage = "A null cannot be casted to 'System.Int32' type.";

				TestError<int, InvalidCastException>(sample, key, expectedMessage);
			}

			[Fact(DisplayName = "Reference Type, not found")]
			public void ReferenceType_NotFound() {
				string key = "key";
				string? expectedValue = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", "abc" }
				};
				bool expectedResult = false;

				TestNormal(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "Value Type, not found")]
			public void ValueType_NotFound() {
				string key = "key";
				int expectedValue = 0;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", 5 }
				};
				bool expectedResult = false;

				TestNormal(sample, key, expectedResult, expectedValue);
			}
		}

		#endregion


		#region GetOptionalValue

		public class GetOptionalValue {
			#nullable disable

			protected void TestNormal<T>(Dictionary<string, object> sample, string key, bool expectedResult, T expectedValue) {
				// Arrange
				Debug.Assert(sample != null);
				IReadOnlyDictionary<string, object> readOnlyDictionarySample = sample;
				IDictionary<string, object> dictionarySample = sample;
				Debug.Assert(key != null);

				// Act
				// overload 1: (bool, T) GetOptionalValue<T>(this IReadOnlyDictionary<string, object>, string)
				(bool, T) actual1 = readOnlyDictionarySample.GetOptionalValue<T>(key);
				// overload 2: (bool, T) GetOptionalValue<T>(this IDictionary<string, object>, string)
				(bool, T) actual2 = dictionarySample.GetOptionalValue<T>(key);
				// overload 3: (bool, T) GetOptionalValue<T>(this Dictionary<string, object>, string)
				(bool, T) actual3 = sample.GetOptionalValue<T>(key);

				// Assert
				Assert.Equal(expectedResult, actual1.Item1);
				Assert.Equal(expectedResult, actual2.Item1);
				Assert.Equal(expectedResult, actual3.Item1);
				Assert.Equal(expectedValue, actual1.Item2);
				Assert.Equal(expectedValue, actual2.Item2);
				Assert.Equal(expectedValue, actual3.Item2);
			}

			protected void TestNormal<T>(Dictionary<string, object> sample, string key, T defaultValue, T expectedValue) {
				// Arrange
				Debug.Assert(sample != null);
				IReadOnlyDictionary<string, object> readOnlyDictionarySample = sample;
				IDictionary<string, object> dictionarySample = sample;
				Debug.Assert(key != null);

				// Act
				// overload 4: T GetOptionalValue<T>(this IReadOnlyDictionary<string, object>, string, T)
				T actualValue1 = readOnlyDictionarySample.GetOptionalValue<T>(key, defaultValue);
				// overload 5: T GetOptionalValue<T>(this IDictionary<string, object>, string, T)
				T actualValue2 = dictionarySample.GetOptionalValue<T>(key, defaultValue);
				// overload 6: T GetOptionalValue<T>(this Dictionary<string, object>, string, T)
				T actualValue3 = sample.GetOptionalValue<T>(key, defaultValue);

				// Assert
				Assert.Equal(expectedValue, actualValue1);
				Assert.Equal(expectedValue, actualValue2);
				Assert.Equal(expectedValue, actualValue3);
			}

			protected void TestError<T, TException>(Dictionary<string, object> sample, string key, T defaultValue, string expectedMessage) where TException : Exception {
				// Arrange
				IReadOnlyDictionary<string, object> readOnlyDictionarySample = sample;
				IDictionary<string, object> dictionarySample = sample;

				// Act

				// overload 1: (bool, T) GetOptionalValue<T>(this IReadOnlyDictionary<string, object>, string)
				TException actualException1 = Assert.Throws<TException>(() => { readOnlyDictionarySample.GetOptionalValue<T>(key); });
				// overload 2: (bool, T) GetOptionalValue<T>(this IDictionary<string, object>, string)
				TException actualException2 = Assert.Throws<TException>(() => { dictionarySample.GetOptionalValue<T>(key); });
				// overload 3: (bool, T) GetOptionalValue<T>(this Dictionary<string, object>, string)
				TException actualException3 = Assert.Throws<TException>(() => { sample.GetOptionalValue<T>(key); });
				// overload 4: T GetOptionalValue<T>(this IReadOnlyDictionary<string, object>, string, T)
				TException actualException4 = Assert.Throws<TException>(() => { readOnlyDictionarySample.GetOptionalValue<T>(key, defaultValue); });
				// overload 5: T GetOptionalValue<T>(this IDictionary<string, object>, string, T)
				TException actualException5 = Assert.Throws<TException>(() => { dictionarySample.GetOptionalValue<T>(key, defaultValue); });
				// overload 6: T GetOptionalValue<T>(this Dictionary<string, object>, string, T)
				TException actualException6 = Assert.Throws<TException>(() => { sample.GetOptionalValue<T>(key, defaultValue); });

				// Assert
				Assert.Equal(expectedMessage, actualException1.Message);
				Assert.Equal(expectedMessage, actualException2.Message);
				Assert.Equal(expectedMessage, actualException3.Message);
				Assert.Equal(expectedMessage, actualException4.Message);
				Assert.Equal(expectedMessage, actualException5.Message);
				Assert.Equal(expectedMessage, actualException6.Message);
			}

			#nullable restore

			[Fact(DisplayName = "Reference Type, conformable")]
			public void ReferenceType_Conformable() {
				string key = "key";
				string expectedValue = "abc";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, expectedValue }
				};
				bool expectedResult = true;

				TestNormal(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "Reference Type, conformable, with defaultValue")]
			public void ReferenceType_Conformable_with_defaultValue() {
				string key = "key";
				string expectedValue = "abc";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, expectedValue }
				};
				string defaultValue = string.Empty;

				TestNormal(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "Value Type, conformable")]
			public void ValueType_Conformable() {
				string key = "key";
				int expectedValue = 7;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, expectedValue }
				};
				bool expectedResult = true;

				TestNormal(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "Value Type, conformable, with defaultValue")]
			public void ValueType_Conformable_with_defaultValue() {
				string key = "key";
				int expectedValue = 7;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, expectedValue }
				};
				int defaultValue = -1;

				TestNormal(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "Reference Type, unconformable")]
			public void ReferenceType_Unconformable() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, 5 }
				};
				string defaultValue = string.Empty;
				string expectedMessage = "A value of 'System.Int32' type cannot be casted to 'System.String' type.";

				TestError<string, InvalidCastException>(sample, key, defaultValue, expectedMessage);
			}

			[Fact(DisplayName = "Value Type, unconformable")]
			public void ValueType_Unconformable() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, "xyz" }
				};
				int defaultValue = 0;
				string expectedMessage = "A value of 'System.String' type cannot be casted to 'System.Int32' type.";

				TestError<int, InvalidCastException>(sample, key, defaultValue, expectedMessage);
			}

			[Fact(DisplayName = "Reference Type, null")]
			public void ReferenceType_Null() {
				string key = "key";
				string? expectedValue = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, null }
				};
				bool expectedResult = true;

				TestNormal(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "Reference Type, null, with defaultValue")]
			public void ReferenceType_Null_with_defaultValue() {
				string key = "key";
				string? expectedValue = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, null }
				};
				string defaultValue = string.Empty;

				TestNormal(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "Value Type, null")]
			public void ValueType_Null() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, null }
				};
				int defaultValue = 0;
				string expectedMessage = "A null cannot be casted to 'System.Int32' type.";

				TestError<int, InvalidCastException>(sample, key, defaultValue, expectedMessage);
			}

			[Fact(DisplayName = "Reference Type, not found")]
			public void ReferenceType_NotFound() {
				string key = "key";
				string? expectedValue = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", "abc" }
				};
				bool expectedResult = false;

				TestNormal(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "Reference Type, not found, with defaultValue")]
			public void ReferenceType_NotFound_with_defaultValue() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", "abc" }
				};
				string defaultValue = "xyz";
				string expectedValue = defaultValue;

				TestNormal(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "Value Type, not found")]
			public void ValueType_NotFound() {
				string key = "key";
				int expectedValue = 0;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", 5 }
				};
				bool expectedResult = false;

				TestNormal(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "Value Type, not found, with defaultValue")]
			public void ValueType_NotFound_with_defaultValue() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", 5 }
				};
				int defaultValue = -1;
				int expectedValue = defaultValue;

				TestNormal(sample, key, defaultValue, expectedValue);
			}
		}

		#endregion
	}
}
