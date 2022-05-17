using Xunit;
using BlogPb;
using Blog.Server.Services;
using test.Helpers;
using MongoDB.Driver;
using MongoDB.Bson;
using Moq;
using Grpc.Core;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Test.blog;

public class BlogServerTest
{
    private Mock<IMongoCollection<BsonDocument>>? _collection;

    [Fact]
    public async void CreateBlogException()
    {
        _collection = new Mock<IMongoCollection<BsonDocument>>();

        _ = _collection.Setup(_ => _.InsertOneAsync(
            It.IsAny<BsonDocument>(),
            It.IsAny<InsertOneOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.FromException(new AggregateException("")));

        var service = new BlogServiceImpl(_collection?.Object);
        try
        {
            _ = await service.CreateBlog(
                new BlogPb.Blog(),
                TestServerCallContext.Create());
            Assert.True(false);
        }
        catch (RpcException e)
        {
            Assert.Equal(StatusCode.Internal, e.StatusCode);
        }
        catch (Exception)
        {
            Assert.True(false);
        }

        _collection = null;
    }

    [Fact]
    public async void ReadBlogInvalidId()
    {
        _collection = new Mock<IMongoCollection<BsonDocument>>();

        var service = new BlogServiceImpl(_collection.Object);
        try
        {
            var response = await service.ReadBlog(
                new BlogId(),
                TestServerCallContext.Create());
            Assert.True(false);
        }
        catch (RpcException e)
        {
            Assert.Equal(StatusCode.InvalidArgument, e.StatusCode);
        }

        _collection = null;
    }

    [Fact]
    public async void UpdateBlogNull()
    {
        _collection = new Mock<IMongoCollection<BsonDocument>>();
        var asyncCursor = new Mock<IAsyncCursor<BsonDocument>>();

        _ = _collection.Setup(_ => _.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<UpdateDefinition<BsonDocument>>(),
            It.IsAny<FindOneAndUpdateOptions<BsonDocument>>(),
            default)
        ).Returns(Task.FromResult<BsonDocument>(null));

        var service = new BlogServiceImpl(_collection.Object);
        try
        {
            var response = await service.UpdateBlog(
                new BlogPb.Blog { Id = new ObjectId().ToString() },
                TestServerCallContext.Create());
            Assert.True(false);
        }
        catch (RpcException e)
        {
            Assert.Equal(StatusCode.NotFound, e.StatusCode);
        }

        _collection = null;
    }

    [Fact]
    public async void UpdateBlogInvalidId()
    {
        _collection = new Mock<IMongoCollection<BsonDocument>>();

        var service = new BlogServiceImpl(_collection.Object);
        try
        {
            var response = await service.UpdateBlog(
                new BlogPb.Blog(),
                TestServerCallContext.Create());
            Assert.True(false);
        }
        catch (RpcException e)
        {
            Assert.Equal(StatusCode.InvalidArgument, e.StatusCode);
        }

        _collection = null;
    }

    [Fact]
    public async void DeleteBlogInvalidId()
    {
        _collection = new Mock<IMongoCollection<BsonDocument>>();

        var service = new BlogServiceImpl(_collection.Object);
        try
        {
            var response = await service.DeleteBlog(
                new BlogId(),
                TestServerCallContext.Create());
            Assert.True(false);
        }
        catch (RpcException e)
        {
            Assert.Equal(StatusCode.InvalidArgument, e.StatusCode);
        }

        _collection = null;
    }

    [Fact]
    public async void DeleteBlogNotFound()
    {
        _collection = new Mock<IMongoCollection<BsonDocument>>();
        var mock = new Mock<DeleteResult>();

        _ = mock.Setup(_ => _.DeletedCount).Returns(0);
        _ = _collection.Setup(_ => _.DeleteOneAsync(
            It.IsAny<FilterDefinition<BsonDocument>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mock.Object));

        var service = new BlogServiceImpl(_collection.Object);
        try
        {
            var response = await service.DeleteBlog(
                new BlogId { Id = new ObjectId().ToString() },
                TestServerCallContext.Create());
            Assert.True(false);
        }
        catch (RpcException e)
        {
            Assert.Equal(StatusCode.NotFound, e.StatusCode);
        }

        _collection = null;
    }
}