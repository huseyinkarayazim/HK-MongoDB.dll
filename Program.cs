using HK_MongoDB.Collections;
using System;


namespace HK_MongoDB
{
    internal class Program
    {
        static void Main(string[] args)
        {

            try
            {

                Builders.BuildCollections(Enum.RequestSource.Server);                
                ExampleCollection exampleCollection = new ExampleCollection();
                exampleCollection.Id = MongoProcessor.GetObjectId();
                exampleCollection.ExampleField = "Test";
                if (MongoProcessor.Add<ExampleCollection>(exampleCollection))
                    Console.WriteLine("OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
