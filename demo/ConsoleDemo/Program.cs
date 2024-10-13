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
var zoneStore = new ZoneStore<MyHasIdRecord>(zoneTree, new IdGenerator(0));
var myRecordStore = new FusionStore<MyHasIdRecord>(zoneStore, fusionCache);

var zoneStore2 = new ZoneStore<MyNonIdRecord>(zoneTree, new IdGenerator(0));
var myRecordStore2 = new FusionStore<MyNonIdRecord>(zoneStore2, fusionCache);


// test with id provided
var myRecord = new MyHasIdRecord(Id: 153, Message: "Hello, World!");

// test without id provided
var myOtherRecord = new MyNonIdRecord("Hello, World!");

var (insertResult, id) = await myRecordStore.Insert(myRecord, CancellationToken.None);
var (insertResult2, id2) = await myRecordStore2.Insert(myOtherRecord, CancellationToken.None);
insertResult.OnSuccess(x => Console.WriteLine("Success: {0} - {1}", x.Id == id, x.Id));
insertResult2.OnSuccess(_ => { Console.WriteLine("Success: {0}", id2); });

var ints = Enumerable.Range(1, 100).ToArray();
foreach (var i in ints)
{
    var record = new MyHasIdRecord(i, $"Hello, {i}");
    var result = await myRecordStore.Save(i, record, CancellationToken.None);
    result.OnSuccess(x => Console.WriteLine("Success: {0}", x.Id));
    result.OnFailure(x => Console.WriteLine($"Failure: {x}"));
}

foreach (var i in ints)
{
    var result = await myRecordStore.Get(i, CancellationToken.None);

    result.OnSuccess(x => Console.WriteLine("Success: {0}", x.Id));
    result.OnFailure(x => Console.WriteLine("Failure: {0}", x));
}