using Serilog;
using System.Diagnostics;
using System.Text;

namespace ArticleParser
{
    internal class PageParser
    {
        public static void Parse(string url)
        {
            Task<HttpResponseMessage> response = new HttpClient().GetAsync(url);

            response.Wait();

            if(response.Result.IsSuccessStatusCode)
            {
                SanitizeHtml(response.Result.Content);
            }
            else
            {
                Log.Logger.Error($"Failed to fetch data from {url}.");
            }
        }

        private static void SanitizeHtml(HttpContent content)
        {
            Log.Logger.Debug("Begining to parse Page.");
            Stopwatch totalTime = new Stopwatch();
            totalTime.Start();

            string contentString = content.ReadAsStringAsync().Result;

            int mainStart = contentString.IndexOf("<main");
            int mainEnd = contentString.IndexOf("</main>");

            contentString = contentString.Substring(mainStart, mainEnd - mainStart);

            int titleStart = contentString.IndexOf("<h1");
            contentString = contentString.Substring(titleStart, contentString.Length - titleStart);

            FilterOutTextPre(ref contentString);
            TitleAdjustment(ref contentString);

            List<string> htmlElementsDeleteContents = new List<string>()
            {
                "script",
                "button",
                "svg",
                "form",
                "input",
                "source",
                "figcaption",
                "img",
                "picture",
                "footer",
            };

            foreach(string htmlElement in htmlElementsDeleteContents)
            {
                RemoveHtmlDeleteContents(htmlElement, ref contentString);
            }

            Dictionary<string, bool> htmlElementsKeepContents = new Dictionary<string, bool>
            {
                { "div", true },
                { "a", true },
                { "span", true },
                { "header", true },
                { "section", true },
                { "main", true },
                { "ul", true },
                { "li", true },
                { "p", true },
                { "h1", true },
                { "h2", true },
                { "h3", true },
                { "h4", true },
                { "h5", true },
                { "h6", true },
                { "strong", true },
                { "figure", true },
                { "time", true },
                { "aside", true },
                { "article", true },
            };

            while (htmlElementsKeepContents.Values.Contains(true))
            {
                foreach(string key in htmlElementsKeepContents.Keys)
                {
                    if(htmlElementsKeepContents[key] == true)
                        htmlElementsKeepContents[key] = RemoveHtmlKeepContents(key, ref contentString);
                }
            }

            FilterOutTextPost(ref contentString);

            totalTime.Stop();
            Log.Logger.Debug($"Finished Parsing Page in {totalTime.Elapsed.TotalSeconds} seconds");
        }

        private static void TitleAdjustment(ref string contentString)
        {
            //Summary
            int summaryOpenStart = contentString.IndexOf("<p id=\"article-summary\"");
            int summaryOpenEnd = contentString.IndexOf(">", summaryOpenStart) + 1;

            contentString = contentString.Replace(contentString.Substring(summaryOpenStart, summaryOpenEnd - summaryOpenStart), ". ");

            //Time stamp
            int timeIndexOpenStart = contentString.IndexOf("<time");
            int timeIndexOpenEnd = contentString.IndexOf(">", timeIndexOpenStart) + 1;

            contentString = contentString.Replace(contentString.Substring(timeIndexOpenStart, timeIndexOpenEnd - timeIndexOpenStart), " ");
            contentString = contentString.Replace("</time>", ". ");
        }

        private static bool RemoveHtmlKeepContents(string htmlElement, ref string contentString)
        {
            if(contentString.Contains($"<{htmlElement}") || contentString.Contains($"</{htmlElement}>"))
            {
                int htmlElementOpenStart = contentString.IndexOf($"<{htmlElement}");
                int htmlElementOpenEnd = contentString.IndexOf(@">") + 1;

                if (htmlElementOpenStart > -1 && htmlElementOpenStart < htmlElementOpenEnd)
                    contentString = contentString.Remove(htmlElementOpenStart, htmlElementOpenEnd - htmlElementOpenStart);

                int htmlElementCloseStart = contentString.IndexOf($"</{htmlElement}>");
                int htmlElementCloseEnd = htmlElementCloseStart + htmlElement.Length + 3;

                if (htmlElementCloseStart > 0)
                    contentString = contentString.Remove(htmlElementCloseStart, htmlElementCloseEnd - htmlElementCloseStart);

                return true;
            }

            return false;
        }

        private static void RemoveHtmlDeleteContents(string htmlElement, ref string contentString)
        {
            while (contentString.Contains($"<{htmlElement}"))
            {
                int htmlElementStart = contentString.IndexOf($"<{htmlElement}");
                int htmlElementEnd = contentString.IndexOf($"</{htmlElement}>");

                if (htmlElementStart <= 0 && htmlElementEnd > 0)
                    htmlElementStart = htmlElementEnd;

                if (htmlElementEnd > 0)
                    htmlElementEnd += htmlElement.Length + 3;
                else
                    htmlElementEnd = contentString.IndexOf($"/>") + 2;

                contentString = contentString.Remove(htmlElementStart, htmlElementEnd - htmlElementStart);
            }
        }

        private static void FilterOutTextPre(ref string contentString)
        {
            List<string> textToFilter = new List<string>()
            {
                "<p>Advertisement</p>"
            };

            foreach (string text in textToFilter)
            {
                contentString = contentString.Replace(text, " ");
            }
        }

        private static void FilterOutTextPost(ref string contentString)
        {
            List<string> textToFilter = new List<string>()
            {
                "Continue reading the main story",
                "As a subscriber, you have " +
                "10 gift articles",
                " to give each month. Anyone can read what you share.",
                "Send any friend a story"
            };

            foreach(string text in textToFilter)
            {
                contentString = contentString.Replace(text, " ");
            }
        }
    }
}
