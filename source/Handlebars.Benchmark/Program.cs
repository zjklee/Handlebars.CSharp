using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using Newtonsoft.Json;

namespace HandlebarsNet.Benchmark
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            // var key = new StructKeyDictionary();
            // key.Setup();
            // for (int i = 0; i < 1000; i++)
            // {
            //     key.FixedSizeDictionaryAdd();
            //     key.Cleanup();
            // }

            // var endToEnd = new EndToEnd();
            // endToEnd.N = 4;
            // endToEnd.DataType = "object";
            // endToEnd.Setup();
            // endToEnd.Default();
            //
            // for (int i = 0; i < 1000000; i++)
            // {
            //     endToEnd.Default();
            // }

            var manualConfig = DefaultConfig.Instance
                .AddJob(Job.MediumRun.WithToolchain(CsProjCoreToolchain.NetCoreApp31).WithLaunchCount(2).WithInvocationCount(16));
            
            // var versions = await GetLatestVersions("Handlebars.CSharp", 1);
            // for (var index = 0; index < versions.Length; index++)
            // {
            //     var version = versions[index];
            //     var packageVersion = version.ToString(3);
            //     var job = Job.MediumRun
            //         .WithToolchain(CsProjCoreToolchain.NetCoreApp31)
            //         .WithLaunchCount(1)
            //         .WithNuGet("Handlebars.CSharp", packageVersion);
            //
            //     //await AddCompatibleVersion(job, "Handlebars.Extension.CompileFast", packageVersion);
            //     await AddCompatibleVersion(job, "Handlebars.Extension.Json", packageVersion);
            //     await AddCompatibleVersion(job, "Handlebars.Extension.NewtonsoftJson", packageVersion);
            //     
            //     manualConfig.AddJob(job);
            // }
            
            manualConfig.AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByMethod);
            
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, manualConfig);
        }

        private static async Task AddCompatibleVersion(Job job, string handlebarsExtensionCompilefast,
            string packageVersion)
        {
            var versions = await GetLatestVersions(handlebarsExtensionCompilefast, 2);
            if(versions.Length == 0) return;
            
            if (versions.Any(o => o.ToString(3) == packageVersion))
            {
                job.WithNuGet(handlebarsExtensionCompilefast, packageVersion);
                return;
            }

            job.WithNuGet(handlebarsExtensionCompilefast, versions.Last().ToString(3));
        }

        private static async Task<Version[]> GetLatestVersions(string package, int count)
        {
            string json;
            using (var httpClient = new HttpClient())
            {
                var responseMessage = await httpClient.GetAsync($"https://api-v2v3search-0.nuget.org/search/query?q=packageid:{package}&ignoreFilter=true&prerelease=true&take=100");
                json = await responseMessage.Content.ReadAsStringAsync();
            }

            try
            {
                var root = JsonConvert.DeserializeObject<Root>(json);
                return root.data.Where(o => !o.Version.Contains("-beta"))
                    .Select(o => new Version(o.NormalizedVersion))
                    .OrderByDescending(o => o)
                    .Take(count)
                    .OrderBy(o => o)
                    .ToArray();
            }
            catch
            {
                return new Version[0];
            }
        }
        
        public class Data    {
            public string Version { get; set; } 
            public string NormalizedVersion { get; set; }
        }

        public class Root    {
            public int totalHits { get; set; } 
            public List<Data> data { get; set; } 
        }
    }
}