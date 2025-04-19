#pragma warning disable CS9264

using Newtonsoft.Json;
using System;
using System.Text;

namespace GvdEdit.Models
{
    public class Station : NotifyPropertyChanged
    {
        internal float DrawY = 0;

        public Guid ID { get; init; } = Guid.NewGuid();

        public StationType StationType
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(StationType));
                    OnPropertyChanged(nameof(PrettyName));
                }
            }
        }

        [JsonIgnore]
        public int StationTypeBinding { get => GetStationTypeBinding(); set => SetStationType(value); }

        public string Name
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(PrettyName));
                }
            }
        }

        [JsonIgnore]
        public string PrettyName => GetName();

        public bool Hidden
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(StationTypeBinding));
                }
            }
        }

        public StationBuilding StationBuilding
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(StationBuilding));
                    OnPropertyChanged(nameof(PrettyName));
                }
            }
        }

        [JsonIgnore]
        public int StationBuildingBinding { get => (int)StationBuilding; set => StationBuilding = (StationBuilding)value; }

        public int TracksLeft
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(TracksLeft));
                }
            }
        }

        public int TracksRight
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(TracksRight));
                }
            }
        }

        public int RouteTracks
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(RouteTracks));
                }
            }
        }

        public float Position
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        public float Position2
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Position2));
                }
            }
        }

        public RouteInterlocking RouteInterlocking
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(RouteInterlocking));
                }
            }
        }

        [JsonIgnore]
        public int RouteInterlockingBinding { get => (int)RouteInterlocking; set => RouteInterlocking = (RouteInterlocking)value; }

        private string GetName()
        {
            StringBuilder sb = new();

            if (StationBuilding == StationBuilding.Left)
                sb.Append("\u25AE ");
            else
                sb.Append("   ");

            if (StationType.HasFlag(StationType.AHr))
                sb.Append("AHr ");

            sb.Append(Name);

            if (StationType.HasFlag(StationType.zast))
                sb.Append(" z");
            else if (StationType.HasFlag(StationType.nz))
                sb.Append(" nz");

            if (StationBuilding == StationBuilding.Right)
                sb.Append(" \u25AE");

            return sb.ToString();
        }

        public string GetPrettyName(bool building = false)
        {
            StringBuilder sb = new();

            if (StationType.HasFlag(StationType.AHr))
                sb.Append("AHr ");

            sb.Append(Name);

            if (StationType.HasFlag(StationType.zast))
                sb.Append(" z");
            else if (StationType.HasFlag(StationType.nz))
                sb.Append(" nz");

            if (StationBuilding == StationBuilding.Right && building)
                sb.Append(" \u25AE");

            return sb.ToString();
        }

        private int GetStationTypeBinding()
        {
            StationType2 type2 = StationType switch
            {
                StationType.ZST => StationType2.ZST,
                StationType.AHr => StationType2.AHr,
                StationType.zast => StationType2.zast,
                StationType.nz => StationType2.nz,
                StationType.AHr | StationType.zast => StationType2.AHrZast,
                StationType.AHr | StationType.nz => StationType2.AHrNz,
                _ => StationType2.Other
            };
            return (int)type2;
        }

        private void SetStationType(int type2)
        {
            StationType = (StationType2)type2 switch
            {
                StationType2.ZST => StationType.ZST,
                StationType2.AHr => StationType.AHr,
                StationType2.zast => StationType.zast,
                StationType2.nz => StationType.nz,
                StationType2.AHrZast => StationType.AHr | StationType.zast,
                StationType2.AHrNz => StationType.AHr | StationType.nz,
                _ => StationType.Other
            };
        }

        public override string ToString() => GetPrettyName(false);
    }

    [Flags]
    public enum StationType
    {
        Other = 0,
        ZST = 1,
        AHr = 2,
        zast = 4,
        nz = 8
    }

    public enum StationType2
    {
        Other,
        ZST,
        AHr,
        AHrZast,
        AHrNz,
        zast,
        nz
    }

    public enum StationBuilding
    {
        None,
        Left,
        Right
    }

    public enum RouteInterlocking
    {
        None,
        Telephone,
        SemiAutomatic,
        Automatic,
        OneWay
    }
}
