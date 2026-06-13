using ContractorControl.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ContractorControl.Domain.Entities;

[Table("events", Schema = "contractor_control")]
public class Event : IAuditable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("contract_execution_id")]
    public int ContractExecutionId { get; set; }
    [Column("state_id")]
    public int StateId { get; set; }
    [Column("create_at")]
    public DateTime CreateAt { get; set; }
    [Column("update_at")]
    public DateTime? UpdateAt { get; set; }
    [Column("delete_at")]
    public DateTime? DeleteAt { get; set; }
    [JsonIgnore]
    [ForeignKey(nameof(StateId))] // Явное указание внешнего ключа
    public State? StateRef { get; set; }
    [JsonIgnore]
    [ForeignKey(nameof(ContractExecutionId))] // Явное указание внешнего ключа
    public ContractExecution? ContractExecutionRef { get; set; }
}
