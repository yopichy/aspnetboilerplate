using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

#nullable enable

namespace AbpAspNetCoreDemo.IntegrationTests;

public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Point the test host content root to the real web project so static files
        // and plugin DLLs (Plugins folder) are discoverable by application startup.
        var baseDir = AppContext.BaseDirectory;

        // Try to find the repository root by searching upward for Abp.sln.
        var dirInfo = new DirectoryInfo(baseDir);
        DirectoryInfo? repoRoot = null;
        while (dirInfo != null)
        {
            if (File.Exists(Path.Combine(dirInfo.FullName, "Abp.sln")))
            {
                repoRoot = dirInfo;
                break;
            }
            dirInfo = dirInfo.Parent;
        }

        var solutionDir = repoRoot?.FullName ?? Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        var webProjectRelative = Path.Combine("test", "aspnet-core-demo", "AbpAspNetCoreDemo");
        var webProjectPath = Path.Combine(solutionDir, webProjectRelative);

        // Try one alternate common parent if the primary guess fails.
        if (!Directory.Exists(webProjectPath))
        {
            var alt = Path.GetFullPath(Path.Combine(baseDir, "..", ".."));
            var altPath = Path.Combine(alt, webProjectRelative);
            if (Directory.Exists(altPath))
            {
                webProjectPath = altPath;
            }
        }

        if (Directory.Exists(webProjectPath))
        {
            builder.UseContentRoot(webProjectPath);
        }
        else
        {
            Console.WriteLine("WARNING: webProjectPath not found: " + webProjectPath);
        }

        builder.ConfigureServices(services =>
        {
            services.BuildServiceProvider();
        });
    }
}