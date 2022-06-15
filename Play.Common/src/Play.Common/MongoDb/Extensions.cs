using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Common.Settings;
using Play.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Play.Common.MongoDb;

public static class Extensions
{
  public static IServiceCollection AddMongo(this IServiceCollection services)
  {

    // Add services to the container.

    //cuando se guarden elementos en mongo db, con el tipo Guid se guardatran con el tipo dtring, y no la representacion por defecto de mongo
    BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
    // guarda las fechas de minjgo como  string, y no el formato estandar de fecha
    BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));


    //* we define a singleton object that is going to be injected into the classes that use the mongo database
    services.AddSingleton(serviceProvider =>
    {

      var Configuration = serviceProvider.GetService<IConfiguration>();

      var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

      MongoDbSettings MongoDbSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
      var mongoClient = new MongoClient(MongoDbSettings.ConnectionString);
      return mongoClient.GetDatabase(serviceSettings.ServiceName);
    });

    return services;
  }

  public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName) where T : IEntity
  {
    services.AddSingleton<IRepository<T>>(ServiceProvider =>
    {
      //sevice.getservice sirve para obtener una instancia de un servicio ya registrado en este caso esa interface corersponde a la de mongodb, cuyo servio se registro en el servicio de mongodb anterior
      var database = ServiceProvider.GetService<IMongoDatabase>();
      return new MongoRepository<T>(database, "items");
    });

    return services;

  }
}