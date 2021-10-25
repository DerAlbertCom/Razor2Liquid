using System;
using System.IO;
using System.Linq;

namespace Razor2Liquid
{
    class Program
    {
        static void Main(string[] args)
        {
           ConvertTemplates();
           //  DumpIt();
        }

        private static void ConvertTemplates()
        {
            var converter = new TemplateConverter();
            converter.ConvertFolder("/Users/aweinert/src/arvato/Marketplace/src/BlobStorageContent/mailtemplates");
       //     converter.ConvertFile("/Users/aweinert/src/arvato/Marketplace/src/BlobStorageContent/mailtemplates/OrderCancellation.Htm.cshtml");
//            converter.ConvertFolder(@"C:\src\arvato\Marketplace\src\BlobStorageContent\mailtemplates\");
        }

        private static void DumpIt()
        {
            var dumper = new TemplateDumper();
        //    var template = File.ReadAllText(@"C:\src\arvato\Marketplace\src\BlobStorageContent\mailtemplates\OrderCancellation.Htm.cshtml");

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
            var t2 = @"
<html>
  <body>
     <img src=""@Model.Urls.ImagesBaseUrl"" />
  </body>
</html>
";
            dumper.Dump(t2);
        }
    }

    class TemplateConverter
    {
        public void ConvertFolder(string path)
        {
            path = Path.GetFullPath(path);
            var razorFiles = Directory.EnumerateFiles(path).Where(s => Path.GetExtension(s) == ".cshtml")
                .OrderBy(s=>s).ToArray();
            foreach (var razorFile in razorFiles)
            {
                ConvertFile(razorFile);
            }
        }

        public void ConvertFile(string file)
        {
            file = Path.GetFullPath(file);
            Console.WriteLine("Converting {0}", file);
            var reader = new RazorReader();
            var model = reader.GetLiquidModel(file);
            var liquidFile = Path.ChangeExtension(file, ".liquid");

            File.WriteAllText(liquidFile, model.Liquid.ToString());
            Console.WriteLine("Converted {0}", liquidFile);
        }
    }
}