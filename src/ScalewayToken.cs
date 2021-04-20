using System;
using System.Text.Json.Serialization;

namespace Scaleway.Functions
{
    public class ScalewayClaim
    {
        [JsonPropertyName("namespace_id")]
        public string NamespaceId { get; init; } = string.Empty;
        [JsonPropertyName("application_id")]
        public string ApplicationId { get; init; } = string.Empty;
    }

    public class ScalewayToken
    {
        [JsonPropertyName("iss")]
        public string Issuer { get; init; } = string.Empty;
        [JsonPropertyName("aud")]
        public string Audience { get; init; } = string.Empty;
        [JsonPropertyName("sub")]
        public string Subject { get; init; } = string.Empty;
        [JsonPropertyName("nbf")]
        public long NotValidBeforeInSecondsSinceEpoch { get; init; } = DateTimeOffset.MinValue.ToUnixTimeSeconds();
        [JsonPropertyName("iat")]
        public long IssuedAtInSecondsSinceEpoch { get; init; } = DateTimeOffset.MinValue.ToUnixTimeSeconds();
        [JsonPropertyName("exp")]
        public long ExpiresAtInSecondsSinceEpoch { get; init; } = DateTimeOffset.MaxValue.ToUnixTimeSeconds();
        [JsonPropertyName("application_claim")]
        public ScalewayClaim[] Claims { get; init; } = Array.Empty<ScalewayClaim>();
    }
}