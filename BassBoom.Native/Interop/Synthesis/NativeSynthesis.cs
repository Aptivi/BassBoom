//
// BassBoom  Copyright (C) 2023  Aptivi
//
// This file is part of Nitrocid KS
//
// BassBoom is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BassBoom is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Runtime.InteropServices;

namespace BassBoom.Native.Interop.Synthesis
{
    public enum syn123_error
    {
        SYN123_OK = 0,
        SYN123_BAD_HANDLE,
        SYN123_BAD_FMT,
        SYN123_BAD_ENC,
        SYN123_BAD_CONV,
        SYN123_BAD_SIZE,
        SYN123_BAD_BUF,
        SYN123_BAD_CHOP,
        SYN123_DOOM,
        SYN123_WEIRD,
        SYN123_BAD_FREQ,
        SYN123_BAD_SWEEP,
        SYN123_OVERFLOW,
        SYN123_NO_DATA,
        SYN123_BAD_DATA,
    }

    public enum syn123_wave_id
    {
        SYN123_WAVE_INVALID = -1,
        SYN123_WAVE_FLAT = 0,
        SYN123_WAVE_SINE,
        SYN123_WAVE_SQUARE,
        SYN123_WAVE_TRIANGLE,
        SYN123_WAVE_SAWTOOTH,
        SYN123_WAVE_GAUSS,
        SYN123_WAVE_PULSE,
        SYN123_WAVE_SHOT,
        SYN123_WAVE_LIMIT,
    }

    public enum syn123_sweep_id
    {
        SYN123_SWEEP_LIN = 0,
        SYN123_SWEEP_QUAD,
        SYN123_SWEEP_EXP,
        SYN123_SWEEP_LIMIT,
    }

    public unsafe struct syn123_handle
    { }

    /// <summary>
    /// Synthesis group from syn123
    /// </summary>
    public static unsafe class NativeSynthesis
    {
        /// <summary>
        /// const char *syn123_distversion(unsigned int *major, unsigned int *minor, unsigned int *patch);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern IntPtr syn123_distversion(ref uint major, ref uint minor, ref uint patch);

        /// <summary>
        /// unsigned int syn123_libversion(unsigned int *patch);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern uint syn123_libversion(ref uint patch);

        /// <summary>
        /// syn123_handle* syn123_new(long rate, int channels, int encoding, size_t maxbuf, int* err);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern syn123_handle syn123_new(int rate, int channels, int encoding, [MarshalAs(UnmanagedType.SysInt)] int maxbuf, ref int err);

        /// <summary>
        /// void syn123_del(syn123_handle *sh);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern void syn123_del(syn123_handle* sh);

        /// <summary>
        /// int syn123_dither(syn123_handle *sh, int dither, unsigned long *seed);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_dither(syn123_handle* sh, int dither, ref uint seed);

        /// <summary>
        /// size_t syn123_read(syn123_handle *sh, void *dst, size_t dst_bytes);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern int syn123_read(syn123_handle* sh, IntPtr dst, [MarshalAs(UnmanagedType.SysInt)] int dst_bytes);
        
        /// <summary>
        /// int syn123_setup_waves(syn123_handle* sh, size_t count, int* id, double* freq, double* phase, int* backwards, size_t *period);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_setup_waves(syn123_handle* sh, [MarshalAs(UnmanagedType.SysInt)] int count, ref int id, ref double freq, ref double phase, ref int backwards, ref int period);

        /// <summary>
        /// int syn123_query_waves(syn123_handle* sh, size_t *count, int* id, double* freq, double* phase, int* backwards, size_t *period);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_query_waves(syn123_handle* sh, ref int count, ref int id, ref double freq, ref double phase, ref int backwards, ref int period);

        /// <summary>
        /// const char* syn123_wave_name(int id);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern IntPtr syn123_wave_name(int id);

        /// <summary>
        /// int syn123_wave_id(const char *name);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_wave_id([In()][MarshalAs(UnmanagedType.LPStr)] string name);

        /// <summary>
        /// int syn123_setup_sweep(syn123_handle* sh, int wave_id, double phase, int backwards, int sweep_id, double* f1, double* f2, int smooth, size_t duration, double* endphase, size_t *period, size_t* buffer_period);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_setup_sweep(syn123_handle* sh, int wave_id, double phase, int backwards, int sweep_id, ref double f1, ref double f2, int smooth, [MarshalAs(UnmanagedType.SysInt)] int duration, ref double endphase, ref int period, ref int buffer_period);

        /// <summary>
        /// int syn123_setup_pink(syn123_handle *sh, int rows, unsigned long seed, size_t* period);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_setup_pink(syn123_handle* sh, int rows, uint seed, ref int period);

        /// <summary>
        /// int syn123_setup_white(syn123_handle *sh, unsigned long seed, size_t *period);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_setup_white(syn123_handle* sh, uint seed, ref int period);

        /// <summary>
        /// int syn123_setup_geiger(syn123_handle *sh, double activity, unsigned long seed, size_t *period);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_setup_geiger(syn123_handle* sh, double activity, uint seed, ref int period);

        /// <summary>
        /// int syn123_setup_silence(syn123_handle *sh);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_setup_silence(syn123_handle* sh);
        
        /// <summary>
        /// int syn123_conv(void * dst, int dst_enc, size_t dst_size, void* src, int src_enc, size_t src_bytes, size_t* dst_bytes, size_t *clipped, syn123_handle* sh);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_conv(IntPtr dst, int dst_enc, [MarshalAs(UnmanagedType.SysInt)] int dst_size, IntPtr src, int src_enc, [MarshalAs(UnmanagedType.SysInt)] int src_bytes, ref int dst_bytes, ref int clipped, syn123_handle* sh);

        /// <summary>
        /// double syn123_db2lin(double db);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern double syn123_db2lin(double db);

        /// <summary>
        /// double syn123_lin2db(double volume);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern double syn123_lin2db(double volume);

        /// <summary>
        /// int syn123_amp(void* buf, int encoding, size_t samples, double volume, double offset, size_t *clipped, syn123_handle* sh);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_amp(IntPtr buf, int encoding, [MarshalAs(UnmanagedType.SysInt)] int samples, double volume, double offset, ref int clipped, syn123_handle* sh);

        /// <summary>
        /// size_t syn123_clip(void *buf, int encoding, size_t samples);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern int syn123_clip(IntPtr buf, int encoding, [MarshalAs(UnmanagedType.SysInt)] int samples);

        /// <summary>
        /// size_t syn123_soft_clip(void *buf, int encoding, size_t samples, double limit, double width, syn123_handle *sh);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern int syn123_soft_clip(IntPtr buf, int encoding, [MarshalAs(UnmanagedType.SysInt)] int samples, double limit, double width, syn123_handle* sh);

        /// <summary>
        /// void syn123_interleave(void * MPG123_RESTRICT dst, void** MPG123_RESTRICT src, int channels, size_t samplesize, size_t samplecount);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern void syn123_interleave(IntPtr dst, ref IntPtr src, int channels, [MarshalAs(UnmanagedType.SysInt)] int samplesize, [MarshalAs(UnmanagedType.SysInt)] int samplecount);

        /// <summary>
        /// void syn123_deinterleave(void ** MPG123_RESTRICT dst, void * MPG123_RESTRICT src, int channels, size_t samplesize, size_t samplecount);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern void syn123_deinterleave(ref IntPtr dst, IntPtr src, int channels, [MarshalAs(UnmanagedType.SysInt)] int samplesize, [MarshalAs(UnmanagedType.SysInt)] int samplecount);

        /// <summary>
        /// void syn123_mono2many(void * MPG123_RESTRICT dst, void * MPG123_RESTRICT src, int channels, size_t samplesize, size_t samplecount);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern void syn123_mono2many(IntPtr dst, IntPtr src, int channels, [MarshalAs(UnmanagedType.SysInt)] int samplesize, [MarshalAs(UnmanagedType.SysInt)] int samplecount);

        /// <summary>
        /// int syn123_mixenc(int src_enc, int dst_enc);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_mixenc(int src_enc, int dst_enc);

        /// <summary>
        /// int syn123_mix(void * MPG123_RESTRICT dst, int dst_enc, int dst_channels, void* MPG123_RESTRICT src, int src_enc, int src_channels, const double* mixmatrix, size_t samples, int silence, size_t *clipped, syn123_handle* sh);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_mix(IntPtr dst, int dst_enc, int dst_channels, IntPtr src, int src_enc, int src_channels, ref double mixmatrix, [MarshalAs(UnmanagedType.SysInt)] int samples, int silence, ref int clipped, syn123_handle* sh);

        /// <summary>
        /// int syn123_setup_filter(syn123_handle *sh, int append, unsigned int order, double* b, double* a, int mixenc, int channels, int init_firstval);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_setup_filter(syn123_handle* sh, int append, uint order, ref double b, ref double a, int mixenc, int channels, int init_firstval);

        /// <summary>
        /// int syn123_query_filter(syn123_handle *sh, size_t position, size_t* count, unsigned int* order, double* b, double* a, int* mixenc, int* channels, int* init_firstval);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_query_filter(syn123_handle* sh, [MarshalAs(UnmanagedType.SysInt)] int position, ref int count, ref uint order, ref double b, ref double a, ref int mixenc, ref int channels, ref int init_firstval);

        /// <summary>
        /// void syn123_drop_filter(syn123_handle *sh, size_t count);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern void syn123_drop_filter(syn123_handle* sh, [MarshalAs(UnmanagedType.SysInt)] int count);

        /// <summary>
        /// int syn123_filter(syn123_handle *sh, void* buf, int encoding, size_t samples);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_filter(syn123_handle* sh, IntPtr buf, int encoding, [MarshalAs(UnmanagedType.SysInt)] int samples);

        /// <summary>
        /// int syn123_setup_resample(syn123_handle *sh, long inrate, long outrate, int channels, int dirty, int smooth);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_setup_resample(syn123_handle* sh, int inrate, int outrate, int channels, int dirty, int smooth);

        /// <summary>
        /// long syn123_resample_maxrate(void);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_resample_maxrate();

        /// <summary>
        /// size_t syn123_resample_count(long inrate, long outrate, size_t ins);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern int syn123_resample_count(int inrate, int outrate, [MarshalAs(UnmanagedType.SysInt)] int ins);

        /// <summary>
        /// size_t syn123_resample_history(long inrate, long outrate, int dirty);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern int syn123_resample_history(int inrate, int outrate, int dirty);

        /// <summary>
        /// size_t syn123_resample_incount(long input_rate, long output_rate, size_t outs);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern int syn123_resample_incount(int input_rate, int output_rate, [MarshalAs(UnmanagedType.SysInt)] int outs);

        /// <summary>
        /// size_t syn123_resample_fillcount(long input_rate, long output_rate, size_t outs);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern int syn123_resample_fillcount(int input_rate, int output_rate, [MarshalAs(UnmanagedType.SysInt)] int outs);

        /// <summary>
        /// size_t syn123_resample_maxincount(long input_rate, long output_rate);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern int syn123_resample_maxincount(int input_rate, int output_rate);

        /// <summary>
        /// size_t syn123_resample_out(syn123_handle *sh, size_t ins, int *err);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern int syn123_resample_out(syn123_handle* sh, [MarshalAs(UnmanagedType.SysInt)] int ins, ref int err);

        /// <summary>
        /// size_t syn123_resample_in(syn123_handle *sh, size_t outs, int *err);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern int syn123_resample_in(syn123_handle* sh, [MarshalAs(UnmanagedType.SysInt)] int outs, ref int err);

        /// <summary>
        /// int64_t syn123_resample_total64(long inrate, long outrate, int64_t ins);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_resample_total64(int inrate, int outrate, int ins);

        /// <summary>
        /// int64_t syn123_resample_intotal64(long inrate, long outrate, int64_t outs);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_resample_intotal64(int inrate, int outrate, int outs);

        /// <summary>
        /// size_t syn123_resample(syn123_handle *sh, float* MPG123_RESTRICT dst, float* MPG123_RESTRICT src, size_t samples);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern int syn123_resample(syn123_handle* sh, ref float dst, ref float src, [MarshalAs(UnmanagedType.SysInt)] int samples);

        /// <summary>
        /// void syn123_swap_bytes(void* buf, size_t samplesize, size_t samplecount);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern void syn123_swap_bytes(IntPtr buf, [MarshalAs(UnmanagedType.SysInt)] int samplesize, [MarshalAs(UnmanagedType.SysInt)] int samplecount);

        /// <summary>
        /// void syn123_host2le(void *buf, size_t samplesize, size_t samplecount);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern void syn123_host2le(IntPtr buf, [MarshalAs(UnmanagedType.SysInt)] int samplesize, [MarshalAs(UnmanagedType.SysInt)] int samplecount);

        /// <summary>
        /// void syn123_host2be(void *buf, size_t samplesize, size_t samplecount);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern void syn123_host2be(IntPtr buf, [MarshalAs(UnmanagedType.SysInt)] int samplesize, [MarshalAs(UnmanagedType.SysInt)] int samplecount);

        /// <summary>
        /// void syn123_le2host(void *buf, size_t samplesize, size_t samplecount);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern void syn123_le2host(IntPtr buf, [MarshalAs(UnmanagedType.SysInt)] int samplesize, [MarshalAs(UnmanagedType.SysInt)] int samplecount);

        /// <summary>
        /// void syn123_be2host(void *buf, size_t samplesize, size_t samplecount);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern void syn123_be2host(IntPtr buf, [MarshalAs(UnmanagedType.SysInt)] int samplesize, [MarshalAs(UnmanagedType.SysInt)] int samplecount);

        /// <summary>
        /// off_t syn123_resample_total(long inrate, long outrate, off_t ins);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_resample_total(int inrate, int outrate, int ins);

        /// <summary>
        /// off_t syn123_resample_intotal(long inrate, long outrate, off_t outs);
        /// </summary>
        [DllImport(LibraryTools.LibraryNameSyn, CharSet = CharSet.Ansi)]
        public static extern int syn123_resample_intotal(int inrate, int outrate, int outs);
    }
}
