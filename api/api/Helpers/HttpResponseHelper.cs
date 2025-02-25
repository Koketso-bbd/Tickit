namespace api.Helpers
{
    public static class HttpResponseHelper
    {
        public static (int StatusCode, string Message) InternalServerErrorGet(string errorFor, ILogger logger, Exception ex)
        {
            logger.LogError(ex, $"An error occured while fetching {errorFor}.");
            return (500, "Internal Error");
        }

        public static (int StatusCode, string Message) InternalServerErrorDelete(string errorFor, ILogger logger, Exception ex)
        {
            logger.LogError(ex, $"An error occured while deleting {errorFor}.");
            return (500, "Internal Error");
        }


    }
}
