namespace Scaleway.Functions
{
    public class ScalewayContext
    {
        public bool IsRunningAsFunction { get; init; }

        public string? HostName { get; init; }
        public string? NamespaceId { get; init; }
        public string? ApplicationId { get; init; }
        public string? ApplicationName { get; init; }
        public int? ApplicationMemoryInMb { get; init; }
        public bool? RequiresAuthentication { get; init; }
        public string? Token { get; init; }
        public ScalewayToken? DecryptedToken { get; init; }
    }
}