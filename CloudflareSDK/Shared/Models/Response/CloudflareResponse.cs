namespace CloudflareSDK.Shared.Models.Response;

public class CloudflareResponse<T>
{
    public List<CloudflareErrorResponse> Errors { get; set; }
    public List<CloudflareResponseMessage> Messages { get; set; }
    public bool Success { get; set; }
    public T Result { get; set; }
}

public class CloudflareErrorResponse
{
    public int Code { get; set; }
    public string Message { get; set; }
}

public class CloudflareResponseMessage
{
    public int Code { get; set; }
    public string Message { get; set; }
}