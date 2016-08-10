using Microsoft.PowerBI.Api.V1;
using Microsoft.PowerBI.Security;
using Microsoft.Rest;
using PbiPaasWebApi.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace PbiPaasWebApi.Controllers
{
    [RoutePrefix("api")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReportsController : ApiController
    {
        private string workspaceCollectionName;
        private Guid workspaceId;
        private string workspaceCollectionAccessKey;
        private string apiUrl;

        public ReportsController()
        {
            this.workspaceCollectionName = ConfigurationManager.AppSettings["powerbi:WorkspaceCollectionName"];
            this.workspaceId = Guid.Parse(ConfigurationManager.AppSettings["powerbi:WorkspaceId"]);
            this.workspaceCollectionAccessKey = ConfigurationManager.AppSettings["powerbi:WorkspaceCollectionAccessKey"];
            this.apiUrl = ConfigurationManager.AppSettings["powerbi:ApiUrl"];
        }
        // GET: api/Reports
        [HttpGet]
        public async Task<IHttpActionResult> Get([FromUri]bool includeTokens = false)
        {
            var credentials = new TokenCredentials(workspaceCollectionAccessKey, "AppKey");
            using (var client = new PowerBIClient(new Uri(apiUrl), credentials))
            {
                var reportsResponse = await client.Reports.GetReportsAsync(this.workspaceCollectionName, this.workspaceId.ToString());
                var reportsWithTokens = reportsResponse.Value
                    .Select(report =>
                    {
                        string accessToken = null;
                        if(includeTokens)
                        {
                            var embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollectionName, this.workspaceId.ToString(), report.Id);
                            accessToken = embedToken.Generate(this.workspaceCollectionAccessKey);
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
            var credentials = new TokenCredentials(workspaceCollectionAccessKey, "AppKey");
            using (var client = new PowerBIClient(new Uri(apiUrl), credentials))
            {
                var reportsResponse = await client.Reports.GetReportsAsync(this.workspaceCollectionName, this.workspaceId.ToString());
                var report = reportsResponse.Value.FirstOrDefault(r => r.Id == id);
                if(report == null)
                {
                    return BadRequest($"No reports were found matching the id: {id}");
                }

                var embedToken = PowerBIToken.CreateReportEmbedToken(workspaceCollectionName, workspaceId.ToString(), report.Id);
                var accessToken = embedToken.Generate(workspaceCollectionAccessKey);
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

            var credentials = new TokenCredentials(workspaceCollectionAccessKey, "AppKey");
            using (var client = new PowerBIClient(new Uri(apiUrl), credentials))
            {
                var reportsResponse = await client.Reports.GetReportsAsync(this.workspaceCollectionName, this.workspaceId.ToString());
                var reports = reportsResponse.Value.Where(r => r.Name.ToLower().StartsWith(query.ToLower()));

                var reportsWithTokens = reports
                    .Select(report =>
                     {
                         string accessToken = null;
                         if (includeTokens)
                         {
                             var embedToken = PowerBIToken.CreateReportEmbedToken(this.workspaceCollectionName, this.workspaceId.ToString(), report.Id);
                             accessToken = embedToken.Generate(this.workspaceCollectionAccessKey);
                         }

                         return new ReportWithToken(report, accessToken);
                     })
                    .ToList();

                return Ok(reportsWithTokens);
            }
        }
    }
}
