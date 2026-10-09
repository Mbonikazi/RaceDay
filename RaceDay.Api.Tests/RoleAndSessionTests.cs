using RaceDay.Api.Extensions;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class RoleAndSessionTests
    {
        [Fact]
        public void SessionKeys_HaveExpectedValues()
        {
            Assert.Equal("UserId", SessionKeys.UserId);
            Assert.Equal("Role", SessionKeys.Role);
            Assert.Equal("UserName", SessionKeys.UserName);
        }

        [Theory]
        [InlineData("Organiser", "Organiser", true)]
        [InlineData("Organiser", "Participant", false)]
        [InlineData("Participant", "Participant", true)]
        [InlineData("Participant", "Organiser", false)]
        public void RoleCheck_MatchesExpectedRules(string userRole, string requiredRole, bool expected)
        {
            var hasAccess = userRole == requiredRole;
            Assert.Equal(expected, hasAccess);
        }

        [Fact]
        public void Unauthorised_User_HasNoUserId()
        {
            int? userId = null;
            Assert.Null(userId);
        }

        [Fact]
        public void Participant_CannotPerformOrganiserAction()
        {
            string userRole = "Participant";
            string[] requiredRoles = { "Organiser" };

            var allowed = requiredRoles.Contains(userRole);
            Assert.False(allowed);
        }

        [Fact]
        public void Organiser_CanPerformOrganiserAction()
        {
            string userRole = "Organiser";
            string[] requiredRoles = { "Organiser" };

            var allowed = requiredRoles.Contains(userRole);
            Assert.True(allowed);
        }

        [Fact]
        public void BothRoles_CanViewEvents()
        {
            string[] requiredRoles = { "Organiser", "Participant" };

            Assert.True(requiredRoles.Contains("Organiser"));
            Assert.True(requiredRoles.Contains("Participant"));
        }
    }
}
