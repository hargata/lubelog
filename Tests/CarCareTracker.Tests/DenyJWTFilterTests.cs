using System.Reflection;
using System.Security.Claims;
using CarCareTracker.Controllers;
using CarCareTracker.Filter;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace CarCareTracker.Tests
{
    public class DenyJWTFilterTests
    {
        private static ActionExecutingContext Context(params string[] roles)
        {
            var identity = new ClaimsIdentity("Custom");
            foreach (var role in roles) identity.AddClaim(new Claim(ClaimTypes.Role, role));
            var http = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
            var action = new ActionContext(http, new RouteData(), new ActionDescriptor());
            return new ActionExecutingContext(action, new List<IFilterMetadata>(), new Dictionary<string, object?>(), controller: null!);
        }

        [Fact]
        public void JwtRequestIsRefused()
        {
            var ctx = Context("APIAuth", DenyJWTFilter.JWTAuthRole, nameof(UserData.IsRootUser));
            new DenyJWTFilter().OnActionExecuting(ctx);
            Assert.IsType<JsonResult>(ctx.Result);
            Assert.Equal(403, ctx.HttpContext.Response.StatusCode);
        }

        [Fact]
        public void OtherAuthIsUntouched()
        {
            var ctx = Context("APIAuth", "APIKeyAuth", nameof(UserData.IsRootUser));
            new DenyJWTFilter().OnActionExecuting(ctx);
            Assert.Null(ctx.Result);
        }

        /// <summary>
        /// Root-only API actions (backups, cleanup, temp files, demo restore, sending reminders) manage the server or
        /// export the whole database, credentials included. A short-lived identity-provider token must never reach them,
        /// even when it maps to the root user.
        /// </summary>
        [Fact]
        public void EveryRootOnlyApiActionRefusesJwt()
        {
            var rootOnly = typeof(APIController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes<AuthorizeAttribute>().Any(a => a.Roles == nameof(UserData.IsRootUser)))
                .ToList();
            Assert.NotEmpty(rootOnly);
            Assert.All(rootOnly, m => Assert.NotNull(m.GetCustomAttribute<DenyJWTFilter>()));
        }
    }
}
