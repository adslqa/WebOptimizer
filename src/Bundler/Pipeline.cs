﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using NUglify.Css;
using NUglify.JavaScript;

namespace Bundler
{
    internal class Pipeline : IPipeline
    {
        public Pipeline(IHostingEnvironment env)
        {
            EnabledBundling = true;
            EnableCaching = !env.IsDevelopment();
        }

        public bool EnabledBundling { get; set; }

        public bool EnableCaching { get; set; }

        public IPipeline Add(IAsset asset)
        {
            AssetManager.Assets.Add(asset);

            return this;
        }

        public IPipeline Add(IEnumerable<IAsset> asset)
        {
            AssetManager.Assets.AddRange(asset);

            return this;
        }

        public IAsset Add(string route, string contentType, params string[] sourceFiles)
        {
            string[] sources = sourceFiles;

            if (sourceFiles.Length == 0)
            {
                sources = new[] { route };
            }

            IAsset asset = Asset.Create(route, contentType, sources);
            AssetManager.Assets.Add(asset);

            return asset;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IPipeline"/>.
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Adds a JavaScript with minification asset to the pipeline.
        /// </summary>
        public static IAsset AddJs(this IPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddJs(route, new CodeSettings(), sourceFiles);
        }

        /// <summary>
        /// Adds a JavaScript with minification asset to the pipeline.
        /// </summary>
        public static IAsset AddJs(this IPipeline pipeline, string route, CodeSettings settings, params string[] sourceFiles)
        {
            return pipeline.Add(route, "application/javascript", sourceFiles)
                           .Bundle()
                           .MinifyJavaScript(settings);
        }

        /// <summary>
        /// Adds a CSS asset with minification to the pipeline.
        /// </summary>
        public static IAsset AddCss(this IPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddCss(route, new CssSettings(), sourceFiles);
        }

        /// <summary>
        /// Adds a CSS asset with minification to the pipeline.
        /// </summary>
        public static IAsset AddCss(this IPipeline pipeline, string route, CssSettings settings, params string[] sourceFiles)
        {
            return pipeline.Add(route, "text/css", sourceFiles)
                           .Bundle()
                           .MinifyCss(settings);
        }

        /// <summary>
        /// Adds loose files to the optimization pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline object.</param>
        /// <param name="contentType">The content type of the response. Example: "text/css".</param>
        /// <param name="sourceFiles">A list of relative file names of the sources to optimize.</param>
        /// <returns></returns>
        public static IEnumerable<IAsset> AddFiles(this IPipeline pipeline, string contentType, params string[] sourceFiles)
        {
            var list = new List<IAsset>();

            foreach (string file in sourceFiles)
            {
                IAsset asset = pipeline.Add(file, contentType, file);
                asset.Processors.Add(new Concatinator());
                list.Add(asset);
            }

            return list;
        }
    }
}
