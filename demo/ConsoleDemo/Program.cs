using ConsoleDemo;
using FusionStore;
using FusionStore.Extensions.Microsoft.Hosting;
using FusionStore.Stores.Sqlite;
using FusionStore.Stores.ZoneTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RickDotNet.Extensions.Base;

// await Demo.Ugly();
const string dataPath = "/tmp/FusionStore//ConsoleDemo/Data";

var zoneConfig = new FusionStoreConfig { StoreName = "ZoneStore" };
var sqliteConfig = new FusionStoreConfig { StoreName = "SqliteStore" };

var builder = Host.CreateApplicationBuilder();
builder.AddFusionStore(fusionBuilder => fusionBuilder
    .WithKey<long>(store => store
        .WithConfig(zoneConfig)
        .WithZoneTree(dataPath))
    .WithKey<long>(store => store
        .WithConfig(sqliteConfig)
        .WithSqlite(dataPath))
);

var app = builder.Build();
var store1 = app.Services.GetRequiredService<IFusionStore<long>>();
var keyedStore1 = app.Services.GetRequiredKeyedService<IFusionStore<long>>("Fusion-BlobStore");
var lookedUpStore1 = app.Services.GetRequiredService<IEnumerable<IFusionStore<long>>>().FirstOrDefault(x => x.StoreName == "BlobStore");

var secondStore = app.Services.GetRequiredService<IFusionStore<string>>();
var secondKeyedStore = app.Services.GetRequiredKeyedService<IFusionStore<string>>("Fusion-DocumentStore");
var secondLookedUpStore = app.Services.GetRequiredService<IEnumerable<IFusionStore<string>>>().FirstOrDefault(x => x.StoreName == "DocumentStore");

var myRecord = new MyRecord(1, "Hello, World");
var result = await keyedStore1.Save(1, myRecord);
result.OnSuccess(x => Console.WriteLine("Success: {0}", x.Id));

Console.ReadKey();