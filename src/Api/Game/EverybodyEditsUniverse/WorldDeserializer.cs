﻿using EEUniverse.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Scarlet.Api.Game.EverybodyEditsUniverse
{
	/// <summary>
	/// This only deserializes as much as is necessary to produce a 2d array
	/// of "important" blocks to generate a minimap from
	/// </summary>
	public static class WorldDeserializer
	{
		public static ushort[] Deserialize(Message init, int width, int height, int worldDataOffset)
		{
			var blocks = new ushort[width * height];

			var blocksIndex = 0;
			for (var i = worldDataOffset; blocksIndex < blocks.Length; i++)
			{
				if (init[i] is bool boolean)
				{
					Debug.Assert(boolean == false, "Boolean values in world deserialization represent empty blocks. The boolean should be false.");
					blocks[blocksIndex++] = 0;
				}
				else if (init[i] is int data)
				{
					// foreground and background blocks are embedded into one int
					ushort background = (ushort)((data >> 16) & 0x0000FFFF);
					ushort foreground = (ushort)(data & 0x0000FFFF);

					var primaryId = IsTransparent(foreground) ? background : foreground;
					blocks[blocksIndex++] = primaryId;

					// there may be additional data after this depending on the block
					if (IsSign(foreground))
					{
						i++; // sign text
						i++; // sign rotation
					}
					else if (IsPortal(foreground))
					{
						i++; // rotation
						i++; // id
						i++; // target
						i++; // flipped
					}
				}
				else
				{
					Debug.Assert(false, "Deserialization error.");
				}
			}

			return blocks;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static bool IsTransparent(ushort blockId)
			=> blockId == 0 // air
			|| blockId == 11 // coin
			|| (blockId >= 13 && blockId <= 17) // action blocks & god
			|| blockId == 44 // spawn
			|| IsSign(blockId)
			|| IsPortal(blockId)
			|| blockId == 70 // crown
			;

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static bool IsSign(ushort blockId)
			=> blockId >= 55 && blockId <= 58;

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static bool IsPortal(ushort blockId)
			=> blockId == 59;
	}
}
