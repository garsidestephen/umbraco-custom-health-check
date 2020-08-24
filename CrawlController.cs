using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Website.Models.Crawl;

namespace Website.Controllers
{
    /// <summary>
    /// Crawl Controller
    /// </summary>
    public class CrawlController : BaseController
    {
        /// <summary>
        /// List of site Urls
        /// </summary>
        private List<string> _siteUrls;

        /// <summary>
        /// Site Crawl Results
        /// </summary>
        /// <returns>200 = OK, 500 = Page Issue</returns>
        [HttpGet]
        public ActionResult HealthCheck()
        {
            Dictionary<string, string> siteCrawlResults = CrawlSitePages();

            var overallSiteStatus = HttpStatusCode.InternalServerError;

            if (siteCrawlResults.Any() && siteCrawlResults.Count(x => string.Equals(x.Value, "ok", StringComparison.CurrentCultureIgnoreCase)) == siteCrawlResults.Count)
            {
                overallSiteStatus = HttpStatusCode.OK;
            }

            return new HttpStatusCodeResult(overallSiteStatus);
        }

        /// <summary>
        /// Crawl Site Pages
        /// </summary>
        /// <returns>Dictionary of Results</returns>
        private Dictionary<string, string> CrawlSitePages()
        {
            IPublishedContent homeNode = Umbraco.ContentAtRoot().FirstOrDefault();
            var crawlResults = new Dictionary<string, string>();

            if (homeNode != null)
            {
                _siteUrls = new List<string>();
                _siteUrls.Add($"{App.Settings.TLD}{homeNode.Url}");

                GetNextNodeUrl(homeNode);

                using (var httpClient = new HttpClient())
                {
                    foreach (string siteUrl in _siteUrls)
                    {
                        HttpResponseMessage response = null;

                        try
                        {
                            response = httpClient.GetAsync(siteUrl).Result;
                            crawlResults.Add(siteUrl, response.StatusCode.ToString());
                        }
                        catch (Exception ex)
                        {
                            crawlResults.Add(siteUrl, $"Error: {response.StatusCode.ToString()} | Exception: {ex.Message}");
                        }
                    }
                }
            }

            return crawlResults;
        }

        /// <summary>
        /// Gets Next Node Url
        /// </summary>
        /// <param name="umbracoNode">An Umbraco Node</param>
        private void GetNextNodeUrl(IPublishedContent umbracoNode)
        {
            if (umbracoNode != null)
            {
                var pageNodes = umbracoNode.Descendants().Where(x => x.Value<bool>("NoIndex", fallback: Fallback.ToDefaultValue, defaultValue: false) == false).Where(x => x.Value<bool>("HideInSitemap", fallback: Fallback.ToDefaultValue, defaultValue: false) == false);

                foreach (var node in pageNodes)
                {
                    string urlToAdd = $"{App.Settings.TLD}{node.Url}";

                    if (_siteUrls.FirstOrDefault(x => string.Equals(x, urlToAdd)) == null)
                    {
                        _siteUrls.Add($"{App.Settings.TLD}{node.Url}");

                        if (node.Children.Where(x => x.Value<bool>("NoIndex", fallback: Fallback.ToDefaultValue, defaultValue: false) == false).Where(x => x.Value<bool>("HideInSitemap", fallback: Fallback.ToDefaultValue, defaultValue: false) == false).Count() > 0)
                        {
                            GetNextNodeUrl(node);
                        }
                    }
                }
            }
        }
    }
}
