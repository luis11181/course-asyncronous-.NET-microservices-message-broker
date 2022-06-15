
using System.Linq.Expressions;
using MongoDB.Driver;
using Play.Common;

namespace Play.Common.MongoDb
{

  public class MongoRepository<T> : IRepository<T> where T : IEntity
  {

    private readonly IMongoCollection<T> dbCollection;

    private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

    public MongoRepository(IMongoDatabase database, string collectionName)
    {

      dbCollection = database.GetCollection<T>(collectionName);
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync()
    {

      //var result = await dbCollection.Find(filterBuilder.Empty).ToListAsync();
      return await dbCollection.Find(filterBuilder.Empty).ToListAsync();
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
    {
      //var result = await dbCollection.Find(filterBuilder.Empty).ToListAsync();
      return await dbCollection.Find(filter).ToListAsync();
    }

    public async Task<T> GetAsync(Guid id)
    {
      FilterDefinition<T> filter = filterBuilder.Eq(x => x.Id, id);

      return await dbCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
    {
      return await dbCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(T entity)
    {
      if (entity == null)
      {
        throw new ArgumentNullException(nameof(entity));
      }
      await dbCollection.InsertOneAsync(entity);

    }

    public async Task UpdateAsync(T entity)
    {
      if (entity == null)
      {
        throw new ArgumentNullException(nameof(entity));
      }
      await dbCollection.ReplaceOneAsync(filterBuilder.Eq(x => x.Id, entity.Id), entity);
    }


    public async Task RemoveAsync(Guid id)
    {
      FilterDefinition<T> filter = filterBuilder.Eq(existing => existing.Id, id);
      await dbCollection.DeleteOneAsync(filter);
    }




  }
}