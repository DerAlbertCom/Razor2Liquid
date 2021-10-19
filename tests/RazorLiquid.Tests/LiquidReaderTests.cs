using System;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace RazorLiquid.Tests
{
    public class LiquidReaderTests : ReaderTests
    {
        readonly ITestOutputHelper _outputHelper;

        public LiquidReaderTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }
        [Fact]
        public void Markup_Simple()
        {
            var result = GetLiquidString("<themarkup></themarkup>");

            result.Should().Be("<themarkup></themarkup>");
        }

        [Fact]
        public void Markup_Multiline()
        {
            var source = @"
<html>
  <body>
  </body>
</html>
";
            var result = GetLiquidString(source);

            result.Should().Be(source);
        }

        [Fact]
        public void Markup_IgnoresUsing()
        {
            var source = @"
@using FooBar.Ding
@using BarFoo.Ding
<html>
  <body>
  </body>
</html>
";

            var expected = @"
<html>
  <body>
  </body>
</html>
";
            var result = GetLiquidString(source);

            result.Should().Be(expected);
        }

        [Fact]
        public void Markup_IgnoresModelAtLast()
        {
            var source = @"
@using FooBar.Ding
@model BarFoo.Ding
<html>
  <body>
  </body>
</html>
";

            var expected = @"
<html>
  <body>
  </body>
</html>
";
            var result = GetLiquidString(source);

            result.Should().Be(expected);
        }

        [Fact]
        public void Markup_IgnoresModelAFirst()
        {
            var source = @"
@model BarFoo.Ding
@using FooBar.Ding
<html>
  <body>
  </body>
</html>
";

            var expected = @"
<html>
  <body>
  </body>
</html>
";
            var result = GetLiquidString(source, (t,a)=>_outputHelper.WriteLine(t,a));

            result.Should().Be(expected);
        }

        [Fact]
        public void Markup_FindLayout()
        {
            var source = @"
@model BarFoo.Ding
@using FooBar.Ding
@{
    Layout = ""MailLayout.Htm.cshtml""; 
}
<html>
  <body>
  </body>
</html>
";

            var expected = @"{% layout 'MailLayout.Htm' %}

<html>
  <body>
  </body>
</html>
";
            var result = GetModel(source);

            result.Liquid.ToString().Should().Be(expected);

            result.Layout.Should().Be("MailLayout.Htm");
        }
        
        [Fact]
        public void Markup_ImgSrc_Model_with_Add()
        {
            var source = @"
<html>
  <body>
     <img src=""@(Model.Urls.ImagesBaseUrl + ""blank.gif"")"" />
  </body>
</html>
";
            var result = GetLiquidString(source);
            var expected = @"
<html>
  <body>
     <img src=""{{ Model.Urls.ImagesBaseUrl | append: ""blank.gif"" }}"" />
  </body>
</html>
";
            result.Should().Be(expected);
        }
        
        [Fact]
        public void Markup_ImgSrc_Model_with_parenthesis()
        {
            var source = @"
<html>
  <body>
     <img src=""@(Model.Urls.ImagesBaseUrl)"" />
  </body>
</html>
";
            var result = GetLiquidString(source);
            var expected = @"
<html>
  <body>
     <img src=""{{ Model.Urls.ImagesBaseUrl }}"" />
  </body>
</html>
";
            result.Should().Be(expected);
        }

        [Fact]
        public void Markup_ImgSrc_Model()
        {
            var source = @"
<html>
  <body>
     <img src=""@Model.Urls.ImagesBaseUrl"" />
  </body>
</html>
";
            var result = GetLiquidString(source);
            var expected = @"
<html>
  <body>
     <img src=""{{ Model.Urls.ImagesBaseUrl }}"" />
  </body>
</html>
";
            result.Should().Be(expected);
        }
        
        [Fact]
        public void Markup_Translate()
        {
            var source = @"
<html>
  <body>@Translate(LocalizationKeys.CourseAvailabilityEmail.CourseAvailabilityHeadline_Text)
  </body>
</html>
";
            var result = GetLiquidString(source);
            var expected = @"
<html>
  <body>{{ ""LocalizationKeys.CourseAvailabilityEmail.CourseAvailabilityHeadline_Text"" | translate }}
  </body>
</html>
";
            result.Should().Be(expected);
        }        
        
        [Fact]
        public void Markup_TranslateFormat()
        {
            var source = @"
<html>
  <body>@TranslateFormat(LocalizationKeys.MultipleEmails.FormofAddress_TextPattern, Model.CustomerName, ""Ding"")
  </body>
</html>
";
            var result = GetLiquidString(source);
            var expected = @"
<html>
  <body>{{ ""LocalizationKeys.MultipleEmails.FormofAddress_TextPattern"" | translate: Model.CustomerName, ""Ding"" }}
  </body>
</html>
";
            result.Should().Be(expected);
        }

        [Fact]
        public void Markup_TranslateFormat_with_culture()
        {
            var source = @"
<html>
@{
    var chineseCulture = System.Globalization.CultureInfo.GetCultureInfo(""zh-Hans"");
      }
  <body>@TranslateFormat(LocalizationKeys.MultipleEmails.FormofAddress_TextPattern, chineseCulture, Model.CustomerName, ""Ding"")
  </body>
</html>
";
            var result = GetLiquidString(source);
            var expected = @"
<html>
{% culture 'zh-Hans' %}
  <body>{{ ""LocalizationKeys.MultipleEmails.FormofAddress_TextPattern"" | translate: Model.CustomerName, ""Ding"" }}
  </body>
</html>
";
            result.Should().Be(expected);
        }
        [Fact]
        public void Markup_CultureAssignedMent()
        {
            var source = @"
<html>
  <body>
@{
    var chineseCulture = System.Globalization.CultureInfo.GetCultureInfo(""zh-Hans"");
      }
  </body>
</html>
";
            var result = GetLiquidString(source);
            var expected = @"
<html>
  <body>
{% culture 'zh-Hans' %}
  </body>
</html>
";
            result.Should().Be(expected);
        }
        [Fact]
        public void Markup_RawMedia()
        {
            var source = @"
<html>
  <body>
		@Raw(""@media only screen and (min-device-width: 768px) and (max-device-width: 1024px) {"")
		/* You guessed it, ipad (tablets, smaller screens, etc) */
			/* repeating for the ipad */
			a[href^=""tel""], a[href^=""sms""] {
						text-decoration: none;
						color: blue; /* or whatever your want */
						pointer-events: none;
						cursor: default;
					}

			.mobile_link a[href^=""tel""], .mobile_link a[href^=""sms""] {
						text-decoration: default;
						color: orange !important;
						pointer-events: auto;
						cursor: default;
					}
  </body>
</html>
";
            var result = GetLiquidString(source);
            var expected = @"
<html>
  <body>
		@media only screen and (min-device-width: 768px) and (max-device-width: 1024px) {
		/* You guessed it, ipad (tablets, smaller screens, etc) */
			/* repeating for the ipad */
			a[href^=""tel""], a[href^=""sms""] {
						text-decoration: none;
						color: blue; /* or whatever your want */
						pointer-events: none;
						cursor: default;
					}

			.mobile_link a[href^=""tel""], .mobile_link a[href^=""sms""] {
						text-decoration: default;
						color: orange !important;
						pointer-events: auto;
						cursor: default;
					}
  </body>
</html>
";
            result.Should().Be(expected);
        }
    }
}