using BlogPb;
using Grpc.Net.Client;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

static async Task<string> CreateBlog(BlogService.BlogServiceClient client)
{
    Console.WriteLine("---CreateBlog was invoked---");

    BlogId id = await client.CreateBlogAsync(new Blog.Blog {
        AuthorId = "Clement",
        Title = "A new blog",
        Content = "Content of the first blog"
    });

    Console.WriteLine($"CreateBlog: {id}");
    return id.Id;
}

static async Task ReadBlog(BlogService.BlogServiceClient client, string id)
{
    Console.WriteLine("---ReadBlog was invoked---");
    try
    {
        Blog.Blog blog = await client.ReadBlogAsync(new BlogId { Id = id });

        Console.WriteLine($"ReadBlog: {blog}");
    }
    catch (RpcException e)
    {
        Console.WriteLine(e);
    }
}

static async Task UpdateBlog(BlogService.BlogServiceClient client, string id)
{
    Console.WriteLine("---UpdateBlog was invoked---");
    try
    {
        Empty blog = await client.UpdateBlogAsync(new Blog.Blog {
            Id = id,
            AuthorId = "not Clement",
            Title = "A newer blog",
            Content = "Content of the first blog, with some awesome additions!"
        });

        Console.WriteLine("Blog updated");
    }
    catch (RpcException e)
    {
        Console.WriteLine(e);
    }
}

static async Task ListBlogs(BlogService.BlogServiceClient client)
{
    Console.WriteLine("---ListBlogs was invoked---");

    using var call = client.ListBlogs(new Empty());

    await foreach (var response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"ListBlogs: {response}");
    }
}

static async Task DeleteBlog(BlogService.BlogServiceClient client, string id)
{
    Console.WriteLine("---DeleteBlog was invoked---");
    try
    {
        Empty blog = await client.DeleteBlogAsync(new BlogId { Id = id });

        Console.WriteLine("Blog deleted");
    }
    catch (RpcException e)
    {
        Console.WriteLine(e);
    }
}

using var channel = GrpcChannel.ForAddress("http://localhost:50051");
var client = new BlogService.BlogServiceClient(channel);

var id = await CreateBlog(client);
await ReadBlog(client, id);
// await ReadBlog(client, "");
await UpdateBlog(client, id);
// await UpdateBlog(client, "");
await ListBlogs(client);
await DeleteBlog(client, id);
// await DeleteBlog(client, "");