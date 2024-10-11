using ConsoleDemo;
using FusionZone;
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
var fusionStore = new FusionStore<MyRecord>(zoneStore, fusionCache);

var ints = Enumerable.Range(1, 100).ToArray();
// foreach (var i in ints)
// {
//     var record = new MyRecord{ Id = 1, Message = $"Hello, {i}" };
//     var result = await fusionStore.Save(i, record, CancellationToken.None);
//     Console.WriteLine($"Result: {result.Error}");
// }

foreach (var i in ints)
{
    var record = await fusionStore.Get(i, CancellationToken.None);
    Console.WriteLine("Success: {0}, Result: {1}", record.Succeeded, record.Value);
}