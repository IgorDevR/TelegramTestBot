using Telegram.Bot;
using TelegramTestBot.Data;
using TelegramTestBot.Services;
using TelegramTestBot.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MyContext>();

builder.Services.AddSingleton<ITelegramBotClient, TelegramBotClient>((ctx) =>
  new TelegramBotClient(builder.Configuration["ApiToken"]!));

// Add services to the container.
builder.Services.AddHostedService<BotEventsHandler>();
builder.Services.AddScoped<DataInitializer>();
builder.Services.AddScoped<InviteService>();
builder.Services.AddScoped<StartService>();
builder.Services.AddScoped<CallbackUserProfileService>();
builder.Services.AddScoped<ButtonService>();
builder.Services.AddScoped<UserEventsService>();
builder.Services.AddScoped<ReplyUserProfileService>();
builder.Services.AddScoped<CommonMessages>();
builder.Services.AddScoped<UserService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataInitializer = scope.ServiceProvider.GetRequiredService<DataInitializer>();
    dataInitializer.Init().Wait();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();