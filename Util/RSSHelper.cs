using RSSJuanK4Blog.Model;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Html;
using Windows.Networking.Connectivity;
using Windows.Web.Syndication;

namespace RSSJuanK4Blog.Util
{
    /// <summary>Utilitary to get RSS Feed info an exposed as ArticleCollection, making rtansforms over original data</summary>
    public static class RSSHelper
    {
        #region Constants
        /// <summary>No internet connection Message</summary>
        private const string INTERNET_REQUIRED = "Esta aplicación requiere acceso a internet para funcionar adecuadamente";
        /// <summary>Max size to content abstract</summary>
        private const int MAX_ABSTRACT_LEN = 300;
        /// <summary>Regular expression to get image urls from a HTML string</summary>
        private const string STR_IMGTAG_SRC_EXP = @"<img\s+[^>]*\bsrc\s*\=\s*[\x27\x22](?<Url>[^\x27\x22]*)[\x27\x22]";
        /// <summary>Default Uri</summary>
        private const string TEMPURI = "http://tempuri.org";
        /// <summary>Agg bur string used in some RSS to stablish a request boundary per post</summary>
        private const string AGGBUG = "aggbug";
        #endregion Constants

        #region Fields
        /// <summary>Default image used when no images or invalid images are found</summary>
        private static readonly string DefaultImageUri = (string)App.Current.Resources["void-img-uri"];
        #endregion Fields

        #region Properties
        /// <summary>
        /// Detects Internet Access Connectivity in WinRT
        /// </summary>
        /// <remarks>
        /// This is a Property to put into a class 
        /// requires using Windows.Networking.Connectivity;
        /// </remarks>
        public static bool InternetConectivity
        {
            get
            {
                var prof = NetworkInformation.GetInternetConnectionProfile();
                return prof != null && prof.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Get Articles From Feed
        /// </summary>
        /// <param name="feedUrl"></param>
        /// <returns></returns>
        public static async Task<ArticleList> GetArticleListFromFeedAsync(string feedUrl)
        {
            var syncClient = new SyndicationClient();
            var lista = new ArticleList();

            if (InternetConectivity)
            {
                var feed = await syncClient.RetrieveFeedAsync(new Uri(feedUrl));
                foreach (var art in feed.Items)
                {
                    var content = CreateContent(art.NodeValue);
                    lista.Add(new Article()
                                {
                                    Title = art.Title.Text,
                                    Content = content,
                                    Summary = CreateSummary(art.Summary, content),
                                    ImgUri = Find1stImageFromHtml(content)
                                });
                }
            }
            else
            {
                throw new Exception(INTERNET_REQUIRED);
            }

            return lista;
        }

        /// <summary>
        /// Create content information based con Syndication content
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string CreateContent(string content)
        {
            string htmlString = string.Empty;
            
            #region preHtml assignation
            //used for markup styling 
            const string preHtml = @"<!DOCTYPE html>
                <html>
                <head>
                    <style>
                        html, body {
                            color:white;
                            background-color:#1D1D1D;
                            font-family:'Segoe UI';
                        }
                        mark{
                            background-color:white; 
                        }
                    </style>
                    <link href='http://blogs.msdn.com/cfs-file.ashx/__key/communityserver-blogs-components-weblogfiles/00-00-01-55-68-prism/7345.prism.css' rel='stylesheet' />
                    <script src='http://blogs.msdn.com/cfs-file.ashx/__key/communityserver-blogs-components-weblogfiles/00-00-01-55-68-prism/7183.prism.js'></script>
                </head>
                <body>";
            #endregion preHtml assignation

            string proHtml = @"</body></html>";

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

        /// <summary>
        /// Craeate content abstract based con syndication data
        /// </summary>
        /// <param name="syndicationText"></param>
        /// <param name="htmlContent"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Find first image Uri from html string
        /// </summary>
        /// <param name="htmlContent"></param>
        /// <returns></returns>
        private static Uri Find1stImageFromHtml(string htmlContent)
        {
            return new Uri(ExtractFirstHtmlImage(htmlContent));
        }
        #endregion Methods

        #region Utilitary Methods
        /// <summary>
        /// Extracts the first image Url from a html string
        /// </summary>
        /// <param name="htmlString">A string containing html code</param>
        /// <returns>a collection with the image Urls contained in htmlString parameter</returns>
        /// <remarks>This method uses regular expressions,so using System.Text.RegularExpressions; 
        /// must be addeed</remarks>
        public static List<string> ExtractImageUrisFromHtml(string htmlString)
        {
            const string URLGROUP = "Url";
            var rgx = new Regex(STR_IMGTAG_SRC_EXP,
                                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var lista = new List<string>();
            var matches = rgx.Matches(htmlString);

            foreach (Match match in matches)
            {
                var url = match.Groups[URLGROUP].Value;
                if (!string.IsNullOrWhiteSpace(url) || url.Contains(AGGBUG))
                {
                    lista.Add(
                    match.Groups[URLGROUP].Value);
                }
            }
            return lista;
        }

        /// <summary>
        /// Extracts the first image Url from a html string
        /// </summary>
        /// <param name="htmlString">A string containing html code</param>
        /// <returns>a string with the Url or first image in the htmlString parameter</returns>
        /// <remarks>This method uses regular expressions,so using System.Text.RegularExpressions; must be addeed</remarks>
        public static string ExtractFirstHtmlImage(string htmlString)
        {
            string respuesta = DefaultImageUri;
            try
            {
                var rgx = new Regex(
                    STR_IMGTAG_SRC_EXP,
                                RegexOptions.IgnoreCase | RegexOptions.Multiline);

                var match = rgx.Match(htmlString);

                respuesta = match.Groups["Url"].Value;

                if (string.IsNullOrWhiteSpace(respuesta) || respuesta.Contains(AGGBUG))
                    respuesta = DefaultImageUri;
            }
            catch { respuesta = DefaultImageUri; }

            return respuesta;
        }
        #endregion Utilitary Methods
    }
}
