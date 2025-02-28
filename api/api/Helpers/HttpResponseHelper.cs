namespace api.Helpers
{
    public static class HttpResponseHelper
    {
        public static (int StatusCode, string Message) InternalServerError(string label, ILogger logger, Exception ex)
        {
            logger.LogError(ex, $"An error occured while {label}.");
            return (500, "Internal Error");
        }
    }
}
