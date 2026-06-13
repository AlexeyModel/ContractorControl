using ContractorControl.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractorControl.Domain.Entities;

[Table("contract_execution", Schema = "contractor_control")]
public class ContractExecution : IAuditable
{
    [Column("id")]
    public int Id { get; set; }
    [Column("contract_id")]
    public int ContractId { get; set; }
    [Column("instance_contract_execution")]
    public Guid InstanceContractExecution { get; set; }
    [Column("create_at")]
    public DateTime CreateAt { get; set; }
    [Column("update_at")]
    public DateTime? UpdateAt { get; set; }
    [Column("delete_at")]
    public DateTime? DeleteAt { get; set; }
}
