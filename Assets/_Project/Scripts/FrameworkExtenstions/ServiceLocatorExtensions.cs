using System;
using TitusGames.Framework;

public static class ServiceLocatorExtensions
{
    /// <summary>
    /// Safely attempts to retrieve a service without crashing if missing.
    /// </summary>
    public static bool TryGet<T>(this ServiceLocator locator, out T service)
    {
        if (locator == null)
        {
            service = default;
            return false;
        }

        try
        {
            service = locator.Get<T>();
            return true;
        }
        catch (Exception)
        {
            service = default;
            return false;
        }
    }
}