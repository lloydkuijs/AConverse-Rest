using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

public class MongoCRUD<T>
{
    private IMongoDatabase _db;

    public MongoCRUD(string database)
    {
        var client = new MongoClient();
        _db = client.GetDatabase(database);
    }

    public void InsertRecord(string table, T record)
    {
        var collection = _db.GetCollection<T>(table);

        collection.InsertOne(record);
    }

    public List<T> LoadRecords(string table)
    {
        var collection = _db.GetCollection<T>(table);

        return collection.Find(new BsonDocument()).ToList();
    }

    public T LoadRecordById(string table, Guid id)
    {
        var collection = _db.GetCollection<T>(table);
        var filter = Builders<T>.Filter.Eq("Id", id);

        return collection.Find(filter).FirstOrDefault();
    }

    public void UpsertRecord(string table, Guid id, T record)
    {
        var collection = _db.GetCollection<T>(table);

        var result = collection.ReplaceOne(
            new BsonDocument("_id", id),
            record,
            new ReplaceOptions { IsUpsert = true });
    }

    public void DeleteRecord(string table, Guid id)
    {
        var collection = _db.GetCollection<T>(table);
        var filter = Builders<T>.Filter.Eq("Id", id);

        collection.DeleteOne(filter);
    }
}