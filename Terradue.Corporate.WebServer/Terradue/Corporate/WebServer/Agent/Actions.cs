using System;
using Terradue.Portal;
using System.Net;
using System.IO;
using Terradue.Util;

namespace Terradue.Corporate.WebServer {
    public class Actions {

        /// <summary>
        /// Cleans the DB Cookies which have an expired validity date.
        /// </summary>
        /// <param name="context">Context.</param>
        public static void CleanDBCookies(IfyContext context) {
            string sql = String.Format ("DELETE * FROM cookie WHERE expire < {0};", StringUtils.EscapeSql (DateTime.UtcNow.ToString (@"yyyy\-MM\-dd\THH\:mm\:ss")));
            context.Execute (sql);
        }
    }
}

