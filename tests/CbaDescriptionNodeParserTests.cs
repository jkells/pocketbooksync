using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PocketBookSync.Exporters;
using Xunit;

namespace PocketBookSync.tests
{
    public class CbaDescriptionNodeParserTests
    {
        [Fact]
        public void parse_description_with_line_break()
        {
            var html = @"<td class=""arrow""><span class=""description""><span class=""merchant""><a href=""javascript: void(0)"" class=""transaction_details icon - expand icon - square - plus""><i role=""presentation"" tabindex="" - 1""><span class=""ScreenReader"">Open transaction details</span></i>PENDING The description</a></span></span><b aria-hidden=""true"" role=""presentation"" class=""barrow"" tabindex="" - 1""><bdo></bdo></b></td>";
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var parser = new CbaDescriptionNodeParser(document.DocumentNode);

            Assert.Equal("The description", parser.Description);
            Assert.Equal(true, parser.IsPending);
        }

        [Fact]
        public void parse_description_with_pending()
        {
            var html = @"<td class=""arrow""><span class=""description""><span class=""merchant"">Direct Credit 123456 Company<br>MY SALARY</span></span><b aria-hidden=""true"" role=""presentation"" class=""barrow"" tabindex="" - 1""><bdo></bdo></b></td>";
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var parser = new CbaDescriptionNodeParser(document.DocumentNode);

            Assert.Equal("Direct Credit 123456 Company MY SALARY", parser.Description);
            Assert.Equal(false, parser.IsPending);
        }

        [Fact]
        public void parse_description_with_link()
        {
            var html = @"<td class=""arrow""><span class=""description""><span class=""merchant""><a href=""javascript: void(0)"" class=""transaction_details icon - expand icon - square - plus""><i role=""presentation"" tabindex="" - 1""><span class=""ScreenReader"">Open transaction details</span></i>Transfer to xx1234 CommBank app</a></span></span><b aria-hidden=""true"" role=""presentation"" class=""barrow"" tabindex="" - 1""><bdo></bdo></b></td>";
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var parser = new CbaDescriptionNodeParser(document.DocumentNode);

            Assert.Equal("Transfer to xx1234 CommBank app", parser.Description);
            Assert.Equal(false, parser.IsPending);
        }
    }
}
