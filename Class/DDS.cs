using DdsFileTypePlus;
using PaintDotNet;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace WoTB_Voice_Mod_Creater.Class
{
    public class DDS_Format
    {
        public static Bitmap Load_To_Bitmap(string File)
        {
            return Load(File).CreateAliasedBitmap();
        }
        public static void DDS_To_PNG(string From_DDS_File, string To_PNG_File, System.Drawing.Imaging.ImageFormat Format)
        {
            System.Drawing.Bitmap bitmap = Load(From_DDS_File).CreateAliasedBitmap();
            bitmap.Save(To_PNG_File, Format);
            bitmap.Dispose();
        }
        public static void PNG_To_DDS(string From_PNG_File, string To_DDS_File, DdsFileFormat Format)
        {
            //こっちのコードもメモリリークしていますが、GCで解放されるため放置
            System.IO.FileStream fileStream = new System.IO.FileStream(To_DDS_File, System.IO.FileMode.Create);
            DdsFile.Save(fileStream, Format, DdsErrorMetric.Perceptual, BC7CompressionMode.Slow, false, false, ResamplingAlgorithm.Bilinear, Surface.CopyFromBitmap(new Bitmap(From_PNG_File)), null);
            fileStream.Close();
        }
        public static void DDS_To_DDS(string From_DDS_File, string To_DDS_File, DdsFileFormat Format)
        {
            System.IO.FileStream fileStream = new System.IO.FileStream(To_DDS_File, System.IO.FileMode.Create);
            DdsFile.Save(fileStream, Format, DdsErrorMetric.Perceptual, BC7CompressionMode.Slow, false, false, ResamplingAlgorithm.Bilinear, Load(From_DDS_File), null);
            fileStream.Close();
        }
        public static void PNG_To_PNG(string From_PNG_File, string To_PNG_File, System.Drawing.Imaging.ImageFormat Format)
        {
            Bitmap bmp = new Bitmap(From_PNG_File);
            bmp.Save(To_PNG_File, Format);
            bmp.Dispose();
        }
        public static unsafe Surface Load(string path)
        {
            byte[] buff = File.ReadAllBytes(path);
            MemoryStream ms = new MemoryStream(buff);
            DdsNative.DdsImage image = DdsNative.Load(ms);
            ms.Close();
            ms.Dispose();
            Surface surface = new Surface(image.Width, image.Height);
            for (int y = 0; y < surface.Height; ++y)
            {
                byte* src = image.GetRowAddressUnchecked(y);
                ColorBgra* dst = surface.GetRowAddressUnchecked(y);
                for (int x = 0; x < surface.Width; ++x)
                {
                    dst->R = src[0];
                    dst->G = src[1];
                    dst->B = src[2];
                    dst->A = src[3];
                    src += 4;
                    ++dst;
                }
            }
            image.Dispose();
            return surface;
        }
    }
}
//メモリリークを起こしていたため自力で修正
namespace DdsFileTypePlus
{
    internal static class DdsNative
    {
        static DdsNative()
        {
            DdsImage.Initalize();
        }
        private static Func<DDSLoadInfo, DdsImage> DdsImageFactory;
        public sealed class DdsImage : IDisposable
        {
            private DDSLoadInfo info;
            private bool disposed;
            public int Width => this.info.width;
            public int Height => this.info.height;
            private DdsImage(DDSLoadInfo info)
            {
                this.info = info;
            }
            internal static void Initalize()
            {
                DdsImageFactory = CreateImage;
            }
            private static DdsImage CreateImage(DDSLoadInfo info)
            {
                return new DdsImage(info);
            }
            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    if (IntPtr.Size == 8)
                    {
                        DdsIO_x64.FreeLoadInfo(ref this.info);
                    }
                    else
                    {
                        DdsIO_x86.FreeLoadInfo(ref this.info);
                    }
                }
            }
            public unsafe byte* GetRowAddressUnchecked(int y)
            {
                return (byte*)this.info.scan0 + (y * this.info.stride);
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public sealed class DDSSaveInfo
        {
            public int width;
            public int height;
            public int arraySize;
            public int mipLevels;
            public DdsFileFormat format;
            public DdsErrorMetric errorMetric;
            public BC7CompressionMode compressionMode;
            [MarshalAs(UnmanagedType.U1)]
            public bool cubeMap;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct DDSLoadInfo
        {
            public IntPtr scan0;
            public int width;
            public int height;
            public int stride;
        }
        private unsafe struct DDSBitmapData
        {
            public byte* scan0;
            public uint width;
            public uint height;
            public uint stride;
            public unsafe DDSBitmapData(Surface surface)
            {
                if (surface == null)
                {
                    throw new ArgumentNullException(nameof(surface));
                }
                this.scan0 = (byte*)surface.Scan0.VoidStar;
                this.width = (uint)surface.Width;
                this.height = (uint)surface.Height;
                this.stride = (uint)surface.Stride;
            }
        }
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DdsProgressCallback(UIntPtr done, UIntPtr total);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint ReadDelegate(IntPtr buffer, uint count);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint WriteDelegate(IntPtr buffer, uint count);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate long SeekDelegate(long offset, int origin);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate long GetSizeDelegate();
        [StructLayout(LayoutKind.Sequential)]
        private sealed class IOCallbacks
        {
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public ReadDelegate Read;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public WriteDelegate Write;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public SeekDelegate Seek;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public GetSizeDelegate GetSize;
        }
        private static class DdsIO_x86
        {
            private const string DllName = "DdsFileTypePlusIO_x86.dll";
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern int Load([In] IOCallbacks callbacks, [In, Out] ref DDSLoadInfo info);
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern void FreeLoadInfo([In, Out] ref DDSLoadInfo info);
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern unsafe int Save(
                [In] DDSSaveInfo input,
                [In] DDSBitmapData* bitmapData,
                [In] uint bitmapDataLength,
                [In] IOCallbacks callbacks,
                [In, MarshalAs(UnmanagedType.FunctionPtr)] DdsProgressCallback progressCallback);
        }
        private static class DdsIO_x64
        {
            private const string DllName = "DdsFileTypePlusIO_x64.dll";
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern int Load([In] IOCallbacks callbacks, [In, Out] ref DDSLoadInfo info);
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern void FreeLoadInfo([In, Out] ref DDSLoadInfo info);
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern unsafe int Save(
                [In] DDSSaveInfo input,
                [In] DDSBitmapData* bitmapData,
                [In] uint bitmapDataLength,
                [In] IOCallbacks callbacks,
                [In, MarshalAs(UnmanagedType.FunctionPtr)] DdsProgressCallback progressCallback);
        }
        private static class HResult
        {
            public const int NotSupported = unchecked((int)0x80070032); // HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED)
            public const int InvalidData = unchecked((int)0x8007000D); // HRESULT_FROM_WIN32(ERROR_INVALID_DATA)
        }
        private static bool FAILED(int hr)
        {
            return hr < 0;
        }
        public static DdsImage Load(Stream stream)
        {
            StreamIOCallbacks streamIO = new StreamIOCallbacks(stream);
            IOCallbacks callbacks = new IOCallbacks
            {
                Read = streamIO.Read,
                Write = streamIO.Write,
                Seek = streamIO.Seek,
                GetSize = streamIO.GetSize
            };
            int hr;
            DDSLoadInfo info = new DDSLoadInfo();
            if (IntPtr.Size == 8)
            {
                hr = DdsIO_x64.Load(callbacks, ref info);
            }
            else
            {
                hr = DdsIO_x86.Load(callbacks, ref info);
            }
            GC.KeepAlive(streamIO);
            GC.KeepAlive(callbacks);
            if (FAILED(hr))
            {
                switch (hr)
                {
                    case HResult.InvalidData:
                        throw new FormatException("The DDS file is invalid.");
                    case HResult.NotSupported:
                        throw new FormatException("The file is not a supported DDS format.");
                    default:
                        Marshal.ThrowExceptionForHR(hr);
                        break;
                }
            }
            return DdsImageFactory(info);
        }
        private sealed class StreamIOCallbacks
        {
            private readonly Stream stream;
            private const int MaxBufferSize = 81920;
            public StreamIOCallbacks(Stream stream)
            {
                this.stream = stream;
            }
            public uint Read(IntPtr buffer, uint count)
            {
                if (count == 0)
                {
                    return 0;
                }
                int bufferSize = (int)Math.Min(MaxBufferSize, count);
                byte[] bytes = new byte[bufferSize];
                long totalBytesRead = 0;
                long remaining = count;
                do
                {
                    int bytesRead = this.stream.Read(bytes, 0, (int)Math.Min(MaxBufferSize, remaining));
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    Marshal.Copy(bytes, 0, new IntPtr(buffer.ToInt64() + totalBytesRead), bytesRead);
                    totalBytesRead += bytesRead;
                    remaining -= bytesRead;
                } while (remaining > 0);
                return (uint)totalBytesRead;
            }
            public uint Write(IntPtr buffer, uint count)
            {
                if (count > 0)
                {
                    int bufferSize = (int)Math.Min(MaxBufferSize, count);
                    byte[] bytes = new byte[bufferSize];
                    long offset = 0;
                    long remaining = count;
                    do
                    {
                        int copySize = (int)Math.Min(MaxBufferSize, remaining);
                        Marshal.Copy(new IntPtr(buffer.ToInt64() + offset), bytes, 0, copySize);
                        this.stream.Write(bytes, 0, copySize);
                        offset += copySize;
                        remaining -= copySize;
                    } while (remaining > 0);
                }
                return count;
            }
            public long Seek(long offset, int origin)
            {
                return this.stream.Seek(offset, (SeekOrigin)origin);
            }
            public long GetSize()
            {
                return this.stream.Length;
            }
        }
    }
}