using FluentAssertions;
using IPNoticeHub.Web.Controllers;
using IPNoticeHub.Web.Models.AdminDashboard;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.AdminControllerTests
{
    public class AdminControllerTests : AdminControllerBase
    {
        [Test]
        public void Index_ShouldReturnView_WithExpectedModelCounts_AndTop5Recent()
        {
            var result = controller.Index();

            var view = result.Should().BeOfType<ViewResult>().Subject;

            var model = 
                view.Model.Should().BeOfType<AdminDashboardViewModel>().Subject;

            model.TotalUsers.Should().Be(7);
            model.TrademarksAdded.Should().Be(4);
            model.CopyrightsAdded.Should().Be(2);
            model.WatchlistedItems.Should().Be(4);

            model.RecentRegistrations.Should().NotBeNull();
            model.RecentRegistrations.Should().HaveCount(5);

            List<string> ids = model.RecentRegistrations.Select(
                u => u.Id).ToList();

            ids.Should().BeInDescendingOrder();
            ids.Should().Equal(new[] { "user07", "user06", "user05", "user04", "user03" });
        }

        [Test]
        public void Sync_Post_ShouldSetSuccessMessage_AndRedirectToIndex()
        {
            var result = controller.Sync();

            var redirect = 
                result.Should().BeOfType<RedirectToActionResult>().Subject;

            redirect.ActionName.Should().Be("Index");

            controller.TempData.ContainsKey("SuccessMessage").Should().BeTrue();
            controller.TempData["SuccessMessage"]!.ToString()!.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void Controller_ShouldHaveArea_Admin()
        {
            var attribute = Attribute.GetCustomAttribute(
                typeof(AdminController), 
                typeof(AreaAttribute)).
                As<AreaAttribute>();

            attribute.Should().NotBeNull();
            attribute!.RouteValue.Should().Be("Admin");
        }

        [Test]
        public void Controller_ShouldBeAuthorized_ForAdminRole()
        {
            var attribute = Attribute.GetCustomAttribute(
                typeof(AdminController), 
                typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute)).
                As<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

            attribute.Should().NotBeNull();
            attribute!.Roles.Should().Be("Admin");
        }
    }
}
