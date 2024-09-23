using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
var app = builder.Build();
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler();

app.UseStatusCodePages();

ConcurrentDictionary<string, Fruit> fruits = new ConcurrentDictionary<string, Fruit>();
fruits.TryAdd("apple", new Fruit(1, "apple"));
int lastId = 1;

app.MapGet("/fruit/list", () => fruits);
app.MapGet("/fruit/{id}", (string id) => {
    Fruit? fruit;
    if (fruits.TryGetValue(id, out fruit))
        return TypedResults.Ok(fruit);
    else
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "id", ["Fruit doesn't exist"] } });
    });

app.MapPut("/fruit/add/{name}", (string name) => fruits.TryAdd(name, new Fruit(++lastId, name)) ? Results.Ok("Fruit added") : 
                                                    Results.ValidationProblem(new Dictionary<string, string[]> { { "name", ["Fruit already exist"] } })
);

app.MapDelete("/fruit/delete/{name}", (string name) => fruits.TryRemove(name, out _) ? Results.Ok("Fruit deleted") :
                                                    Results.ValidationProblem(new Dictionary<string, string[]> { { "name", ["Fruit doesn't exist"] } }));
app.Run();

record Fruit(int id, string name);