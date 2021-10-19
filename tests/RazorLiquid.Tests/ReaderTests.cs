using System.IO;
using Razor2Liquid;

namespace RazorLiquid.Tests
{
    public class ReaderTests
    {
        protected LiquidModel GetModel(string template)
        {
            var reader = new RazorReader();

            var stringReader = new StringReader(template);
            return reader.GetLiquidModel(stringReader);
        }

        protected string GetLiquidString(string template)
        {
            return GetModel(template).Liquid.ToString();
        }
    }
}