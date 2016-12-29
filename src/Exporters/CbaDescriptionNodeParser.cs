using System;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace PocketBookSync.Exporters
{
    public class CbaDescriptionNodeParser
    {
        public CbaDescriptionNodeParser(HtmlNode node)
        {
            var descriptionNode = node.SelectSingleNode(".//span[contains(@class, 'merchant')]");

            if (descriptionNode == null)
            {
                IsValid = false;
                return;
            }

            descriptionNode.SelectNodes(".//i")?.FirstOrDefault()?.Remove();
            descriptionNode.InnerHtml = descriptionNode.InnerHtml.Replace("<br>", " ");
            
            Description = Regex.Replace(descriptionNode.InnerText, @"[\s-]+", " ",
                RegexOptions.Multiline);
            Description = Description.Replace("\r", "").Replace("\n", " ");

            if (Description.StartsWith("PENDING", StringComparison.OrdinalIgnoreCase))
            {
                Description = Description.Substring(8);
                IsPending = true;
            }
            IsValid = true;
        }

        public bool IsPending { get; private set; }
        public string Description { get; } = "";

        public bool IsValid { get; private set; }
    }
}