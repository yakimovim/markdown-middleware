using System;
using System.Diagnostics;
using Markdig;
using Microsoft.Owin.FileSystems;

namespace EdlinSoftware.Owin.Markdown
{
    /// <summary>
    /// Configuration of OWIN Markdown Middleware.
    /// </summary>
    public class MarkdownConfiguration
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _requestPath = string.Empty;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IFileSystem _fileSystem;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<MarkdownPipeline> _pipelineGenerator = DefaultPipelineGenerator;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private MarkdownPipeline _pipeline;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _htmlTemplate = DefaultHtmlTemplate;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<string, string, string, string> _titleGenerator = DefaultTitleGenerator;

        private static MarkdownPipeline DefaultPipelineGenerator() => new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        
        private static string DefaultHtmlTemplate = 
@"<!DOCTYPE html>
<html lang=""en"">
    <head>
        <meta charset=""utf-8"">
        <title>{Title}</title>
    </head>
    <body>
        {Content}
    </body>
</html>";

        private static string DefaultTitleGenerator(string fileName, string markdown, string html) => "Documentation";

        /// <summary>
        /// Gets or sets part of URL where Markdown files should be served.
        /// </summary>
        /// <remarks>You can place or omit final '/'.</remarks>
        /// <example>
        /// Let's say you want your Markdown files to be served from http://www.example.com/api/doc
        /// (e.g. http://www.example.com/api/doc/documentation.md). In this case, you should set this
        /// property to "/api/doc".
        /// </example>
        public string RequestPath
        {
            [DebuggerStepThrough]
            get => _requestPath;
            [DebuggerStepThrough]
            set => _requestPath = (value ?? string.Empty).TrimEnd('/');
        }

        /// <summary>
        /// Gets or sets file system for the folder with Markdown files.
        /// </summary>
        /// <example>
        /// Let's say you want your Markdown files to be served from http://www.example.com/api/doc
        /// (e.g. http://www.example.com/api/doc/documentation.md). In this case, you should set this
        /// property to the folder where documentation.md exists.
        /// </example>
        public IFileSystem FileSystem
        {
            [DebuggerStepThrough]
            get => _fileSystem ?? throw new InvalidOperationException($"{nameof(FileSystem)} property can't be null.");
            [DebuggerStepThrough]
            set => _fileSystem = value;
        }

        /// <summary>
        /// Gets or sets generator of Markdown pipeline.
        /// </summary>
        /// <remarks>
        /// Default value is 'new MarkdownPipelineBuilder().UseAdvancedExtensions().Build()'.
        /// </remarks>
        public Func<MarkdownPipeline> PipelineGenerator
        {
            [DebuggerStepThrough]
            get => _pipelineGenerator;
            [DebuggerStepThrough]
            set => _pipelineGenerator = value ?? DefaultPipelineGenerator;
        }

        /// <summary>
        /// Gets Markdown pipeline.
        /// </summary>
        internal MarkdownPipeline Pipeline => _pipeline ?? (_pipeline = PipelineGenerator());

        /// <summary>
        /// Gets or sets HTML template for Markdown page.
        /// </summary>
        public string HtmlTemplate
        {
            [DebuggerStepThrough]
            get => _htmlTemplate;
            set
            {
                value = value ?? string.Empty;

                if(!HtmlTemplateIsValid(value))
                    throw new ArgumentException($"Value of {nameof(HtmlTemplate)} property should contain '{{Content}}' string", nameof(value));

                _htmlTemplate = value;
            } 
        }

        private bool HtmlTemplateIsValid(string value) => value.Contains(@"{Content}");

        /// <summary>
        /// Combines full text of HTML from HTML template, title and result of conversion of a Markdown document into HTML.
        /// </summary>
        /// <param name="htmlTitle">Title of HTML document.</param>
        /// <param name="htmlContent">Content of HTML document.</param>
        internal string GetFullHtml(string htmlTitle, string htmlContent)
        {
            return HtmlTemplate
                .Replace("{Content}", htmlContent)
                .Replace("{Title}", htmlTitle);
        }

        /// <summary>
        /// Gets or sets function for generation of title of HTML document created from a Markdown.
        /// </summary>
        /// <remarks>
        /// Parameters of the function are:
        /// * Name of the Markdown file.
        /// * Content of the Markdown file.
        /// * HTML generated from the content of the Markdown file.
        /// </remarks>
        public Func<string, string, string, string> TitleGenerator
        {
            [DebuggerStepThrough]
            get => _titleGenerator;
            [DebuggerStepThrough]
            set => _titleGenerator = value ?? DefaultTitleGenerator;
        }
    }
}