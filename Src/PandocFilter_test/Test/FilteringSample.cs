using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace PandocUtil.PandocFilter.Test {
	public struct FilteringSample {
		#region data

		public readonly string InputFilePath;

		public readonly string AnswerFilePath;

		#endregion


		#region creation

		public FilteringSample(string inputFilePath, string answerFilePath) {
			// argument checks
			if (string.IsNullOrEmpty(inputFilePath)) {
				throw new ArgumentNullException(nameof(inputFilePath));
			}
			if (string.IsNullOrEmpty(answerFilePath)) {
				throw new ArgumentNullException(nameof(answerFilePath));
			}

			// initialize members
			this.InputFilePath = inputFilePath;
			this.AnswerFilePath = answerFilePath;
		}

		public static FilteringSample GetSample(string group, string name) {
			// argument checks
			if (string.IsNullOrEmpty(group)) {
				throw new ArgumentNullException(nameof(group));
			}
			if (string.IsNullOrEmpty(name)) {
				throw new ArgumentNullException(nameof(name));
			}

			// create a instance
			string pathSeparator = TestUtil.DirectorySeparator;
			string resourceDirPath = TestUtil.GetFilteringResourceDir(group);
			string inputFilePath = $"{resourceDirPath}{pathSeparator}Inputs{pathSeparator}{name}.json";
			string answerFilePath = $"{resourceDirPath}{pathSeparator}Answers{pathSeparator}{name}.json";
			return new FilteringSample(inputFilePath, answerFilePath);	
		}

		#endregion


		#region methods

		public Stream OpenInput() {
			return File.OpenRead(this.InputFilePath);
		}

		public string GetExpected() {
			return File.ReadAllText(this.AnswerFilePath);
		}

		public void AssertEqual(string actual) {
			Assert.Equal(GetExpected(), actual);
		}

		#endregion
	}
}
