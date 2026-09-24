namespace Application.Models.VideoProcessing;

public class VideoProgressUpdate
{
    public double Percentage { get; set; }
    public string EstimatedTimeRemaining { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class VideoProcessingResult
{
    public string TrackingId { get; set; } = string.Empty;
}
