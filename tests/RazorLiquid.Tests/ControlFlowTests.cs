using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace RazorLiquid.Tests
{
    public class ControlFlowTests : ReaderTests
    {
        readonly ITestOutputHelper _outputHelper;

        public ControlFlowTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void If_should_render_to_if()
        {
            var template = @"
@{ var a = true }
@if (a) {
  <hello>@a</hello>
}";
            var expected = @"
{% assign a = true %}

{% if a %}
  <hello>{{ a }}</hello>

{% endif %}";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().Be(expected);
        }

        [Fact]
        public void If_with_more()
        {
            var template = "@if (course.IsBundleItem)";
            var expected = @"{% if course.IsBundleItem %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
        }
        
        [Fact]
        public void If_not_with_more()
        {
            var template = "@if (!course.IsBundleItem)";
            var expected = @"{% if course.IsBundleItem == false %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
        }
        
        [Fact]
        public void If_with_greater_then()
        {
            var template = "@if (course.IsBundleItem > 0)";
            var expected = @"{% if course.IsBundleItem > 0 %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
        }
        [Fact]
        public void If_with_lower_then()
        {
            var template = "@if (course.IsBundleItem < 0)";
            var expected = @"{% if course.IsBundleItem < 0 %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
        }
        [Fact]
        public void If_assign()
        {
            var template = @"
@{ var priceWidthInPercent = 40; }
@if (course.IsBundleItem) {
  priceWidthInPercent = 30;
  <ding>@priceWidthInPercent</ding>
}";

            var expected = @"
{% assign priceWidthInPercent = 40 %}

{% if course.IsBundleItem %}
{% assign priceWidthInPercent = 30 %}
  <ding>{{ priceWidthInPercent }}</ding>

{% endif %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
        }

        [Fact]
        public void IfThenElse()
        {
            var template = @"
                                        @{
                                            string deliveryOptionLabelLocalizationId;
                                            if (course.IsDigital)
                                            {
                                                deliveryOptionLabelLocalizationId = LocalizationKeys.OrderConfirmationEmail.DeliveryOption1Label_Text;
                                            }
                                            else
                                            {
                                                deliveryOptionLabelLocalizationId = LocalizationKeys.OrderConfirmationEmail.DeliveryOption2Label_Text;
                                            }
                                        }";
            var expected = @"
{% assign deliveryOptionLabelLocalizationId = """" %}
{% if course.IsDigital %}
{% assign deliveryOptionLabelLocalizationId = LocalizationKeys.OrderConfirmationEmail.DeliveryOption1Label_Text %}

{% else %}

{% assign deliveryOptionLabelLocalizationId = LocalizationKeys.OrderConfirmationEmail.DeliveryOption2Label_Text %}
{% endif %}
";
            
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
            
        }
        
        [Fact]
        public void SimpleForEachWithAnAssign()
        {
            var template = @"
@{
    @foreach (var course in Model.CurrentCart.SortedCourseItems) { 
        var a = course;
    }
}";
            var expected = @"
{% for course in Model.CurrentCart.SortedCourseItems %}
{% assign a = course %}

{% endfor %}
";
            
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
            
        }

        [Fact]
        public void If_with_equals()
        {
            var template = @"
@if (Cws.Shop.Model.Enumerations.CourseType.PrometricVoucher.Equals(course.CourseType) || Cws.Shop.Model.Enumerations.CourseType.PracticeTest.Equals(course.CourseType) || Cws.Shop.Model.Enumerations.CourseType.StudentPass.Equals(course.CourseType) || Cws.Shop.Model.Enumerations.CourseType.MctVoucher.Equals(course.CourseType))
";
            var expected = @"
{% if Cws.Shop.Model.Enumerations.CourseType.PrometricVoucher == course.CourseType or Cws.Shop.Model.Enumerations.CourseType.PracticeTest == course.CourseType or Cws.Shop.Model.Enumerations.CourseType.StudentPass == course.CourseType or Cws.Shop.Model.Enumerations.CourseType.MctVoucher == course.CourseType %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);

        }
        
        [Fact]
        public void If_with_not_and()
        {
            var template = @"
											@if (!Cws.Shop.Model.Enumerations.CourseType.Bundle.Equals(course.CourseType)
                                                 && !Cws.Shop.Model.Enumerations.CourseType.OcmBundle.Equals(course.CourseType)
                                                 && !Cws.Shop.Model.Enumerations.CourseType.CommunityBundle.Equals(course.CourseType))";
            var expected = @"
{% if Cws.Shop.Model.Enumerations.CourseType.Bundle != course.CourseType and Cws.Shop.Model.Enumerations.CourseType.OcmBundle != course.CourseType and Cws.Shop.Model.Enumerations.CourseType.CommunityBundle != course.CourseType %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);

        }
        [Fact]
        public void If_with_toString()
        {
            var template = @"
@if (course.Status == Cws.Shop.Model.Enumerations.OrderStatus.WaitingForPayment.ToString())
";
            var expected = @"
{% if course.Status == Cws.Shop.Model.Enumerations.OrderStatus.WaitingForPayment %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);

        }        
        
        [Fact]
        public void If_not_with_boolean_and_null_or_empty()
        {
            var template = @"
@if (!isMpnOrderDetailsHeaderDisplayed && !string.IsNullOrEmpty(course.CustomerNumberForProductVoucherTransfer))
";
            var expected = @"
{% if isMpnOrderDetailsHeaderDisplayed == false and course.CustomerNumberForProductVoucherTransfer | is_null_or_empty == false %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);

        }
        
        [Fact]
        public void If_elseif_else()
        {
            var template = @"
@if (Cws.Shop.Model.Enumerations.CourseType.PrometricVoucher.Equals(course.CourseType))
		{
			if (course.Status == Cws.Shop.Model.Enumerations.OrderStatus.WaitingForPayment.ToString())
			{
				@Translate(LocalizationKeys.MultiplePages.WaitingForPaymentInfoLiteral_Text)
			}
			else if (course.Status == Cws.Shop.Model.Enumerations.OrderStatus.Delivered.ToString())
			{
				@Translate(LocalizationKeys.OrderConfirmationEmail.DistributePrometricVoucherLiteral_Text)
			}
			else
			{
				@Translate(LocalizationKeys.MultiplePages.PrometricVoucherDistributionDelayInfoLiteral_Text)
			}
		}
";
            var expected = @"
{% if Cws.Shop.Model.Enumerations.CourseType.PrometricVoucher == course.CourseType %}
  {% if course.Status == Cws.Shop.Model.Enumerations.OrderStatus.WaitingForPayment %}

{{ ""LocalizationKeys.MultiplePages.WaitingForPaymentInfoLiteral_Text"" | translate }}

  {% elsif course.Status == Cws.Shop.Model.Enumerations.OrderStatus.Delivered %}
{{ ""LocalizationKeys.OrderConfirmationEmail.DistributePrometricVoucherLiteral_Text"" | translate }}

  {% else %}
{{ ""LocalizationKeys.MultiplePages.PrometricVoucherDistributionDelayInfoLiteral_Text"" | translate }}
  {% endif %}
{% endif %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);

        }
        
        [Fact(Skip = "To hard ;) Manual is easier")]
        public void If_elseif_else_more_complex()
        {
            var template = @"
@if (Cws.Shop.Model.Enumerations.CourseType.PrometricVoucher.Equals(course.CourseType))
		{
			if (course.Status == Cws.Shop.Model.Enumerations.OrderStatus.WaitingForPayment.ToString())
			{
				@Translate(LocalizationKeys.MultiplePages.WaitingForPaymentInfoLiteral_Text)
			}
			else if (course.Status == Cws.Shop.Model.Enumerations.OrderStatus.Delivered.ToString())
			{
				@Translate(LocalizationKeys.OrderConfirmationEmail.DistributePrometricVoucherLiteral_Text)
			}
			else
			{
				@Translate(LocalizationKeys.MultiplePages.PrometricVoucherDistributionDelayInfoLiteral_Text)
			}
		}
else if (Cws.Shop.Model.Enumerations.CourseType.MsLearningProductVoucher.Equals(course.CourseType))
											{
												if (course.Status == Cws.Shop.Model.Enumerations.OrderStatus.WaitingForPayment.ToString())
												{
													@Translate(LocalizationKeys.MultiplePages.WaitingForPaymentInfoLiteral_Text)
												}
}
";
            var expected = @"
{% if Cws.Shop.Model.Enumerations.CourseType.PrometricVoucher == course.CourseType %}
  {% if course.Status == Cws.Shop.Model.Enumerations.OrderStatus.WaitingForPayment %}

{{ ""LocalizationKeys.MultiplePages.WaitingForPaymentInfoLiteral_Text"" | translate }}

  {% elsif course.Status == Cws.Shop.Model.Enumerations.OrderStatus.Delivered %}
{{ ""LocalizationKeys.OrderConfirmationEmail.DistributePrometricVoucherLiteral_Text"" | translate }}

  {% else %}
{{ ""LocalizationKeys.MultiplePages.PrometricVoucherDistributionDelayInfoLiteral_Text"" | translate }}
  {% endif %}
{% elsif Cws.Shop.Model.Enumerations.CourseType.MsLearningProductVoucher = course.CourseType %}
  {% if course.Status == Cws.Shop.Model.Enumerations.OrderStatus.WaitingForPayment %}
{{ ""LocalizationKeys.MultiplePages.PrometricVoucherDistributionDelayInfoLiteral_Text"" | translate }}
  {% endif %}
{% endif %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);

        }
    }
}