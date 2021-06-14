using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace ImageProcessing.TIFF
{
    public static class JpegTiff
    {
        public static Stream Write(IEnumerable<BitmapFrame> frames, Stream stream, long quality)
        {
            return Write(frames.Select(frame => Jpeg.FromBitmapFrame(frame, quality)).ToList(), stream);
        }

        public static Stream Write(IEnumerable<Mat> frames, Stream stream, long quality)
        {
            return Write(frames.Select(frame => Jpeg.FromBitmap(frame.Bitmap, quality)).ToList(), stream);
        }

        public static Stream Write(IEnumerable<Bitmap> frames, Stream stream, long quality)
        {
            return Write(frames.Select(frame => Jpeg.FromBitmap(frame, quality)).ToList(), stream);
        }

        
        private static Stream Write(List<Jpeg> jpegs, Stream stream)
        {
            if (jpegs == null || jpegs.Count == 0 || jpegs.Any(j => j.Data.Length == 0))
                throw new ArgumentNullException("Image Data must not be null or empty");
            
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Default, true))
            {
                uint offset = 8;                //Size of header, offset to IFD
                const ushort bitsPerSample = 8;
                ushort entryCount = 14;         // entries per IFD

                /*
                 * Image file header (IFH)
                 */
                //Magic number
                if (BitConverter.IsLittleEndian)
                    writer.Write(0x002A4949);
                else
                    writer.Write(0x4D4D002A);

                //Offset to (first) IFD
                writer.Write(offset);

                /*
                 * Image file directory (IFD)
                 */
                //Write image file directories for each jpeg
                for (int i = 0; offset > 0; i++)
                {
                    var jpeg = jpegs[i];

                    uint width = jpeg.Width;
                    uint length = jpeg.Height;
                    uint xres = jpeg.HorizontalResolution;
                    uint yres = jpeg.VerticalResolution;

                    //Add lengths of entries, entry-count and next-ifd-offset
                    offset += 6 + 12 * (uint)entryCount;
                    
                    //Count of entries
                    writer.Write(entryCount);

                    //TIFF-fields / IFD-entrys:
                    //{TAG, TYPE (3 = short, 4 = long, 5 = rational), COUNT, VALUE/OFFSET}
                    uint[,] fields = new uint[,] {
                        {254, 4, 1, 0},             //NewSubfileType
                        {256, 4, 1, width},         //ImageWidth
                        {257, 4, 1, length},        //ImageLength
                        {258, 3, 3, offset},        //BitsPerSample
                        {259, 3, 1, 7},             //Compression (new JPEG)
                        {262, 3, 1, 6},             //PhotometricInterpretation (YCbCr)
                        {273, 4, 1, offset + 22},   //StripOffsets (offset IFH + entries + values of BitsPerSample & YResolution & XResolution)
                        {277, 3, 1, 3},             //SamplesPerPixel
                        {278, 4, 1, length},        //RowsPerStrip
                        {279, 4, 1, (uint)jpeg.Data.LongLength}, // StripByteCounts
                        {282, 5, 1, offset + 6},    //XResolution (offset IFH + entries + values of BitsPerSample)
                        {283, 5, 1, offset + 14},   //YResolution (offset IFH + entries + values of BitsPerSample & YResolution)
                        {284, 3, 1, 1},             //PlanarConfiguration (chunky)
                        {296, 3, 1, 2}              //ResolutionUnit
                    };

                    //Write fields
                    for (int f = 0; f < entryCount; f++)
                    {
                        writer.Write((ushort)fields[f, 0]);
                        writer.Write((ushort)fields[f, 1]);
                        writer.Write(fields[f, 2]);
                        writer.Write(fields[f, 3]);
                    }

                    //Offset of next IFD
                    if (i == jpegs.Count - 1)
                        offset = 0;
                    else
                        offset += 22 + (uint)jpeg.Data.LongLength; //Add values (of fields) length and jpeg length
                    writer.Write(offset);
                    
                    //Bits per sample
                    writer.Write(bitsPerSample);
                    writer.Write(bitsPerSample);
                    writer.Write(bitsPerSample);

                    //XResolution
                    writer.Write(xres);
                    writer.Write(1);

                    //YResolution
                    writer.Write(yres);
                    writer.Write(1);
                    
                    //Actual image Data
                    writer.Write(jpeg.Data);
                }
                writer.Close();
                return stream;
            }
        }
    }
}