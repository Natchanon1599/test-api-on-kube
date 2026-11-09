using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TestKubeLibrary.Models.Services;

[DataContract]
public class StoreAPIRs
{
    [DataMember]
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [DataMember]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [DataMember]
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }
    [DataMember]
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [DataMember]
    [JsonPropertyName("category")]
    public string? Category { get; set; }
    [DataMember]
    [JsonPropertyName("image")]
    public string? Image { get; set; }
    [DataMember]
    [JsonPropertyName("rating")]
    public Rating? Rating { get; set; }
  
}
public class Rating
{
    [DataMember]
    [JsonPropertyName("rate")]
    public decimal? Rate { get; set; }
    [DataMember]
    [JsonPropertyName("count")]
    public int? Count { get; set; }
}