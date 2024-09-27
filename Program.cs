using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
var app = builder.Build();
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler();

app.UseStatusCodePages();

ConcurrentDictionary<int, Fruit> fruits = new ConcurrentDictionary<int, Fruit>();
fruits.TryAdd(1, new Fruit(1, "apple"));
int lastId = 1;

app.MapGet("/fruit/list", () => fruits);
app.MapGet("/fruit", (int id) => {
    if (fruits.TryGetValue(id, out Fruit? fruit))
        return TypedResults.Ok(fruit);
    else
        return Results.ValidationProblem(new Dictionary<string, string[]> { { "id", ["Fruit doesn't exist"] } });
    });

app.MapPut("/fruit/add", (Fruit fruit) => fruits.TryAdd(++lastId, new Fruit(fruit.weigth, fruit.name)) ? Results.Ok("Fruit added") : 
                                                    Results.ValidationProblem(new Dictionary<string, string[]> { { "name", ["Fruit already exist"] } })
).WithParameterValidation();

app.MapPut("/fruit/edit/{id}", (Fruit fruit, int id) => {
    fruits.TryGetValue(id, out Fruit? currentFruit);
    return fruits.TryUpdate(id, fruit, currentFruit) ? Results.Ok("Fruit edited") :
                                                    Results.ValidationProblem(new Dictionary<string, string[]> { { "name", ["Fruit already exist"] } });
}).WithParameterValidation();

app.MapDelete("/fruit/delete", (int id) => fruits.TryRemove(id, out _) ? Results.Ok("Fruit deleted") :
                                                    Results.ValidationProblem(new Dictionary<string, string[]> { { "name", ["Fruit doesn't exist"] } })).WithParameterValidation();
app.Run();

record Fruit{
    [Required]
    public int weigth { get; set; }
    [Required, 
     StringLength(100)]
    public string name { get; set; }

    public Fruit(int weigth, string name) {
        this.weigth = weigth;
        this.name = name;
    }
}