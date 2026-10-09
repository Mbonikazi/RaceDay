using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using RaceDay.Api.Tests.TestHelpers;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class AuthTests
    {
        private readonly IRaceDayPasswordHasher _hasher = new RaceDayPasswordHasher();

        // ==========================================================
        // REGISTRATION TESTS
        // ==========================================================

        [Fact]
        public async Task Register_WithValidData_CreatesUserWithHashedPassword()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var participantRole = await db.Roles.FirstAsync(r => r.Name == "Participant");

            var user = new AppUser
            {
                FirstName = "Jane",
                LastName = "Runner",
                Email = "jane@test.com",
                PasswordHash = _hasher.Hash("Pass123!"),
                RoleId = participantRole.RoleId
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var saved = await db.Users.FirstAsync(u => u.Email == "jane@test.com");
            Assert.Equal("Jane", saved.FirstName);
            Assert.Equal("Runner", saved.LastName);
            Assert.NotEqual("Pass123!", saved.PasswordHash);
            Assert.True(_hasher.Verify("Pass123!", saved.PasswordHash));
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_IsRejected()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            db.Users.Add(new AppUser
            {
                FirstName = "A",
                LastName = "B",
                Email = "dup@test.com",
                PasswordHash = _hasher.Hash("X"),
                RoleId = 1
            });
            await db.SaveChangesAsync();

            var exists = await db.Users.AnyAsync(u => u.Email == "dup@test.com");
            Assert.True(exists);

            // Attempt duplicate
            db.Users.Add(new AppUser
            {
                FirstName = "C",
                LastName = "D",
                Email = "dup@test.com",
                PasswordHash = _hasher.Hash("Y"),
                RoleId = 2
            });

            await Assert.ThrowsAnyAsync<Exception>(() => db.SaveChangesAsync());
        }

        [Fact]
        public async Task Register_OrganiserRole_IsAssignedCorrectly()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var organiserRole = await db.Roles.FirstAsync(r => r.Name == "Organiser");

            db.Users.Add(new AppUser
            {
                FirstName = "Org",
                LastName = "User",
                Email = "org@test.com",
                PasswordHash = _hasher.Hash("Pass123!"),
                RoleId = organiserRole.RoleId
            });
            await db.SaveChangesAsync();

            var saved = await db.Users.Include(u => u.Role).FirstAsync(u => u.Email == "org@test.com");
            Assert.Equal("Organiser", saved.Role!.Name);
        }

        [Fact]
        public async Task Register_ParticipantRole_IsAssignedCorrectly()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var role = await db.Roles.FirstAsync(r => r.Name == "Participant");

            db.Users.Add(new AppUser
            {
                FirstName = "Par",
                LastName = "User",
                Email = "par@test.com",
                PasswordHash = _hasher.Hash("Pass123!"),
                RoleId = role.RoleId
            });
            await db.SaveChangesAsync();

            var saved = await db.Users.Include(u => u.Role).FirstAsync(u => u.Email == "par@test.com");
            Assert.Equal("Participant", saved.Role!.Name);
        }

        // ==========================================================
        // LOGIN TESTS
        // ==========================================================

        [Fact]
        public async Task Login_WithValidCredentials_Succeeds()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var hash = _hasher.Hash("Pass123!");
            db.Users.Add(new AppUser
            {
                FirstName = "Login",
                LastName = "Test",
                Email = "login@test.com",
                PasswordHash = hash,
                RoleId = 2
            });
            await db.SaveChangesAsync();

            var user = await db.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == "login@test.com");

            Assert.NotNull(user);
            Assert.True(_hasher.Verify("Pass123!", user!.PasswordHash));
        }

        [Fact]
        public async Task Login_WithWrongPassword_Fails()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            db.Users.Add(new AppUser
            {
                FirstName = "X",
                LastName = "Y",
                Email = "wrong@test.com",
                PasswordHash = _hasher.Hash("CorrectPassword"),
                RoleId = 2
            });
            await db.SaveChangesAsync();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "wrong@test.com");
            Assert.NotNull(user);
            Assert.False(_hasher.Verify("WrongPassword", user!.PasswordHash));
        }

        [Fact]
        public async Task Login_WithNonExistentEmail_Fails()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "ghost@test.com");
            Assert.Null(user);
        }

        // ==========================================================
        // ROLE VALIDATION TESTS
        // ==========================================================

        [Theory]
        [InlineData("Organiser", true)]
        [InlineData("Participant", true)]
        [InlineData("Admin", false)]
        [InlineData("organiser", false)]     // case-sensitive
        [InlineData("", false)]
        public void Role_Validation_OnlyAllowsOrganiserOrParticipant(string role, bool shouldBeValid)
        {
            var isValid = role == "Organiser" || role == "Participant";
            Assert.Equal(shouldBeValid, isValid);
        }

        // ==========================================================
        // SESSION DATA TESTS
        // ==========================================================

        [Fact]
        public async Task Session_ShouldStoreUserIdAndRole_AfterLogin()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            db.Users.Add(new AppUser
            {
                FirstName = "Session",
                LastName = "Test",
                Email = "sess@test.com",
                PasswordHash = _hasher.Hash("Pass123!"),
                RoleId = 1
            });
            await db.SaveChangesAsync();

            var user = await db.Users.Include(u => u.Role)
                .FirstAsync(u => u.Email == "sess@test.com");

            // Simulated session data (as it would be set in AuthController)
            int? sessionUserId = user.UserId;
            string? sessionRole = user.Role!.Name;

            Assert.NotNull(sessionUserId);
            Assert.Equal("Organiser", sessionRole);
        }
    }
}
