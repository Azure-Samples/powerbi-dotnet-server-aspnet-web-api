using Microsoft.PowerBI.Api.V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PbiPaasWebApi.Models
{
    public class ReportWithToken : Report
    {
        public string Type { get; set; }
        public string AccessToken { get; set; }

        public ReportWithToken(Report report, string accessToken = null)
            : base(report.Id, report.Name, report.WebUrl, report.EmbedUrl)
        {
            Type = "report";
            AccessToken = accessToken;
        }
    }
}