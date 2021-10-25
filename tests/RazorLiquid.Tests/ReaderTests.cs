using System;
using System.Collections.Generic;
using System.IO;
using Razor2Liquid;

namespace RazorLiquid.Tests
{
    public class ReaderTests
    {
        protected LiquidModel GetModel(string template, Action<string,object[]> console = null)
        {
            var reader = new RazorReader(console);

            var stringReader = new StringReader(template);
            return reader.GetLiquidModel(stringReader);
        }

        protected string GetLiquidString(string template, Action<string,object[]> console = null)
        {
            return GetModel(template, console).Liquid.ToString();
        }
        
        protected IDictionary<string, string> GetHelper(string template, Action<string,object[]> console = null)
        {
            var reader = new RazorReader(console);

            var stringReader = new StringReader(template);
            return reader.GetHelpers(stringReader);
        }
    }
}