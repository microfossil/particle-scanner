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
    public class NetworkInfo2 : ObservableObject
    {
        public String name { get; set; }
        public String description { get; set; }
        public String type { get; set; }
        public String protobuf { get; set; }
        public double accuracy { get; set; }
        public double precision { get; set; }
        public double recall { get; set; }
        public double f1score { get; set; }

        [XmlArray("labels")]
        [XmlArrayItem(ElementName = "label")]
        public ObservableCollection<LabelInfo> Labels { get; set; }

        [XmlArray("inputs")]
        [XmlArrayItem(ElementName = "input")]
        public ObservableCollection<TensorInfo> InputTensors { get; set; }

        [XmlArray("outputs")]
        [XmlArrayItem(ElementName = "output")]
        public ObservableCollection<TensorInfo> OutputTensors { get; set; }

        [XmlIgnore]
        public String BaseDirectory { get; private set; }

        [XmlIgnore]
        public string definitionFilename;
        public String DefinitionFilename
        {
            get => definitionFilename;
            set => Set(ref definitionFilename, value);
        }

        public void Serialize(string filename)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            XmlSerializer ser = new XmlSerializer(typeof(NetworkInfo2));
            using (TextWriter writer = new StreamWriter(filename))
            {
                ser.Serialize(writer, this);
            }
        }

        public void Serialize()
        {
            XmlSerializer ser = new XmlSerializer(typeof(NetworkInfo2));
            StringBuilder xml = new StringBuilder();
            using (TextWriter writer = new StringWriter(xml))
            {
                ser.Serialize(writer, this);
            }
            Debug.WriteLine("XML: {0}", (object)xml.ToString());
        }

        public static NetworkInfo2 Deserialize(string filename)
        {
            NetworkInfo2 definition = null;
            if (File.Exists(filename))
            {
                XmlSerializer ser = new XmlSerializer(typeof(NetworkInfo2));
                using (TextReader reader = new StreamReader(filename))
                {
                    definition = (NetworkInfo2)ser.Deserialize(reader);
                    definition.DefinitionFilename = filename;
                    definition.BaseDirectory = Path.GetDirectoryName(filename);
                }
            }
            return definition;
        }
    }

    /*

    <network version = "2.1" >
  < name > endless_forams </ name >
  < description > endless_forams: base_cyclicresnet_cyclic model trained on data from C:\Users\rossm\Documents\Data\Foraminifera\EndlessForams\border_removed\endless_forams_20190914_165343(27672 images in 30 classes).
Accuracy: 89.2 (P: 83.2, R: 79.6, F1 80.7)</description>
  <type>base_cyclicresnet_cyclic</type>
  <date>2019-10-22_071445</date>
  <protobuf>frozen_model.pb</protobuf>
  <params>
    <name>endless_forams</name>
    <description>None</description>
    <type>base_cyclicresnet_cyclic</type>
    <filters>4</filters>
    <use_batch_norm>True</use_batch_norm>
    <global_pooling>None</global_pooling>
    <activation>relu</activation>
    <img_height>224</img_height>
    <img_width>224</img_width>
    <img_channels>1</img_channels>
    <batch_size>64</batch_size>
    <max_epochs>10000</max_epochs>
    <alr_epochs>10</alr_epochs>
    <alr_drops>4</alr_drops>
    <input_source>C:\Users\rossm\Documents\Data\Foraminifera\EndlessForams\border_removed\endless_forams_20190914_165343</input_source>
    <output_dir>experiment_v3/</output_dir>
    <data_min_count>40</data_min_count>
    <data_split>0.2</data_split>
    <data_split_offset>0.0</data_split_offset>
    <data_map_others>False</data_map_others>
    <seed>0</seed>
    <use_class_weights>True</use_class_weights>
    <class_mapping>None</class_mapping>
    <delete_mmap_files>False</delete_mmap_files>
    <mmap_directory>C:\Users\rossm\Documents\Data</mmap_directory>
    <save_model>None</save_model>
    <save_mislabeled>False</save_mislabeled>
    <use_augmentation>True</use_augmentation>
    <aug_rotation>True</aug_rotation>
    <aug_gain>[0.8, 1, 1.2]</aug_gain>
    <aug_gamma>[0.5, 1, 2]</aug_gamma>
    <aug_bias>None</aug_bias>
    <aug_zoom>[0.9, 1, 1.1]</aug_zoom>
    <aug_gaussian_noise>None</aug_gaussian_noise>
    <class_weights>[10.79144659  3.24554785  0.20832908  0.85139618  3.4811118   0.46665715
  0.20486847  0.68084837  1.58116434  0.07298916  0.143551    5.67970873
  1.50403437  1.76908961  0.28066181  0.39601639  1.30017429  0.39820836
  2.49513216 10.03855497  1.8136885   1.92704403  0.49388772  0.21222117
  0.18973972  1.34893082  0.81138696  1.93568549  9.3838666   1.42461341]</class_weights>
    <num_classes>30</num_classes>
  </params>
  <inputs>
    <input>
      <name>image</name>
      <operation>input_1</operation>
      <height>224</height>
      <width>224</width>
      <channels>1</channels>
    </input>
  </inputs>
  <outputs>
    <output>
      <name>pred</name>
      <operation>dense_1/Softmax</operation>
      <height>30</height>
      <width>0</width>
      <channels>0</channels>
    </output>
    <output>
      <name>vector</name>
      <operation>dense/Relu</operation>
      <height>512</height>
      <width>0</width>
      <channels>0</channels>
    </output>
  </outputs>
  <source_data>C:\Users\rossm\Documents\Data\Foraminifera\EndlessForams\border_removed\endless_forams_20190914_165343</source_data>
  <source_size>27672</source_size>
  <labels>
    <label>
      <code>Beella_digitata</code>
      <count>40</count>
      <precision>0.8</precision>
      <recall>0.6666666666666666</recall>
      <f1score>0.7272727272727272</f1score>
      <support>6</support>
    </label>
    <label>
      <code>Candeina_nitida</code>
      <count>133</count>
      <precision>0.8125</precision>
      <recall>0.5652173913043478</recall>
      <f1score>0.6666666666666667</f1score>
      <support>23</support>
    </label>
    <label>
      <code>Globigerina_bulloides</code>
      <count>2072</count>
      <precision>0.8610421836228288</precision>
      <recall>0.8401937046004843</recall>
      <f1score>0.8504901960784312</f1score>
      <support>413</support>
    </label>
    <label>
      <code>Globigerina_falconensis</code>
      <count>507</count>
      <precision>0.6915887850467289</precision>
      <recall>0.7474747474747475</recall>
      <f1score>0.7184466019417476</f1score>
      <support>99</support>
    </label>
    <label>
      <code>Globigerinella_calida</code>
      <count>124</count>
      <precision>0.46153846153846156</precision>
      <recall>0.75</recall>
      <f1score>0.5714285714285714</f1score>
      <support>16</support>
    </label>
    <label>
      <code>Globigerinella_siphonifera</code>
      <count>925</count>
      <precision>0.8388888888888889</precision>
      <recall>0.8988095238095238</recall>
      <f1score>0.867816091954023</f1score>
      <support>168</support>
    </label>
    <label>
      <code>Globigerinita_glutinata</code>
      <count>2107</count>
      <precision>0.892271662763466</precision>
      <recall>0.9136690647482014</recall>
      <f1score>0.9028436018957345</f1score>
      <support>417</support>
    </label>
    <label>
      <code>Globigerinoides_conglobatus</code>
      <count>634</count>
      <precision>0.9401709401709402</precision>
      <recall>0.859375</recall>
      <f1score>0.8979591836734694</f1score>
      <support>128</support>
    </label>
    <label>
      <code>Globigerinoides_elongatus</code>
      <count>273</count>
      <precision>0.7045454545454546</precision>
      <recall>0.543859649122807</recall>
      <f1score>0.613861386138614</f1score>
      <support>57</support>
    </label>
    <label>
      <code>Globigerinoides_ruber</code>
      <count>5914</count>
      <precision>0.9316096747289407</precision>
      <recall>0.9514480408858603</recall>
      <f1score>0.9414243573535609</f1score>
      <support>1174</support>
    </label>
    <label>
      <code>Globigerinoides_sacculifer</code>
      <count>3007</count>
      <precision>0.9333333333333333</precision>
      <recall>0.9647435897435898</recall>
      <f1score>0.9487785657998424</f1score>
      <support>624</support>
    </label>
    <label>
      <code>Globoquadrina_conglomerata</code>
      <count>76</count>
      <precision>0.8333333333333334</precision>
      <recall>0.7692307692307693</recall>
      <f1score>0.8</f1score>
      <support>13</support>
    </label>
    <label>
      <code>Globorotalia_crassaformis</code>
      <count>287</count>
      <precision>0.5846153846153846</precision>
      <recall>0.6666666666666666</recall>
      <f1score>0.6229508196721312</f1score>
      <support>57</support>
    </label>
    <label>
      <code>Globorotalia_hirsuta</code>
      <count>244</count>
      <precision>0.711864406779661</precision>
      <recall>0.84</recall>
      <f1score>0.7706422018348624</f1score>
      <support>50</support>
    </label>
    <label>
      <code>Globorotalia_inflata</code>
      <count>1538</count>
      <precision>0.8888888888888888</precision>
      <recall>0.8533333333333334</recall>
      <f1score>0.8707482993197277</f1score>
      <support>300</support>
    </label>
    <label>
      <code>Globorotalia_menardii</code>
      <count>1090</count>
      <precision>0.9688888888888889</precision>
      <recall>0.9356223175965666</recall>
      <f1score>0.9519650655021834</f1score>
      <support>233</support>
    </label>
    <label>
      <code>Globorotalia_scitula</code>
      <count>332</count>
      <precision>0.9807692307692307</precision>
      <recall>1.0</recall>
      <f1score>0.9902912621359222</f1score>
      <support>51</support>
    </label>
    <label>
      <code>Globorotalia_truncatulinoides</code>
      <count>1084</count>
      <precision>0.9192825112107623</precision>
      <recall>0.9360730593607306</recall>
      <f1score>0.927601809954751</f1score>
      <support>219</support>
    </label>
    <label>
      <code>Globorotalia_tumida</code>
      <count>173</count>
      <precision>0.7837837837837838</precision>
      <recall>0.7435897435897436</recall>
      <f1score>0.7631578947368421</f1score>
      <support>39</support>
    </label>
    <label>
      <code>Globorotaloides_hexagonus</code>
      <count>43</count>
      <precision>0.8</precision>
      <recall>0.4444444444444444</recall>
      <f1score>0.5714285714285714</f1score>
      <support>9</support>
    </label>
    <label>
      <code>Globoturborotalita_rubescens</code>
      <count>238</count>
      <precision>0.6428571428571429</precision>
      <recall>0.35294117647058826</recall>
      <f1score>0.4556962025316456</f1score>
      <support>51</support>
    </label>
    <label>
      <code>Globoturborotalita_tenella</code>
      <count>224</count>
      <precision>0.5714285714285714</precision>
      <recall>0.5454545454545454</recall>
      <f1score>0.5581395348837208</f1score>
      <support>44</support>
    </label>
    <label>
      <code>Neogloboquadrina_dutertrei</code>
      <count>874</count>
      <precision>0.930379746835443</precision>
      <recall>0.8909090909090909</recall>
      <f1score>0.9102167182662539</f1score>
      <support>165</support>
    </label>
    <label>
      <code>Neogloboquadrina_incompta</code>
      <count>2034</count>
      <precision>0.9016393442622951</precision>
      <recall>0.8953488372093024</recall>
      <f1score>0.898483080513419</f1score>
      <support>430</support>
    </label>
    <label>
      <code>Neogloboquadrina_pachyderma</code>
      <count>2275</count>
      <precision>0.8742004264392325</precision>
      <recall>0.9030837004405287</recall>
      <f1score>0.8884073672806067</f1score>
      <support>454</support>
    </label>
    <label>
      <code>Orbulina_universa</code>
      <count>320</count>
      <precision>0.9714285714285714</precision>
      <recall>0.9714285714285714</recall>
      <f1score>0.9714285714285714</f1score>
      <support>70</support>
    </label>
    <label>
      <code>Pulleniatina_obliquiloculata</code>
      <count>532</count>
      <precision>0.9223300970873787</precision>
      <recall>0.8796296296296297</recall>
      <f1score>0.9004739336492891</f1score>
      <support>108</support>
    </label>
    <label>
      <code>Sphaeroidinella_dehiscens</code>
      <count>223</count>
      <precision>0.8913043478260869</precision>
      <recall>0.9534883720930233</recall>
      <f1score>0.9213483146067417</f1score>
      <support>43</support>
    </label>
    <label>
      <code>Turborotalita_humilis</code>
      <count>46</count>
      <precision>1.0</precision>
      <recall>0.9</recall>
      <f1score>0.9473684210526316</f1score>
      <support>10</support>
    </label>
    <label>
      <code>Turborotalita_quinqueloba</code>
      <count>303</count>
      <precision>0.9148936170212766</precision>
      <recall>0.6825396825396826</recall>
      <f1score>0.7818181818181817</f1score>
      <support>63</support>
    </label>
  </labels>
  <prepro>
    <name>rescale</name>
    <params>
      <param>255</param>
      <param>0</param>
      <param>1</param>
    </params>
  </prepro>
  <accuracy>0.8917600289121792</accuracy>
  <precision>0.8319792559364991</precision>
  <recall>0.7955080439584482</recall>
  <f1score>0.8069718066939813</f1score>
  <load>
    <training_epochs>226</training_epochs>
    <training_time>13703.452524662018</training_time>
    <training_split>0.2</training_split>
    <training_time_per_image>0.0027389936435903263</training_time_per_image>
    <inference_time_per_image>0.8069718066939813</inference_time_per_image>
  </load>
</network>

    */
}
