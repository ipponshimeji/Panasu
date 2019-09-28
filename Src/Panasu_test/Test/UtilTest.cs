using System;
using System.Collections.Generic;
using System.Text;
using Panasu;
using Xunit;

namespace Panasu.Test {
	public class UtilTest {
		#region tests

		public class GetOptionalValue {
			public void Existent() {
				// Arrange
				IDictionary<string, object> dictionary = new Dictionary<string, object>();
				string key = "Key";
				string defaultValue = "DefaultValue";
				string expectedValue = "Value";

				// Act
				(bool exist, string value1) = Util.GetOptionalValue<string>(dictionary, key);
				string value2 = Util.GetOptionalValue<string>(dictionary, key, defaultValue);

				// Assert
				Assert.True(exist);
				Assert.Equal(expectedValue, value1);
				Assert.Equal(expectedValue, value2);
			}
		}

		#endregion
	}
}
