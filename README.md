[![Build status](https://ci.appveyor.com/api/projects/status/18ht9mlcaak2elbb?svg=true)](https://ci.appveyor.com/project/IvanIakimov/markdown-middleware)

# EdlinSoftware OWIN Markdown middleware

This package contains OWIN middleware that allows providing content of Markdown files as HTML.

If you have some Markdown documents, you can easily present them as HTML pages through your OWIN application.

## If you use Internet Information Services (IIS)...

In case if you use Internet Information Services (IIS) you must make some modifications to your `web.config` file. You see, IIS can provide static files all by itself. For example, if you'll ask for http://myhost/help/root.md, IIS will understand, that there is such a file on the disk. Then it'll try to return it. It means, that IIS will not pass the request to our application. But this is not what is required. You don't want to return raw Markdown file. You want to convert it to HTML first. This is why you need to modify `web.config`. You must instruct IIS, that it should pass all requests to our application. You do it by altering the `system.webServer` section:

```xml
<system.webServer>
  <modules runAllManagedModulesForAllRequests="true" />
  <handlers>
    <remove name="ExtensionlessUrlHandler-Integrated-4.0" />
    <remove name="OPTIONSVerbHandler" />
    <remove name="TRACEVerbHandler" />
    <add name="Owin" verb="" path="*" type="Microsoft.Owin.Host.SystemWeb.OwinHttpHandler, Microsoft.Owin.Host.SystemWeb" />
  </handlers>
</system.webServer>
```

Here you set `runAllManagedModulesForAllRequests` to `true` and add `Owin` handler to process all requests.

## Serving static files

Now IIS will not process static files. But you still need it (e.g. for images in our documentation). This is why you should use `Microsoft.Owin.StaticFiles` NuGet package. Let's say, you want your documentation to be available at path `/api/doc`. In this case, you should configure this package the following way:

```cs
[assembly: OwinStartup(typeof(OwinMarkdown.Startup))]

namespace OwinMarkdown
{
    public class Startup
    {
        private static readonly string HelpUrlPart = "/api/doc";

        public void Configuration(IAppBuilder app)
        {
            var basePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = new PathString(HelpUrlPart),
                FileSystem = new PhysicalFileSystem(Path.Combine(basePath, "Help"))
            });

            ...
        }
    }
}
```

Here it is assumed that your Markdown files are stored in the `Help` subfolder of your application.

## Configuring Markdown middleware

The configuration of the Markdown middleware can be done the following way in the `Configuration` method of the `Startup` class:

```cs
app.AddMarkdown(new MarkdownConfiguration
{
    RequestPath = HelpUrlPart,
    FileSystem = new PhysicalFileSystem(Path.Combine(basePath, "Help"))
});
```

This code should be executed before calling `app.UseStaticFiles`. Otherwise, your Markdown files will be provided 'as is' without conversion to HTML.

The whole code looks like this:

```cs
[assembly: OwinStartup(typeof(OwinMarkdown.Startup))]

namespace OwinMarkdown
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

            ...
        }
    }
}
```

`MarkdownConfiguration` object has the following properties:

* `RequestPath` of type `string`. It defines URL where your Markdown files should be accessible. For example, if you want to access your `root.md` file as http://localhost:500/doc you can set this property to `/doc`.
* `FileSystem` of type `IFileSystem` from the `Microsoft.Owin.FileSystems` package. It defines the location where your Markdown files are. Generally, you can use it the same way as in `Microsoft.Owin.StaticFiles` package.
* `PipelineGenerator` of type `Func<MarkdownPipeline>`. Here class `MarkdownPipeline` is taken from the `Markdig` package. The `MarkdownPipeline` object is used for conversion of Markdown document into HTML. By default, this pipeline is created by command `new MarkdownPipelineBuilder().UseAdvancedExtensions().Build()`. But if you want another pipeline, you can assign your own method for the pipeline creation to the `PipelineGenerator` property.
* `HtmlTemplate` of type `string`. Actually, Markdown files are converted only into a part of a complete HTML page. There will be no `head` or `body` tags there. The `HtmlTemplate` property allows forming of a complete HTML page from a Markdown file. The default value of `HtmlTemplate` property is:

    ```html
    @"<!DOCTYPE html>
    <html lang="en">
        <head>
            <meta charset="utf-8">
            <title>{Title}</title>
        </head>
        <body>
            {Content}
        </body>
    </html>";
    ```

    As you can see, this text contains `{Content}` and `{Title}` placeholders. They will be replaced with the title (returned by `TitleGenerator` function) and content of a Markdown document converted into HTML. You can define your own template (e.g. with a link to your CSS file). Be aware, that presence of `{Content}` placeholder in such a template is mandatory. `{Title}` placeholder can be omitted.

* `TitleGenerator` of type `Func<string, string, string, string>`. It is not entirely clear how to form title for HTML page from a Markdown document. This is what this function for. It has the following parameters:

    * Name of the Markdown file,
    * Content of the Markdown file,
    * HTML generated from the content of the Markdown file.

    By default, this function always returns the word `Documentation`. But you can write your own implementation and assign it to the `TitleGenerator` property.