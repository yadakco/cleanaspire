namespace CleanAspire.PWA.Configurations;

public class ClientAppSettings
{
    public const string KEY = nameof(ClientAppSettings);
    public string AppName { get; set; } = "Progressive Web Application";
    public string Version { get; set; } = "0.0.1";
    public string ServiceBaseUrl { get; set; } = "http://127.0.0.1:8090";
    
}

