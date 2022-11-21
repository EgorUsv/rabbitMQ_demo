using Rd.RabbitMQ.DependencyInjection;

//Create web builder
var builder = WebApplication.CreateBuilder(args);

//Add logging settings
builder.Logging.AddConfiguration(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRabbitMQ();

//Start app pipline
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
