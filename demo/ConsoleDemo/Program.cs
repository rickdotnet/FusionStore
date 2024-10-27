using ConsoleDemo;
using FusionStore;
using FusionStore.Extensions.Microsoft.Hosting;
using FusionStore.Stores.ZoneTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RickDotNet.Extensions.Base;

// await Demo.Ugly();
const string dataPath = "/tmp/FusionStore//ConsoleDemo/Data";

var config = new FusionStoreConfig { StoreName = "BlobStore" };
var secondConfig = new FusionStoreConfig { StoreName = "DocumentStore" };
var builder = Host.CreateApplicationBuilder();
builder.AddFusionStore(fusionBuilder => fusionBuilder
    .WithKey<long>(store => store
        .WithConfig(config)
        .WithZoneTree(dataPath))
    .WithKey<string>(store => store
        .WithConfig(secondConfig)
    ));

var app = builder.Build();
var store = app.Services.GetRequiredService<IFusionStore<long>>();
var keyedStore = app.Services.GetRequiredKeyedService<IFusionStore<long>>("Fusion-BlobStore");
var secondStore = app.Services.GetRequiredService<IFusionStore<string>>();
var secondKeyedStore = app.Services.GetRequiredKeyedService<IFusionStore<string>>("Fusion-DocumentStore");

var myRecord = new MyRecord(1, "Hello, World");
var result = await keyedStore.Save(1, myRecord, CancellationToken.None);
result.OnSuccess(x => Console.WriteLine("Success: {0}", x.Id));

Console.ReadKey();