using FusionStore;
using FusionStore.Stores.ZoneTree;
using IdGen;
using RickDotNet.Extensions.Base;
using ZiggyCreatures.Caching.Fusion;

namespace ConsoleDemo;

public class Demo
{
    public static async Task Ugly()
    {
        const string dataPath = "/tmp/FusionStore//ConsoleDemo/Data";
        if (!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);

        var fusionStoreConfig = new FusionStoreConfig
        {
            StoreName = "BlobStore",
            SkipCache = false,
            DefaultFusionCacheEntryOptions = new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(5),
            }
        };

        var zoneStoreConfig = new ZoneStoreConfig
        {
            StoreName = fusionStoreConfig.StoreName,
            DataPath = dataPath
        };

        var zoneStore = new ZoneStore<long>(zoneStoreConfig, new IdGenerator(0));
        var blobStore = new FusionStore<long>(zoneStore, fusionStoreConfig);

        var ids = Enumerable.Range(1, 100).ToArray();
        foreach (var i in ids)
        {
            var record = new MyRecord(i, $"Hello, {i}");
            var result = await blobStore.Save(i, record, CancellationToken.None);
            result.OnSuccess(x => Console.WriteLine("Success: {0}", x.Id));
            result.OnFailure(x => Console.WriteLine($"Failure: {x}"));
        }

        var stringResult = await blobStore.Save(120, "this is a string", CancellationToken.None);
        var compareList = await blobStore.List<MyRecord>().ValueOrDefaultAsync();
        Console.WriteLine("List count: {0}", compareList?.Count());

        var criteria = FilterCriteria.For<MyRecord>(x => x.Id % 2 == 0).Take(20);
        var firstEvens = await blobStore.List(criteria).ValueOrDefaultAsync();
        foreach (var item in firstEvens!)
        {
            var result = await blobStore.Delete<MyRecord>(item.Id);
            result.OnSuccess(x => Console.WriteLine("Deleted: {0}", x.Id));
        }

        var list3 = await blobStore.List<MyRecord>();
        var leftOvers = list3.ValueOrDefault()!;
        foreach (var (id, message) in leftOvers)
        {
            Console.WriteLine("Id: {0}, Message: {1}", id, message);
        }

        foreach (var i in ids)
        {
            var result = await blobStore.Get<MyRecord>(i, CancellationToken.None);
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
    }
}