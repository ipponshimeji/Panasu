using System;
using System.Collections.Generic;
using System.Diagnostics;
using Panasu.Testing;
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


		#region utilities

		protected static string GetDefaultInvalidCastExceptionMessage<TFrom, TTo>() {
			return $"Unable to cast object of type '{typeof(TFrom).FullName}' to type '{typeof(TTo).FullName}'.";
		}

		protected static Action<InvalidCastException> GetInvalidCastExceptionTest<TFrom, TTo>() {
			string message = GetDefaultInvalidCastExceptionMessage<TFrom, TTo>();
			return ExceptionTester<InvalidCastException>.GetTest(message);
		}

		protected static string GetNullValueExceptionMessage<TTo>() {
			return "Its value is a null.";
		}

		protected static Action<InvalidCastException> GetNullValueExceptionTest<TTo>() {
			string message = GetNullValueExceptionMessage<TTo>();
			return ExceptionTester<InvalidCastException>.GetTest(message);
		}

		protected static string GetMissingKeyExceptionMessage<TKey>(TKey key) {
			return $"The indispensable key '{key}' is missing in the dictionary.";
		}

		protected static Action<KeyNotFoundException> GetMissingKeyExceptionTest<TKey>(TKey key) {
			string message = GetMissingKeyExceptionMessage<TKey>(key);
			return ExceptionTester<KeyNotFoundException>.GetTest(message);
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
			#region utilities

			protected void TestNormal<T>(Dictionary<string, object?> sample, string key, bool expectedResult, T expectedValue) where T: notnull {
				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				T actualValue1;
				T actualValue2;
				T actualValue3;
				// overload 1: bool TryGetValue<T>(this IReadOnlyDictionary<string, object?>, string, out T)
				bool actualResult1 = readOnlyDictionarySample.TryGetValue<T>(key, out actualValue1);
				// overload 2: bool TryGetValue<T>(this IDictionary<string, object?>, string, out T)
				bool actualResult2 = dictionarySample.TryGetValue<T>(key, out actualValue2);
				// overload 3: bool TryGetValue<T>(this IDictionary<string, object?>, string, out T)
				bool actualResult3 = sample.TryGetValue<T>(key, out actualValue3);

				// Assert
				Assert.Equal(expectedResult, actualResult1);
				Assert.Equal(expectedResult, actualResult2);
				Assert.Equal(expectedResult, actualResult3);
				Assert.Equal(expectedValue, actualValue1);
				Assert.Equal(expectedValue, actualValue2);
				Assert.Equal(expectedValue, actualValue3);
			}

			protected void TestError<T, TException>(Dictionary<string, object?> sample, string key, Action<TException> testException) where T: notnull where TException: Exception {
				// argument checks
				Debug.Assert(testException != null);

				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				T dummy;
				// overload 1: bool TryGetValue<T>(this IReadOnlyDictionary<string, object>, string, out T)
				TException actualException1 = Assert.Throws<TException>(() => { readOnlyDictionarySample.TryGetValue<T>(key, out dummy); });
				// overload 2: bool TryGetValue<T>(this IDictionary<string, object>, string, out T)
				TException actualException2 = Assert.Throws<TException>(() => { dictionarySample.TryGetValue<T>(key, out dummy); });
				// overload 3: bool TryGetValue<T>(this IDictionary<string, object>, string, out T)
				TException actualException3 = Assert.Throws<TException>(() => { sample.TryGetValue<T>(key, out dummy); });

				// Assert
				testException(actualException1);
				testException(actualException2);
				testException(actualException3);
			}

			#endregion


			#region tests

			[Fact(DisplayName = "value: conformable, Reference Type")]
			public void value_conformable_ReferenceType() {
				string key = "key";
				string value = "value";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				bool expectedResult = true;
				string expectedValue = value;

				TestNormal<string>(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "value: conformable, Value Type")]
			public void value_conformable_ValueType() {
				string key = "key";
				int value = 7;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				bool expectedResult = true;
				int expectedValue = value;

				TestNormal<int>(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "value: unconformable, Reference Type")]
			public void value_unconformable_ReferenceType() {
				string key = "key";
				Version value = new Version(1, 2, 3);
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				Action<InvalidCastException> testException = GetInvalidCastExceptionTest<Version, string>();

				TestError<string, InvalidCastException>(sample, key, testException);
			}

			[Fact(DisplayName = "value: unconformable, Value Type")]
			public void value_unconformable_ValueType() {
				string key = "key";
				bool value = true;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				Action<InvalidCastException> testException = GetInvalidCastExceptionTest<bool, int>();

				TestError<int, InvalidCastException>(sample, key, testException);
			}

			[Fact(DisplayName = "value: null, Reference Type")]
			public void value_null_ReferenceType() {
				string key = "key";
				object? value = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				Action<InvalidCastException> testException = GetNullValueExceptionTest<string>();

				TestError<string, InvalidCastException>(sample, key, testException);
			}

			[Fact(DisplayName = "value: null, Value Type")]
			public void value_null_ValueType() {
				string key = "key";
				object? value = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				Action<InvalidCastException> testException = GetNullValueExceptionTest<int>();

				TestError<int, InvalidCastException>(sample, key, testException);
			}

			[Fact(DisplayName = "value: missing, Reference Type")]
			public void value_missing_ReferenceType() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", "abc" }
				};

				bool expectedResult = false;
				string expectedValue = null!;

				TestNormal<string>(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "value: missing, Value Type")]
			public void value_missing_ValueType() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", 5 }
				};

				bool expectedResult = false;
				int expectedValue = 0;

				TestNormal<int>(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "dictionary: null")]
			public void dictionary_null() {
				string key = "key";
				Dictionary<string, object?> sample = null!;

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("dictionary");

				TestError<string, ArgumentNullException>(sample, key, testException);
			}

			[Fact(DisplayName = "key: null")]
			public void key_null() {
				string key = null!;
				Dictionary<string, object?> sample = new Dictionary<string, object?>();

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("key");

				TestError<string, ArgumentNullException>(sample, key, testException);
			}

			#endregion
		}

		#endregion


		#region TryGetNullableValue

		public class TryGetNullableValue {
			#region utilities

			protected void TestNormal<T>(Dictionary<string, object?> sample, string key, bool expectedResult, T? expectedValue) where T: class {
				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				T? actualValue1;
				T? actualValue2;
				T? actualValue3;
				// overload 1: bool TryGetNullableValue<T>(this IReadOnlyDictionary<string, object?>, string, out T)
				bool actualResult1 = readOnlyDictionarySample.TryGetNullableValue<T>(key, out actualValue1);
				// overload 2: bool TryGetNullableValue<T>(this IDictionary<string, object?>, string, out T)
				bool actualResult2 = dictionarySample.TryGetNullableValue<T>(key, out actualValue2);
				// overload 3: bool TryGetNullableValue<T>(this IDictionary<string, object?>, string, out T)
				bool actualResult3 = sample.TryGetNullableValue<T>(key, out actualValue3);

				// Assert
				Assert.Equal(expectedResult, actualResult1);
				Assert.Equal(expectedResult, actualResult2);
				Assert.Equal(expectedResult, actualResult3);
				Assert.Equal(expectedValue, actualValue1);
				Assert.Equal(expectedValue, actualValue2);
				Assert.Equal(expectedValue, actualValue3);
			}

			protected void TestError<T, TException>(Dictionary<string, object?> sample, string key, Action<TException> testException) where T: class where TException: Exception {
				// argument checks
				Debug.Assert(testException != null);

				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				T? dummy;
				// overload 1: bool TryGetNullableValue<T>(this IReadOnlyDictionary<string, object>, string, out T)
				TException actualException1 = Assert.Throws<TException>(() => { readOnlyDictionarySample.TryGetNullableValue<T>(key, out dummy); });
				// overload 2: bool TryGetNullableValue<T>(this IDictionary<string, object>, string, out T)
				TException actualException2 = Assert.Throws<TException>(() => { dictionarySample.TryGetNullableValue<T>(key, out dummy); });
				// overload 3: bool TryGetNullableValue<T>(this IDictionary<string, object>, string, out T)
				TException actualException3 = Assert.Throws<TException>(() => { sample.TryGetNullableValue<T>(key, out dummy); });

				// Assert
				testException(actualException1);
				testException(actualException2);
				testException(actualException3);
			}

			#endregion


			#region tests

			[Fact(DisplayName = "value: conformable")]
			public void value_conformable() {
				string key = "key";
				string? value = "value";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				bool expectedResult = true;
				string? expectedValue = value;

				TestNormal<string>(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "value: unconformable")]
			public void value_unconformable() {
				string key = "key";
				Version value = new Version(1, 2, 3);
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				Action<InvalidCastException> testException = GetInvalidCastExceptionTest<Version, string>();

				TestError<string, InvalidCastException>(sample, key, testException);
			}

			[Fact(DisplayName = "value: null")]
			public void value_null() {
				string key = "key";
				string? value = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				bool expectedResult = true;
				string? expectedValue = value;

				TestNormal<string>(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "value: missing")]
			public void value_missing() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", "abc" }
				};

				bool expectedResult = false;
				string? expectedValue = null;

				TestNormal<string>(sample, key, expectedResult, expectedValue);
			}

			[Fact(DisplayName = "dictionary: null")]
			public void dictionary_null() {
				string key = "key";
				Dictionary<string, object?> sample = null!;

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("dictionary");

				TestError<string, ArgumentNullException>(sample, key, testException);
			}

			[Fact(DisplayName = "key: null")]
			public void key_null() {
				string key = null!;
				Dictionary<string, object?> sample = new Dictionary<string, object?>();

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("key");

				TestError<string, ArgumentNullException>(sample, key, testException);
			}

			#endregion
		}

		#endregion


		#region GetOptionalValue

		public class GetOptionalValue {
			#region utilities

			protected void TestNormal<T>(Dictionary<string, object?> sample, string key, T defaultValue, T expectedValue) where T: notnull {
				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				// overload 1: T GetOptionalValue<T>(this IReadOnlyDictionary<string, object?>, string, T)
				T actual1 = readOnlyDictionarySample.GetOptionalValue<T>(key, defaultValue);
				// overload 2: T GetOptionalValue<T>(this IDictionary<string, object?>, string, T)
				T actual2 = dictionarySample.GetOptionalValue<T>(key, defaultValue);
				// overload 3: T GetOptionalValue<T>(this IDictionary<string, object?>, string, T)
				T actual3 = sample.GetOptionalValue<T>(key, defaultValue);

				// Assert
				Assert.Equal(expectedValue, actual1);
				Assert.Equal(expectedValue, actual2);
				Assert.Equal(expectedValue, actual3);
			}

			protected void TestError<T, TException>(Dictionary<string, object?> sample, string key, T defaultValue, Action<TException> testException) where T: notnull where TException: Exception {
				// argument checks
				Debug.Assert(testException != null);

				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				// overload 1: T GetOptionalValue<T>(this IReadOnlyDictionary<string, object>, string, T)
				TException actualException1 = Assert.Throws<TException>(() => { readOnlyDictionarySample.GetOptionalValue<T>(key, defaultValue); });
				// overload 2: T GetOptionalValue<T>(this IDictionary<string, object>, string, T)
				TException actualException2 = Assert.Throws<TException>(() => { dictionarySample.GetOptionalValue<T>(key, defaultValue); });
				// overload 3: T GetOptionalValue<T>(this IDictionary<string, object>, string, T)
				TException actualException3 = Assert.Throws<TException>(() => { sample.GetOptionalValue<T>(key, defaultValue); });

				// Assert
				testException(actualException1);
				testException(actualException2);
				testException(actualException3);
			}

			#endregion


			#region tests

			[Fact(DisplayName = "value: conformable, Reference Type")]
			public void value_conformable_ReferenceType() {
				string key = "key";
				string value = "value";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};
				string defaultValue = "default";

				string expectedValue = value;

				TestNormal<string>(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "value: conformable, Value Type")]
			public void value_conformable_ValueType() {
				string key = "key";
				int value = 7;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};
				int defaultValue = -1;

				int expectedValue = value;

				TestNormal<int>(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "value: unconformable, Reference Type")]
			public void value_unconformable_ReferenceType() {
				string key = "key";
				Version value = new Version(1, 2, 3);
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};
				string defaultValue = "default";

				Action<InvalidCastException> testException = GetInvalidCastExceptionTest<Version, string>();

				TestError<string, InvalidCastException>(sample, key, defaultValue, testException);
			}

			[Fact(DisplayName = "value: unconformable, Value Type")]
			public void value_unconformable_ValueType() {
				string key = "key";
				bool value = false;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};
				int defaultValue = -1;

				Action<InvalidCastException> testException = GetInvalidCastExceptionTest<bool, int>();

				TestError<int, InvalidCastException>(sample, key, defaultValue, testException);
			}

			[Fact(DisplayName = "value: null, Reference Type")]
			public void value_null_ReferenceType() {
				string key = "key";
				object? value = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};
				string defaultValue = "default";

				Action<InvalidCastException> testException = GetNullValueExceptionTest<string>();

				TestError<string, InvalidCastException>(sample, key, defaultValue, testException);
			}

			[Fact(DisplayName = "value: null, Value Type")]
			public void value_null_ValueType() {
				string key = "key";
				object? value = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};
				int defaultValue = -1;

				Action<InvalidCastException> testException = GetNullValueExceptionTest<int>();

				TestError<int, InvalidCastException>(sample, key, defaultValue, testException);
			}

			[Fact(DisplayName = "value: missing, Reference Type")]
			public void value_missing_ReferenceType() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", "abc" }
				};
				string defaultValue = "default";

				string expectedValue = defaultValue;

				TestNormal<string>(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "value: missing, Value Type")]
			public void value_missing_ValueType() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", 5 }
				};
				int defaultValue = 3;

				int expectedValue = defaultValue;

				TestNormal<int>(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "dictionary: null")]
			public void dictionary_null() {
				string key = "key";
				Dictionary<string, object?> sample = null!;
				string defaultValue = "default";

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("dictionary");

				TestError<string, ArgumentNullException>(sample, key, defaultValue, testException);
			}

			[Fact(DisplayName = "key: null")]
			public void key_null() {
				string key = null!;
				Dictionary<string, object?> sample = new Dictionary<string, object?>();
				string defaultValue = "default";

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("key");

				TestError<string, ArgumentNullException>(sample, key, defaultValue, testException);
			}

			#endregion
		}

		#endregion


		#region GetOptionalNullableValue

		public class GetOptionalNullableValue {
			#region utilities

			protected void TestNormal<T>(Dictionary<string, object?> sample, string key, T? defaultValue, T? expectedValue) where T: class {
				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				// overload 1: T? GetOptionalNullableValue<T>(this IReadOnlyDictionary<string, object?>, string, T)
				T? actual1 = readOnlyDictionarySample.GetOptionalNullableValue<T>(key, defaultValue);
				// overload 2: T? GetOptionalNullableValue<T>(this IDictionary<string, object?>, string, T)
				T? actual2 = dictionarySample.GetOptionalNullableValue<T>(key, defaultValue);
				// overload 3: T? GetOptionalNullableValue<T>(this IDictionary<string, object?>, string, T)
				T? actual3 = sample.GetOptionalNullableValue<T>(key, defaultValue);

				// Assert
				Assert.Equal(expectedValue, actual1);
				Assert.Equal(expectedValue, actual2);
				Assert.Equal(expectedValue, actual3);
			}

			protected void TestError<T, TException>(Dictionary<string, object?> sample, string key, T? defaultValue, Action<TException> testException) where T: class where TException: Exception {
				// argument checks
				Debug.Assert(testException != null);

				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				// overload 1: T? GetOptionalNullableValue<T>(this IReadOnlyDictionary<string, object>, string, T)
				TException actualException1 = Assert.Throws<TException>(() => { readOnlyDictionarySample.GetOptionalNullableValue<T>(key, defaultValue); });
				// overload 2: T? GetOptionalNullableValue<T>(this IDictionary<string, object>, string, T)
				TException actualException2 = Assert.Throws<TException>(() => { dictionarySample.GetOptionalNullableValue<T>(key, defaultValue); });
				// overload 3: T? GetOptionalNullableValue<T>(this IDictionary<string, object>, string, T)
				TException actualException3 = Assert.Throws<TException>(() => { sample.GetOptionalNullableValue<T>(key, defaultValue); });

				// Assert
				testException(actualException1);
				testException(actualException2);
				testException(actualException3);
			}

			#endregion


			#region tests

			[Fact(DisplayName = "value: conformable")]
			public void value_conformable() {
				string key = "key";
				string? value = "value";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};
				string? defaultValue = "default";

				string? expectedValue = value;

				TestNormal<string>(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "value: unconformable")]
			public void value_unconformable() {
				string key = "key";
				Version value = new Version(1, 2, 3);
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};
				string? defaultValue = "default";

				Action<InvalidCastException> testException = GetInvalidCastExceptionTest<Version, string>();

				TestError<string, InvalidCastException>(sample, key, defaultValue, testException);
			}

			[Fact(DisplayName = "value: null")]
			public void value_null() {
				string key = "key";
				string? value = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};
				string? defaultValue = "default";

				string? expectedValue = value;

				TestNormal<string>(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "value: missing; defaultValue: non-null")]
			public void value_missing_defaultValue_nonnull() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", "abc" }
				};
				string? defaultValue = "default";

				string? expectedValue = defaultValue;

				TestNormal<string>(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "value: missing; defaultValue: null")]
			public void value_missing_defaultValue_null() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", "abc" }
				};
				string? defaultValue = null;

				string? expectedValue = defaultValue;

				TestNormal<string>(sample, key, defaultValue, expectedValue);
			}

			[Fact(DisplayName = "dictionary: null")]
			public void dictionary_null() {
				string key = "key";
				Dictionary<string, object?> sample = null!;
				string defaultValue = "default";

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("dictionary");

				TestError<string, ArgumentNullException>(sample, key, defaultValue, testException);
			}

			[Fact(DisplayName = "key: null")]
			public void key_null() {
				string key = null!;
				Dictionary<string, object?> sample = new Dictionary<string, object?>();
				string defaultValue = "default";

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("key");

				TestError<string, ArgumentNullException>(sample, key, defaultValue, testException);
			}

			#endregion
		}

		#endregion


		#region GetIndispensableValue

		public class GetIndispensableValue {
			#region utilities

			protected void TestNormal<T>(Dictionary<string, object?> sample, string key, T expectedValue) where T: notnull {
				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				// overload 1: T GetIndispensableValue<T>(this IReadOnlyDictionary<string, object?>, string)
				T actual1 = readOnlyDictionarySample.GetIndispensableValue<T>(key);
				// overload 2: T GetIndispensableValue<T>(this IDictionary<string, object?>, string)
				T actual2 = dictionarySample.GetIndispensableValue<T>(key);
				// overload 3: T GetIndispensableValue<T>(this IDictionary<string, object?>, string)
				T actual3 = sample.GetIndispensableValue<T>(key);

				// Assert
				Assert.Equal(expectedValue, actual1);
				Assert.Equal(expectedValue, actual2);
				Assert.Equal(expectedValue, actual3);
			}

			protected void TestError<T, TException>(Dictionary<string, object?> sample, string key, Action<TException> testException) where T: notnull where TException: Exception {
				// argument checks
				Debug.Assert(testException != null);

				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				// overload 1: T GetIndispensableValue<T>(this IReadOnlyDictionary<string, object>, string)
				TException actualException1 = Assert.Throws<TException>(() => { readOnlyDictionarySample.GetIndispensableValue<T>(key); });
				// overload 2: T GetIndispensableValue<T>(this IDictionary<string, object>, string)
				TException actualException2 = Assert.Throws<TException>(() => { dictionarySample.GetIndispensableValue<T>(key); });
				// overload 3: T GetIndispensableValue<T>(this IDictionary<string, object>, string)
				TException actualException3 = Assert.Throws<TException>(() => { sample.GetIndispensableValue<T>(key); });

				// Assert
				testException(actualException1);
				testException(actualException2);
				testException(actualException3);
			}

			#endregion


			#region tests

			[Fact(DisplayName = "value: conformable, Reference Type")]
			public void value_conformable_ReferenceType() {
				string key = "key";
				string value = "value";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				string expectedValue = value;

				TestNormal<string>(sample, key, expectedValue);
			}

			[Fact(DisplayName = "value: conformable, Value Type")]
			public void value_conformable_ValueType() {
				string key = "key";
				int value = 7;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				int expectedValue = value;

				TestNormal<int>(sample, key, expectedValue);
			}

			[Fact(DisplayName = "value: unconformable, Reference Type")]
			public void value_unconformable_ReferenceType() {
				string key = "key";
				Version value = new Version(1, 2, 3);
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				Action<InvalidCastException> testException = GetInvalidCastExceptionTest<Version, string>();

				TestError<string, InvalidCastException>(sample, key, testException);
			}

			[Fact(DisplayName = "value: unconformable, Value Type")]
			public void value_unconformable_ValueType() {
				string key = "key";
				bool value = true;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				Action<InvalidCastException> testException = GetInvalidCastExceptionTest<bool, int>();

				TestError<int, InvalidCastException>(sample, key, testException);
			}

			[Fact(DisplayName = "value: null, Reference Type")]
			public void value_null_ReferenceType() {
				string key = "key";
				object? value = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				Action<InvalidCastException> testException = GetNullValueExceptionTest<string>();

				TestError<string, InvalidCastException>(sample, key, testException);
			}

			[Fact(DisplayName = "value: null, Value Type")]
			public void value_null_ValueType() {
				string key = "key";
				object? value = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				Action<InvalidCastException> testException = GetNullValueExceptionTest<int>();

				TestError<int, InvalidCastException>(sample, key, testException);
			}

			[Fact(DisplayName = "value: missing, Reference Type")]
			public void value_missing_ReferenceType() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", "abc" }
				};

				Action<KeyNotFoundException> testException = GetMissingKeyExceptionTest(key);

				TestError<string, KeyNotFoundException>(sample, key, testException);
			}

			[Fact(DisplayName = "value: missing, Value Type")]
			public void value_missing_ValueType() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", 5 }
				};

				Action<KeyNotFoundException> testException = GetMissingKeyExceptionTest(key);

				TestError<int, KeyNotFoundException>(sample, key, testException);
			}

			[Fact(DisplayName = "dictionary: null")]
			public void dictionary_null() {
				string key = "key";
				Dictionary<string, object?> sample = null!;

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("dictionary");

				TestError<string, ArgumentNullException>(sample, key, testException);
			}

			[Fact(DisplayName = "key: null")]
			public void key_null() {
				string key = null!;
				Dictionary<string, object?> sample = new Dictionary<string, object?>();

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("key");

				TestError<string, ArgumentNullException>(sample, key, testException);
			}

			#endregion
		}

		#endregion


		#region GetIndispensableNullableValue

		public class GetIndispensableNullableValue {
			#region utilities

			protected void TestNormal<T>(Dictionary<string, object?> sample, string key, T? expectedValue) where T: class {
				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				// overload 1: T? GetIndispensableNullableValue<T>(this IReadOnlyDictionary<string, object?>, string)
				T? actual1 = readOnlyDictionarySample.GetIndispensableNullableValue<T>(key);
				// overload 2: T? GetIndispensableNullableValue<T>(this IDictionary<string, object?>, string)
				T? actual2 = dictionarySample.GetIndispensableNullableValue<T>(key);
				// overload 3: T? GetIndispensableNullableValue<T>(this IDictionary<string, object?>, string)
				T? actual3 = sample.GetIndispensableNullableValue<T>(key);

				// Assert
				Assert.Equal(expectedValue, actual1);
				Assert.Equal(expectedValue, actual2);
				Assert.Equal(expectedValue, actual3);
			}

			protected void TestError<T, TException>(Dictionary<string, object?> sample, string key, Action<TException> testException) where T: class where TException: Exception {
				// argument checks
				Debug.Assert(testException != null);

				// Arrange
				IReadOnlyDictionary<string, object?> readOnlyDictionarySample = sample;
				IDictionary<string, object?> dictionarySample = sample;

				// Act
				// overload 1: T? GetIndispensableNullableValue<T>(this IReadOnlyDictionary<string, object>, string)
				TException actualException1 = Assert.Throws<TException>(() => { readOnlyDictionarySample.GetIndispensableNullableValue<T>(key); });
				// overload 2: T? GetIndispensableNullableValue<T>(this IDictionary<string, object>, string)
				TException actualException2 = Assert.Throws<TException>(() => { dictionarySample.GetIndispensableNullableValue<T>(key); });
				// overload 3: T? GetIndispensableNullableValue<T>(this IDictionary<string, object>, string)
				TException actualException3 = Assert.Throws<TException>(() => { sample.GetIndispensableNullableValue<T>(key); });

				// Assert
				testException(actualException1);
				testException(actualException2);
				testException(actualException3);
			}

			#endregion


			#region tests

			[Fact(DisplayName = "value: conformable")]
			public void value_conformable() {
				string key = "key";
				string? value = "value";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				string? expectedValue = value;

				TestNormal<string>(sample, key, expectedValue);
			}

			[Fact(DisplayName = "value: unconformable")]
			public void value_unconformable() {
				string key = "key";
				Version value = new Version(1, 2, 3);
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				Action<InvalidCastException> testException = GetInvalidCastExceptionTest<Version, string>();

				TestError<string, InvalidCastException>(sample, key, testException);
			}

			[Fact(DisplayName = "value: null")]
			public void value_null() {
				string key = "key";
				string? value = null;
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ key, value }
				};

				string? expectedValue = value;

				TestNormal<string>(sample, key, expectedValue);
			}

			[Fact(DisplayName = "value: missing")]
			public void value_missing() {
				string key = "key";
				Dictionary<string, object?> sample = new Dictionary<string, object?>() {
					{ "anotherKey", "abc" }
				};

				Action<KeyNotFoundException> testException = GetMissingKeyExceptionTest(key);

				TestError<string, KeyNotFoundException>(sample, key, testException);
			}

			[Fact(DisplayName = "dictionary: null")]
			public void dictionary_null() {
				string key = "key";
				Dictionary<string, object?> sample = null!;

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("dictionary");

				TestError<string, ArgumentNullException>(sample, key, testException);
			}

			[Fact(DisplayName = "key: null")]
			public void key_null() {
				string key = null!;
				Dictionary<string, object?> sample = new Dictionary<string, object?>();

				Action<ArgumentNullException> testException = ArgumentExceptionTester<ArgumentNullException>.GetTest("key");

				TestError<string, ArgumentNullException>(sample, key, testException);
			}

			#endregion
		}

		#endregion


		#region IsRelativeUri

		public class IsRelativeUri {
			[Fact(DisplayName = "uriString: relative, simple")]
			public void uriString_relative_simple() {
				// Arrange
				string uriString = "a/b/c";

				// Act
				bool actual = Util.IsRelativeUri(uriString);

				// Assert
				Assert.True(actual);
			}

			[Fact(DisplayName = "uriString: relative, with fragment")]
			public void uriString_relative_with_fragment() {
				// Arrange
				string uriString = "a/b/c#def";

				// Act
				bool actual = Util.IsRelativeUri(uriString);

				// Assert
				Assert.True(actual);
			}

			[Fact(DisplayName = "uriString: absolute, url")]
			public void uriString_absolute_url() {
				// Arrange
				string uriString = "http://www.example.org/a/b/c";

				// Act
				bool actual = Util.IsRelativeUri(uriString);

				// Assert
				Assert.False(actual);
			}

			[Fact(DisplayName = "uriString: absolute, path")]
			public void uriString_absolute_file() {
				// Arrange
				string uriString = "/a/b/c";

				// Act
				bool actual = Util.IsRelativeUri(uriString);

				// Assert
				Assert.False(actual);
			}

			[Fact(DisplayName = "uriString: null")]
			public void uriString_null() {
				// Arrange
				string uriString = null!;

				// Act and Assert
				ArgumentNullException actual = Assert.Throws<ArgumentNullException>(
					"uriString",
					() => {	Util.IsRelativeUri(uriString);}
				);
			}

			[Fact(DisplayName = "uriString: empty")]
			public void uriString_empty() {
				// Arrange
				string uriString = string.Empty;

				// Act and Assert
				ArgumentNullException actual = Assert.Throws<ArgumentNullException>(
					"uriString",
					() => {	Util.IsRelativeUri(uriString); }
				);
			}
		}

		#endregion


		#region DecomposeRelativeUri

		public class DecomposeRelativeUri {
			[Fact(DisplayName = "relativeUriString: typical")]
			public void relativeUriString_typical() {
				// Arrange
				string relativeUriString = "a/b/c#def";

				// Act
				(string unescapedPath, string fragment) = Util.DecomposeRelativeUri(relativeUriString);

				// Assert
				Assert.Equal("a/b/c", unescapedPath);
				Assert.Equal("#def", fragment);
			}

			[Fact(DisplayName = "relativeUriString: no fragment")]
			public void relativeUriString_fragment_none() {
				// Arrange
				string relativeUriString = "a/b/c";

				// Act
				(string unescapedPath, string fragment) = Util.DecomposeRelativeUri(relativeUriString);

				// Assert
				Assert.Equal("a/b/c", unescapedPath);
				Assert.Empty(fragment);
			}

			[Fact(DisplayName = "relativeUriString: empty fragment")]
			public void relativeUriString_fragment_empty() {
				// Arrange
				string relativeUriString = "a/b/c#";

				// Act
				(string unescapedPath, string fragment) = Util.DecomposeRelativeUri(relativeUriString);

				// Assert
				Assert.Equal("a/b/c", unescapedPath);
				Assert.Equal("#", fragment);
			}

			[Fact(DisplayName = "relativeUriString: URL-encoded fragment, unescapePath: true")]
			public void relativeUriString_fragment_encoded_unescapePath_true() {
				// Arrange
				string relativeUriString = "a/b/c#%2F%20abc%2F";
				bool unescapePath = true;

				// Act
				(string unescapedPath, string fragment) = Util.DecomposeRelativeUri(relativeUriString, unescapePath);

				// Assert
				Assert.Equal("a/b/c", unescapedPath);
				Assert.Equal("#%2F%20abc%2F", fragment);	// fragment should not be unescaped
			}

			[Fact(DisplayName = "relativeUriString: URL-encoded fragment, unescapePath: false")]
			public void relativeUriString_fragment_encoded_unescapePath_false() {
				// Arrange
				string relativeUriString = "a/b/c#%2F%20abc%2F";
				bool unescapePath = false;

				// Act
				(string unescapedPath, string fragment) = Util.DecomposeRelativeUri(relativeUriString, unescapePath);

				// Assert
				Assert.Equal("a/b/c", unescapedPath);
				Assert.Equal("#%2F%20abc%2F", fragment);    // fragment should not be unescaped
			}

			[Fact(DisplayName = "relativeUriString: no path")]
			public void relativeUriString_path_none() {
				// Arrange
				string relativeUriString = "#def";

				// Act
				(string unescapedPath, string fragment) = Util.DecomposeRelativeUri(relativeUriString);

				// Assert
				Assert.Empty(unescapedPath);
				Assert.Equal("#def", fragment);
			}

			[Fact(DisplayName = "relativeUriString: URL-encoded path, unescapePath: true")]
			public void relativeUriString_path_encoded_unescapePath_true() {
				// Arrange
				string relativeUriString = "a/b%20%23/c#def";
				bool unescapePath = true;

				// Act
				(string unescapedPath, string fragment) = Util.DecomposeRelativeUri(relativeUriString, unescapePath);

				// Assert
				Assert.Equal("a/b #/c", unescapedPath);   // path should be unescaped
				Assert.Equal("#def", fragment);
			}

			[Fact(DisplayName = "relativeUriString: URL-encoded path, unescapePath: false")]
			public void relativeUriString_path_encoded_unescapePath_false() {
				// Arrange
				string relativeUriString = "a/b%20%23/c#def";
				bool unescapePath = false;

				// Act
				(string unescapedPath, string fragment) = Util.DecomposeRelativeUri(relativeUriString, unescapePath);

				// Assert
				Assert.Equal("a/b%20%23/c", unescapedPath);
				Assert.Equal("#def", fragment);
			}

			[Fact(DisplayName = "relativeUriString: null")]
			public void relativeUriString_null() {
				// Arrange
				string relativeUriString = null!;

				// Act and Assert
				ArgumentNullException actual = Assert.Throws<ArgumentNullException>(
					"relativeUriString",
					() => { Util.DecomposeRelativeUri(relativeUriString); }
				);
			}

			[Fact(DisplayName = "relativeUriString: empty")]
			public void relativeUriString_empty() {
				// Arrange
				string relativeUriString = string.Empty;

				// Act
				(string unescapedPath, string fragment) = Util.DecomposeRelativeUri(relativeUriString);

				// Assert
				Assert.Empty(unescapedPath);
				Assert.Empty(fragment);
			}
		}

		#endregion


		#region RebaseRelativeUri

		public class RebaseRelativeUri {
			[Fact(DisplayName = "typical")]
			public void typical() {
				// Arrange
				Uri oldBaseUri = new Uri("file:///a/b/c");
				string relativeUriString = "d/e/f#ghi";
				Uri newBaseUri = new Uri("file:///x/y/z");

				// Act
				string actual = Util.RebaseRelativeUri(oldBaseUri, relativeUriString, newBaseUri);

				// Assert
				Assert.Equal("../../a/b/d/e/f#ghi", actual);
			}

			[Fact(DisplayName = "ancestor")]
			public void ancestor() {
				// Arrange
				Uri oldBaseUri = new Uri("file:///a/b/c");
				string relativeUriString = "d/e/f#ghi";
				Uri newBaseUri = new Uri("file:///a/b/x/y/z");

				// Act
				string actual = Util.RebaseRelativeUri(oldBaseUri, relativeUriString, newBaseUri);

				// Assert
				Assert.Equal("../../d/e/f#ghi", actual);
			}

			[Fact(DisplayName = "decendent")]
			public void decendent() {
				// Arrange
				Uri oldBaseUri = new Uri("file:///a/b/c");
				string relativeUriString = "d/e/f#ghi";
				Uri newBaseUri = new Uri("file:///x");

				// Act
				string actual = Util.RebaseRelativeUri(oldBaseUri, relativeUriString, newBaseUri);

				// Assert
				Assert.Equal("a/b/d/e/f#ghi", actual);
			}

			[Fact(DisplayName = "no fragment")]
			public void fragment_none() {
				// Arrange
				Uri oldBaseUri = new Uri("file:///a/b/c");
				string relativeUriString = "d/e/f";
				Uri newBaseUri = new Uri("file:///x/y/z");

				// Act
				string actual = Util.RebaseRelativeUri(oldBaseUri, relativeUriString, newBaseUri);

				// Assert
				Assert.Equal("../../a/b/d/e/f", actual);
			}

			[Fact(DisplayName = "absolute")]
			public void absolute() {
				// Arrange
				Uri oldBaseUri = new Uri("http://a.example.org/a/b/c");
				string relativeUriString = "d/e/f#ghi";
				Uri newBaseUri = new Uri("http://b.example.org/a/b/c");

				// Act
				string actual = Util.RebaseRelativeUri(oldBaseUri, relativeUriString, newBaseUri);

				// Assert
				// The uri cannot ba rebased. The absolute uri to the original is returned. 
				Assert.Equal("http://a.example.org/a/b/d/e/f#ghi", actual);
			}

			[Fact(DisplayName = "URL-encoded")]
			public void encoded() {
				// Arrange
				Uri oldBaseUri = new Uri("file:///a/b%20/c");
				string relativeUriString = "d/e%20%23/f#g%2Fh";
				Uri newBaseUri = new Uri("file:///x/y/z");

				// Act
				string actual = Util.RebaseRelativeUri(oldBaseUri, relativeUriString, newBaseUri);

				// Assert
				Assert.Equal("../../a/b%20/d/e%20%23/f#g%2Fh", actual);
			}

			[Fact(DisplayName = "oldBaseUri: null")]
			public void oldBaseUri_null() {
				// Arrange
				Uri oldBaseUri = null!;
				string relativeUriString = "e/f/g";
				Uri newBaseUri = new Uri("file:///a/b/d");

				// Act and Assert
				ArgumentNullException actual = Assert.Throws<ArgumentNullException>(
					"oldBaseUri",
					() => { Util.RebaseRelativeUri(oldBaseUri, relativeUriString, newBaseUri); }
				);
			}

			[Fact(DisplayName = "relativeUriString: null")]
			public void relativeUriString_null() {
				// Arrange
				Uri oldBaseUri = new Uri("file:///a/b/c");
				string relativeUriString = null!;
				Uri newBaseUri = new Uri("file:///a/b/d");

				// Act and Assert
				ArgumentNullException actual = Assert.Throws<ArgumentNullException>(
					"relativeUriString",
					() => { Util.RebaseRelativeUri(oldBaseUri, relativeUriString, newBaseUri); }
				);
			}

			[Fact(DisplayName = "newBaseUri: null")]
			public void newBaseUri_null() {
				// Arrange
				Uri oldBaseUri = new Uri("file:///a/b/c");
				string relativeUriString = "e/f/g";
				Uri newBaseUri = null!;

				// Act and Assert
				ArgumentNullException actual = Assert.Throws<ArgumentNullException>(
					"newBaseUri",
					() => { Util.RebaseRelativeUri(oldBaseUri, relativeUriString, newBaseUri); }
				);
			}
		}

		#endregion
	}
}
