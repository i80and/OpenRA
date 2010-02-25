﻿using System;
using System.Drawing;
using System.Linq;
using OpenRa.FileFormats;
using Tao.FreeType;
using System.Runtime.InteropServices;

namespace OpenRa.Graphics
{
	class SpriteFont
	{
		int size;
		public SpriteFont(Renderer r, string name, int size)
		{
			this.size = size;

			if (0 != FT.FT_New_Face(library, name, 0, out face))
				throw new InvalidOperationException("FT_New_Face failed");

			FT.FT_Set_Pixel_Sizes(face, 0, (uint)size);
			glyphs = new Cache<char, GlyphInfo>(CreateGlyph);

			// setup a 1-channel SheetBuilder for our private use
			if (builder == null) builder = new SheetBuilder(r, TextureChannel.Alpha);

			// precache glyphs for U+0020 - U+007f
			for (var n = (char)0x20; n < (char)0x7f; n++)
				if (glyphs[n] == null)
					throw new InvalidOperationException();
		}

		public void DrawText(SpriteRenderer r, string text, float2 location, Color c)
		{
			location.Y += size;	// baseline vs top

			var p = location;
			foreach (var s in text)
			{
				if (s == '\n')
				{
					location.Y += size;
					p = location;
					continue;
				}

				var g = glyphs[s];
				r.DrawSprite(g.Sprite, p + g.Offset, "chrome");
				p.X += g.Advance;
			}

			r.Flush();
		}

		public int2 Measure(string text)
		{
			return new int2((int)text.Split( '\n' ).Max( s => s.Sum(a => glyphs[a].Advance)), size);
		}

		Cache<char, GlyphInfo> glyphs;
		IntPtr face;

		GlyphInfo CreateGlyph(char c)
		{
			var index = FT.FT_Get_Char_Index(face, (uint)c);
			FT.FT_Load_Glyph(face, index, FT.FT_LOAD_RENDER);

			var _face = (FT_FaceRec)Marshal.PtrToStructure(face, typeof(FT_FaceRec));
			var _glyph = (FT_GlyphSlotRec)Marshal.PtrToStructure(_face.glyph, typeof(FT_GlyphSlotRec));

			var s = builder.Allocate(new Size(_glyph.metrics.width >> 6, _glyph.metrics.height >> 6));

			var g = new GlyphInfo
			{
				Sprite = s,
				Advance = _glyph.metrics.horiAdvance >> 6,
				Offset = { X = -_glyph.bitmap_left, Y = -_glyph.bitmap_top }
			};

			// todo: sensible blit, rather than just `white box`
			for (var j = 0; j < s.size.Y; j++)
				for (var i = 0; i < s.size.X; i++)
					s.sheet.Bitmap.SetPixel(i + s.bounds.Left, j + s.bounds.Top, Color.White);

			s.sheet.Texture.SetData(s.sheet.Bitmap);
			return g;
		}

		static SpriteFont()
		{
			FT.FT_Init_FreeType(out library);
		}

		static IntPtr library;
		static SheetBuilder builder;
	}

	class GlyphInfo
	{
		public float Advance;
		public int2 Offset;
		public Sprite Sprite;
	}
}
