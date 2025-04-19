using GvdEdit.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

#pragma warning disable CS8618, CS9264

namespace GvdEdit.ViewModels
{
    public class TrainVM : NotifyPropertyChanged
    {
        public Guid ID { get; init; }

        public int Number
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Number));
                    OnPropertyChanged("Name");
                }
            }
        }

        public int Category
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Category));
                    OnPropertyChanged("Name");
                }
            }
        }

        public bool AdHocPath
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(AdHocPath));
                }
            }
        }

        public TimeSpan DepartureTime => DepartureDate.TimeOfDay;

        public DateTime DepartureDate
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(DepartureDate));
                }
            }
        }

        public Station StationFrom
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(StationFrom));
                }
            }
        }

        public Station StationTo
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(StationTo));
                }
            }
        }

        public ObservableCollection<StopVM> TrainStops { get; set; } = [];

        public TrainVM()
        {
            ID = Guid.NewGuid();
        }

        public TrainVM(Train train)
        {
            ID = train.ID;
            Number = train.Number;
            Category = (int)train.Category;
            AdHocPath = train.AdHocPath;
            DepartureDate = DateTime.Today.AddTicks(train.Stops.First().Departure.Ticks);
            StationFrom = App.Data.Stations.First(x => x.ID == train.Stops.First().Station);
            StationTo = App.Data.Stations.First(x => x.ID == train.Stops.Last().Station);

            foreach (var stop in train.Stops)
            {
                var stopVM = new StopVM(stop);
                stopVM.StopChanged += RecountStops;
                TrainStops.Add(stopVM);
            }
        }

        public Train GetTrain()
        {
            return new Train
            {
                ID = ID,
                AdHocPath = AdHocPath,
                Category = (TrainCategory)Category,
                Stops = TrainStops.Select(x => x.GetStop()).ToList()
            };
        }

        protected void AddStop(Station station)
        {
            var stop = new StopVM(station);
            stop.StopChanged += RecountStops;
            TrainStops.Add(stop);
            RecountStops(this, EventArgs.Empty);
        }

        private void RecountStops(object? sender, EventArgs e)
        {
            TimeSpan time = DepartureTime;

            foreach (var stop in TrainStops)
            {
                stop.Arrival = time;
                time += TimeSpan.FromMinutes(stop.WaitTime);
                stop.Departure = time;
                time += TimeSpan.FromMinutes(stop.TravelTime);
            }

            App.MainWindow.RedrawGVD();
        }

        public void CreateStops()
        {
            int id1 = App.Data.Stations.IndexOf(StationFrom);
            int id2 = App.Data.Stations.IndexOf(StationTo);

            if (id1 == -1 || id2 == -1 || id1 == id2)
                return;

            TrainStops.Clear();

            if (id1 > id2)
            {
                for (int i = id1; i >= id2; i--)
                {
                    var stop = new StopVM(App.Data.Stations[i]);
                    stop.StopChanged += RecountStops;
                    TrainStops.Add(stop);
                }
            }
            else
            {
                for (int i = id1; i <= id2; i++)
                {
                    var stop = new StopVM(App.Data.Stations[i]);
                    stop.StopChanged += RecountStops;
                    TrainStops.Add(stop);
                }
            }
        }

        public string Name => $"{Enum.GetName((TrainCategory)Category)} {Number}";

        public override string ToString() => Name;
    }
}
