using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Sdk;

namespace Panasu.Test {
	public class EqualJsonException: AssertActualExpectedException {
		#region constants

		public const string ValuePoint = "value";

		public const string TypePoint = "type";

		public const string CountPoint = "count";

		#endregion


		#region data

		public string Point { get; private set; }

		public string Path { get; private set; }

		#endregion


		#region constructors

		public EqualJsonException(string path, string point, object expected, object actual) : base(expected, actual, "TestUtil.EqualJson()  Failure") {
			// initialize members
			this.Path = path ?? string.Empty;
			this.Point = point;
		}

		public EqualJsonException(IEnumerable<object> path, string point, object expected, object actual) :this(GetPath(path), point, expected, actual) {
		}

		private static string GetPath(IEnumerable<object> path) {
			// argument checks
			if (path == null) {
				return string.Empty;
			}

			// build the path
			StringBuilder buf = new StringBuilder();
			foreach (object item in path) {
				if (item is int) {
					// array index
					buf.Append($"[{item}]");
				} else {
					// proeprty name
					if (0 < buf.Length) {
						buf.Append(".");
					}
					buf.Append(item);
				}
			}

			return buf.ToString();
		}

		#endregion


		#region overrides

		public override string Message {
			get {
				string getText(string value) {
					return value ?? "(null)";
				}
				string newLine = Environment.NewLine;
				string pointText = (this.Point == null) ? string.Empty : $"Point:    {this.Point}{newLine}";

				return (
					$"TestUtil.EqualJson()  Failure{newLine}" +
					$"Path:     {this.Path}{newLine}" +
					pointText +
					$"Expected: {getText(this.Expected)}{newLine}" +
					$"Actual:   {getText(this.Actual)}{newLine}"
				);
			}
		}

		#endregion
	}
}
