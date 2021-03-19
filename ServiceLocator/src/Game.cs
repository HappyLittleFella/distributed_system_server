using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace ServiceLocator
{
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
	public unsafe struct Game
	{

		public const int MAX_NAME_LENGTH = 32;
		public const int MAX_IP_LENGTH = 64;

		public ulong id;

		/* Private properties */
		public long timeToLive;
		public long created;
		public long started;
		private int nameLength;
		private int ipLength;

		/* Public properties */
		public GameStatus status;
		public ushort port;
		public fixed char ip[MAX_IP_LENGTH];
		public fixed char name[MAX_NAME_LENGTH];
		
		public Game (
			ulong id,
			long timeToLive,
			long created,
			long started,
			GameStatus status,
			ushort port,
			string ip,
			string name
		) {
			if (name.Length >= MAX_NAME_LENGTH) {
				throw new ArgumentOutOfRangeException (nameof (name));
			}
			if (ip.Length >= MAX_IP_LENGTH) {
				throw new ArgumentOutOfRangeException (nameof (ip));
			}
			this.id = id;
			this.timeToLive = timeToLive;
			this.created = created;
			this.started = started;
			this.status = status;
			this.port = port;
			this.nameLength = name.Length;
			this.ipLength = ip.Length;
			for (
				int i = 0;
				i < this.nameLength;
				++i
			) {
				this.name[i] = name[i];
			}
			for (
				int i = 0;
				i < this.ipLength;
				++i
			) {
				this.ip[i] = ip[i];
			}
			;
		}

		public bool isStale (long now) {
			return
				status == GameStatus.ENDED
				|| created + timeToLive < now;
		}

		public string nameToString () {
			StringBuilder sb = new StringBuilder (Game.MAX_NAME_LENGTH);
			unsafe {
				for (
					int i = 0;
					i < nameLength;
					++i
				) {
					sb.Append (name[i]);
				}
			}
			return sb.ToString ();
		}

		public string ipToString () {
			StringBuilder sb = new StringBuilder (Game.MAX_IP_LENGTH);
			for (
				int i = 0;
				i < ipLength;
				++i
			) {
				sb.Append (ip[i]);
			}
			return sb.ToString ();
		}
	}
}
