using ContractorControl.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractorControl.Domain.Entities;

[Table("state_type", Schema = "contractor_control")]
public class StateType : IAuditable
{
    [Column("id")]
    public int Id { get; set; }
    [Column("type")]
    public string Type { get; set; } = string.Empty;
    [Column("create_at")]
    public DateTime CreateAt { get; set; }
    [Column("update_at")]
    public DateTime? UpdateAt { get; set; }
    [Column("delete_at")]
    public DateTime? DeleteAt { get; set; }
}
