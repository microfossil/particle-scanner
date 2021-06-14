using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;

namespace ImageProcessing.Classification.Tensorflow
{
    [XmlRoot("network")]
    public class NetworkInfo : ObservableObject
    {
        public int width;
        public int height;
        public int channels;
        public int range;
        public String input;
        public String output;
        public String vector;
        public string filename;

        [XmlArray("labels")]
        [XmlArrayItem(ElementName = "label")]
        public ObservableCollection<Label> Labels { get; set; }

        [XmlIgnore]
        public String BaseDirectory { get; private set; }

        [XmlIgnore]
        public string definitionFilename;
        public String DefinitionFilename
        {
            get => definitionFilename;
            set => Set(ref definitionFilename, value);
        }

        public NetworkInfo()
        {
            Labels = new ObservableCollection<Label>();
        }

        public void Serialize(string filename)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            XmlSerializer ser = new XmlSerializer(typeof(NetworkInfo));
            using (TextWriter writer = new StreamWriter(filename))
            {
                ser.Serialize(writer, this);
            }
        }

        public void Serialize()
        {
            XmlSerializer ser = new XmlSerializer(typeof(NetworkInfo));
            StringBuilder xml = new StringBuilder();
            using (TextWriter writer = new StringWriter(xml))
            {
                ser.Serialize(writer, this);
            }
            Debug.WriteLine("XML: {0}", (object)xml.ToString());
        }

        public static NetworkInfo Deserialize(string filename)
        {
            NetworkInfo definition = null;
            if (File.Exists(filename))
            {
                XmlSerializer ser = new XmlSerializer(typeof(NetworkInfo));
                using (TextReader reader = new StreamReader(filename))
                {
                    definition = (NetworkInfo)ser.Deserialize(reader);
                    definition.DefinitionFilename = filename;
                    definition.BaseDirectory = Path.GetDirectoryName(filename);
                }
            }
            return definition;
        }
    }
}
