using System;
using Xunit;

namespace EdlinSoftware.Owin.Markdown.Tests
{
    public class MarkdownConfigurationTest
    {
        private readonly MarkdownConfiguration _configuration;

        public MarkdownConfigurationTest()
        {
            _configuration = new MarkdownConfiguration();
        }

        [Fact]
        public void RequestPath_TrimsFinalSlash()
        {
            _configuration.RequestPath = @"/api/doc/";

            Assert.Equal(@"/api/doc", _configuration.RequestPath);
        }

        [Fact]
        public void RequestPath_NeverReturnsNull()
        {
            _configuration.RequestPath = null;

            Assert.Equal(string.Empty, _configuration.RequestPath);
        }

        [Fact]
        public void FileSystem_CantBeNull()
        {
            Assert.Throws<InvalidOperationException>(() => _configuration.FileSystem);
        }

        [Fact]
        public void PipelineGenerator_NeverReturnsNull()
        {
            Assert.NotNull(_configuration.PipelineGenerator);

            _configuration.PipelineGenerator = null;

            Assert.NotNull(_configuration.PipelineGenerator);
        }

        [Fact]
        public void Pipeline_AlwaysReturnsTheSameValue()
        {
            var pipeline1 = _configuration.Pipeline;
            var pipeline2 = _configuration.Pipeline;

            Assert.Same(pipeline1, pipeline2);
        }

        [Fact]
        public void HtmlTemplate_ShouldContainContent()
        {
            var exception = Assert.Throws<ArgumentException>(() => { _configuration.HtmlTemplate = string.Empty; });

            Assert.Contains(@"{Content}", exception.Message);
            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public void HtmlTemplate_ShouldAcceptCorrectTemplate()
        {
            _configuration.HtmlTemplate = @"{Content}";

            Assert.Equal(@"{Content}", _configuration.HtmlTemplate);
        }

        [Fact]
        public void GetFullHtml_ContentIsCorrect()
        {
            _configuration.HtmlTemplate = @"<div>{Content}</div>";

            Assert.Equal(@"<div><h1>Hello</h1></div>", _configuration.GetFullHtml("Title", @"<h1>Hello</h1>"));
        }

        [Fact]
        public void GetFullHtml_TitleIsCorrect()
        {
            _configuration.HtmlTemplate = @"<div>{Title}</div><div>{Content}</div>";

            Assert.Equal(@"<div>Documentation</div><div><h1>Hello</h1></div>", _configuration.GetFullHtml("Documentation", @"<h1>Hello</h1>"));
        }

        [Fact]
        public void TitleGenerator_NeverReturnsNull()
        {
            _configuration.TitleGenerator = null;

            Assert.NotNull(_configuration.TitleGenerator);
        }
    }
}