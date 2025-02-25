﻿namespace api.Helpers
{
    public static class HttpResponseHelper
    {
        public static (int StatusCode, string Message) InternalServerErrorFetching(string errorFor, ILogger logger, Exception ex)
        {
            logger.LogError(ex, $"An error occured while fetching {errorFor}.");
            return (500, "Internal Error");
        }
    }
}
