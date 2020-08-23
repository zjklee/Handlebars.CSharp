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
            // var execution = new EndToEndJson();
            // execution.N = 1;
            // execution.Setup();
            // //execution.SystemTextJson();
            // for (int i = 0; i < 10000000; i++)
            // {
            //     execution.SystemTextJson();
            // }

            var manualConfig = DefaultConfig.Instance
                .AddJob(Job.MediumRun.WithToolchain(CsProjCoreToolchain.NetCoreApp31).WithLaunchCount(1));
                //.AddJob(Job.MediumRun.WithToolchain(CsProjCoreToolchain.NetCoreApp31).WithNuGet("Handlebars.Net", "1.10.1"));
            
            // var versions = await GetLatestVersions(1);
            // for (var index = 0; index < versions.Length; index++)
            // {
            //     var version = versions[index];
            //     var packageVersion = version.ToString(3);
            //     var job = Job.MediumRun
            //         .WithToolchain(CsProjCoreToolchain.NetCoreApp31)
            //         .WithNuGet("Handlebars.CSharp", packageVersion)
            //         .WithNuGet("Handlebars.Extension.CompileFast", packageVersion);
            //
            //     if (index == 0)
            //     {
            //         job.AsBaseline();
            //     }
            //     
            //     manualConfig.AddJob(job);
            // }
            
            manualConfig.AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByMethod);
            
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, manualConfig);
        }

        private static async Task<Version[]> GetLatestVersions(int count)
        {
            string json;
            using (var httpClient = new HttpClient())
            {
                var responseMessage = await httpClient.GetAsync("https://api-v2v3search-0.nuget.org/search/query?q=packageid:handlebars.csharp&ignoreFilter=true&prerelease=true&take=100");
                json = await responseMessage.Content.ReadAsStringAsync();
            }
            
            var root = JsonConvert.DeserializeObject<Root>(json);
            var versions = root.data.Where(o => !o.Version.Contains("-beta"))
                .Select(o => new Version(o.NormalizedVersion))
                .OrderByDescending(o => o)
                .Take(count)
                .OrderBy(o => o)
                .ToArray();

            return versions;
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