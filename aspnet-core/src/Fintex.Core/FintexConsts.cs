using Fintex.Debugging;

namespace Fintex;

public class FintexConsts
{
    public const string LocalizationSourceName = "Fintex";

    public const string ConnectionStringName = "Default";

    public const bool MultiTenancyEnabled = true;


    /// <summary>
    /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
    /// </summary>
    public static readonly string DefaultPassPhrase =
        DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "49622cd800c448e092e300906f8b743a";
}
