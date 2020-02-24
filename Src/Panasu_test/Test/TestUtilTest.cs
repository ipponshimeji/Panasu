using System;
using System.Collections.Generic;
using Panasu.Test;
using Xunit;
using Xunit.Sdk;

namespace Panasu.Test {
	public class TestUtilTest {
		#region tests

		public class EqualJson {
			[Fact(DisplayName ="Success, null")]
			public void Success_null() {
				// Arrange
				object? expected = null;
				object? actual = null;

				// Act
				TestUtil.EqualJson(expected, actual);

				// Arrange
				// no exception should be thrown
			}

			[Fact(DisplayName = "Success, number")]
			public void Success_number() {
				// Arrange
				object expected = 1.23;
				object actual = 1.23;

				// Act
				TestUtil.EqualJson(expected, actual);

				// Arrange
				// no exception should be thrown
			}

			[Fact(DisplayName = "Success, boolean")]
			public void Success_boolean() {
				// Arrange
				object expected = true;
				object actual = true;

				// Act
				TestUtil.EqualJson(expected, actual);

				// Arrange
				// no exception should be thrown
			}

			[Fact(DisplayName = "Success, string")]
			public void Success_string() {
				// Arrange
				object expected = true;
				object actual = true;

				// Act
				TestUtil.EqualJson(expected, actual);

				// Arrange
				// no exception should be thrown
			}

			[Fact(DisplayName = "Success, array")]
			public void Success_array() {
				// Arrange
				object expected = new object?[] { "abc", 567.89, null, false };
				object actual = new object?[] { "abc", 567.89, null, false };

				// Act
				TestUtil.EqualJson(expected, actual);

				// Arrange
				// no exception should be thrown
			}

			[Fact(DisplayName = "Success, object")]
			public void Success_object() {
				// Arrange
				object expected = new Dictionary<string, object> { { "abc", 123.45 }, { "xyz", false } };
				object actual = new Dictionary<string, object> { { "abc", 123.45 }, { "xyz", false } };

				// Act
				TestUtil.EqualJson(expected, actual);

				// Arrange
				// no exception should be thrown
			}

			[Fact(DisplayName = "Success, object, non-ordered")]
			public void Success_object_nonordered() {
				// Arrange
				object expected = new Dictionary<string, object> { { "abc", 123.45 }, { "xyz", false } };
				object actual = new Dictionary<string, object> { { "xyz", false }, { "abc", 123.45 } };

				// Act
				TestUtil.EqualJson(expected, actual);

				// Arrange
				// no exception should be thrown
			}

			[Fact(DisplayName = "Success, object-array")]
			public void Success_object_array() {
				// Arrange
				object expected = new Dictionary<string, object> {
					{ "abc", new object[] { "a", "b", "c" } },
					{ "xyz", new object?[] { 1.23, false, null } }
				};
				object actual = new Dictionary<string, object> {
					{ "xyz", new object?[] { 1.23, false, null } },
					{ "abc", new object[] { "a", "b", "c" } }
				};

				// Act
				TestUtil.EqualJson(expected, actual);

				// Arrange
				// no exception should be thrown
			}

			[Fact(DisplayName = "Success, array-object")]
			public void Success_array_object() {
				// Arrange
				object expected = new object[] {
					true,
					new Dictionary<string, object> { { "abc", 123.4 }, { "xyz", false } }
				};
				object actual = new object[] {
					true,
					new Dictionary<string, object> { { "xyz", false }, { "abc", 123.4 } }
				};

				// Act
				TestUtil.EqualJson(expected, actual);

				// Arrange
				// no exception should be thrown
			}

			[Fact(DisplayName = "Failure, null")]
			public void Failure_null() {
				// Arrange
				object? expected = null;
				object actual = "abc";

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal(string.Empty, exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Null(exception.Expected);
				Assert.Equal("abc", exception.Actual);
			}

			[Fact(DisplayName = "Failure, number")]
			public void Failure_number() {
				// Arrange
				object expected = 1.23;
				object actual = -9.57;
				
				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal(string.Empty, exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("1.23", exception.Expected);
				Assert.Equal("-9.57", exception.Actual);
			}

			[Fact(DisplayName = "Failure, boolean")]
			public void Failure_boolean() {
				// Arrange
				object expected = true;
				object actual = false;

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal(string.Empty, exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("True", exception.Expected);
				Assert.Equal("False", exception.Actual);
			}

			[Fact(DisplayName = "Failure, string")]
			public void Failure_string() {
				// Arrange
				object expected = "abc";
				object actual = "xyz";

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal(string.Empty, exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("abc", exception.Expected);
				Assert.Equal("xyz", exception.Actual);
			}

			[Fact(DisplayName = "Failure, string, case-sensitivity")]
			public void Failure_string_casesensitivity() {
				// Arrange
				object expected = "abc";
				object actual = "ABC";		// different only case

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal(string.Empty, exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("abc", exception.Expected);
				Assert.Equal("ABC", exception.Actual);
			}

			[Fact(DisplayName = "Failure, array, type")]
			public void Failure_array_type() {
				// Arrange
				object expected = new object?[] { "abc", 567.89, null, false };
				object actual = false;

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal(string.Empty, exception.Path);
				Assert.Equal(EqualJsonException.TypePoint, exception.Point);
				Assert.Equal("(compatible with JSON array)", exception.Expected);
				Assert.Equal("System.Boolean", exception.Actual);
			}

			[Fact(DisplayName = "Failure, array, count")]
			public void Failure_array_count() {
				// Arrange
				object expected = new object?[] { "abc", 567.89, null, false };
				object actual = new object?[] { "abc", 567.89, null };

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal(string.Empty, exception.Path);
				Assert.Equal(EqualJsonException.CountPoint, exception.Point);
				Assert.Equal("4", exception.Expected);
				Assert.Equal("3", exception.Actual);
			}

			[Fact(DisplayName = "Failure, array, contents")]
			public void Failure_array_contents() {
				// Arrange
				object expected = new object?[] { "abc", 567.89, null, false };
				object actual = new object?[] { "abc", 567.89, null, true };

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal("[3]", exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("False", exception.Expected);
				Assert.Equal("True", exception.Actual);
			}

			[Fact(DisplayName = "Failure, object, type")]
			public void Failure_object_type() {
				// Arrange
				object expected = new Dictionary<string, object> { { "abc", 123.45 }, { "xyz", false } };
				object actual = "string";

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal(string.Empty, exception.Path);
				Assert.Equal(EqualJsonException.TypePoint, exception.Point);
				Assert.Equal("(compatible with JSON object)", exception.Expected);
				Assert.Equal("System.String", exception.Actual);
			}

			[Fact(DisplayName = "Failure, object, contents")]
			public void Failure_object_contents() {
				// Arrange
				object expected = new Dictionary<string, object> { { "abc", 123.45 }, { "xyz", false } };
				object actual = new Dictionary<string, object> { { "xyz", false }, { "abc", 9.87 } };

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal("abc", exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("123.45", exception.Expected);
				Assert.Equal("9.87", exception.Actual);
			}

			[Fact(DisplayName = "Failure, object, missing item")]
			public void Failure_object_missingitem() {
				// Arrange
				object expected = new Dictionary<string, object> { { "abc", 123.45 }, { "xyz", false } };
				object actual = new Dictionary<string, object> { { "xyz", false } };

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal("abc", exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("123.45", exception.Expected);
				Assert.Equal("(missing)", exception.Actual);
			}

			[Fact(DisplayName = "Failure, object, unexpected item")]
			public void Failure_object_unexpecteditem() {
				// Arrange
				object expected = new Dictionary<string, object> { { "abc", 123.45 }, { "xyz", false } };
				object actual = new Dictionary<string, object?> { { "unexpected", null }, { "xyz", false }, { "abc", 123.45 } };

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal("unexpected", exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("(missing)", exception.Expected);
				Assert.Null(exception.Actual);
			}

			[Fact(DisplayName = "Failure, nested, array type")]
			public void Failure_nested_arraytype() {
				// Arrange
				object expected = new object[] {
					true,
					new Dictionary<string, object> {
						{ "abc", new object[] { 1, 2, 3 } },
						{ "xyz", false }
					}
				};
				object actual = new object[] {
					true,
					new Dictionary<string, object> {
						{ "abc", 123.45 },
						{ "xyz", false }
					}
				};

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal("[1].abc", exception.Path);
				Assert.Equal(EqualJsonException.TypePoint, exception.Point);
				Assert.Equal("(compatible with JSON array)", exception.Expected);
				Assert.Equal("System.Double", exception.Actual);
			}

			[Fact(DisplayName = "Failure, nested, array count")]
			public void Failure_nested_arraycount() {
				// Arrange
				object expected = new object[] {
					true,
					new Dictionary<string, object> {
						{ "abc", new object[] { 1, 2, 3 } },
						{ "xyz", false }
					}
				};
				object actual = new object[] {
					true,
					new Dictionary<string, object> {
						{ "abc", new object[] { 1, 2, 3, 4 } },
						{ "xyz", false }
					}
				};

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal("[1].abc", exception.Path);
				Assert.Equal(EqualJsonException.CountPoint, exception.Point);
				Assert.Equal("3", exception.Expected);
				Assert.Equal("4", exception.Actual);
			}

			[Fact(DisplayName = "Failure, nested, array contents")]
			public void Failure_nested_arraycontents() {
				// Arrange
				object expected = new object[] {
					true,
					new Dictionary<string, object> {
						{ "abc", new object[] { 1, 2, 3 } },
						{ "xyz", false }
					}
				};
				object actual = new object[] {
					true,
					new Dictionary<string, object> {
						{ "abc", new object[] { 1, 2, false } },
						{ "xyz", false }
					}
				};

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal("[1].abc[2]", exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("3", exception.Expected);
				Assert.Equal("False", exception.Actual);
			}

			[Fact(DisplayName = "Failure, nested, object type")]
			public void Failure_nested_objecttype() {
				// Arrange
				object expected = new Dictionary<string, object> {
					{ "abc", new object[] { "a", "b", "c" } },
					{ "xyz", new object[] { new Dictionary<string, object> { {"k1", true }, {"k2", false } }, false, 1.23 } }
				};
				object actual = new Dictionary<string, object> {
					{ "xyz", new object?[] { null, true, 1.23 } },
					{ "abc", new object[] { "a", "b", "c" } }
				};

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal("xyz[0]", exception.Path);
				Assert.Equal(EqualJsonException.TypePoint, exception.Point);
				Assert.Equal("(compatible with JSON object)", exception.Expected);
				Assert.Equal("null", exception.Actual);
			}

			[Fact(DisplayName = "Failure, nested, object missing item")]
			public void Failure_nested_objectmissingitem() {
				// Arrange
				object expected = new Dictionary<string, object> {
					{ "abc", new object[] { "a", "b", "c" } },
					{ "xyz", new object[] { new Dictionary<string, object> { {"k1", true }, {"k2", false } }, false, 1.23 } }
				};
				object actual = new Dictionary<string, object> {
					{ "xyz", new object[] { new Dictionary<string, object> { {"k1", true } }, false, 1.23 } },
					{ "abc", new object[] { "a", "b", "c" } }
				};

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal("xyz[0].k2", exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("False", exception.Expected);
				Assert.Equal("(missing)", exception.Actual);
			}

			[Fact(DisplayName = "Failure, nested, object unexpected item")]
			public void Failure_nested_objectunexpecteditem() {
				// Arrange
				object expected = new Dictionary<string, object> {
					{ "abc", new object[] { "a", "b", "c" } },
					{ "xyz", new object[] { new Dictionary<string, object> { {"k1", true }, {"k2", false } }, false, 1.23 } }
				};
				object actual = new Dictionary<string, object> {
					{ "xyz", new object[] { new Dictionary<string, object> { {"k1", true }, { "k2", false }, { "k3", true } }, false, 1.23 } },
					{ "abc", new object[] { "a", "b", "c" } }
				};

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal("xyz[0].k3", exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("(missing)", exception.Expected);
				Assert.Equal("True", exception.Actual);
			}

			[Fact(DisplayName = "Failure, nested, object contents")]
			public void Failure_nested_objectcontents() {
				// Arrange
				object expected = new Dictionary<string, object> {
					{ "abc", new object[] { "a", "b", "c" } },
					{ "xyz", new object[] { new Dictionary<string, object> { {"k1", true }, {"k2", false } }, false, 1.23 } }
				};
				object actual = new Dictionary<string, object> {
					{ "xyz", new object[] { new Dictionary<string, object> { {"k1", false }, { "k2", false } }, false, 1.23 } },
					{ "abc", new object[] { "a", "b", "c" } }
				};

				// Act
				EqualJsonException exception = Assert.Throws<EqualJsonException>(
					() => TestUtil.EqualJson(expected, actual)
				);

				// Arrange
				Assert.Equal("xyz[0].k1", exception.Path);
				Assert.Equal(EqualJsonException.ValuePoint, exception.Point);
				Assert.Equal("True", exception.Expected);
				Assert.Equal("False", exception.Actual);
			}
		}

		#endregion
	}
}
