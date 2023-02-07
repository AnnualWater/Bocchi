using HtmlAgilityPack;

namespace Bocchi.HtmlAgilityPack;

public static class HtmlNodeHelper
{
    public static string TryGetText(this HtmlNode node)
    {
        // try
        // {
        //     return node.InnerText;
        // }
        // catch
        // {
        //     return string.Empty;
        // }
        if (node == null || string.IsNullOrEmpty(node.InnerText))
        {
            return string.Empty;
        }

        return node.InnerText;
    }
}