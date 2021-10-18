using System.Collections.Generic;
using System.Text;

namespace Razor2Liquid
{
    public class LiquidModel
    {
        public LiquidModel()
        {
            Liquid = new StringBuilder();
        }

        public string Layout { get; set; }
        public StringBuilder Liquid { get; }

        private readonly List<ParseError> _errors = new List<ParseError>();
        
        public IEnumerable<ParseError> Errors => _errors;

        public void AddError(ParseError parseError)
        {
            _errors.Add(parseError);
        }
    }
}