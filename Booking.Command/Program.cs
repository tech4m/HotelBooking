using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Booking.Command.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors
                        (x =>
                            x.AddDefaultPolicy
                            (p =>
                                p.AllowAnyOrigin().
                                AllowAnyHeader().
                                AllowAnyMethod()
                             )
                        );


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/book", async ([FromBody] BookingRequest request) =>
{
    var token = request.IdToken;//Jwt Token
    var idTokenDetails = new JwtSecurityToken(token);

    //sub contains the user id of the user.
    var userId = idTokenDetails.Claims.FirstOrDefault(x => x.Type == "sub")?.Value ?? "";

    var giveName = idTokenDetails.Claims.FirstOrDefault(x => x.Type == "given_name")?.Value ?? "";

    var familyName = idTokenDetails.Claims.FirstOrDefault(x => x.Type == "family_name")?.Value ?? "";

    var email = idTokenDetails.Claims.FirstOrDefault(x => x.Type == "email")?.Value ?? "";

    var phoneNumber = idTokenDetails.Claims.FirstOrDefault(x => x.Type == "phone_number")?.Value ?? "";

    var dto = new BookingDto()
    {
        Id = Guid.NewGuid().ToString(),
        HotelId = request.HotelId,
        CheckinDate = request.CheckinDate,
        CheckoutDate = request.CheckoutDate,
        Email = email,
        FamilyName = familyName,
        UserId = userId,
        GivenName = giveName,
        PhoneNumber = phoneNumber,
        Status = BookingStatus.Pending
    };

    using var dbClient = new AmazonDynamoDBClient();

    using var dbContext = new DynamoDBContext(dbClient);

    await dbContext.SaveAsync(dto);
});

app.MapGet("/health", () => new HttpResponseMessage(HttpStatusCode.OK));

app.Run();

