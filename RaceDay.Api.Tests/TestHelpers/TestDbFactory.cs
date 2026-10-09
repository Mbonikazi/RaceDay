using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests.TestHelpers
{
    /// <summary>
    /// Factory that creates a fresh in-memory database for each test.
    /// </summary>
    public static class TestDbFactory
    {
        public static RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new RaceDayDbContext(options);
            db.Database.EnsureCreated();
            return db;
        }

        /// <summary>
        /// Seeds the standard roles into the in-memory database.
        /// </summary>
        public static async Task SeedRolesAsync(RaceDayDbContext db)
        {
            if (!await db.Roles.AnyAsync())
            {
                db.Roles.AddRange(
                    new Role { RoleId = 1, Name = "Organiser" },
                    new Role { RoleId = 2, Name = "Participant" }
                );
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Convenience method that creates a DbContext with seeded roles.
        /// </summary>
        public static async Task<RaceDayDbContext> CreateContextWithRolesAsync()
        {
            var db = CreateContext();
            await SeedRolesAsync(db);
            return db;
        }
    }
}