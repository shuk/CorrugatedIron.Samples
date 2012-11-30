using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using CorrugatedIron;
using CorrugatedIron.Extensions;
using CorrugatedIron.Models;
using CorrugatedIron.Models.MapReduce;
using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using Ninject;
using Sample.Autofac;
using Sample.Ninject;
using Sample.Structuremap;
using Sample.TinyIoc;
using Sample.Unity;

namespace Sample
{
    /// <summary>
    /// This is just a sample which shows how to load the CI client via Unity
    /// and demonstrates how you can interact with it to perform various functions.
    /// 
    /// This is <b>not</b> intended to show you the idea way of structuring your
    /// application. That is up to you. This is reference material only.
    /// </summary>
    class Program
    {
        private const string Bucket = "sample_data_bucket";

        static void Main(string[] args)
        {
            Console.WriteLine("Choose a bootstrapper:");
            Console.WriteLine(" 0 : None (manual)");
            Console.WriteLine(" 1 : Unity");
            Console.WriteLine(" 2 : TinyIoC");
            Console.WriteLine(" 3 : Autofac");
            Console.WriteLine(" 4 : Autofac");
            Console.WriteLine(" 5 : Ninject");
            Console.Write("Enter number [0 to 5] > ");

            // get an instance of a RiakClient that we can use
            IRiakClient client;

            switch (Console.ReadLine())
            {
                case "1":
                    {
                        Console.WriteLine("Firing up Unity...");
                        var container = UnityBootstrapper.Bootstrap();
                        client = container.Resolve<IRiakClient>();
                        break;
                    }
                case "2":
                    {
                        Console.WriteLine("Firing up TinyIoC...");
                        var container = TinyIocBootstrapper.Bootstrap();
                        client = container.Resolve<IRiakClient>();
                        break;
                    }
                case "3":
                    {
                        Console.WriteLine("Firing up Autofac...");
                        var container = AutofacBootstrapper.Bootstrap();
                        client = container.Resolve<IRiakClient>();
                        break;
                    }
                case "4":
                    {
                        Console.WriteLine("Firing up StructureMap...");
                        var container = StructuremapBootstrapper.Bootstrap();
                        client = container.GetInstance<IRiakClient>();
                        break;
                    }
                case "5":
                    {
                        Console.WriteLine("Firing up Ninject...");
                        var container = NinjectBootstrapper.Bootstrap();
                        client = container.Get<IRiakClient>();
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Manual configuration...");
                        var cluster = RiakCluster.FromConfig("riakConfig");
                        client = cluster.CreateClient();
                        break;
                    }
            }
            Run(client);
        }

        private static void Run(IRiakClient client)
        {
            // is the server alive?
            Console.WriteLine("Pinging the server ...");
            var pingResult = client.Ping();
            System.Diagnostics.Debug.Assert(pingResult.IsSuccess);

            // here's how you'd go about setting the properties on a bucket
            // (there are lots more than demoed here).
            Console.WriteLine("Setting some bucket properties via REST ...");
            var restProps = new RiakBucketProperties()
                .SetAllowMultiple(true)
                .SetWVal(3);
            client.SetBucketProperties(Bucket, restProps);
            // you'll notice that this is slow, because behind the scenes the client
            // has detected properties that can't be set via the PBC interface
            // so instead it has degraded to the REST api.

            // here's a sample which uses just the PBC properties and hence runs a
            // lot faster.
            Console.WriteLine("Setting some bucket properties via PBC ...");
            var pbcProps = new RiakBucketProperties()
                .SetAllowMultiple(false);
            client.SetBucketProperties(Bucket, pbcProps);

            // we'll keep track of the keys we store as we create them
            var keys = new List<string>();

            // let's write some stuff to Riak, starting with a simple put
            Console.WriteLine("Simple Put ...");
            var simplePutData = CreateData(0);
            var simplePutResponse = client.Put(simplePutData);
            System.Diagnostics.Debug.Assert(simplePutResponse.IsSuccess);
            keys.Add(simplePutData.Key);

            // next write and pull out the resulting object at the same time,
            // and specifying a different write quorum
            var putWithBody = CreateData(1);
            Console.WriteLine("Simple Put with custom quorum ...");
            var putWithBodyResponse = client.Put(putWithBody, new RiakPutOptions { ReturnBody = true, W = 1 });
            System.Diagnostics.Debug.Assert(putWithBodyResponse.IsSuccess);
            System.Diagnostics.Debug.Assert(putWithBodyResponse.Value != null);
            System.Diagnostics.Debug.Assert(putWithBodyResponse.Value.VectorClock != null);
            keys.Add(putWithBody.Key);

            // let's bang out a few more objects to do a bulk load
            var objects = new List<RiakObject>();
            for (var i = 1; i < 11; ++i)
            {
                var obj = CreateData(i);
                objects.Add(obj);
                keys.Add(obj.Key);
            }
            Console.WriteLine("Bulk insert ...");
            var bulkInsertResults = client.Put(objects);
            // verify that they all went in
            foreach (var r in bulkInsertResults)
            {
                System.Diagnostics.Debug.Assert(r.IsSuccess);
            }

            // let's see if we can get out all the objects that we expect to retrieve
            // starting with a simple get:
            Console.WriteLine("Simple Get ...");
            var simpleGetResult = client.Get(Bucket, keys[0]);
            System.Diagnostics.Debug.Assert(simpleGetResult.IsSuccess);
            System.Diagnostics.Debug.Assert(simpleGetResult.Value != null);

            // let's do a bulk get of all the objects we've written so far, again
            // mucking with the quorum value
            var objectIds = keys.Select(k => new RiakObjectId(Bucket, k));
            Console.WriteLine("Bulk Get ...");
            var bulkGetResults = client.Get(objectIds, 1);

            // verify that we got everything
            foreach (var r in bulkGetResults)
            {
                System.Diagnostics.Debug.Assert(r.IsSuccess);
                System.Diagnostics.Debug.Assert(r.Value != null);
            }

            // let's try a map/reduce function, with javascript, to count the
            // number of objects in the bucket
            var sumMapRed = new RiakMapReduceQuery()
                .Inputs(Bucket)
                .MapJs(m => m.Source(@"function(o) {return [ 1 ];}"))
                .ReduceJs(r => r.Name(@"Riak.reduceSum").Keep(true));

            // execute this query with blocking IO, waiting for all the results to come
            // back before we process them
            Console.WriteLine("Blocking map/reduce query ...");
            var blockingMrResult = client.MapReduce(sumMapRed);
            System.Diagnostics.Debug.Assert(blockingMrResult.IsSuccess);
            // next, pull out the phase we're interested in to get the result we want
            var reducePhaseResult = blockingMrResult.Value.PhaseResults.Last().GetObjects<int[]>().SelectMany(p => p).ToArray();
            System.Diagnostics.Debug.Assert(reducePhaseResult[0] == 12);

            // now let's do the same thing, but with the blocking version that streams
            // the results back per phase, rather than waiting for all the reults to
            // be calculated first
            Console.WriteLine("Blocking streaming map/reduce query ...");
            var streamingMRResult = client.StreamMapReduce(sumMapRed);
            System.Diagnostics.Debug.Assert(streamingMRResult.IsSuccess);
            foreach (var result in streamingMRResult.Value.PhaseResults)
            {
                if (result.Phase == 1)
                {
                    var json = JArray.Parse(result.Values[0].FromRiakString());
                    System.Diagnostics.Debug.Assert(json[0].Value<int>() == 12);
                }
            }

            // each of the above methods have an async equivalent that has an extra
            // parameter to pass in which is an Action that takes the result. This
            // is executed on the worker thread that the work is done on. For the
            // sake of this example, we'll only demonstrate how to do this with
            // streaming map/reduce as applying the principle to the other functions
            // is a simple thing to do. All the async methods are exposed via the
            // 'Async' property.

            // create an event to wait on while the results are being processed
            // (usually you wouldn't worry about this in a Ui app, you'd just take
            // the result of the other thread and dispatch it to the UI when processed)
            Console.WriteLine("Starting async streaming map/reduce query ...");
            var task = client.Async.StreamMapReduce(sumMapRed).ContinueWith(HandleStreamingMapReduce);
            Console.WriteLine("Waiting for async streaming map/reduce query result ...");
            task.Wait();

            // finally delete the bucket (this can also be done asynchronously)
            // this calls ListKeys behind the scenes, so it's a very slow process. Riak
            // doesn't currently have the ability to quickly delete a bucket.
            Console.WriteLine("Deleting the whole test bucket ...");
            client.DeleteBucket(Bucket);

            Console.WriteLine("Sample app complete!");
        }

        static void HandleStreamingMapReduce(Task<RiakResult<RiakStreamedMapReduceResult>> task)
        {
            var streamingMrResult = task.Result;
            System.Diagnostics.Debug.Assert(streamingMrResult.IsSuccess);
            foreach (var result in streamingMrResult.Value.PhaseResults)
            {
                Console.WriteLine("Handling async result ...");
                if (result.Phase == 1)
                {
                    System.Diagnostics.Debug.Assert(result.GetObjects<int[]>().First()[0] == 12);
                }
            }
        }

        static RiakObject CreateData(int index)
        {
            return new RiakObject(Bucket, Guid.NewGuid().ToString(), new { index = index, data = Guid.NewGuid().ToString() });
        }
    }
}
