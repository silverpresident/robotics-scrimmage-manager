using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RoboticsManager.Web.Models;

namespace RoboticsManager.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var requestId = HttpContext.TraceIdentifier;

            _logger.LogWarning(
                "Error {StatusCode} occurred. Request ID: {RequestId}, Path: {Path}",
                statusCode,
                requestId,
                statusCodeResult?.OriginalPath
            );

            return View("Error", new ErrorViewModel(statusCode, requestId));
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var requestId = HttpContext.TraceIdentifier;

            if (exceptionDetails?.Error != null)
            {
                _logger.LogError(
                    exceptionDetails.Error,
                    "An unhandled exception occurred. Request ID: {RequestId}, Path: {Path}",
                    requestId,
                    exceptionDetails.Path
                );

                return View(new ErrorViewModel(exceptionDetails.Error, requestId));
            }

            _logger.LogError(
                "An unknown error occurred. Request ID: {RequestId}",
                requestId
            );

            return View(new ErrorViewModel(requestId));
        }

        [Route("AccessDenied")]
        public IActionResult AccessDenied()
        {
            var requestId = HttpContext.TraceIdentifier;

            _logger.LogWarning(
                "Access denied. Request ID: {RequestId}, User: {User}",
                requestId,
                User.Identity?.Name ?? "Unknown"
            );

            return View("Error", new ErrorViewModel(403, requestId));
        }

        [Route("NotFound")]
        public IActionResult NotFound()
        {
            var requestId = HttpContext.TraceIdentifier;
            var path = HttpContext.Request.Path;

            _logger.LogWarning(
                "Resource not found. Request ID: {RequestId}, Path: {Path}",
                requestId,
                path
            );

            return View("Error", new ErrorViewModel(404, requestId));
        }
    }
}
