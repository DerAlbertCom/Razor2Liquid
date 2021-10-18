using System.IO;
using System.Linq;

namespace Razor2Liquid
{
    class Program
    {
        static void Main(string[] args)
        {
        //    ConvertTemplates();
            DumpIt();
        }

        private static void ConvertTemplates()
        {
            var converter = new TemplateConverter();
            converter.ConvertFolder(@"C:\src\arvato\Marketplace\src\BlobStorageContent\mailtemplates\");
        }

        private static void DumpIt()
        {
            var dumper = new TemplateDumper();

            //            var template = File.ReadAllText(@"C:\src\arvato\Marketplace\src\BlobStorageContent\mailtemplates\OrderCancellation.Htm.cshtml");

            var template = @"
<body>
	@helper ShowBoleto(Cws.Shop.Model.Order.BankPayment payment)
	{
		<br />
		<font face=""Arial, Helvetica, sans-serif"" color=""#000000"" style=""font-size: 13px; text-decoration: none; line-height: 19px;"">
			@Translate(LocalizationKeys.OrderConfirmationEmail.BoletoLiteral_Text)
		</font>
			@if (!string.IsNullOrEmpty(payment.CustomerAccountHolderName))
			{
<intheif>/></intheif>
                }
		<br />
}
</body>
";
            dumper.Dump(template);
        }
    }

    class TemplateConverter
    {
        public void ConvertFolder(string path)
        {
            var razorFiles = Directory.EnumerateFiles(path).Where(s => Path.GetExtension(s) == ".cshtml");
            foreach (var razorFile in razorFiles)
            {
                ConfertFile(razorFile);
            }
        }

        public void ConfertFile(string file)
        {
            var reader = new RazorReader();
            var model = reader.GetLiquidModel(file);
            var liquidFile = Path.ChangeExtension(file,".liquid");
            
            File.WriteAllText(liquidFile, model.Liquid.ToString());

        }
    }
}