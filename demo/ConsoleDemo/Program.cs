using ConsoleDemo;
using FusionZone;
using FusionZone.Stores.ZoneTree;
using IdGen;
using RickDotNet.Extensions.Base;
using Tenray.ZoneTree;
using Tenray.ZoneTree.Comparers;
using Tenray.ZoneTree.Serializers;
using ZiggyCreatures.Caching.Fusion;

const string dataPath = "C:\\Temp\\FusionZone\\ConsoleDemo\\Data";
if (!Directory.Exists(dataPath))
    Directory.CreateDirectory(dataPath);

var config = new ZoneStoreConfig
{
    StoreName = "MyRecord",
    DataPath = dataPath
};

var fusionCache = new FusionCache(new FusionCacheOptions());
var zoneStore = new ZoneStore<long>(config, new IdGenerator(0));
var myRecordStore = new FusionStore<long>(zoneStore, fusionCache);

var criteria = FilterCriteria.For<MyRecord>(x => x.Id % 2 == 0).Take(20);
//var items = await myRecordStore.List(criteria, CancellationToken.None);
//var count = items.ValueOrDefault()?.Count() ?? 0;

var ids = Enumerable.Range(1, 100).ToArray();
foreach (var i in ids)
{
    var record = new MyRecord(i, $"Hello, {i}");
    var result = await myRecordStore.Save(i, record, CancellationToken.None);
    result.OnSuccess(x => Console.WriteLine("Success: {0}", x.Id));
    result.OnFailure(x => Console.WriteLine($"Failure: {x}"));
}

foreach (var i in ids)
{
    var result = await myRecordStore.Get<MyRecord>(i, CancellationToken.None);
    if (result)
        Console.WriteLine("Success: {0}", i);

    Console.WriteLine("Success: {0}", result.Select(x => x.Id));

    // success
    result.OnSuccess(item => Console.WriteLine("Success: {0}", item.Id));

    // failure or exceptional failure
    result.OnError(error => Console.WriteLine("Failure: {0}", error));

    // success, failure, or exceptional failure
    result.Resolve(
        onSuccess: item => Console.WriteLine("Success: {0}", item.Id),
        onFailure: error => Console.WriteLine("Failure: {0}", error),
        onException: ex => Console.WriteLine("Failure: {0}", ex.Message)
    );

    // down and dirty
    Console.WriteLine("Value: {0}", result.ValueOrDefault()?.Id);
}