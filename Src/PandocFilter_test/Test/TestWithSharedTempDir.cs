using System;
using Xunit;

namespace Panasu.Test {
	public class TestWithSharedTempDir: IClassFixture<TempDirFixture> {
		#region data

		private readonly TempDirFixture fixture;

		#endregion


		#region properties

		protected string TempDirPath {
			get {
				return this.fixture.TempDirPath;
			}
		}

		#endregion


		#region constructors

		public TestWithSharedTempDir(TempDirFixture fixture) {
			// initialize members
			this.fixture = fixture;
		}

		#endregion
	}
}
