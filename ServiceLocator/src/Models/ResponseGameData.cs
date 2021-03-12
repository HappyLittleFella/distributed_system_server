using System.Net;

namespace ServiceLocator
{
	public class ResponseGameData : GameData
	{

		public GameStatus status {
			get; set;
		}

		public string ip {
			get; set;
		}

		public ResponseGameData (Game game) {
			this.id = game.id;
			this.port = game.port;
			this.name = game.nameToString ();
			this.status = game.status;
			this.ip = game.ipToString ();
		}

	}
}
