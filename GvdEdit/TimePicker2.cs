using System;
using System.Globalization;
using Xceed.Wpf.Toolkit;

namespace GvdEdit
{
    public class TimePicker2 : TimePicker
    {
        protected override DateTime? ConvertTextToValue(string text)
        {
            DateTime time;
            if (DateTime.TryParseExact(text, "HHmm", null, DateTimeStyles.None, out time))
                return time;
            if (DateTime.TryParseExact(text, "HHmmss", null, DateTimeStyles.None, out time))
                return time;
            if (DateTime.TryParseExact(text, "HH:mm", null, DateTimeStyles.None, out time))
                return time;
            if (DateTime.TryParseExact(text, "HH:mm:ss", null, DateTimeStyles.None, out time))
                return time;

            return base.ConvertTextToValue(text);
        }
    }
}
