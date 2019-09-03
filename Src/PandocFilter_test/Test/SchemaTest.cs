using System;
using System.Collections.Generic;
using System.Text;
using PandocUtil.PandocFilter;
using Xunit;
using Utf8Json;

namespace PandocUtil.PandocFilter.Test {
	public class SchemaTest {
		#region tests

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

			#endregion
		}

		#endregion
	}
}
