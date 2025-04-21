using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GvdEdit.Models
{
    public class Train
    {
        [JsonIgnore]
        internal bool Drawn = false;

        public Guid ID { get; init; } = Guid.NewGuid();

        public int Number { get; set; }

        public TrainCategory Category { get; set; }

        public bool AdHocPath { get; set; }

        public List<Stop> Stops { get; set; } = [];

        public override string ToString() => $"{Enum.GetName(Category)} {Number}";
    }

    public class CisTrain : Train
    {
        public DateTime ValidityStart { get; set; }

        public DateTime ValidityEnd { get; set; }

        public List<bool> Days { get; set; } = [];

        public bool GoesOn(DateTime date)
        {
            date = date.Date;

            if (date < ValidityStart || date > ValidityEnd)
                return false;

            int day = (int)(date - ValidityStart.Date).TotalDays;

            if (Days.Count <= day)
                return true;

            return Days[day];
        }
    }

    public class Stop
    {
        public Guid Station { get; set; }

        [JsonIgnore]
        public string StationName => App.Data.Stations.FirstOrDefault(x => x.ID == Station)?.GetPrettyName(false) ?? string.Empty;

        public TrainCategory Category { get; set; }

        public TimeSpan Arrival { get; set; }

        public TimeSpan Departure { get; set; }

        public bool LeftTrack { get; set; }

        public bool ShortStop { get; set; }

        public bool OnlyIn { get; set; }

        public bool OnlyOut { get; set; }

        public bool ZDD { get; set; }

        public bool TelD3 { get; set; }

        public bool Starts { get; set; }

        public bool Ends { get; set; }
    }

    public enum TrainCategory
    {
        Ex,
        R,
        Sp,
        Os,
        Sv,
        Nex,
        Pn,
        Mn,
        Vlec,
        Sluz,
        Lv,
        Pom,

        Invalid = 65535
    }
}
