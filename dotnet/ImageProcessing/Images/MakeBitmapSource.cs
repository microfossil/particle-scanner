using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageProcessing.Images
{
    public class MakeBitmapSource
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);    

        public static BitmapSource FromNativePointer(IntPtr pData, int w, int h, int ch)
        {
            PixelFormat format = PixelFormats.Default;

            if (ch == 1) format = PixelFormats.Gray8; //grey scale image 0-255
            if (ch == 3) format = PixelFormats.Bgr24; //RGB
            if (ch == 4) format = PixelFormats.Bgr32; //RGB + alpha


            WriteableBitmap wbm = new WriteableBitmap(w, h, 96, 96, format, null);
            CopyMemory(wbm.BackBuffer, pData, (uint)(w * h * ch));

            wbm.Lock();
            wbm.AddDirtyRect(new Int32Rect(0, 0, wbm.PixelWidth, wbm.PixelHeight));
            wbm.Unlock();

            return wbm;
        }

        public static BitmapSource FromArray(byte[] data, int w, int h, int ch)
        {
            PixelFormat format = PixelFormats.Default;

            if (ch == 1) format = PixelFormats.Gray8; //grey scale image 0-255
            if (ch == 3) format = PixelFormats.Bgr24; //RGB
            if (ch == 4) format = PixelFormats.Bgr32; //RGB + alpha

            WriteableBitmap wbm = new WriteableBitmap(w, h, 96, 96, format, null);
            wbm.WritePixels(new Int32Rect(0, 0, w, h), data, ch * w, 0);

            return wbm;
        }
    }
}