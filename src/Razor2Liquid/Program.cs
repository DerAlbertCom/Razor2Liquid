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
          //   DumpIt();
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
    @ShowBoleto(payment)
    <br/>
 @helper ShowBoleto(Payment payment) {
     <hr />
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
            dumper.Dump(template);
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
            WriteHelpers(file);
            Console.WriteLine("Converted {0}", liquidFile);
        }

        void WriteHelpers(string file)
        {
            var reader = new RazorReader();
            var helpers = reader.GetHelpers(file);
            foreach (var keyValue in helpers)
            {
                Console.WriteLine("Converting {0} Helper {1}", file, keyValue.Key);
                var model = reader.GetLiquidModel(new StringReader(keyValue.Value));
                var liquidFile = Path.ChangeExtension(file, $"{keyValue.Key}.liquid");
                File.WriteAllText(liquidFile, model.Liquid.ToString());
            }
        }
    }
}