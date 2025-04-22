using GvdEdit.ViewModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GvdEdit.Models
{
    public class GvdData : NotifyPropertyChanged
    {
        public ObservableCollection<Station> Stations { get; set; } = [];

        public List<Train> Trains { get; set; } = [];

        [JsonIgnore]
        public ObservableCollection<TrainVM> TrainsVM { get; set; } = [];

        public string TimetableName
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(TimetableName));
                }
            }
        } = string.Empty;

        public string Route
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Route));
                }
            }
        } = string.Empty;

        public string Variant
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Variant));
                }
            }
        } = string.Empty;

        public void PrepareTrains()
        {
            ClearTrains();
            Trains.AddRange(GetTrains());
        }

        public void LoadTrains()
        {
            TrainsVM.Clear();
            foreach (var train in Trains)
                TrainsVM.Add(new(train));
            ClearTrains();
        }

        public void ClearTrains() => Trains.Clear();

        public IEnumerable<Train> GetTrains() => TrainsVM.Select(x => x.GetTrain());

        internal IEnumerable<Train> GetDrawableTrains()
        {
            foreach (Train train in TrainsVM.Select(x => x.GetTrain()))
            {
                yield return train;

                if (train.GoesOverMidnight())
                    yield return train.AddDays(-1);
            }
        }
    }
}
