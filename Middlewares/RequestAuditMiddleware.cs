using System.Diagnostics;

namespace TelemarketingControlSystem.Middlewares
{
    public class RequestAuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestAuditMiddleware> _logger;

        public RequestAuditMiddleware(RequestDelegate next, ILogger<RequestAuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Read request details
            var request = context.Request;
            var userAgent = request.Headers["User-Agent"].ToString();
            var method = request.Method;
            var path = request.Path;

            _logger.LogInformation($"[Request] {method} {path} - User-Agent: {userAgent}");

            // Copy the response body
            var originalBodyStream = context.Response.Body;
            using (var newBodyStream = new MemoryStream())
            {
                context.Response.Body = newBodyStream;

                // Call the next middleware
                await _next(context);

                stopwatch.Stop();

                // Read response body
                newBodyStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(newBodyStream).ReadToEndAsync();
                newBodyStream.Seek(0, SeekOrigin.Begin);

                // Compute response hash
                var responseHash = Helper.ComputeSHA256.ComputeSHA256Func(responseBody);
                var statusCode = context.Response.StatusCode;

                _logger.LogInformation($"[Response] {method} {path} - Status: {statusCode}, Hash: {responseHash}, Time: {stopwatch.ElapsedMilliseconds}ms");

                // Copy the response back
                await newBodyStream.CopyToAsync(originalBodyStream);
            }
        }

        
    }
}
