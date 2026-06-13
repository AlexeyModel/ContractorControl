using System.ComponentModel.DataAnnotations.Schema;

namespace ContractorControl.Domain.Entities;

[Table("versions", Schema = "contractor_control")]
public class Version
{
    [Column("id")]
    public int Id { get; set; }
    [Column("version")]
    public string VersionNumber { get; set; } = string.Empty;
    [Column("release_notes")]
    public string? ReleaseNotes { get; set; }
}
