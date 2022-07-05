using Grpc.Core;
using MongoDB.Driver;
using MongoDB.Bson;
using Google.Protobuf.WellKnownTypes;

namespace Blog.Server.Services;

public class BlogServiceImpl : BlogService.BlogServiceBase
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _db;
    private readonly IMongoCollection<BsonDocument> _collection;

    private static IMongoCollection<BsonDocument> InitCollection(IMongoDatabase db)
    {
        return db.GetCollection<BsonDocument>("blog");
    }

    public BlogServiceImpl(IMongoCollection<BsonDocument>? init = null)
    {
        _client = new("mongodb://root:root@localhost:27017");
        _db = _client.GetDatabase("blogdb");
        _collection = init ?? InitCollection(_db);
    }

    private static ObjectId TryParseId(string id)
    {
        try { return new ObjectId(id); }
        catch (Exception)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Invalid OID"
            ));
        }
    }

    public override async Task<BlogId> CreateBlog(
      Blog request,
      ServerCallContext context)
    {
        Console.WriteLine($"CreateBlog was invoked with {request}");

        BsonDocument doc = new BsonDocument("author_id", request.AuthorId)
            .Add("title", request.Title)
            .Add("content", request.Content);

        try
        {
            await _collection.InsertOneAsync(doc);
        }
        catch (AggregateException e)
        {
            throw new RpcException(new Status(
                StatusCode.Internal,
                e.Message
            ));
        }

        return new BlogId()
        {
            Id = doc.GetValue("_id").ToString()
        };
    }

    public override async Task<Blog> ReadBlog(
        BlogId request,
        ServerCallContext context)
    {
        Console.WriteLine($"ReadBlog was invoked with {request}");

        var id = TryParseId(request.Id);
        var filter = new FilterDefinitionBuilder<BsonDocument>().Eq(
            "_id",
            id
        );
        var cursor = await _collection.FindAsync(filter);
        var result = cursor.SingleOrDefault() ?? throw new RpcException(new Status(
            StatusCode.NotFound,
            $"The blog with id {request.Id} wasn't found"
        ));


        return new Blog()
        {
            AuthorId = result.GetValue("author_id").AsString,
            Title = result.GetValue("title").AsString,
            Content = result.GetValue("content").AsString
        };
    }

    public override async Task<Empty> UpdateBlog(
        Blog request,
        ServerCallContext context)
    {
        Console.WriteLine($"UpdateBlog was invoked with {request}");

        var id = TryParseId(request.Id);
        var filter = new FilterDefinitionBuilder<BsonDocument>().Eq(
            "_id",
            id
        );

        var doc = new BsonDocument("author_id", request.AuthorId)
                            .Add("title", request.Title)
                            .Add("content", request.Content);

        _ = await _collection.FindOneAndUpdateAsync(filter, doc) ?? throw new RpcException(new Status(
            StatusCode.NotFound,
            $"The blog with id {request.Id} wasn't found"
        ));

        return new Empty();
    }

    public override async Task ListBlogs(
        Empty request,
        IServerStreamWriter<Blog> responseStream,
        ServerCallContext context)
    {
        Console.WriteLine("ListBlog was invoked");

        var filter = new FilterDefinitionBuilder<BsonDocument>().Empty;
        var result = await _collection.FindAsync(filter);

        foreach (var item in result.ToList())
        {
            await responseStream.WriteAsync(new Blog()
            {
                Id = item.GetValue("_id").ToString(),
                AuthorId = item.GetValue("author_id").AsString,
                Content = item.GetValue("content").AsString,
                Title = item.GetValue("title").AsString
            });
        }
    }

    public override async Task<Empty> DeleteBlog(
        BlogId request,
        ServerCallContext context)
    {
        Console.WriteLine($"DeleteBlog was invoked with {request}");

        var id = TryParseId(request.Id);
        var filter = new FilterDefinitionBuilder<BsonDocument>().Eq(
            "_id",
            id
        );

        try
        {
            var result = await _collection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                throw new RpcException(new Status(
                    StatusCode.NotFound,
                    $"The blog with id {request.Id} wasn't found"
                ));
            }
        }
        catch (AggregateException e)
        {
            throw new RpcException(new Status(
                StatusCode.Internal,
                e.Message
            ));
        }

        return new Empty();
    }
}