using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Owin;

[assembly: OwinStartup(typeof(EdlinSoftware.Owin.Markdown.Service.Startup))]

namespace EdlinSoftware.Owin.Markdown.Service
{
    public class Startup
    {
        private static readonly string HelpUrlPart = "/api/doc";

        public void Configuration(IAppBuilder app)
        {
            var basePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            app.AddMarkdown(new MarkdownConfiguration
            {
                RequestPath = HelpUrlPart,
                FileSystem = new PhysicalFileSystem(Path.Combine(basePath, "Help"))
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = new PathString(HelpUrlPart),
                FileSystem = new PhysicalFileSystem(Path.Combine(basePath, "Help"))
            });

            HttpConfiguration config = new HttpConfiguration();

            config.Formatters.Clear();
            config.Formatters.Add(
                new JsonMediaTypeFormatter
                {
                    SerializerSettings = GetJsonSerializerSettings()
                });

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            app.UseWebApi(config);
        }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings();

            settings.Converters.Add(new StringEnumConverter { CamelCaseText = false });

            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            return settings;
        }
    }
}