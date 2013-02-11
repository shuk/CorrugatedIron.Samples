using System;
using CorrugatedIron;
using CorrugatedIron.Models;
using CorrugatedIron.Models.Search;
using CorrugatedIron.Models.MapReduce;
using CorrugatedIron.Util;

namespace MiscellaneousSamples
{
    public class RiakSearch
    {
        public void NumberSearch() {
            Console.WriteLine("Beginning NumberSearch:");
            Console.WriteLine("\tAdding 10 items with ages between 21 and 30");

            var Cluster = RiakCluster.FromConfig("riakConfig");
            var Client = Cluster.CreateClient();
            
            var bucket = "rsds";
            
            var props = new RiakBucketProperties()
                .SetSearch(true);
            
            var setResult = Client.SetBucketProperties(bucket, props);
            
            Client.Batch(batch =>
                         {
                for (var i = 1; i < 11; i++)
                {
                    var d = DateTime.Now.AddDays(0 - i);
                    var doc = new RiakObject(bucket, i.ToString(), new { value = i, created_date = d, age_int = 20 + i });
                    
                    batch.Put(doc);
                }
            });

            Console.WriteLine("\tSearching for items between the ages of 19 and 25 (exclusive)");
            var request = new RiakSearchRequest
            {
                Query = new RiakFluentSearch(bucket, "age_int").Between("19", "25").Build()
            };
            
            var result = Client.Search(request);
            
            Console.WriteLine("\tNumberSearch is a success: {0}", result.IsSuccess);
            Console.WriteLine("\tWe found {0} items", result.Value.Documents.Count);
            
            Client.DeleteBucket(bucket);
        }

        public void NumberSearchWithNot() {
            Console.WriteLine("Beginning NumberSearchWithNot:");
            Console.WriteLine("\tAdding 10 items with ages between 21 and 30");
            var Cluster = RiakCluster.FromConfig("riakConfig");
            var Client = Cluster.CreateClient();
            
            var bucket = "rsds";
            
            var props = new RiakBucketProperties()
                .SetSearch(true);
            
            var setResult = Client.SetBucketProperties(bucket, props);
            
            Client.Batch(batch =>
                         {
                for (var i = 1; i < 11; i++)
                {
                    var d = DateTime.Now.AddDays(0 - i);
                    var doc = new RiakObject(bucket, i.ToString(), new { value = i, created_date = d, age_int = 20 + i });
                    
                    batch.Put(doc);
                }
            });

            Console.WriteLine("\tSearching for items NOT between the ages of 19 and 25 (exclusive)");
            var request = new RiakSearchRequest
            {
                Query = new RiakFluentSearch(bucket, "age_int").Between("19", "25").Not().Build()
            };
            
            var result = Client.Search(request);
            
            Console.WriteLine("\tNumberSearchWithNot is a success: {0}", result.IsSuccess);
            Console.WriteLine("\tWe found {0} items", result.Value.Documents.Count);
            
            Client.DeleteBucket(bucket);
        }

        public void InclusiveNumberSearch() {
            Console.WriteLine("Beginning InclusiveNumberSearch:");
            Console.WriteLine("\tAdding 10 items with ages between 21 and 30");
            
            var Cluster = RiakCluster.FromConfig("riakConfig");
            var Client = Cluster.CreateClient();
            
            var bucket = "rsds";
            
            var props = new RiakBucketProperties()
                .SetSearch(true);
            
            var setResult = Client.SetBucketProperties(bucket, props);
            
            Client.Batch(batch =>
                         {
                for (var i = 1; i < 11; i++)
                {
                    var d = DateTime.Now.AddDays(0 - i);
                    var doc = new RiakObject(bucket, i.ToString(), new { value = i, created_date = d, age_int = 20 + i });
                    
                    batch.Put(doc);
                }
            });
            
            Console.WriteLine("\tSearching for items between the ages of 19 and 25 (inclusive)");
            var request = new RiakSearchRequest
            {
                Query = new RiakFluentSearch(bucket, "age_int").Between("19", "25", true).Build()
            };
            
            var result = Client.Search(request);
            
            Console.WriteLine("\InclusiveNumberSearch is a success: {0}", result.IsSuccess);
            Console.WriteLine("\tWe found {0} items", result.Value.Documents.Count);
            
            Client.DeleteBucket(bucket);
        }

        public void InclusiveNumberSearchWithNot() {
            Console.WriteLine("Beginning InclusiveNumberSearchWithNot:");
            Console.WriteLine("\tAdding 10 items with ages between 21 and 30");
            var Cluster = RiakCluster.FromConfig("riakConfig");
            var Client = Cluster.CreateClient();
            
            var bucket = "rsds";
            
            var props = new RiakBucketProperties()
                .SetSearch(true);
            
            var setResult = Client.SetBucketProperties(bucket, props);
            
            Client.Batch(batch =>
                         {
                for (var i = 1; i < 11; i++)
                {
                    var d = DateTime.Now.AddDays(0 - i);
                    var doc = new RiakObject(bucket, i.ToString(), new { value = i, created_date = d, age_int = 20 + i });
                    
                    batch.Put(doc);
                }
            });
            
            Console.WriteLine("\tSearching for items NOT between the ages of 19 and 25 (inclusive)");
            var request = new RiakSearchRequest
            {
                Query = new RiakFluentSearch(bucket, "age_int").Between("19", "25", true).Not().Build()
            };
            
            var result = Client.Search(request);
            
            Console.WriteLine("\tNumberSearchWithNot is a success: {0}", result.IsSuccess);
            Console.WriteLine("\tWe found {0} items", result.Value.Documents.Count);
            
            Client.DeleteBucket(bucket);
        }

        public void DateSearch() {
            Console.WriteLine("Beginning DateSearch:");
            Console.WriteLine("\tAdding 10 items with dates between 10 days ago and yesterday");
            var Cluster = RiakCluster.FromConfig("riakConfig");
            var Client = Cluster.CreateClient();

            var bucket = "rsds";

            var props = new RiakBucketProperties()
                .SetSearch(true);
            
            var setResult = Client.SetBucketProperties(bucket, props);
            
            Client.Batch(batch =>
            {
                for (var i = 1; i < 11; i++)
                {
                    var d = DateTime.Now.AddDays(0 - i);
                    var doc = new RiakObject(bucket, i.ToString(), new { value = i, created_date = d });
                                
                    batch.Put(doc);
                }
            });

            var start = DateTime.Now.AddDays(-4);
            var end = DateTime.Now.AddDays(-2);

            Console.WriteLine("\tSearching for items NOT between the aget of 19 and 25 (exclusive)");
            var request = new RiakSearchRequest
            {
                Query = new RiakFluentSearch(bucket, "created_date").Between(start.ToString("s"), end.ToString("s")).Build()
            };

            var result = Client.Search(request);

            Console.WriteLine("\tDateSearch is a success: {0}", result.IsSuccess);
            Console.WriteLine("\tWe found {0} items", result.Value.Documents.Count);

            Client.DeleteBucket(bucket);
        }
    }
}

