using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

class Program
{
    static void Main()
    {
        #region APIConfig
        string apiKey = "05a4e5dde1b246a48fc71057231705";
        string city = "Sonderborg";
        #endregion

        #region DontLook
        string mongoConnectionString = "mongodb+srv://<user>:<Password>@cluster0.rsfq3yg.mongodb.net/?retryWrites=true&w=majority";
        string databaseName = "WeatherDataCollection";
        string collectionName = "WeatherCity";
        #endregion

        bool extractFromMongoDB = false;

        Timer timer = new Timer(async (_) =>
        {
            JObject weatherData = await ExtractApiData(apiKey, city);

            await LoadRawToMongoDB(weatherData, mongoConnectionString, databaseName, collectionName);

            if (extractFromMongoDB)
            {
                DataTable transformedData = await ExtractMongoDBData(mongoConnectionString, databaseName, collectionName, extractFromMongoDB);

                LoadMappedDataToSQL(transformedData);
            }

        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

        Console.WriteLine("Press any key to stop the program...");
        Console.ReadKey();

        timer.Dispose();
    }

    static async Task<JObject> ExtractApiData(string apiKey, string city)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={city}";

            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(json);

            return data;
        }
    }

    static async Task LoadRawToMongoDB(JObject weatherData, string connectionString, string databaseName, string collectionName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        var collection = database.GetCollection<BsonDocument>(collectionName);

        BsonDocument document = BsonDocument.Parse(weatherData.ToString());
        await collection.InsertOneAsync(document);
    }

    static async Task<DataTable> ExtractMongoDBData(string connectionString, string databaseName, string collectionName, bool extractFromMongoDB)
    {
        if (!extractFromMongoDB)
        {
            return null;
        }

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        var collection = database.GetCollection<BsonDocument>(collectionName);

        var documents = await collection.Find(new BsonDocument()).ToListAsync();

        DataTable transformedData = new DataTable();
        transformedData.Columns.Add("City", typeof(string));
        transformedData.Columns.Add("Temperature (C)", typeof(double));
        transformedData.Columns.Add("Humidity (%)", typeof(int));
        transformedData.Columns.Add("Wind Speed (km/h)", typeof(double));
        transformedData.Columns.Add("Last Updated", typeof(DateTime));

        foreach (var document in documents)
        {
            if (document.TryGetValue("location", out BsonValue locationValue) && locationValue.IsBsonDocument)
            {
                var location = locationValue.AsBsonDocument;
                string cityName = location["name"].AsString;
                double temperature = 0;
                int humidity = 0;
                double windSpeed = 0;
                DateTime lastUpdated = DateTime.UtcNow;

                if (document.TryGetValue("current", out BsonValue currentValue) && currentValue.IsBsonDocument)
                {
                    var current = currentValue.AsBsonDocument;
                    temperature = current["temp_c"].AsDouble;
                    humidity = current["humidity"].AsInt32;
                    windSpeed = current["wind_kph"].AsDouble;

                    if (current.TryGetValue("last_updated", out BsonValue lastUpdatedValue))
                    {
                        if (DateTime.TryParse(lastUpdatedValue.ToString(), out DateTime parsedLastUpdated))
                        {
                            lastUpdated = parsedLastUpdated;
                        }
                    }
                }

                transformedData.Rows.Add(cityName, temperature, humidity, windSpeed, lastUpdated);
            }
        }

        return transformedData;
    }

    static void LoadMappedDataToSQL(DataTable transformedData)
    {
        if (transformedData != null)
        {
            using (var context = new WeatherDataContext())
            {
                var existingLastUpdatedValues = context.WeatherData.Select(d => d.LastUpdated).ToList();

                foreach (DataRow row in transformedData.Rows)
                {
                    DateTime lastUpdated = row.Field<DateTime>("Last Updated");

                    bool exists = existingLastUpdatedValues.Contains(lastUpdated);

                    if (!exists)
                    {
                        var weatherData = new WeatherData
                        {
                            City = row.Field<string>("City"),
                            Temperature = row.Field<double>("Temperature (C)"),
                            Humidity = row.Field<int>("Humidity (%)"),
                            WindSpeed = row.Field<double>("Wind Speed (km/h)"),
                            LastUpdated = lastUpdated
                        };

                        context.WeatherData.Add(weatherData);

                        existingLastUpdatedValues.Add(lastUpdated);
                    }
                }

                context.SaveChanges();
            }
        }
    }
}
