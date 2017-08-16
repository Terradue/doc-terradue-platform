using System;
using ServiceStack.Common.Web;
using Terradue.Portal;
using System.Web;
using System.Security.Cryptography;

namespace Terradue.Corporate.WebServer {
    public class OAuthUtils {

		public static HttpResult DoRedirect(IfyContext context, string redirect, bool ajax) {
			context.LogDebug(context, string.Format("redirect to {0}", redirect));
			if (ajax) {
				HttpResult redirectResponse = new HttpResult();
				redirectResponse.Headers[HttpHeaders.Location] = redirect;
				redirectResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
				return redirectResponse;
			} else {
				HttpResult redirectResponse = new HttpResult();
				redirectResponse.Headers[HttpHeaders.Location] = redirect;
                redirectResponse.StatusCode = System.Net.HttpStatusCode.Redirect;
				return redirectResponse;
            }
            return null;
		}

		public static string HashHMAC(string key, string msg) {
            var encoding = new System.Text.ASCIIEncoding();
            var bkey = encoding.GetBytes(key);
            var bmsg = encoding.GetBytes(msg);
            var hash = new HMACSHA256(bkey);
            var hashmac = hash.ComputeHash(bmsg);
            return BitConverter.ToString(hashmac).Replace("-", "").ToLower();
        }

    }
}
