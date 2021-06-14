using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace MicroscopeImaging.Models
{
    public class AquisitionDataflow
    {
        private ITargetBlock<(Mat, string)> block;
        
        public AquisitionDataflow()
        {

        }

        public void Start()
        {
            var block =
                new ActionBlock<(Mat, string)>(tuple =>
                {
                    Mat im = tuple.Item1;
                    string filename = tuple.Item2;
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                    CvInvoke.Imwrite(filename, im, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, 90));
                    im.Dispose();
                }, new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = 60,
                    MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded
                });
            this.block = block;
        }

        public async Task Stop()
        {
            block.Complete();
            await block.Completion;
        }

        public async Task SaveMat(Mat mat, string filename)
        {
            await block.SendAsync((mat, filename));
        }
    }
}