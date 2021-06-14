using System;

namespace ImageProcessing.Classification.Tensorflow
{
    public class Probability
    {
        public String Label { get; set; }
        public int Index { get; set; }= 0;
        public float Value { get; set; }= 0;

        public Probability(String label, int index, float value)
        {
            Label = label;
            Index = index;
            Value = value;
        }

        public override string ToString()
        {
            return String.Format("class {0} ({1}) - {2:0.00}", Label, Index, Value);
        }
    }
}
