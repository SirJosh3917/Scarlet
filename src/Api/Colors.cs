﻿using Nett;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/*
 * This is where the color parsing, and all related types, get worked on.
 */

namespace Scarlet
{
	[StructLayout(LayoutKind.Explicit)]
	public struct Rgba32
	{
		public const int Size = 4;

		// all just to circumvent System.Text.Json's inability to serialize fields

		[FieldOffset(0)] private byte _R;
		[FieldOffset(1)] private byte _B;
		[FieldOffset(2)] private byte _G;
		[FieldOffset(3)] private byte _A;

		public byte R { get => _R; set => _R = value; }
		public byte B { get => _B; set => _B = value; }
		public byte G { get => _G; set => _G = value; }
		public byte A { get => _A; set => _A = value; }

		public static Rgba32 FromARGB(uint color)
			=> new Rgba32
			{
				A = (byte)(color >> 24),
				R = (byte)((color >> 16) & 0b11111111),
				G = (byte)((color >> 8) & 0b11111111),
				B = (byte)(color & 0b11111111),
			};

		public static uint ToUInt32(ref Rgba32 rgba32)
			=> Unsafe.As<Rgba32, uint>(ref rgba32);
	}

	public class Colors
	{
		public Colors(Memory<Rgba32> colors)
		{
			Values = colors;
		}

#if DEBUG
		public Colors(Memory<bool> hadValues, Memory<Rgba32> colors)
		{
			HadValues = hadValues;
			Values = colors;
		}
#endif

#if DEBUG
		public Memory<bool> HadValues { get; }
#endif
		public Memory<Rgba32> Values { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Colors FromFile(string filePath)
		{
			var table = Toml.ReadFile(filePath);
			return FromToml(table);
		}

		public static Colors FromToml(TomlTable table)
		{
			var colorsMap = new Dictionary<int, Rgba32>();
			var hadValuesMap = new Dictionary<int, bool>();

			foreach (var (key, value) in table)
			{
				if (!int.TryParse(key, out var intKey)) throw new ArgumentException("Invalid TOML data.");
				if (!(value is TomlTable valueTable)) throw new ArgumentException("Invalid TOML data.");

				var rgba32 = new Rgba32();

				rgba32.R = valueTable.Get<byte>("r");
				rgba32.G = valueTable.Get<byte>("g");
				rgba32.B = valueTable.Get<byte>("b");
				rgba32.A = byte.MaxValue; // fully visible

				if (valueTable.TryGetValue("a", out var alphaObject)
					&& alphaObject is TomlInt alphaInt)
				{
					rgba32.A = alphaInt.Get<byte>();
				}

				if (rgba32.A == 0)
				{
					// if full transparency, set RGB to 255 because that draws fully transparent
					// pixels for some reason
					rgba32.R = byte.MaxValue;
					rgba32.G = byte.MaxValue;
					rgba32.B = byte.MaxValue;
				}

				colorsMap[intKey] = rgba32;
				hadValuesMap[intKey] = true;
			}

			var colors = new Rgba32[colorsMap.Max(x => x.Key) + 1];
			var hadValues = new bool[hadValuesMap.Max(x => x.Key) + 1];

			foreach (var (key, value) in colorsMap)
			{
				colors[key] = value;
			}

			foreach (var (key, value) in hadValuesMap)
			{
				hadValues[key] = value;
			}

#if DEBUG
			return new Colors(hadValues, colors);
#else
			return new Colors(colors);
#endif
		}
	}
}