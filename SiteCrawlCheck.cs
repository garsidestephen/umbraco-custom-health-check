using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Website;

namespace Umbraco.Web.HealthCheck.Checks.Services
{
    /// <summary>
    /// Custom Site Crawl Health Check 
    /// </summary>
    [HealthCheck(
        "eddb42dc-2767-4aa6-9620-e04e210357f9",
        "Site Crawl",
        Description = "Checks all pages are responding with a http200.",
        Group = "Custom")]
    public class SiteCrawlCheck : HealthCheck
    {
        /// <summary>
        /// Site Crawl Check Constructor
        /// </summary>
        public SiteCrawlCheck()
        {
        }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns>Health Check Status</returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckSiteCrawl() };
        }

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        /// <param name="action">Health Check Action</param>
        /// <returns></returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("SiteCrawlCheck has no executable actions");
        }

        /// <summary>
        /// Call Site Crawl Check Endpoint
        /// </summary>
        /// <returns>Health Check Status</returns>
        private HealthCheckStatus CheckSiteCrawl()
        {
            HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError;

            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpStatusCode = httpClient.GetAsync($"{App.Settings.TLD}/crawl/health-check").Result.StatusCode;
                }
                catch (Exception)
                {
                }
            }

            string message = httpStatusCode == HttpStatusCode.OK ? "All site pages available to crawl" : "Error on 1 or many pages on site crawl";

            return new HealthCheckStatus(message)
            {
                ResultType = httpStatusCode == HttpStatusCode.OK ? StatusResultType.Success : StatusResultType.Error,
                Actions = new List<HealthCheckAction>()
            };
        }
    }
}
