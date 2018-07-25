using Owin;

namespace EdlinSoftware.Owin.Markdown
{
    /// <summary>
    /// Contains extension methods for <see cref="IAppBuilder"/> instance.
    /// </summary>
    public static class AppBuilderExtension
    {
        /// <summary>
        /// Adds Markdown middleware.
        /// </summary>
        /// <param name="app">An instance of <see cref="IAppBuilder"/>.</param>
        /// <param name="configuration">Markdown middleware configuration.</param>
        public static IAppBuilder AddMarkdown(this IAppBuilder app, MarkdownConfiguration configuration)
        {
            return app.Use(typeof(MarkdownMiddleware), configuration);
        }
    }
}