using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
    }

    public class Stop
    {
        public Guid Station { get; set; }

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
