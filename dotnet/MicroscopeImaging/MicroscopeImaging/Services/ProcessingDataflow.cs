using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Emgu.CV;
using Emgu.CV.CvEnum;
using ImageProcessing.Images;

namespace MicroscopeImaging.Services
{
    public class ProcessingDataflow
    {
        // private ITargetBlock<(Mat, string)> block;

        private int threshold = 128;
        
        public ProcessingDataflow()
        {

        }

        public void Process(Mat mat)
        {
            
        }

        // public void Start()
        // {
        //     // 1. Mask image
        //     // 2. 
        //     
        //     
        //     var block =
        //         new ActionBlock<(Mat, string)>(tuple =>
        //         {
        //             Mat im = tuple.Item1;
        //             string filename = tuple.Item2;
        //             Directory.CreateDirectory(Path.GetDirectoryName(filename));
        //             CvInvoke.Imwrite(filename, im, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, 90));
        //             im.Dispose();
        //         }, new ExecutionDataflowBlockOptions()
        //         {
        //             BoundedCapacity = 60,
        //             MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded
        //         });
        //     this.block = block;
        // }
        //
        // public async Task Stop()
        // {
        //     block.Complete();
        //     await block.Completion;
        // }
        //
        // public async Task SaveMat(Mat mat, string filename)
        // {
        //     await block.SendAsync((mat, filename));
        // }
    }
}