using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using GalaSoft.MvvmLight;
using TensorFlow;

namespace ImageProcessing.Classification.Tensorflow
{
    public class Network : ObservableObject
    {
        private TFSession session;

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set => Set(ref isActive, value);
        }

        private NetworkInfo2 definition;
        public NetworkInfo2 Definition
        {
            get => definition;
            set
            {
                definition = value;
                RaisePropertyChanged();
            }
        }

        private String definitionFilename;
        public String DefinitionFilename
        {
            get { return definitionFilename; }
            set
            {
                definitionFilename = value;
                RaisePropertyChanged();
            }
        }

        public Network()
        {
            Definition = new NetworkInfo2();
        }

        public bool Initialise()
        {
            return Initialise(DefinitionFilename);
        }

        public bool Initialise(string definition)
        {
          
            IsActive = false;

            if (definition != null && File.Exists(definition))
            {
                var info = NetworkInfo2.Deserialize(definition);
                if (info != null)
                {
                    IsActive = TryStartNetwork(info);
                    
                }
            }
            if (IsActive)
            {
                DefinitionFilename = definition;
            }
            else
            {
                Definition = null;
                DefinitionFilename = null;
                //DebugHelper.MethodInfo("Error starting network");
            }
            return IsActive;
        }

        private bool TryStartNetwork(NetworkInfo2 def)
        {
            bool success = false;
            
            String graphFilename;
            try
            {
                graphFilename = Path.Combine(def.BaseDirectory, def.protobuf);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error starting network\n" + ex.Message);
                return success;
            }
            if (File.Exists(graphFilename))
            {
                try
                {
                    TFGraph graph = new TFGraph();
                    graph.Import(File.ReadAllBytes(graphFilename));
                    session?.CloseSession();
                    session = new TFSession(graph);
                    success = true;
                    IsActive = true;
                    Definition = def;
                }
                catch (TFException tfex)
                {
                    Debug.WriteLine("Error starting network\n" + tfex.Message);
                    IsActive = false;
                }                
            }
            return success;
        }
        
        public Probability Classify(TFTensor tensor)
        {
            //DebugHelper.MethodInfo(" ----------- ");
            String inputTensor = Definition.InputTensors[0].operation;
            String outputTensor = Definition.OutputTensors[0].operation;
            var runner = session.GetRunner();
            var graph = session.Graph;
            runner.AddInput(graph[inputTensor][0], tensor).Fetch(graph[outputTensor][0]);
            var result = runner.Run()[0];
            var classes = (float[][]) result.GetValue(jagged: true);
            var probs = classes[0];
            float maxValue = probs.Max();
            int maxIndex = probs.ToList().IndexOf(maxValue);
            return new Probability(Definition.Labels[maxIndex].code, maxIndex, maxValue);
        }

        public Probability Classify(Mat mat)
        {
            int height = Definition.InputTensors[0].height;
            int width = Definition.InputTensors[0].width;
            int channels = Definition.InputTensors[0].channels;
            //mat.ConvertTo(mat, DepthType.Cv32F);

            // Resize
            Mat resizedMat = new Mat();
            if (mat.Size.Width != width || mat.Size.Height != height)
            {
                CvInvoke.Resize(mat, resizedMat, new System.Drawing.Size(width, height));
            }
            else
            {
                resizedMat = mat.Clone();
            }

            //Reorder
            var matChannels = resizedMat.NumberOfChannels;
            if (matChannels == 3)
            {
                if (channels == 3) CvInvoke.CvtColor(resizedMat, resizedMat, ColorConversion.Bgr2Rgb);
                else CvInvoke.CvtColor(resizedMat, resizedMat, ColorConversion.Bgr2Gray);
                //if (channels == 1) CvInvoke.CvtColor(resizedMat, resizedMat, ColorConversion.Bgr2Gray);
            }
            else
            {
                if (channels == 3) CvInvoke.CvtColor(resizedMat, resizedMat, ColorConversion.Gray2Bgr);
            }

            //Pre-process
            //resizedMat.ConvertTo(resizedMat, DepthType.Cv32F);
            //resizedMat.Multiply(1.0 / 255);

            // Normalise
            // double minVal = 0, maxVal = 0;
            // Point minLoc = new Point(), maxLoc = new Point();
            // Mat gMat = new Mat();
            // if (resizedMat.NumberOfChannels == 3)
            // {
            //     CvInvoke.CvtColor(resizedMat, gMat, ColorConversion.Bgr2Gray);
            // }
            // else
            // {
            //     resizedMat.ConvertTo(gMat, DepthType.Cv32F);
            // }
            // CvInvoke.MinMaxLoc(gMat, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            // Debug.WriteLine($"Min: {minVal}, Max: {maxVal}");
            // gMat.Dispose();
            //Mat div = new Mat(tMat.Size, tMat.Depth, 1);
            //Mat sub = new Mat(tMat.Size, tMat.Depth, 1);
            //div.SetTo(new MCvScalar(maxVal-100));
            //sub.SetTo(new MCvScalar(100));
            //CvInvoke.Subtract(tMat, sub, tMat);
            //CvInvoke.Divide(tMat, div, tMat);

            var array = resizedMat.GetData();
            var buffer = new float[height * width * channels];

            //TODO change this to array copy
            Buffer.BlockCopy(array, 0, buffer, 0, array.Length * sizeof(float));

            Debug.WriteLine(buffer.Max());

            TFTensor tensor = TFTensor.FromBuffer(new TFShape(1, height, width, channels), buffer, 0, height*width*channels);

            resizedMat.Dispose();
            //mat.ConvertTo(mat, DepthType.Cv8U, alpha: 255, beta: 0);

            return Classify(tensor);
        }
    }
}
