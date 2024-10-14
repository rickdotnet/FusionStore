using ConsoleDemo;
using FusionZone;
using FusionZone.Abstractions;
using FusionZone.Stores.ZoneTree;
using IdGen;
using Tenray.ZoneTree;
using Tenray.ZoneTree.Comparers;
using Tenray.ZoneTree.Serializers;
using ZiggyCreatures.Caching.Fusion;

const string dataPath = "C:\\Temp\\FusionZone\\ConsoleDemo\\Data";
if (!Directory.Exists(dataPath))
    Directory.CreateDirectory(dataPath);

var zoneTree = new ZoneTreeFactory<long, string>()
    .SetComparer(new Int64ComparerAscending())
    .SetDataDirectory(dataPath)
    .SetKeySerializer(new Int64Serializer())
    .SetValueSerializer(new Utf8StringSerializer())
    .OpenOrCreate();

var fusionCache = new FusionCache(new FusionCacheOptions());
var zoneStore = new ZoneStore<MyRecord>(zoneTree, new IdGenerator(0));
var myRecordStore = new FusionStore<MyRecord>(zoneStore, fusionCache);

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
    var result = await myRecordStore.Get(i, CancellationToken.None);
    if (result)
        Console.WriteLine("Success: {0}", i);

    result.Select(x => x.Id).OnSuccess(x => Console.WriteLine("Success: {0}", x));

    result.OnSuccess(item => Console.WriteLine("Success: {0}", item.Id));
    result.OnFailure(ex => Console.WriteLine("Failure: {0}", ex.Message));

    result.Resolve(
        onSuccess: item => Console.WriteLine("Success: {0}", item.Id),
        onFailure: ex => Console.WriteLine("Failure: {0}", ex.Message)
    );
}