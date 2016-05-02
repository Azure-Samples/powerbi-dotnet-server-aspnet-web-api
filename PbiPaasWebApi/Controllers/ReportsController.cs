using Microsoft.PowerBI.Api.Beta;
using Microsoft.PowerBI.Security;
using Microsoft.Rest;
using PbiPaasWebApi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace PbiPaasWebApi.Controllers
{
    [RoutePrefix("api")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReportsController : ApiController
    {
        private string workspaceCollection;
        private Guid workspaceId;
        private string signingKey;
        private string apiUrl;

        public ReportsController()
        {
            this.workspaceCollection = ConfigurationManager.AppSettings["powerbi:WorkspaceCollection"];
            this.workspaceId = Guid.Parse(ConfigurationManager.AppSettings["powerbi:WorkspaceId"]);
            this.signingKey = ConfigurationManager.AppSettings["powerbi:SigningKey"];
            this.apiUrl = ConfigurationManager.AppSettings["powerbi:ApiUrl"];
        }
        // GET: api/Reports
        [HttpGet]
        public async Task<IHttpActionResult> Get([FromUri]bool includeTokens = false)
        {
            var devToken = PowerBIToken.CreateDevToken(this.workspaceCollection, this.workspaceId);
            using (var client = this.CreatePowerBIClient(devToken))
            {
                var reportsResponse = await client.Reports.GetReportsAsync(this.workspaceCollection, this.workspaceId.ToString());
                var reportsWithTokens = reportsResponse.Value
                    .Select(report =>
                    {
                        string accessToken = null;
                        if(includeTokens)
                        {
                            var embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollection, this.workspaceId, Guid.Parse(report.Id));
                            accessToken = embedToken.Generate(this.signingKey);
                        }

                        return new ReportWithToken(report, accessToken);
                    })
                    .ToList();

                return Ok(reportsWithTokens);
            }
        }

        // GET: api/Reports/386818d4-f37f-485f-b750-08f982b0c146
        [HttpGet]
        public async Task<IHttpActionResult> Get(string id)
        {
            var devToken = PowerBIToken.CreateDevToken(this.workspaceCollection, this.workspaceId);
            using (var client = this.CreatePowerBIClient(devToken))
            {
                var reportsResponse = await client.Reports.GetReportsAsync(this.workspaceCollection, this.workspaceId.ToString());
                var report = reportsResponse.Value.FirstOrDefault(r => r.Id == id);
                if(report == null)
                {
                    return BadRequest($"No reports were found matching the id: {id}");
                }

                var embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollection, this.workspaceId, Guid.Parse(report.Id));
                var accessToken = embedToken.Generate(this.signingKey);
                var reportWithToken = new ReportWithToken(report, accessToken);

                return Ok(reportWithToken);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> SearchByName([FromUri]string query, [FromUri]bool includeTokens = false)
        {
            if(string.IsNullOrWhiteSpace(query))
            {
                return Ok(Enumerable.Empty<ReportWithToken>());
            }

            var devToken = PowerBIToken.CreateDevToken(this.workspaceCollection, this.workspaceId);
            using (var client = this.CreatePowerBIClient(devToken))
            {
                var reportsResponse = await client.Reports.GetReportsAsync(this.workspaceCollection, this.workspaceId.ToString());
                var reports = reportsResponse.Value.Where(r => r.Name.ToLower().StartsWith(query.ToLower()));

                var reportsWithTokens = reports
                    .Select(report =>
                     {
                         string accessToken = null;
                         if (includeTokens)
                         {
                             var embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollection, this.workspaceId, Guid.Parse(report.Id));
                             accessToken = embedToken.Generate(this.signingKey);
                         }

                         return new ReportWithToken(report, accessToken);
                     })
                    .ToList();

                return Ok(reportsWithTokens);
            }
        }

        private IPowerBIClient CreatePowerBIClient(PowerBIToken token)
        {
            var jwt = token.Generate(signingKey);
            var credentials = new TokenCredentials(jwt, "AppToken");
            var client = new PowerBIClient(credentials)
            {
                BaseUri = new Uri(apiUrl)
            };

            return client;
        }
    }
}
