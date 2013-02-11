using System;
using System.Linq;
using CorrugatedIron;
using CorrugatedIron.Models;
using CorrugatedIron.Models.MapReduce;
using CorrugatedIron.Models.MapReduce.Inputs;
using CorrugatedIron.Util;

namespace MiscellaneousSamples
{
    public class SecondaryIndices
    {
        public void RangeQuery()
        {
            var Cluster = RiakCluster.FromConfig("riakConfig");
            var Client = Cluster.CreateClient();

            var bucket = "rsds";

            Client.Batch(batch =>
            {
                for (var i = 1; i < 11; i++)
                {
                    var d = DateTime.Now.AddDays(0 - i);
                    var doc = new RiakObject(bucket, i.ToString(), new { value = i, created_date = d });

                    var position = 100 + i;

                    doc.BinIndex("position").Set(position.ToString());
                    
                    batch.Put(doc);
                }
            });

            var query = new RiakMapReduceQuery()
                    .Inputs(RiakIndex.Range("bucket", "position", 100, 200))
                    .MapJs(m => m.Name("Riak.mapValuesJson").Keep(true));

            var result = Client.MapReduce(query);
            //var items = result.Value.PhaseResults.SelectMany(x => x.GetObjects<dynamic>);
        }

    }
}

