using GvdEdit.Models;

#pragma warning disable CS8618

namespace GvdEdit
{
    public class TrainCategoryItem
    {
        public string Name { get; set; }

        public TrainCategory Category { get; set; }

        public override string ToString() => Name;
    }
}
