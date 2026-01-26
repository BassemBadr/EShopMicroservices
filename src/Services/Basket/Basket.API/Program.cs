var builder = WebApplication.CreateBuilder(args);

//  add services to container
var assembly = typeof(Program).Assembly;

builder.Services.AddCarter(new DependencyContextAssemblyCatalog([assembly]));

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
    opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
    opts.DisableNpgsqlLogging = true;
}).UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

//builder.Services.AddScoped<BasketRepository>();
//builder.Services.AddScoped<IBasketRepository, CachedBasketRepository>(
//    provider => {
//        var basketRepo = provider.GetRequiredService<BasketRepository>();
//        return new CachedBasketRepository(
//            basketRepo,
//            provider.GetRequiredService<IDistributedCache>());
//    });

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

// configure the HTTP request pipeline
app.MapCarter();
app.UseExceptionHandler(options => { });

app.Run();
