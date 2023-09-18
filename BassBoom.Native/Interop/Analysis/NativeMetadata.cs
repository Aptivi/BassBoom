
//   BassBoom  Copyright (C) 2023  Aptivi
// 
//   This file is part of BassBoom
// 
//   BassBoom is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
// 
//   BassBoom is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
// 
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Init;

namespace BassBoom.Native.Interop.Analysis
{
    public enum mpg123_text_encoding
    {
        mpg123_text_unknown  = 0,
        mpg123_text_utf8     = 1,
        mpg123_text_latin1   = 2,
        mpg123_text_icy      = 3,
        mpg123_text_cp1252   = 4,
        mpg123_text_utf16    = 5,
        mpg123_text_utf16bom = 6,
        mpg123_text_utf16be  = 7,
        mpg123_text_max      = 7
    }

    public enum mpg123_id3_enc
    {
        mpg123_id3_latin1   = 0,
        mpg123_id3_utf16bom = 1,
        mpg123_id3_utf16be  = 2,
        mpg123_id3_utf8     = 3,
        mpg123_id3_enc_max  = 3
    }

    public enum mpg123_id3_pic_type
    {
        mpg123_id3_pic_other          =  0,
        mpg123_id3_pic_icon           =  1,
        mpg123_id3_pic_other_icon     =  2,
        mpg123_id3_pic_front_cover    =  3,
        mpg123_id3_pic_back_cover     =  4,
        mpg123_id3_pic_leaflet        =  5,
        mpg123_id3_pic_media          =  6,
        mpg123_id3_pic_lead           =  7,
        mpg123_id3_pic_artist         =  8,
        mpg123_id3_pic_conductor      =  9,
        mpg123_id3_pic_orchestra      = 10,
        mpg123_id3_pic_composer       = 11,
        mpg123_id3_pic_lyricist       = 12,
        mpg123_id3_pic_location       = 13,
        mpg123_id3_pic_recording      = 14,
        mpg123_id3_pic_performance    = 15,
        mpg123_id3_pic_video          = 16,
        mpg123_id3_pic_fish           = 17,
        mpg123_id3_pic_illustration   = 18,
        mpg123_id3_pic_artist_logo    = 19,
        mpg123_id3_pic_publisher_logo = 20
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct mpg123_string
    {
        internal char* p;
        internal IntPtr size;
        internal IntPtr fill;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct mpg123_text
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal byte[] lang;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal byte[] id;
        internal mpg123_string description;
        internal mpg123_string text;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct mpg123_picture
    {
        internal char type;
        internal mpg123_string description;
        internal mpg123_string mime_type;
        internal IntPtr size;
        internal char* data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct mpg123_id3v1
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal char[] tag;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        internal char[] title;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        internal char[] artist;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        internal char[] album;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal char[] year;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        internal char[] comment;
        internal int genre;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct mpg123_id3v2
    {
        internal int version;
        internal mpg123_string* title;
        internal mpg123_string* artist;
        internal mpg123_string* album;
        internal mpg123_string* year;
        internal mpg123_string* genre;
        internal mpg123_string* comment;
        internal IntPtr comment_list;
        internal int comments;
        internal IntPtr text;
        internal int texts;
        internal IntPtr extra;
        internal int extras;
        internal IntPtr picture;
        internal int pictures;
    }

    /// <summary>
    /// Metadata group from mpg123
    /// </summary>
    public static unsafe class NativeMetadata
    {
        internal const int MPG123_ID3 = 0x3;
        internal const int MPG123_NEW_ID3 = 0x1;
        internal const int MPG123_ICY = 0xc;
        internal const int MPG123_NEW_ICY = 0x4;
            
        /// <summary>
        /// MPG123_EXPORT mpg123_string* mpg123_new_string(const char* val);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern mpg123_string* mpg123_new_string(string val);

        /// <summary>
        /// MPG123_EXPORT void mpg123_delete_string(mpg123_string* sb);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern void mpg123_delete_string(mpg123_string* sb);

        /// <summary>
        /// MPG123_EXPORT void mpg123_init_string(mpg123_string* sb);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern void mpg123_init_string(mpg123_string* sb);

        /// <summary>
        /// MPG123_EXPORT void mpg123_free_string(mpg123_string* sb);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern void mpg123_free_string(mpg123_string* sb);

        /// <summary>
        /// MPG123_EXPORT int mpg123_resize_string(mpg123_string* sb, size_t news);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_resize_string(mpg123_string* sb, int news);

        /// <summary>
        /// MPG123_EXPORT int mpg123_grow_string(mpg123_string* sb, size_t news);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_grow_string(mpg123_string* sb, int news);

        /// <summary>
        /// MPG123_EXPORT int mpg123_copy_string(mpg123_string* from, mpg123_string* to);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_copy_string(mpg123_string* from, mpg123_string* to);

        /// <summary>
        /// MPG123_EXPORT int mpg123_move_string(mpg123_string* from, mpg123_string* to);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_move_string(mpg123_string* from, mpg123_string* to);

        /// <summary>
        /// MPG123_EXPORT int mpg123_add_string(mpg123_string* sb, const char* stuff);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_add_string(mpg123_string* sb, string stuff);

        /// <summary>
        /// MPG123_EXPORT int mpg123_add_substring( mpg123_string *sb
        /// ,   const char *stuff, size_t from, size_t count );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_add_substring(mpg123_string* sb, string stuff, int @from, int count);

        /// <summary>
        /// MPG123_EXPORT int mpg123_set_string(mpg123_string* sb, const char* stuff);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_set_string(mpg123_string* sb, string stuff);

        /// <summary>
        /// MPG123_EXPORT int mpg123_set_substring( mpg123_string *sb
        /// ,   const char *stuff, size_t from, size_t count );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_set_substring(mpg123_string* sb, string stuff, int @from, int count);

        /// <summary>
        /// MPG123_EXPORT size_t mpg123_strlen(mpg123_string *sb, int utf8);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_strlen(mpg123_string* sb, int utf8);

        /// <summary>
        /// MPG123_EXPORT int mpg123_chomp_string(mpg123_string *sb);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_chomp_string(mpg123_string *sb);

        /// <summary>
        /// MPG123_EXPORT int mpg123_same_string(mpg123_string *a, mpg123_string *b);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_same_string(mpg123_string* a, mpg123_string* b);

        /// <summary>
        /// MPG123_EXPORT enum mpg123_text_encoding mpg123_enc_from_id3(unsigned char id3_enc_byte);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern mpg123_text_encoding mpg123_enc_from_id3(char id3_enc_byte);

        /// <summary>
        /// MPG123_EXPORT int mpg123_enc_from_id3_2(unsigned char id3_enc_byte);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_enc_from_id3_2(char id3_enc_byte);

        /// <summary>
        /// MPG123_EXPORT int mpg123_store_utf8(mpg123_string *sb, enum mpg123_text_encoding enc, const unsigned char *source, size_t source_size);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_store_utf8(mpg123_string* sb, mpg123_text_encoding enc, char* source, int source_size);

        /// <summary>
        /// MPG123_EXPORT int mpg123_store_utf8_2(mpg123_string *sb
        /// ,   int enc, const unsigned char *source, size_t source_size);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_store_utf8_2(mpg123_string* sb, int enc, char* source, int source_size);

        /// <summary>
        /// MPG123_EXPORT int mpg123_meta_check(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_meta_check(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT void mpg123_meta_free(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern void mpg123_meta_free(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_id3( mpg123_handle *mh
        /// ,   mpg123_id3v1 **v1, mpg123_id3v2 **v2 );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_id3(mpg123_handle* mh, ref IntPtr v1, ref IntPtr v2);

        /// <summary>
        /// MPG123_EXPORT int mpg123_id3_raw( mpg123_handle *mh
        /// ,   unsigned char **v1, size_t *v1_size
        /// ,   unsigned char **v2, size_t *v2_size );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_id3_raw(mpg123_handle* mh, ref IntPtr v1, ref int v1_size, ref IntPtr v2, ref int v2_size);

        /// <summary>
        /// MPG123_EXPORT int mpg123_icy(mpg123_handle *mh, char **icy_meta);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_icy(mpg123_handle* mh, ref string icy_meta);

        /// <summary>
        /// MPG123_EXPORT char* mpg123_icy2utf8(const char* icy_text);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern string mpg123_icy2utf8(string icy_text);
    }
}
