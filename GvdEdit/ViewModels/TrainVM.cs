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

        internal bool Initializing = false;

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

        public TrainCategory Category
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

        public bool Highlight
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Highlight));
                }
            }
        }

        public TimeSpan DepartureTime
        {
            get => DepartureDate.TimeOfDay;
            set
            {
                DepartureDate = DateTime.Today.AddTicks(value.Ticks);
            } 
        }

        public DateTime DepartureDate
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(DepartureDate));
                    RecountStops();
                }
            }
        } = DateTime.Today;

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
            Initializing = true;

            ID = train.ID;
            Number = train.Number;
            AdHocPath = train.AdHocPath;
            Highlight = train.Highlight;
            DepartureTime = train.Stops.First().Departure;
            StationFrom = App.Data.Stations.First(x => x.ID == train.Stops.First().Station);
            StationTo = App.Data.Stations.First(x => x.ID == train.Stops.Last().Station);

            foreach (var stop in train.Stops)
            {
                var stopVM = new StopVM(stop);
                stopVM.StopChanged += RecountStops;
                TrainStops.Add(stopVM);
            }

            for (int i = 0; i < TrainStops.Count - 1; i++)
            {
                StopVM current = TrainStops[i];
                StopVM next = TrainStops[i + 1];

                if (i == 0)
                    Category = current.Category;

                current.TravelTime = (float)(next.Arrival - current.Departure).TotalMinutes;
            }

            Initializing = false;
        }

        public Train GetTrain()
        {
            return new Train
            {
                ID = ID,
                AdHocPath = AdHocPath,
                Highlight = Highlight,
                Category = Category,
                Number = Number,
                Stops = TrainStops.Select(x => x.GetStop()).ToList()
            };
        }

        public Train CloneTrain()
        {
            return new Train
            {
                ID = Guid.NewGuid(),
                AdHocPath = AdHocPath,
                Highlight = Highlight,
                Category = Category,
                Number = Number + 2,
                Stops = TrainStops.Select(x => x.GetStop()).ToList()
            };
        }

        protected void AddStop(Station station)
        {
            var stop = new StopVM(station);
            stop.StopChanged += RecountStops;
            TrainStops.Add(stop);
            RecountStops();
        }

        public void AddStop(StopVM stop)
        {
            stop.StopChanged += RecountStops;
            TrainStops.Add(stop);
            RecountStops();
        }

        public void RecountStops() => RecountStops(this, EventArgs.Empty);

        private void RecountStops(object? sender, EventArgs e)
        {
            if (Initializing)
                return;

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

        public void CreateStops(bool ignoreHiddenStops)
        {
            int id1 = App.Data.Stations.IndexOf(StationFrom);
            int id2 = App.Data.Stations.IndexOf(StationTo);

            if (id1 == -1 || id2 == -1 || id1 == id2)
                return;

            if (StationFrom.Hidden || StationTo.Hidden)
                ignoreHiddenStops = false;

            TrainStops.Clear();

            if (id1 > id2)
            {
                for (int i = id1; i >= id2; i--)
                {
                    Station station = App.Data.Stations[i];
                    if (station.Hidden && ignoreHiddenStops)
                        continue;
                    addStop(station);
                }
            }
            else
            {
                for (int i = id1; i <= id2; i++)
                {
                    Station station = App.Data.Stations[i];
                    if (station.Hidden && ignoreHiddenStops)
                        continue;
                    addStop(station);
                }
            }

            RecountStops();

            void addStop(Station station)
            {
                StopVM stop = new(station) { Category = Category };
                stop.StopChanged += RecountStops;
                TrainStops.Add(stop);
            }
        }

        public string Name => $"{Enum.GetName(Category)} {Number}";

        public override string ToString() => Name;
    }
}
