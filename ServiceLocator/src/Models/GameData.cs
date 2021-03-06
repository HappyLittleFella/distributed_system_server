using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

namespace ServiceLocator
{

	public class GameData
	{

		public ulong id { get; set; }

		public ushort port { get; set; }

		public string name { get; set; }

		public GameData () {
			name = "";
		}

		public GameData (
			ushort port,
			string name
		) {
			this.port = port;
			this.name = name;
		}

	}
}
