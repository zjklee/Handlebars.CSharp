using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using HandlebarsDotNet;
using HandlebarsDotNet.Extension.CompileFast;
using Newtonsoft.Json.Linq;

namespace HandlebarsNet.Benchmark
{
    public class EndToEnd : IDisposable
    {
        private object _data;
        private Action<TextWriter, object> _pure;
        private Action<TextWriter, object> _fast;

        [Params(2)]
        public int N { get; set; }
        
        [Params("object", "dictionary")]
        public string DataType { get; set; }
        
        [GlobalSetup]
        public void Setup()
        {
            const string template = @"
                childCount={{level1.Count}}
                {{#each level1}}
                    id={{id}}
                    childCount={{level2.Count}}
                    index=[{{@../../index}}:{{@../index}}:{{@index}}]
                    pow1=[{{pow1 @index}}]
                    pow2=[{{pow2 @index}}]
                    pow3=[{{pow3 @index}}]
                    pow4=[{{pow4 @index}}]
                    pow5=[{{#pow5 @index}}empty{{/pow5}}]
                    {{#each level2}}
                        id={{id}}
                        childCount={{level3.Count}}
                        index=[{{@../../index}}:{{@../index}}:{{@index}}]
                        pow1=[{{pow1 @index}}]
                        pow2=[{{pow2 @index}}]
                        pow3=[{{pow3 @index}}]
                        pow4=[{{pow4 @index}}]
                        pow5=[{{#pow5 @index}}empty{{/pow5}}]
                        {{#each level3}}
                            id={{id}}
                            index=[{{@../../index}}:{{@../index}}:{{@index}}]
                            pow1=[{{pow1 @index}}]
                            pow2=[{{pow2 @index}}]
                            pow3=[{{pow3 @index}}]
                            pow4=[{{pow4 @index}}]
                            pow5=[{{#pow5 @index}}empty{{/pow5}}]
                        {{/each}}
                    {{/each}}    
                {{/each}}";

            switch (DataType)
            {
                case "object":
                    _data = new { level1 = ObjectLevel1Generator()};
                    break;
                
                case "dictionary":
                    _data = new Dictionary<string, object>{["level1"] = DictionaryLevel1Generator()};
                    break;
                
                case "json":
                    _data = new JObject {["level1"] = JsonLevel1Generator()};
                    break;
            }

            {
                var pure = Handlebars.Create();
                pure.RegisterHelper("pow1", (context, arguments) => (int)arguments[0] * (int) arguments[0]);
                pure.RegisterHelper("pow2", (output, context, arguments) => output.WriteSafeString(((int)arguments[0] * (int) arguments[0]).ToString()));
                pure.RegisterHelper("pow5", (output, options, context, arguments) => output.WriteSafeString(((int)arguments[0] * (int) arguments[0]).ToString()));

                using (var reader = new StringReader(template))
                {
                    _pure = pure.Compile(reader);
                }
                
                pure.RegisterHelper("pow3", (context, arguments) => (int)arguments[0] * (int) arguments[0]);
                pure.RegisterHelper("pow4", (output, context, arguments) => output.WriteSafeString(((int)arguments[0] * (int) arguments[0]).ToString()));
            }
            
            {
                var fast = Handlebars.Create();
                fast.RegisterHelper("pow1", (context, arguments) => (int)arguments[0] * (int) arguments[0]);
                fast.RegisterHelper("pow2", (output, context, arguments) => output.WriteSafeString(((int)arguments[0] * (int) arguments[0]).ToString()));
                fast.RegisterHelper("pow5", (output, options, context, arguments) => output.WriteSafeString(((int)arguments[0] * (int) arguments[0]).ToString()));
                fast.Configuration.UseCompileFast();

                using (var reader = new StringReader(template))
                {
                    _fast = fast.Compile(reader);
                }
                
                fast.RegisterHelper("pow3", (context, arguments) => (int)arguments[0] * (int) arguments[0]);
                fast.RegisterHelper("pow4", (output, context, arguments) => output.WriteSafeString(((int)arguments[0] * (int) arguments[0]).ToString()));
            }
            
            List<object> ObjectLevel1Generator()
            {
                var level = new List<object>();
                for (int i = 0; i < N; i++)
                {
                    level.Add(new
                    {
                        id = $"{i}",
                        level2 = ObjectLevel2Generator(i)
                    });
                }

                return level;
            }
            
            List<object> ObjectLevel2Generator(int id1)
            {
                var level = new List<object>();
                for (int i = 0; i < N; i++)
                {
                    level.Add(new
                    {
                        id = $"{id1}-{i}",
                        level3 = ObjectLevel3Generator(id1, i)
                    });
                }

                return level;
            }
            
            List<object> ObjectLevel3Generator(int id1, int id2)
            {
                var level = new List<object>();
                for (int i = 0; i < N; i++)
                {
                    level.Add(new
                    {
                        id = $"{id1}-{id2}-{i}"
                    });
                }

                return level;
            }

            List<Dictionary<string, object>> DictionaryLevel1Generator()
            {
                var level = new List<Dictionary<string, object>>();
                for (int i = 0; i < N; i++)
                {
                    level.Add(new Dictionary<string, object>()
                    {
                        ["id"] = $"{i}",
                        ["level2"] = DictionaryLevel2Generator(i)
                    });
                }

                return level;
            }
            
            List<Dictionary<string, object>> DictionaryLevel2Generator(int id1)
            {
                var level = new List<Dictionary<string, object>>();
                for (int i = 0; i < N; i++)
                {
                    level.Add(new Dictionary<string, object>()
                    {
                        ["id"] = $"{id1}-{i}",
                        ["level3"] = DictionaryLevel3Generator(id1, i)
                    });
                }

                return level;
            }
            
            List<Dictionary<string, object>> DictionaryLevel3Generator(int id1, int id2)
            {
                var level = new List<Dictionary<string, object>>();
                for (int i = 0; i < N; i++)
                {
                    level.Add(new Dictionary<string, object>()
                    {
                        ["id"] = $"{id1}-{id2}-{i}"
                    });
                }

                return level;
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
            _pure(TextWriter.Null, _data);
        }

        [Benchmark]
        public void Fast()
        {
            _fast(TextWriter.Null, _data);
        }

        public void Dispose()
        {
            Handlebars.Cleanup();
        }
    }
}