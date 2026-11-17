namespace CloudGameShare.Catalog.Application.Exceptions;

public sealed class SteamStoreUnavailableException(Exception innerException)
    : Exception("Steam is temporarily unavailable. Try again shortly.", innerException);
