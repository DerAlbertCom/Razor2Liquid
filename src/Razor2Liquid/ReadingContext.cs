using System;
using System.Text;

namespace Razor2Liquid
{
    public class ReadingContext
    {
        public LiquidModel Model { get; }
        public ReadingHint Hint { get; set; }

        public StringBuilder Liquid => Model.Liquid;
        public int BarsCounter { get; set; }
        public int CodeCounter { get; set; }

        public ReadingContext(LiquidModel model)
        {
            Hint = ReadingHint.None;
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}