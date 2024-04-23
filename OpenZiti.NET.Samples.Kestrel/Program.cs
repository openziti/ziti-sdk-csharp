using OpenZiti;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using ZitiRestServerCSharp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseDelegatedTransport();

builder.Services.AddControllers();
builder.Services.AddDbContext<MetricContext>(opt =>
    opt.UseInMemoryDatabase("Metrics"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
