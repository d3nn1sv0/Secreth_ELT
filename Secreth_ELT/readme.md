# Weather Data ELT Pipeline

This is a sample ELT (Extract, Load, Transform) pipeline program that extracts weather data from the WeatherAPI.com API, loads the raw data into MongoDB, and optionally transforms and loads the data into SQL Server.

## Prerequisites

Before running the program, ensure that you have the following components set up:
- WeatherAPI.com API key: Obtain an API key from WeatherAPI.com to access the weather data.
- MongoDB Atlas: Set up a MongoDB Atlas account and obtain the connection string. This will be used to store the raw weather data.
- SQL Server: Have a SQL Server instance available to store the transformed weather data (optional).

## Configuration

Make sure to update the following variables in the `Main` method of the `Program.cs` file with your own values:
- `apiKey`: Your WeatherAPI.com API key.
- `city`: The city for which you want to fetch weather data.
- `mongoConnectionString`: Your MongoDB Atlas connection string.
- `databaseName`: The name of the MongoDB database to store the raw weather data.
- `collectionName`: The name of the MongoDB collection to store the raw weather data.
- `extractFromMongoDB`: Set this to `true` if you want to extract and transform data from MongoDB Atlas.

## Usage

1. Configure the variables mentioned above with your API keys, connection strings, and other required details.
2. Build and run the program.
3. The program will extract weather data from the WeatherAPI.com API and load the raw data into MongoDB at the specified collection.
4. If `extractFromMongoDB` is set to `true`, the program will extract the data from MongoDB Atlas, transform it, and load it into SQL Server.
5. The program will continue running in intervals of 10 minutes, fetching and processing new weather data.

## Additional Notes

- The program uses the MongoDB driver to interact with the MongoDB database and perform CRUD operations.
- If you don't want to use the Atlas Data API, set `extractFromMongoDB` to `false`. The program will skip the transformation and loading steps.
- If you want to use the Atlas Data API, you would need to modify the code accordingly to make HTTP requests and handle the API responses.

## Method Documentation

### `ExtractApiData(string apiKey, string city)`

This method extracts weather data from the WeatherAPI.com API for the specified city.

- `apiKey`: Your WeatherAPI.com API key.
- `city`: The city for which you want to fetch weather data.
- Returns: A `JObject` containing the weather data in JSON format.

### `LoadRawToMongoDB(JObject weatherData, string connectionString, string databaseName, string collectionName)`

This method loads the raw weather data into MongoDB.

- `weatherData`: The weather data in `JObject` format.
- `connectionString`: Your MongoDB Atlas connection string.
- `databaseName`: The name of the MongoDB database to store the raw weather data.
- `collectionName`: The name of the MongoDB collection to store the raw weather data.

### `ExtractMongoDBData(string connectionString, string databaseName, string collectionName, bool extractFromMongoDB)`

This method extracts and transforms the weather data from MongoDB Atlas.

- `connectionString`: Your MongoDB Atlas connection string.
- `databaseName`: The name of the MongoDB database where the raw weather data is stored.
- `collectionName`: The name of the MongoDB collection where the raw weather data is stored.
- `extractFromMongoDB`: Set this to `true` to extract and transform the data from MongoDB Atlas. Set it to `false` to skip this step.
- Returns: A `DataTable` containing the transformed weather data.

### `LoadMappedDataToSQL(DataTable transformedData)`

This method loads the transformed weather data into SQL Server.

- `transformedData`: The transformed weather data in a `DataTable` format.

Please refer to the code comments within the `Program.cs` file for more detailed explanations of the code logic.

