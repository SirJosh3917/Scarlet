﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Scarlet.Api.Game.EverybodyEditsUniverse
{
	public class ScarletGameApi : IScarletGameApi
	{
		private readonly ClientProvider _clientProvider;
		private readonly Colors _colors;

		public ScarletGameApi(ClientProvider clientProvider, Colors everybodyEditsUniverseColors)
		{
			_clientProvider = clientProvider;
			_colors = everybodyEditsUniverseColors;
		}

		public async ValueTask<ScarletGameWorld> World(string worldId)
		{
			// we can be pretty positive that the same world won't get
			// requested twice in a row, because this "World" function should
			// only run when the FileCache is trying to get some data that
			// doesn't quite yet exist.
			var client = await _clientProvider.Obtain().ConfigureAwait(false);
			var world = await client.DownloadWorld(worldId).ConfigureAwait(false);

			// TODO: if there is a background color, copy the colors and modify
			// the first color (0) to be the background color.

			var result = new ScarletGameWorld
			{
				SerializeRequest = new PngSerializer.SerializeWorldRequest
				{
					Width = world.Width,
					Height = world.Height,
					Blocks = world.WorldData,
					Palette = _colors.Values
				},
				Metadata = JsonSerializer.SerializeToUtf8Bytes(world)
			};

			return result;
		}
	}
}