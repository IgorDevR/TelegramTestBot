using Microsoft.EntityFrameworkCore;
using TelegramTestBot.Models;

namespace TelegramTestBot.Data
{
    public class DataInitializer
    {
        private readonly MyContext _context;
        private readonly IServiceProvider _serviceProvider;

        public DataInitializer(MyContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public async Task Init()
        {
            await _context.Database.MigrateAsync();
            await Seed();
        }

        public async Task Seed()
        {
            if (!_context.Invites.Any())
            {
                var invite = new Invite
                {
                    Role = UserRole.Teacher,
                    Code = "qqq",
                    IsActive = true,
                };

                _context.Invites.Add(invite);

                var invite2 = new Invite
                {
                    Role = UserRole.Admin,
                    Code = "www",
                    IsActive = true,
                };

                _context.Invites.Add(invite2);
                await _context.SaveChangesAsync();
            }
        }
    }
}