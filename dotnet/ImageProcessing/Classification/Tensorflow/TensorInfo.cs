using System;
using System.Xml.Serialization;

namespace ImageProcessing.Classification.Tensorflow
{
    [XmlRoot]
    public class TensorInfo
    {
        public String name { get; set; }
        public String operation { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public int channels { get; set; }
    }
}
