using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;

namespace EdlinSoftware.Owin.Markdown
{
    internal class MarkdownMiddleware : OwinMiddleware
    {
        private readonly MarkdownConfiguration _configuration;

        [DebuggerStepThrough]
        public MarkdownMiddleware(OwinMiddleware next, MarkdownConfiguration configuration)
            : base(next)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public override async Task Invoke(IOwinContext context)
        {
            IFileInfo markDownFileInfo = GetMarkdownFileInfo(context.Request.Path.ToString());

            if (markDownFileInfo == null)
            {
                await Next.Invoke(context);

                return;
            }

            using (var reader = new StreamReader(markDownFileInfo.CreateReadStream()))
            {
                var markDownFileContent = reader.ReadToEnd();
                
                var htmlContent = Markdig.Markdown.ToHtml(markDownFileContent, _configuration.Pipeline);

                var htmlTitle = _configuration.TitleGenerator(markDownFileInfo.Name, markDownFileContent, htmlContent);

                context.Response.ContentType = @"text/html";

                // Send our modified content to the response body.
                await context.Response.WriteAsync(_configuration.GetFullHtml(htmlTitle, htmlContent));
            }
        }

        private IFileInfo GetMarkdownFileInfo(string path)
        {
            if (Path.GetExtension(path) != ".md")
                return null;

            var helpPosition = path.IndexOf(_configuration.RequestPath + "/", StringComparison.OrdinalIgnoreCase);
            if (helpPosition < 0)
                return null;

            var markDownPathPart = path.Substring(helpPosition + _configuration.RequestPath.Length + 1);

            if (!_configuration.FileSystem.TryGetFileInfo(markDownPathPart, out var fileInfo))
                return null;

            if (fileInfo.IsDirectory)
                return null;

            return fileInfo;
        }
    }
}