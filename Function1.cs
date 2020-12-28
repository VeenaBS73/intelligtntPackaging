using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Xml;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ApplicationInsights;

namespace FunctionApp
{
    public static class Function1
    {
		[FunctionName("Function1")]
		public static HttpResponseMessage Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "01/{gtin}")] HttpRequest req,
			ILogger log, string gtin, ExecutionContext context)
		{
			string requestUrl = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(req);

			log.LogInformation("Request Url", requestUrl);

			string OriginUrl = string.Empty;

			var data = File.ReadAllText(context.FunctionAppDirectory + "/GtinUrl.xml");

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(data);
			foreach (XmlNode node in doc.DocumentElement.ChildNodes)
			{
				if (node.ChildNodes[0]?.InnerText == gtin)
				{
					OriginUrl = node.ChildNodes[1].InnerText;
				}
			}

			log.LogInformation("Redirect Url", OriginUrl);

			HttpRequestMessage request = new HttpRequestMessage();
			var response = request.CreateResponse(HttpStatusCode.TemporaryRedirect);

			if (OriginUrl != null && OriginUrl != "")
			{
				response.Headers.Location = new Uri(OriginUrl);
			}
			else
			{
				return request.CreateResponse(HttpStatusCode.InternalServerError);
			}

			return response;
		}
	}
}
