using ContractorControl.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ContractorControl.Domain.Entities;

[Table("states", Schema = "contractor_control")]
public class State : IAuditable
{
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("description")]
    public string? Description { get; set; }
    [Column("state_type_id")]
    public int StateTypeId { get; set; }
    [Column("contract_id")]
    public int ContractId { get; set; }
    [Column("create_at")]
    public DateTime CreateAt { get; set; }
    [Column("update_at")]
    public DateTime? UpdateAt { get; set; }
    [Column("delete_at")]
    public DateTime? DeleteAt { get; set; }

    [JsonIgnore]
    public StateType? StateTypeRef { get; set; }
}
