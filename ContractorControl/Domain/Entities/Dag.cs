using ContractorControl.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractorControl.Domain.Entities;

[Table("dags", Schema = "contractor_control")]
public class Dag : IAuditable
{
    [Column("id")]
    public int Id { get; set; }
    [Column("state_source_id")]
    public int StateSourceId { get; set; }
    [Column("state_destination_id")]
    public int StateDestinationId { get; set; }
    [Column("create_at")]
    public DateTime CreateAt { get; set; }
    [Column("update_at")]
    public DateTime? UpdateAt { get; set; }
    [Column("delete_at")]
    public DateTime? DeleteAt { get; set; }
}
