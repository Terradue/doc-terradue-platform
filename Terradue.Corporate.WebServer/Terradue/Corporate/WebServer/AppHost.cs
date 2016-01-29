using System;
using System.IO;
using System.Text;
using System.Xml;
using Funq;
using Mono.Addins;
using ServiceStack.Configuration;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using Terradue.Corporate.WebServer;
using Terradue.ServiceModel.Syndication;

namespace Terradue.Corporate.WebServer {
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    /// <summary>The Singleton AppHost class. Set initial ServiceStack options and register your web services dependencies and run onload scripts</summary>
    public class AppHost
        : AppHostBase {
        /// <summary>AppHost contructor</summary>
        public AppHost()
            : base("Terradue Corporate Web Services", typeof(LoginService).Assembly) {
        }

        /// <summary>Override Configure method</summary>
        public override void Configure(Container container) {
            System.Configuration.Configuration rootWebConfig =
                System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);

            JsConfig.ExcludeTypeInfo = false;
            JsConfig.IncludePublicFields = false;

            //Permit modern browsers (e.g. Firefox) to allow sending of any REST HTTP Method
            base.SetConfig(new EndpointHostConfig {
                GlobalResponseHeaders = {
                    { "Access-Control-Allow-Origin", "*" }
                    //{ "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" }//,
                },
                DebugMode = true, //Enable StackTraces in development
                WebHostUrl = rootWebConfig.AppSettings.Settings["BaseUrl"].Value,
                WriteErrorsToResponse = false, //custom exception handling
                WsdlServiceNamespace = "t2api",
                ReturnsInnerException = false,

            });

            this.ContentTypeFilters.Register("application/opensearchdescription+xml", AppHost.CustomXmlSerializer, null);
            ResponseFilters.Add(CustomResponseFilter);

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

        }

        public static void CustomXmlSerializer(IRequestContext reqCtx, object res, IHttpResponse stream) {
            stream.AddHeader("Content-Encoding", Encoding.Default.EncodingName);
            using (XmlWriter writer = XmlWriter.Create(stream.OutputStream, new XmlWriterSettings() {
                OmitXmlDeclaration = false,
                Encoding = Encoding.Default
            })) {
                new System.Xml.Serialization.XmlSerializer(res.GetType()).Serialize(writer, res);
            }
        }

        /// <summary>
        /// Customs the response filter.
        /// </summary>
        public static void CustomResponseFilter(IHttpRequest request, IHttpResponse response, object responseDto) {
            if (request.QueryString["format"] == "rss") {
                response.ContentType = "application/rss+xml";
            }

            if (request.QueryString["format"] == "atom") {
                response.ContentType = "application/atom+xml";
            }

            if (request.QueryString["format"] == "csv") {
                response.ContentType = "text/csv";
            }
        }

        public static void SerializeToStream(IRequestContext requestContext, object response, Stream stream) {
            var syndicationFeed = response as SyndicationFeed;
            if (syndicationFeed == null) return;

            using (XmlWriter xmlWriter = XmlWriter.Create(stream)) {
                Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(syndicationFeed);
                atomFormatter.WriteTo(xmlWriter);
            }
        }

        public static object DeserializeFromStream(Type type, Stream stream) {
            throw new NotImplementedException();
        }

//        private void ConfigureAuth(Container container)
//        {
//            //Enable and register existing services you want this host to make use of.
//            //Look in Web.config for examples on how to configure your oauth providers, e.g. oauth.facebook.AppId, etc.
//            var appSettings = new AppSettings();
//
//            //Register all Authentication methods you want to enable for this web app.            
//            Plugins.Add(new AuthFeature(
//                () => new CustomUserSession(), //Use your own typed Custom UserSession type
//                new IAuthProvider[] {
//                    new CredentialsAuthProvider(),              //HTML Form post of UserName/Password credentials
//                    new TwitterAuthProvider(appSettings),       //Sign-in with Twitter
//                    new FacebookAuthProvider(appSettings),      //Sign-in with Facebook
//                    new DigestAuthProvider(appSettings),        //Sign-in with Digest Auth
//                    new BasicAuthProvider(),                    //Sign-in with Basic Auth
//                    new T2OpenIdOAuthProvider(appSettings),     //Sign-in with T2
//                    new GoogleOpenIdOAuthProvider(appSettings), //Sign-in with Google OpenId
//                    new YahooOpenIdOAuthProvider(appSettings),  //Sign-in with Yahoo OpenId
//                    new OpenIdOAuthProvider(appSettings),       //Sign-in with Custom OpenId
//                    new GoogleOAuth2Provider(appSettings),      //Sign-in with Google OAuth2 Provider
//                    new LinkedInOAuth2Provider(appSettings),    //Sign-in with LinkedIn OAuth2 Provider
//                }));
//
//            #if HTTP_LISTENER
//            //Required for DotNetOpenAuth in HttpListener 
//            OpenIdOAuthProvider.OpenIdApplicationStore = new InMemoryOpenIdApplicationStore();
//            #endif
//
//        }

    }
}

