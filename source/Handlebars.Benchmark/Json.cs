using System;
using System.IO;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using HandlebarsDotNet;
// using HandlebarsDotNet.Extension.Json;
// using HandlebarsDotNet.Extension.NewtonsoftJson;
using Newtonsoft.Json.Linq;

namespace HandlebarsNet.Benchmark
{
    public class Json : IDisposable
    {
        private string _json;
        private Action<TextWriter, object> _default;
        private Action<TextWriter, object> _systemJson;
        private Action<TextWriter, object> _newtonsoft;

        [Params(2, 5, 10)]
        public int N { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            const string template = @"
                {{#each level1}}
                    id={{id}}
                    {{#each level2}}
                        id={{id}}
                        {{#each level3}}
                            id={{id}}
                        {{/each}}
                    {{/each}}    
                {{/each}}";

            _json = (new JObject {["level1"] = JsonLevel1Generator()}).ToString();

            {
                var handlebars = Handlebars.Create();

                using var reader = new StringReader(template);
                _default = handlebars.Compile(reader);
            }
            
            {
                var handlebars = Handlebars.Create();
                handlebars.Configuration.UseJson();

                using var reader = new StringReader(template);
                _systemJson = handlebars.Compile(reader);
            }
            
            {
                var handlebars = Handlebars.Create();
                handlebars.Configuration.UseNewtonsoftJson();

                using var reader = new StringReader(template);
                _newtonsoft = handlebars.Compile(reader);
            }
            
            JArray JsonLevel1Generator()
            {
                var level = new JArray();
                for (int i = 0; i < N; i++)
                {
                    level.Add(new JObject
                    {
                        ["id"] = $"{i}",
                        ["level2"] = JsonLevel2Generator(i)
                    });
                }

                return level;
            }
            
            JArray JsonLevel2Generator(int id1)
            {
                var level = new JArray();
                for (int i = 0; i < N; i++)
                {
                    level.Add(new JObject
                    {
                        ["id"] = $"{id1}-{i}",
                        ["level3"] = JsonLevel3Generator(id1, i)
                    });
                }

                return level;
            }
            
            JArray JsonLevel3Generator(int id1, int id2)
            {
                var level = new JArray();
                for (int i = 0; i < N; i++)
                {
                    level.Add(new JObject()
                    {
                        ["id"] = $"{id1}-{id2}-{i}"
                    });
                }

                return level;
            }
        }
        
        [Benchmark]
        public void Default()
        {
            var document = JObject.Parse(_json);
            _default(TextWriter.Null, document);
        }
        
        [Benchmark]
        public void SystemTextJson()
        {
            var document = JsonDocument.Parse(_json);
            _systemJson(TextWriter.Null, document);
        }

        [Benchmark]
        public void NewtonsoftJson()
        {
            var document = JObject.Parse(_json);
            _newtonsoft(TextWriter.Null, document);
        }

        [GlobalCleanup]
        public void Dispose() => Handlebars.Cleanup();
    }
}