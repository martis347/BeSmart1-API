using System.Collections.Generic;

namespace Lunch.Domain
{
    public class SheetStyleRequest
    {
        public IList<StyleSheetPartRequest> Requests { get; set; }
    }

    public class StyleSheetPartRequest
    {
        public RepeatCell RepeatCell { get; set; }
    }

    public class RepeatCell
    {
        public Range Range { get; set; }
        public Cell Cell { get; set; }
        public string Fields { get; set; }
    }

    public class Range
    {
        public string SheetId { get; set; }
        public int StartRowIndex { get; set; }
        public int EndRowIndex { get; set; }
        public int StartColumnIndex { get; set; }
        public int EndColumnIndex { get; set; }
    }

    public class Cell
    {
        public UserEnteredFormat UserEnteredFormat { get; set; }
    }

    public class UserEnteredFormat
    {
        public TextFormat TextFormat { get; set; }
        public NumberFormat NumberFormat { get; set; }
    }

    public class TextFormat
    {
        public string FontFamily { get; set; }
        public string FontSize { get; set; }
        public ForegroundColor ForegroundColor { get; set; }
    }

    public class NumberFormat
    {
        public string Type { get; set; }
        public string Pattern { get; set; }
    }

    public class ForegroundColor
    {
        public string Red { get; set; }
        public string Green { get; set; }
        public string Blue { get; set; }
    }
}