using RSSJuanK4Blog.Model;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Html;
using Windows.Web.Syndication;

namespace RSSJuanK4Blog.Util
{
    public static class RSSHelper
    {
        public static async Task<ArticleList> GetArticleListFromFeedAsync(string feedUrl)
        {
            var syncClient = new SyndicationClient();
            var lista = new ArticleList();

            var feed = await syncClient.RetrieveFeedAsync(new Uri(feedUrl));
            foreach (var art in feed.Items)
            {
                var content = CreateContent(art.NodeValue);

                {
                    var newArticle = new Article() { Title = art.Title.Text,
                    Content = content,
                    Summary = CreateSummary(art.Summary, content),
                    ImgUri = Find1stImageFromHtml(content) };
                    lista.Add(newArticle);
                }
            }

            return lista;
        }

        private static string CreateContent(string content)
        {
            string htmlString = string.Empty;
            string preHtml = string.Empty;
            string proHtml = string.Empty;

            if (content != null)
            {
                //Hallar el primer tag html
                var htmlBeginIndex = content.IndexOf('<');
                //Recortar la cadena desde el primer tag html hallado
                if (htmlBeginIndex != -1)
                    htmlString = content.Substring(htmlBeginIndex);
            }

            return preHtml + htmlString + proHtml;
        }

        const int MAX_ABSTRACT_LEN = 300;
        private static string CreateSummary(ISyndicationText syndicationText,
            string htmlContent)
        {
            string summaryText = String.Empty;

            if (syndicationText != null && !string.IsNullOrWhiteSpace(syndicationText.Text))
                summaryText = HtmlUtilities.ConvertToText(syndicationText.Text);
            else
                summaryText = HtmlUtilities.ConvertToText(htmlContent);

            summaryText = summaryText.Substring(0, Math.Min(summaryText.Length, MAX_ABSTRACT_LEN));

            return summaryText;
        }

        private static Uri Find1stImageFromHtml(string htmlContent)
        {
            return new Uri(ExtractFirstHtmlImage(htmlContent));
        }

        /// <summary>Default Uri</summary>
        private const string TEMPURI = "http://tempuri.org";

        /// <summary>
        /// Extracts the first image Url from a html string
        /// </summary>
        /// <param name="htmlString">A string containing html code</param>
        /// <returns>a string with the Url or first image in the htmlString parameter</returns>
        /// <remarks>This method uses regular expressions,so using System.Text.RegularExpressions; must be addeed</remarks>
        public static string ExtractFirstHtmlImage(string htmlString)
        {
            string respuesta = TEMPURI;
            try
            {
                var rgx = new Regex(
                    @"<img\b[^>]*?\b(?(1)src|src)\s*=\s*(?:""(?<URL>(?:\\""|[^""])*)""|'(?<URL>(?:\\'|[^'])*)')",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline);

                var match = rgx.Match(htmlString);

                respuesta = match.Groups["URL"].Value;

                if (respuesta == "")
                    respuesta = TEMPURI;
            }
            catch { respuesta = TEMPURI; }

            return respuesta;
        }
    }
}
