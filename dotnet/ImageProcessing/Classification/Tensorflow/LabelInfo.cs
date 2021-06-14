using System;
using System.Xml.Serialization;

namespace ImageProcessing.Classification.Tensorflow
{
    [XmlRoot]
    public class LabelInfo
    {
        public String code { get; set; }
        public int count { get; set; }
        public double precision { get; set; }
        public double recall { get; set; }
        public double f1score { get; set; }
        public int support { get; set; }
    }
}
