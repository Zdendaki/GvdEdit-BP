using GvdEdit.ViewModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GvdEdit.Models
{
    public class GvdData
    {
        public ObservableCollection<Station> Stations { get; set; } = [];

        public List<Train> Trains
        {
            get => TrainsVM.Select(x => x.GetTrain()).ToList();
            set
            {
                TrainsVM.Clear();
                foreach (var train in value)
                {
                    var trainVM = new TrainVM(train);
                    TrainsVM.Add(trainVM);
                }
            }
        }

        [JsonIgnore]
        public ObservableCollection<TrainVM> TrainsVM { get; set; } = [];

        public void Initialize()
        {

        }
    }
}
