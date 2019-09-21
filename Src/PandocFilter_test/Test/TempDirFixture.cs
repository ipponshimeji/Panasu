using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace PandocUtil.PandocFilter.Test {
	public class TempDirFixture: IDisposable {
		#region data

		private string tempDirPath;

		#endregion


		#region properties

		public string TempDirPath {
			get {
				string value = this.tempDirPath;
				if (value == null) {
					throw new ObjectDisposedException(null);
				}
				return value;
			}
		}

		#endregion


		#region creation

		public TempDirFixture() {
			// create a temp dir
			string path = Path.GetTempFileName();
			File.Delete(path);
			Directory.CreateDirectory(path);

			this.tempDirPath = path;
		}

		public void Dispose() {
			string path = Interlocked.Exchange(ref this.tempDirPath, null);
			if (path != null) {
				Directory.Delete(path, true);
			}
		}

		#endregion
	}
}
