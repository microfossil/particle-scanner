using System;
using System.Xml.Serialization;

namespace ImageProcessing.Classification.Tensorflow
{
    [XmlRoot("label")]
    public class Label
    {
        public String code { get; set; }
        public String name { get; set; }
        public String description { get; set; }
        public bool isMorphotype { get; set; }

        public Label()
        {

        }
    }
}
