using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Common
{
	public class DataStore : SharedMemory.BufferWithLocks
	{
		
		public const string DEFAULT_NAME = "shared_memory_storage";
		public const long DEFAULT_SIZE = 128 * 1024 * 1024;

		private int index = 0;
		private uint elementSize = 0;

		private object appendLock = new object ();
		private object newIdLock = new object ();

		public DataStore (string name, long bufferSize, bool ownsSharedMemory)
			: base (name, bufferSize, ownsSharedMemory) {
			Open ();
		}

		static public DataStore NewClient (string name) {
			return new DataStore (name, 0, false);
		}

		public void Write<T> (ref T data, uint index) where T : struct {
			base.Write<T> (ref data, getElementPosition<T> (index));
		}

		public void Append<T> (ref T data) where T : struct {
			lock (appendLock) {
				uint appendIndex = getElementCount ();
				base.Write<T> (ref data, getElementPosition<T> (appendIndex));
				setElementCount (appendIndex + 1);
			}
		}

		public void Read<T> (out T data, uint index) where T : struct {
			base.Read<T> (out data, getElementPosition<T> (index));
		}

		public List<T> ReadAll<T> () where T : struct {
			uint count = getElementCount ();
			List<T> elements = new List<T> ();
			for (uint i = 0; i < count; ++i) {
				Read<T> (out T element, i);
				elements.Add (element);
			}
			return elements;
		}

		private uint ElementSize<T> () {
			if (elementSize == 0) {
				elementSize = (uint)Marshal.SizeOf (typeof (T));
			}
			return elementSize;
		}

		private uint getElementPosition<T> (uint index) {
			return index * ElementSize<T> () + sizeof (uint) + sizeof (ulong);
		}

		public uint getElementCount () {
			base.Read<uint> (out uint count, 0);
			return count;
		}

		private void setElementCount (uint count) {
			base.Write<uint> (ref count, 0);
		}

		public ulong getIdCounter () {
			base.Read<ulong> (out ulong id, sizeof (uint));
			return id;
		}

		public ulong getNewId () {
			ulong id;
			lock (newIdLock) {
				base.Read<ulong> (out id, sizeof (uint));
				++id;
				setIdCounter (id);
			}
			return id;
		}

		private void setIdCounter (ulong id) {
			base.Write<ulong> (ref id, sizeof (uint));
		}

	}
}
