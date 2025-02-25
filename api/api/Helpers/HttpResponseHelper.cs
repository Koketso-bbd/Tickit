namespace api.Helpers
{
    public static class HttpResponseHelper
    {
        public static (int StatusCode, string Message) InternalServerErrorGet(string label, ILogger logger, Exception ex)
        {
            logger.LogError(ex, $"An error occured while fetching {label}.");
            return (500, "Internal Error");
        }

        public static (int StatusCode, string Message) InternalServerErrorDelete(string label, ILogger logger, Exception ex)
        {
            logger.LogError(ex, $"An error occured while deleting {label}.");
            return (500, "Internal Error");
        }

        public static (int StatusCode, string Message) InternalServerErrorPost(string label, ILogger logger, Exception ex)
        {
            logger.LogError(ex, $"An error occured while adding {label}.");
            return (500, "Internal Error");
        }

        public static (int StatusCode, string Message) InternalServerErrorPut(string label, ILogger logger, Exception ex)
        {
            logger.LogError(ex, $"An error occured while updating {label}.");
            return (500, "Internal Error");
        }
    }
}
