using System;
using System.Threading;

using Common;

namespace MemoryStore
{
	class Program
	{
	
		static public int Main (string[] args) {
			string storageName = DataStore.DEFAULT_NAME;
			long storageSize = DataStore.DEFAULT_SIZE;
			if (args.Length > 0 && args[0].Length == 0) {
				storageName = args[0];
			}
			if (args.Length > 1 && long.TryParse (args[1], out long tmp) && tmp > 1024) {
				storageSize = tmp;
			}

			(double size, string unit) = bytesToUnit (storageSize);
			Console.WriteLine ("Initializing shared memory storage \"{0}\" of size {1} {2}",
				storageName,
				size,
				unit
			);
			DataStore storage;
			try {
				storage = new DataStore(storageName, storageSize, true);
			} catch (Exception e) {
				Console.WriteLine ("ERROR: Failed to initialize shared memory.");
				printException (e);
				return -1;
			}

			Console.WriteLine ("Memory storage initialized.");
			Console.WriteLine ("Running...");

			keepAlive (storage);

			Console.WriteLine ("Terminating.");
			if (!terminate (storage)) {
				return -1;
			}
			return 0;
		}

		static private void keepAlive (DataStore storage) {
			EventWaitHandle quitEvent = new EventWaitHandle(false, EventResetMode.ManualReset);

			Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs args) => {
				quitEvent.Set ();
				args.Cancel = true;
			};
			AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs args) => {
				quitEvent.Set ();
				terminate (storage);
			};

			quitEvent.WaitOne ();
		}

		static private bool terminate (DataStore storage) {
			try {
				storage.Close ();
			} catch (Exception e) {
				Console.WriteLine ("ERROR: Failed to close shared memory.");
				printException (e);
				return false;
			}
			return true;
		}

		static private (double, string) bytesToUnit (long bytes) {
			string[] units = new string [] {
				"B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB"
			};
			double size = Math.Log2 (bytes);
			return (size, units [(int)Math.Floor(size / 10)]);
		}

		static private void printException (Exception? e) {
			while (e != null) {
				Console.WriteLine (e.Message);
				e = e.InnerException;
			};
		}
	}
}
