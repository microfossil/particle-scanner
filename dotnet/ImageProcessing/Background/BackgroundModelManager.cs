using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace ImageProcessing.Background
{
    public class BackgroundModelManager : ObservableObject
    {
        public List<ColourBackgroundModel> Models { get; private set; }

        public event Action<ColourBackgroundModel, int> ModelUpdatedEvent = delegate { };

        private int initialThreshold;
        public int InitialThreshold
        {
            get => initialThreshold;
            set => Set(ref initialThreshold, value);
        }

        //Actual stack size changes when we do an update in the BackgroundModel
        private int stackSize;
        public int StackSize
        {
            get => stackSize;
            set
            {
                Set(ref stackSize, value);
                if (selectionIndex >= stackSize) SelectionIndex = stackSize - 1;
            }
        }

        private int selectionIndex;
        public int SelectionIndex
        {
            get => selectionIndex;
            set
            {
                Set(ref selectionIndex, value);
                OnSelectionIndexChanged();
            }
        }

        //public ColourBackgroundModel Model(int idx)
        //{
        //    return BackgroundModelManager[idx];
        //}

        public BackgroundModelManager(int size, int width, int height, int stackSize, int selectionIndex, int initialThreshold)
        {
            this.stackSize = stackSize;
            this.selectionIndex = selectionIndex;
            this.initialThreshold = initialThreshold;

            Models = new List<ColourBackgroundModel>();
            for (int i = 0; i < size; i++)
            {
                var model = new ColourBackgroundModel(i, width, height, StackSize, SelectionIndex, InitialThreshold);
                Models.Add(model);
                model.ModelUpdatedEvent += BackgroundModelManager_ModelUpdatedEvent;
            }
        }

        private void BackgroundModelManager_ModelUpdatedEvent(ColourBackgroundModel obj)
        {
            ModelUpdatedEvent(obj, obj.Id);
        }

        public void InitialiseAll()
        {
            foreach (var model in Models)
            {
                model.Initialise();
            }
        }

        private void OnSelectionIndexChanged()
        {
            foreach (var model in Models)
            {
                model.SelectionIndex = SelectionIndex;
            }
        }
    }
}
