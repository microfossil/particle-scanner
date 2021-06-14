using System.Collections.Generic;
using Emgu.CV;

namespace ImageProcessing.Background
{
    public class BackgroundStack
    {
        private List<Mat> stacks;
        public List<Mat> Models { get; private set; }
        private int[] currentIdx;
        private int width = 0;
        private int height = 0;
        private int numModels = 0;
        private int currentStackSize = 0;

        public Mat Stack { get; set; }
        public Mat SortedStack { get; set; }

        public BackgroundStack(int stackSize)
        {

        }
    }
}
