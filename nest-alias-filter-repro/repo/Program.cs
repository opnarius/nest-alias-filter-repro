﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Bogus.Extensions;
using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;

namespace repo
{
    class Program
    {
        private static ElasticClient client;

        static Program()
        {
            var elasticServer = "http://localhost:9200";
            var pool = new SingleNodeConnectionPool(new Uri(elasticServer));
            var settings = new ConnectionSettings(pool, sourceSerializer: JsonNetSerializer.Default);
            client = new ElasticClient(settings);
        }

        static async Task Main(string[] args)
        {
            var indexName = "eugene-test-repro";

            Console.WriteLine("Press Enter TO Generate DATA!!!!!!!!!!!!!!");
            Console.ReadLine();

            await CreateIndexAndTestData(indexName, 1000);
            // CreateIndexAlias(indexName);

            try
            {
                var docIndex = await client.GetIndexAsync(indexName);
                var typeIndex = docIndex.Indices[indexName];
            }
            catch (Exception ex)
            {
                PrintException(ex);
            }
        }

        private static void CreateIndexAlias(string indexName)
        {
            var json = createAliasBody;
            Console.WriteLine(json);

            var createResponse = client.LowLevel.IndicesUpdateAliasesForAll<StringResponse>(
                PostData.String(json));

            Console.WriteLine($"Create Alias Reponse: {Environment.NewLine}{createResponse}");
        }

        private static string createAliasBody = @"
{
    ""actions"": [
        {
            ""add"": {
                ""index"": "".es-test-repro"",
                ""alias"": "".es-test-repro-alias1"",
                ""filter"": {
                    ""bool"": {
                        ""must_not"": {
                            ""exists"": {
                                ""field"": ""field1""
                            }
                        }
                    }
                }
            }
        }
    ]
}";

        private static async Task CreateIndexAndTestData(string indexName, int count)
        {
            var counter = 1;
            var testUsers = new Faker<User>()
                .RuleFor(x => x.Email, f => f.Person.Email)
                .RuleFor(x => x.FirstName, f => f.Person.FirstName)
                .RuleFor(x => x.LastName, f => f.Person.LastName)
                .RuleFor(x => x.State, f => f.Person.Address.State)
                .RuleFor(x => x.IsActive, f => f.Random.Bool(.8f))
                .RuleFor(x => x.Number, f => counter++)
                .RuleFor(x => x.RegistrationDate, f => f.Date.Recent(100))
                .RuleFor(x => x.EmptyValue, f =>
                {
                    if (counter % 2 == 0)
                        return f.Random.AlphaNumeric(5);
                    if (counter % 3 == 0)
                        return null;
                    return "";
                })
                .RuleFor(x => x.LastOrderDate, f => f.Date.Recent(365).OrNull(f));

            var userBatch = testUsers.Generate(count);

            var bulkResponse = await client.BulkAsync(x => x
                .Index(indexName)
                .IndexMany(userBatch));

            Console.WriteLine(bulkResponse.ToString());
        }

        private static void PrintException(Exception ex)
        {
            if (ex.InnerException != null)
            {
                PrintException(ex.InnerException);
            }

            Console.WriteLine(ex.Message);
        }
    }
}
