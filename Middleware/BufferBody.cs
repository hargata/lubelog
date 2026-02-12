namespace CarCareTracker.Middleware
{
    public class BufferBody
    {
        private readonly RequestDelegate _next;
        public BufferBody(RequestDelegate next) =>
            _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            await _next(context);
        }
    }
}