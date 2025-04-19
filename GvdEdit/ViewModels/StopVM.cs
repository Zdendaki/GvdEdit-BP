using GvdEdit.Models;
using System;
using System.Linq;

#pragma warning disable CS8618

namespace GvdEdit.ViewModels
{
    public class StopVM : NotifyPropertyChanged
    {
        public event EventHandler? StopChanged;

        public Station Station { get; set; }

        public TimeSpan Arrival
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Arrival));
                }
            }
        }

        public TimeSpan Departure
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Departure));
                }
            }
        }

        public float TravelTime
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(TravelTime));
                    OnStopChanged();
                }
            }
        }

        public float WaitTime
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(WaitTime));
                    OnStopChanged();
                }
            }
        }

        public bool LeftTrack
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(LeftTrack));
                }
            }
        }

        public bool ShortStop
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ShortStop));
                }
            }
        }

        public bool OnlyIn
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(OnlyIn));
                }
            }
        }

        public bool OnlyOut
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(OnlyOut));
                }
            }
        }

        public bool ZDD
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(ZDD));
                }
            }
        }

        public bool TelD3
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(TelD3));
                }
            }
        }

        public bool Starts
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Starts));
                }
            }
        }

        public bool Ends
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Ends));
                }
            }
        }

        public StopVM() { }

        public StopVM(Station station)
        {
            Station = station;
        }

        public StopVM(Stop stop)
        {
            Station = App.Data.Stations.First(x => x.ID == stop.Station);
            Arrival = stop.Arrival;
            Departure = stop.Departure;
            LeftTrack = stop.LeftTrack;
            ShortStop = stop.ShortStop;
            OnlyIn = stop.OnlyIn;
            OnlyOut = stop.OnlyOut;
            ZDD = stop.ZDD;
            TelD3 = stop.TelD3;
            Starts = stop.Starts;
            Ends = stop.Ends;
        }

        public Stop GetStop()
        {
            return new Stop
            {
                Station = Station.ID,
                Arrival = Arrival,
                Departure = Departure,
                LeftTrack = LeftTrack,
                ShortStop = ShortStop,
                OnlyIn = OnlyIn,
                OnlyOut = OnlyOut,
                ZDD = ZDD,
                TelD3 = TelD3,
                Starts = Starts,
                Ends = Ends
            };
        }

        protected void OnStopChanged()
        {
            StopChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString() => Station.Name;
    }
}
