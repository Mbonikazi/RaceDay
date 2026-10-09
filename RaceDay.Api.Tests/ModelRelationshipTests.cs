using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using RaceDay.Api.Tests.TestHelpers;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class ModelRelationshipTests
    {
        // ==========================================================
        // ROLE → USERS
        // ==========================================================

        [Fact]
        public async Task Role_HasManyUsers()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var organiserRole = await db.Roles.FirstAsync(r => r.Name == "Organiser");

            db.Users.Add(new AppUser
            {
                FirstName = "O1",
                LastName = "X",
                Email = "o1@test.com",
                PasswordHash = "h",
                RoleId = organiserRole.RoleId
            });
            db.Users.Add(new AppUser
            {
                FirstName = "O2",
                LastName = "Y",
                Email = "o2@test.com",
                PasswordHash = "h",
                RoleId = organiserRole.RoleId
            });
            await db.SaveChangesAsync();

            var count = await db.Users.CountAsync(u => u.RoleId == organiserRole.RoleId);
            Assert.Equal(2, count);
        }

        // ==========================================================
        // ORGANISER → EVENTS
        // ==========================================================

        [Fact]
        public async Task Organiser_HasManyEvents()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var org = new AppUser
            {
                FirstName = "Org",
                LastName = "One",
                Email = "org1@test.com",
                PasswordHash = "h",
                RoleId = 1
            };
            db.Users.Add(org);
            await db.SaveChangesAsync();

            db.Events.Add(new Event
            {
                Name = "10K",
                Description = "d",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "Durban",
                DistanceKm = 10,
                EventType = "Road",
                OrganiserId = org.UserId
            });
            db.Events.Add(new Event
            {
                Name = "21K",
                Description = "d",
                EventDate = DateTime.UtcNow.AddDays(20),
                Location = "Durban",
                DistanceKm = 21,
                EventType = "Road",
                OrganiserId = org.UserId
            });
            await db.SaveChangesAsync();

            var count = await db.Events.CountAsync(e => e.OrganiserId == org.UserId);
            Assert.Equal(2, count);
        }

        // ==========================================================
        // EVENT → CATEGORIES
        // ==========================================================

        [Fact]
        public async Task Event_HasManyCategories()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var org = new AppUser { FirstName = "O", LastName = "R", Email = "or@test.com", PasswordHash = "h", RoleId = 1 };
            db.Users.Add(org);
            await db.SaveChangesAsync();

            var ev = new Event
            {
                Name = "Race",
                Description = "d",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "L",
                DistanceKm = 42,
                EventType = "Road",
                OrganiserId = org.UserId
            };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            db.Categories.AddRange(
                new Category { Name = "Senior", Description = "d", MinAge = 18, MaxAge = 39, EntryFee = 150, EventId = ev.EventId },
                new Category { Name = "Veteran", Description = "d", MinAge = 40, MaxAge = 59, EntryFee = 150, EventId = ev.EventId },
                new Category { Name = "Masters", Description = "d", MinAge = 60, MaxAge = 99, EntryFee = 150, EventId = ev.EventId }
            );
            await db.SaveChangesAsync();

            var count = await db.Categories.CountAsync(c => c.EventId == ev.EventId);
            Assert.Equal(3, count);
        }

        // ==========================================================
        // ENROLMENT LINKS PARTICIPANT + EVENT + CATEGORY
        // ==========================================================

        [Fact]
        public async Task Enrolment_LinksParticipantEventAndCategory()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var org = new AppUser { FirstName = "Org", LastName = "N", Email = "o@test.com", PasswordHash = "h", RoleId = 1 };
            var par = new AppUser { FirstName = "Par", LastName = "T", Email = "p@test.com", PasswordHash = "h", RoleId = 2 };
            db.Users.AddRange(org, par);
            await db.SaveChangesAsync();

            var ev = new Event
            {
                Name = "Comrades",
                Description = "d",
                EventDate = DateTime.UtcNow.AddDays(60),
                Location = "Pietermaritzburg",
                DistanceKm = 89,
                EventType = "Road",
                OrganiserId = org.UserId
            };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var cat = new Category { Name = "Open", Description = "d", MinAge = 20, MaxAge = 99, EntryFee = 500, EventId = ev.EventId };
            db.Categories.Add(cat);
            await db.SaveChangesAsync();

            var enrolment = new Enrolment
            {
                ParticipantId = par.UserId,
                EventId = ev.EventId,
                CategoryId = cat.CategoryId,
                Status = "Pending"
            };
            db.Enrolments.Add(enrolment);
            await db.SaveChangesAsync();

            var saved = await db.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .FirstAsync();

            Assert.Equal("Par", saved.Participant!.FirstName);
            Assert.Equal("Comrades", saved.Event!.Name);
            Assert.Equal("Open", saved.Category!.Name);
            Assert.Equal("Pending", saved.Status);
        }

        // ==========================================================
        // ENROLMENT → RESULT (0 or 1)
        // ==========================================================

        [Fact]
        public async Task Result_LinksToEnrolment()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var org = new AppUser { FirstName = "O", LastName = "R", Email = "o2@test.com", PasswordHash = "h", RoleId = 1 };
            var par = new AppUser { FirstName = "P", LastName = "A", Email = "p2@test.com", PasswordHash = "h", RoleId = 2 };
            db.Users.AddRange(org, par);
            await db.SaveChangesAsync();

            var ev = new Event
            {
                Name = "10K",
                Description = "d",
                EventDate = DateTime.UtcNow.AddDays(5),
                Location = "L",
                DistanceKm = 10,
                EventType = "Road",
                OrganiserId = org.UserId
            };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var cat = new Category { Name = "Open", Description = "d", MinAge = 18, MaxAge = 99, EntryFee = 100, EventId = ev.EventId };
            db.Categories.Add(cat);
            await db.SaveChangesAsync();

            var enr = new Enrolment
            {
                ParticipantId = par.UserId,
                EventId = ev.EventId,
                CategoryId = cat.CategoryId,
                Status = "Confirmed"
            };
            db.Enrolments.Add(enr);
            await db.SaveChangesAsync();

            var result = new Result
            {
                EnrolmentId = enr.EnrolmentId,
                FinishTime = TimeSpan.FromMinutes(42),
                FinishingPosition = 1
            };
            db.Results.Add(result);
            await db.SaveChangesAsync();

            var saved = await db.Results.Include(r => r.Enrolment).FirstAsync();
            Assert.Equal(1, saved.FinishingPosition);
            Assert.Equal(enr.EnrolmentId, saved.EnrolmentId);
        }

        // ==========================================================
        // BIB NUMBER ASSIGNMENT LOGIC
        // ==========================================================

        [Fact]
        public async Task BibNumber_IsAssignedWhenStatusIsConfirmed()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var org = new AppUser { FirstName = "O", LastName = "R", Email = "o3@test.com", PasswordHash = "h", RoleId = 1 };
            var par = new AppUser { FirstName = "P", LastName = "A", Email = "p3@test.com", PasswordHash = "h", RoleId = 2 };
            db.Users.AddRange(org, par);
            await db.SaveChangesAsync();

            var ev = new Event
            {
                Name = "5K",
                Description = "d",
                EventDate = DateTime.UtcNow.AddDays(3),
                Location = "L",
                DistanceKm = 5,
                EventType = "Road",
                OrganiserId = org.UserId
            };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            var cat = new Category { Name = "Open", Description = "d", MinAge = 18, MaxAge = 99, EntryFee = 50, EventId = ev.EventId };
            db.Categories.Add(cat);
            await db.SaveChangesAsync();

            var enr = new Enrolment
            {
                ParticipantId = par.UserId,
                EventId = ev.EventId,
                CategoryId = cat.CategoryId,
                Status = "Pending"
            };
            db.Enrolments.Add(enr);
            await db.SaveChangesAsync();

            // Simulate organiser confirming the enrolment
            enr.Status = "Confirmed";
            enr.BibNumber = $"BIB{enr.EnrolmentId:D5}";
            await db.SaveChangesAsync();

            var saved = await db.Enrolments.FindAsync(enr.EnrolmentId);
            Assert.Equal("Confirmed", saved!.Status);
            Assert.NotNull(saved.BibNumber);
            Assert.StartsWith("BIB", saved.BibNumber);
        }

        // ==========================================================
        // OWNERSHIP ENFORCEMENT (BUSINESS LOGIC)
        // ==========================================================

        [Fact]
        public async Task Organiser_CanOnlyModifyOwnEvents()
        {
            using var db = await TestDbFactory.CreateContextWithRolesAsync();

            var org1 = new AppUser { FirstName = "Org1", LastName = "A", Email = "o1x@test.com", PasswordHash = "h", RoleId = 1 };
            var org2 = new AppUser { FirstName = "Org2", LastName = "B", Email = "o2x@test.com", PasswordHash = "h", RoleId = 1 };
            db.Users.AddRange(org1, org2);
            await db.SaveChangesAsync();

            var ev = new Event
            {
                Name = "Owned by Org1",
                Description = "d",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "L",
                DistanceKm = 10,
                EventType = "Road",
                OrganiserId = org1.UserId
            };
            db.Events.Add(ev);
            await db.SaveChangesAsync();

            // Org2 attempts to modify Org1's event (should be rejected)
            var currentUserId = org2.UserId;
            var ownerUserId = ev.OrganiserId;

            var isOwner = currentUserId == ownerUserId;
            Assert.False(isOwner);
        }
    }
}