using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Net;
using System.Collections.Generic;
using Common;
using System;

namespace ServiceLocator.Controllers
{
	[ApiController]
	[Route ("games")]
	public class GamesController : ControllerBase
	{

		const long DEFAULT_GAME_TIME_TO_LIVE = 300; // Seconds

		private readonly ILogger<GamesController> _logger;
		private readonly GameDataConstraints constraints;

		private readonly object newGameLock = new object ();
		private readonly object updateGameLock = new object ();

		public GamesController (ILogger<GamesController> logger) {
			_logger = logger;
			constraints = new GameDataConstraints (Game.MAX_NAME_LENGTH, 4, new HashSet<IPAddress> ());
		}

		[HttpGet]
		public IActionResult Get () {
			DataStore store = MemoryStorage.get ().store;
			List<Game> games = store.ReadAll<Game> ();
			List<ResponseGameData> response = new List<ResponseGameData> (games.Count);
			long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds ();
			foreach (Game game in games) {
				if (!game.isStale (now)) {
					response.Add (new ResponseGameData (game));
				}
			}
			return Ok(response);
		}

		[HttpPost]
		public IActionResult Post (GameData game) {
			// Register game
			IPAddress? ip = Request.HttpContext.Connection.RemoteIpAddress;
			if (ip == null) {
				return BadRequest ();
			}
			Errors? err = createConstraintsSatisfied (game, ip);
			if (err != null) {
				return BadRequest ();
			}

			Game g;
			try {
				g = storeGame (game, ip);
			} catch (Exception e) {
				return StatusCode (500);
			}
			return Ok (new ResponseGameData (g));
		}

		[HttpPut]
		public IActionResult Put (GameData game) {
			IPAddress? ip = Request.HttpContext.Connection.RemoteIpAddress;
			if (ip == null) {
				return BadRequest ();
			}
			Errors? err = updateConstraintsSatisfied (game);
			if (err != null) {
				return BadRequest ();
			}

			try {
				startGame (game, ip);
			} catch (Exception e) {
				return StatusCode (500);
			}
			return Ok ();
		}

		[HttpDelete]
		public IActionResult Delete (GameData game) {
			// Deregister game
			IPAddress? ip = Request.HttpContext.Connection.RemoteIpAddress;
			if (ip == null) {
				return BadRequest ();
			}
			Errors? err = deleteConstraintsSatisfied (game);
			if (err != null) {
				return BadRequest ();
			}
			try {
				deleteGame (game, ip);
			} catch {
				return StatusCode (500);
			}
			return Ok ();
		}

		private Errors? createConstraintsSatisfied (GameData game, IPAddress ip) {
			Errors? err = null;
			int length = 0;
			if (game.name == null) {
				Error e = new Error("Name missing.");
				(err ??= new Errors ()).addError (e);
			} else {
				length = game.name.Length;
				if (length < constraints.nameMinLength || constraints.nameMaxLength < length) {
					Error e = new Error(string.Format("Bad name length. Must be {0} - {1} characters long.", constraints.nameMinLength, constraints.nameMaxLength));
					(err ??= new Errors ()).addError (e);
				}
			}
			if (game.port <= 0) {
				Error e = new Error("Invalid game port.");
				(err ??= new Errors ()).addError (e);
			}
			if (constraints.ipBlacklist.Contains(ip)) {
				Error e = new Error ("Access denied.");
				(err ??= new Errors ()).addError (e);
			}
			return err;
		}

		private Errors? updateConstraintsSatisfied (GameData game) {
			Errors? err = null;
			if (game.id == 0) {
				Error e = new Error ("Invalid game id.");
				(err ??= new Errors ()).addError (e);
			}
			return err;
		}

		private Errors? deleteConstraintsSatisfied (GameData game) {
			Errors? err = null;
			if (game.id == 0) {
				Error e = new Error ("Invalid game id.");
				(err ??= new Errors ()).addError (e);
			}
			return err;
		}

		private Game storeGame (GameData game, IPAddress ip) {
			long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds ();
			DataStore store = MemoryStorage.get ().store;
			uint storageIndex = 0;
			uint i;
			Game tmp, g;

			lock (newGameLock) {
				uint count = store.getElementCount ();
				ulong id = store.getNewId ();
				game.id = id;
				g = new Game (
					id,
					DEFAULT_GAME_TIME_TO_LIVE,
					DateTimeOffset.UtcNow.ToUnixTimeSeconds (),
					0,
					GameStatus.OPEN,
					game.port,
					ip.ToString(),
					game.name
				);

				for (i = 0; i < count; ++i) {
					store.Read<Game> (out tmp, i);
					if (tmp.isStale (now)) {
						storageIndex = i;
						break;
					}
				}
				if (i == count) {
					store.Append<Game> (ref g);
				} else {
					store.Write<Game> (ref g, storageIndex);
				}
			}
			return g;
		}

		private void startGame (GameData game, IPAddress ip) {
			long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds ();
			DataStore store = MemoryStorage.get ().store;
			
			if (!findRequestGame (game, ip, out Game g, out uint index) || g.isStale (now)) {
				return;
			}

			if (g.status != GameStatus.OPEN) {
				return;
			}

			// Update game data
			g.status = GameStatus.RUNNING;
			g.started = now;

			// Store game status
			store.Write<Game> (ref g, index);
			return;
		}

		private void deleteGame (GameData game, IPAddress ip) {
			long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds ();
			DataStore store = MemoryStorage.get ().store;

			if (!findRequestGame (game, ip, out Game g, out uint index) || g.isStale (now)) {
				return;
			}

			// Update game data
			g.status = GameStatus.ENDED;

			// Store game status
			store.Write<Game> (ref g, index);
		}

		private bool findRequestGame (GameData game, IPAddress ip, out Game g, out uint index) {
			ulong gameId = game.id;
			string ipAddress = ip.ToString ();
			DataStore store = MemoryStorage.get ().store;
			Game tmp = default;
			uint count = store.getElementCount ();

			// Find correct game
			for (uint i = 0; i < count; ++i) {
				store.Read<Game> (out tmp, i);
				if (
					tmp.id == gameId
					&& tmp.ipToString () == ipAddress
					&& tmp.port == game.port
				) {
					g = tmp;
					index = i;
					return true;
				}
			}
			g = default;
			index = 0;
			return false;
		}

	}
}
