using ContractorControl.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractorControl.Domain.Entities;

[Table("contracts", Schema = "contractor_control")]
public class Contract : IAuditable
{
    [Column("id")]
    public int Id { get; set; }
    [Column("code")]
    public string Code { get; set; } = string.Empty;
    [Column("create_at")]
    public DateTime CreateAt { get; set; }
    [Column("update_at")]
    public DateTime? UpdateAt { get; set; }
    [Column("delete_at")]
    public DateTime? DeleteAt { get; set; }
}
