using System;
using System.Collections.Generic;
using System.Text;
using Panasu;
using Xunit;
using Utf8Json;

namespace Panasu.Test {
	public class SchemaTest {
		#region utilities

		public static string GetInvalidContentsFormatMessage(string typeName, string expectedContentsType, string actualContentsType) {
			return $"Invalid AST format: The contents in a '{typeName}' element is not '{expectedContentsType}' but '{actualContentsType}'.";
		}

		#endregion


		#region tests

		public class RestoreMetadata {
			[Fact(DisplayName = "null")]
			public void Null() {
				// arrange
				string input = @"{
					""t"":""MetaInlines"",
					""c"":[
						{""t"":""Str"",""c"":""null""}
					]
				}";
				string expected = "\"null\"";	// not null but string "null"

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = null;

				// act
				object actual = Schema.RestoreMetadata(value, formatName);

				// assert
				TestUtil.EqualJson(expected, actual);
			}

			[Fact(DisplayName = "number")]
			public void Number() {
				// arrange
				string input = @"{
					""t"":""MetaInlines"",
					""c"":[
						{""t"":""Str"",""c"":""123""}
					]
				}";
				string expected = "\"123\"";   // not 123 but string "123"

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = null;

				// act
				object actual = Schema.RestoreMetadata(value, formatName);

				// assert
				TestUtil.EqualJson(expected, actual);
			}

			[Fact(DisplayName = "boolean, general")]
			public void Boolean_General() {
				// arrange
				string input = @"{
					""t"":""MetaBool"",
					""c"":false
				}";
				string expected = "false";

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = null;

				// act
				object actual = Schema.RestoreMetadata(value, formatName);

				// assert
				TestUtil.EqualJson(expected, actual);
			}

			[Fact(DisplayName = "boolean, invalid contents")]
			public void Boolean_InvalidContents() {
				// arrange
				string input = @"{
					""t"":""MetaBool"",
					""c"":123
				}";
				string expectedMessage = GetInvalidContentsFormatMessage("MetaBool", "boolean", "number");

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = null;

				// act
				FormatException actual = Assert.Throws<FormatException>(() => {
					Schema.RestoreMetadata(value, formatName);
				});

				// assert
				Assert.Equal(expectedMessage, actual.Message);
			}

			[Fact(DisplayName = "string, simple")]
			public void String_Simple() {
				// arrange
				string input = @"{
					""t"":""MetaInlines"",
					""c"":[
						{""t"":""Str"",""c"":""string""}
					]
				}";
				string expected = "\"string\"";

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = null;

				// act
				object actual = Schema.RestoreMetadata(value, formatName);

				// assert
				TestUtil.EqualJson(expected, actual);
			}

			[Fact(DisplayName = "string, complex")]
			public void String_Complex() {
				// arrange
				string input = @"{
					""t"":""MetaInlines"",
					""c"":[
						{""t"":""Str"",""c"":""string""},
						{""t"":""Space""},
						{""t"":""Str"",""c"":""with""},
						{""t"":""Space""},
						{""t"":""Str"",""c"":""spaces""}
					]
				}";
				string expected = "\"string with spaces\"";

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = null;

				// act
				object actual = Schema.RestoreMetadata(value, formatName);

				// assert
				TestUtil.EqualJson(expected, actual);
			}

			[Fact(DisplayName = "string, RawInline, target format")]
			public void String_RawInline_TargetFormat() {
				// arrange
				string input = @"{
					""t"":""MetaInlines"",
					""c"":[
						{""t"":""RawInline"",""c"":[""params"",""raw inline""]}
					]
				}";
				string expected = "\"raw inline\"";

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = "params";

				// act
				object actual = Schema.RestoreMetadata(value, formatName);

				// assert
				TestUtil.EqualJson(expected, actual);
			}

			[Fact(DisplayName = "string, RawInline, not target format")]
			public void String_RawInline_NotTargetFormat() {
				// arrange
				string input = @"{
					""t"":""MetaInlines"",
					""c"":[
						{""t"":""Str"",""c"":""abc""},
						{""t"":""RawInline"",""c"":[""params"",""raw inline""]},
						{""t"":""Str"",""c"":""def""}
					]
				}";
				string expected = "\"abcdef\"";

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = "unknown";	// not "params"

				// act
				object actual = Schema.RestoreMetadata(value, formatName);

				// assert
				TestUtil.EqualJson(expected, actual);
			}

			[Fact(DisplayName = "string, RawBlock, target format")]
			public void String_RawBlock_TargetFormat() {
				// arrange
				string input = @"{
					""t"":""MetaBlocks"",
					""c"":[
						{""t"":""RawBlock"",""c"":[""params"",""line1
line2""]}
					]
				}";
				string sourceNewLine = @"
";
				string expected = $"\"line1{sourceNewLine}line2\"";

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = "params";

				// act
				object actual = Schema.RestoreMetadata(value, formatName);

				// assert
				TestUtil.EqualJson(expected, actual);
			}

			[Fact(DisplayName = "string, RawBlock, not target format")]
			public void String_RawBlock_NotTargetFormat() {
				// arrange
				string input = @"{
					""t"":""MetaBlocks"",
					""c"":[
						{""t"":""Para"",""c"":[{""t"":""Str"",""c"":""abc""}]},
						{""t"":""RawBlock"",""c"":[""params"",""line1
line2""]},
						{""t"":""Para"",""c"":[{""t"":""Str"",""c"":""def""}]}
					]
				}";
				string expected = $"\"abc{Environment.NewLine}def{Environment.NewLine}\"";

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = "unknown";  // not "params"

				// act
				object actual = Schema.RestoreMetadata(value, formatName);

				// assert
				TestUtil.EqualJson(expected, actual);
			}

			[Fact(DisplayName = "string, invalid contents")]
			public void String_InvalidContents() {
				// arrange
				string input = @"{
					""t"":""MetaInlines"",
					""c"":true
				}";
				string expectedMessage = GetInvalidContentsFormatMessage("MetaInlines", "array", "boolean");

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = null;

				// act
				FormatException actual = Assert.Throws<FormatException>(() => {
					Schema.RestoreMetadata(value, formatName);
				});

				// assert
				Assert.Equal(expectedMessage, actual.Message);
			}

			[Fact(DisplayName = "array, general")]
			public void Array_General() {
				// arrange
				string input = @"{
					""t"":""MetaList"",
					""c"":[
						{""t"":""MetaInlines"",""c"":[{""t"":""Str"",""c"":""item-1""}]},
						{""t"":""MetaInlines"",""c"":[{""t"":""Str"",""c"":""2""}]},
						{""t"":""MetaBool"",""c"":true}
					]
				}";
				string expected = "[\"item-1\",\"2\",true]";

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = null;

				// act
				object actual = Schema.RestoreMetadata(value, formatName);

				// assert
				TestUtil.EqualJson(expected, actual);
			}

			[Fact(DisplayName = "object, general")]
			public void Object_General() {
				// arrange
				string input = @"{
					""t"":""MetaMap"",
					""c"":{
						""key-1"":{""t"":""MetaInlines"",""c"":[{""t"":""Str"",""c"":""null""}]},
						""key-2"":{""t"":""MetaBool"",""c"":false},
						""key-3"":{""t"":""MetaInlines"",""c"":[{""t"":""Str"",""c"":""item-3""}]}
					}
				}";
				string expected = "{\"key-1\":\"null\",\"key-2\":false,\"key-3\":\"item-3\"}";

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = null;

				// act
				object actual = Schema.RestoreMetadata(value, formatName);

				// assert
				TestUtil.EqualJson(expected, actual);
			}

			[Fact(DisplayName = "unknown, element")]
			public void Unknown_Element() {
				// arrange
				string input = @"{
					""t"":""Str"",
					""c"":""string""
				}";

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = null;

				// act
				FormatException actual = Assert.Throws<FormatException>(() => {
					Schema.RestoreMetadata(value, formatName);
				});

				// assert
			}

			[Fact(DisplayName = "unknown, non-element")]
			public void Unknown_NonElement() {
				// arrange
				string input = @"{
					""abc"":""def""
				}";

				object value = JsonSerializer.Deserialize<object>(input);
				string formatName = null;

				// act
				FormatException actual = Assert.Throws<FormatException>(() => {
					Schema.RestoreMetadata(value, formatName);
				});

				// assert
			}
		}

		public class GetMetadataStringValue {
			#region methods

			protected static void TestArray(string expected, string jsonString, string formatName = null) {
				// argument checks
				// expected can be null
				if (string.IsNullOrEmpty(jsonString)) {
					throw new ArgumentNullException(nameof(jsonString));
				}
				// formatName can be null

				// Arrange
				List<object> json = JsonSerializer.Deserialize<List<object>>(jsonString);

				// Act
				string actual1 = Schema.GetMetadataStringValue(json, formatName);
				string actual2 = Schema.GetMetadataStringValue((object)json, formatName);

				// Assert
				Assert.Equal(expected, actual1);
				Assert.Equal(expected, actual2);
			}

			#endregion


			#region tests

			[Fact(DisplayName = "array - empty")]
			public void Array_Empty() {
				TestArray(
					expected: string.Empty,
					jsonString: "[]"
				);
			}

			[Fact(DisplayName = "array - single null")]
			public void Array_Null() {
				TestArray(
					expected: string.Empty,
					jsonString: "[ null ]"
				);
			}

			[Fact(DisplayName = "array - single Str")]
			public void Array_Str() {
				TestArray(
					expected: "hello",
					jsonString: "[ {\"t\":\"Str\", \"c\": \"hello\"} ]"
				);
			}

			[Fact(DisplayName = "array - single Space")]
			public void Array_Space() {
				TestArray(
					expected: " ",
					jsonString: "[ {\"t\":\"Space\"} ]"
				);
			}

			[Fact(DisplayName = "array - single RawInline")]
			public void Array_RawInline() {
				// RawInline should preserve consecutive spaces
				TestArray(
					expected: "a  b   c",
					jsonString: "[ {\"t\":\"RawInline\", \"c\": [\"test\",\"a  b   c\"]} ]",
					formatName: "test"
				);
			}

			[Fact(DisplayName = "array - single RawBlock")]
			public void Array_RawBlock() {
				// RawBlock should preserve consecutive spaces
				TestArray(
					expected: "a  b\n   c",
					jsonString: "[ {\"t\":\"RawBlock\", \"c\": [\"test\",\"a  b\n   c\"]} ]",
					formatName: "test"
				);
			}

			// TODO: test cases for multi-lines

			#endregion
		}

		#endregion
	}
}
