using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Booking.Query.Models;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(c => c.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi 

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();
app.UseCors();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/query", async (string? idToken) =>
{
    var idTokenDetails = new JwtSecurityToken(idToken);
    var userId = idTokenDetails.Claims.First(x => x.Type == "sub")?.Value ?? "";
    var groups = idTokenDetails.Claims.First(x => x.Type == "cognito:groups")?.Value ?? "";

    var result = new List<BookingDto>();
    using var dbClient = new AmazonDynamoDBClient();
    using var dbContext = new DynamoDBContext(dbClient);

    //if (string.IsNullOrEmpty(groups))//normal user
    //{
    result.AddRange(await dbContext.FromQueryAsync<BookingDto>
                    (
                        new QueryOperationConfig()
                        {
                            Filter = new QueryFilter("UserId", QueryOperator.Equal, userId),
                            IndexName = "UserId-index"
                        }
                    ).GetRemainingAsync());
    //}
    //else//user present in cognito or its a signed user
    //{
    //    if (groups.Contains("HotelManager"))// Hotel Admin
    //    {
    //        var dbResult = await dbContext.QueryAsync<BookingDto>
    //                    (1, new DynamoDBOperationConfig
    //                    {
    //                        IndexName = "Status-index"
    //                    }).GetRemainingAsync();
    //        result.AddRange(dbResult);
    //    }
    //}
    return result;
});

app.MapGet("/", () => true);

app.Run();
