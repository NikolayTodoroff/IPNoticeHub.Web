using FluentAssertions;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
using IPNoticeHub.Web.Models.Errors;
using Microsoft.AspNetCore.Mvc;
using IPNoticeHub.Web.Controllers;
using NUnit.Framework;
using System.Diagnostics;

namespace IPNoticeHub.Tests.UnitTests.ControllerTests.HomeControllerTests
{
    public class HomeControllerErrorTests : HomeControllerBase
    {
        [Test]
        public void StatusCode_WithCode404_ReturnsNotFoundView()
        {
            var controller = 
                TestHomeControllerFactory.CreateHomeController(
                service.Object);

            var actionResult = controller.ErrorStatus(404);

            var viewResult = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            viewResult.ViewName.Should().Be("NotFound");
            viewResult.Model.Should().BeNull();
        }

        [Test]
        public void StatusCode_WithCode500_ReturnsErrorViewWithModel()
        {
            var controller = 
                TestHomeControllerFactory.CreateHomeController(
                service.Object);

            var actionResult = controller.ErrorStatus(500);

            var viewResult = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            viewResult.ViewName.Should().Be("Error");
            
            var errorViewModel = 
                viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;

            errorViewModel.RequestId.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void StatusCode_WithCode403_ReturnsErrorViewWithModel()
        {
            var controller = 
                TestHomeControllerFactory.CreateHomeController(
                service.Object);

            var actionResult = controller.ErrorStatus(403);

            var viewResult = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            viewResult.ViewName.Should().Be("Error");
            
            var errorViewModel = 
                viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;

            errorViewModel.RequestId.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void StatusCode_WithActivityCurrent_UsesActivityId()
        {
            var activity = new Activity("TestActivity");
            activity.Start();

            try
            {
                var controller = 
                    TestHomeControllerFactory.CreateHomeController(
                    service.Object);

                var actionResult = controller.ErrorStatus(500);

                var viewResult = 
                    actionResult.Should().BeOfType<ViewResult>().Subject;

                var errorViewModel = 
                    viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
                
                errorViewModel.RequestId.Should().Be(activity.Id);
            }

            finally
            {
                activity.Stop();
            }
        }

        [Test]
        public void StatusCode_WithoutActivityCurrent_UsesTraceIdentifier()
        {
            var controller = TestHomeControllerFactory.CreateHomeController(
                service.Object);

            var actionResult = controller.ErrorStatus(500);

            var viewResult = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            var errorViewModel = 
                viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
            
            errorViewModel.RequestId.Should().Be(controller.HttpContext.TraceIdentifier);
        }

        [Test]
        public void Error_ReturnsViewWithErrorViewModel()
        {
            var controller = 
                TestHomeControllerFactory.CreateHomeController(
                service.Object);

            var actionResult = controller.Error();

            var viewResult = 
                actionResult.Should().BeOfType<ViewResult>().Subject;

            viewResult.ViewName.Should().BeNull();

            var model = 
                viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;

            model.RequestId.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void Error_WithActivityCurrent_UsesActivityId()
        {
            var activity = new Activity("TestActivity");
            activity.Start();

            try
            {
                var controller = 
                    TestHomeControllerFactory.CreateHomeController(
                    service.Object);

                var actionResult = controller.Error();

                var viewResult = 
                    actionResult.Should().BeOfType<ViewResult>().Subject;

                var errorViewModel = 
                    viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
                
                errorViewModel.RequestId.Should().Be(activity.Id);
            }

            finally
            {
                activity.Stop();
            }
        }

        [Test]
        public void Error_WithoutActivityCurrent_UsesTraceIdentifier()
        {
            var controller = 
                TestHomeControllerFactory.CreateHomeController(
                service.Object);

            var actionResult = controller.Error();

            var viewResult = actionResult.Should().BeOfType<ViewResult>().Subject;

            var errorViewModel = 
                viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
            
            errorViewModel.RequestId.Should().Be(controller.HttpContext.TraceIdentifier);
        }

        [Test]
        public void Error_HasResponseCacheAttribute()
        {
            var methodInfo = 
                typeof(Web.Controllers.HomeController).GetMethod("Error");

            var attributes = methodInfo!.GetCustomAttributes(
                typeof(ResponseCacheAttribute), 
                false);

            attributes.Should().NotBeEmpty();
            
            var cacheAttribute = 
                attributes[0].Should().BeOfType<ResponseCacheAttribute>().Subject;

            cacheAttribute.Duration.Should().Be(0);
            cacheAttribute.Location.Should().Be(ResponseCacheLocation.None);
            cacheAttribute.NoStore.Should().BeTrue();
        }

        [Test]
        public void ErrorStatus_HasAllowAnonymousAttribute()
        {
            var methodInfo = typeof(HomeController).
                GetMethod("ErrorStatus");

            var attributes = methodInfo!.GetCustomAttributes(
                typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), 
                false);

            attributes.Should().NotBeEmpty();
        }
    }
}