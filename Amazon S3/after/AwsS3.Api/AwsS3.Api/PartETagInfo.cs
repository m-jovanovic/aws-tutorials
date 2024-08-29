using System.ComponentModel.DataAnnotations;

public class PartETagInfo
{
    [Required]
    public int PartNumber { get; set; }

    [Required]
    public string ETag { get; set; }
}
