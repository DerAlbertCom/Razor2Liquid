using System;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Razor2Liquid
{
    public class ReadingContext
    {
        public LiquidModel Model { get; }
        public ReadingHint Hint { get; set; }

        public StringBuilder Liquid => Model.Liquid;
        public int BarsCounter { get; set; }
        public int CodeCounter { get; set; }
        public string CurrentCulture { get; set; }
        public CSharpSyntaxNode AsComment { get; set; }

        public ReadingContext(LiquidModel model)
        {
            Hint = ReadingHint.None;
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}