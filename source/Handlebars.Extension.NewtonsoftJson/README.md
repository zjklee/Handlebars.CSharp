Handlebars.Extension.NewtonsoftJson [![Nuget](https://img.shields.io/nuget/v/Handlebars.Extension.NewtonsoftJson)](https://www.nuget.org/packages/Handlebars.Extension.NewtonsoftJson/)
--

## Purpose

Adds proper Newtonsoft Json support to Handlebars.CSharp

### Install
```cmd
dotnet add package Handlebars.Extension.NewtonsoftJson
```

### Usage
```c#
var handlebars = Handlebars.Create();
handlebars.Configuration.UseNewtonsoftJson();
```

### Example
```c#
[Fact]
public void JsonTestObjects()
{
    var model = JObject.Parse("{\"Key1\": \"Val1\", \"Key2\": \"Val2\"}");

    var source = "{{#each this}}{{@key}}{{@value}}{{/each}}";

    var handlebars = Handlebars.Create();
    handlebars.Configuration.UseNewtonsoftJson();

    var template = handlebars.Compile(source);

    var output = template(model);

    Assert.Equal("Key1Val1Key2Val2", output);
}
```

### History
- Requested in [zjklee/Handlebars.CSharp/issues/18](https://github.com/zjklee/Handlebars.CSharp/issues/18)

