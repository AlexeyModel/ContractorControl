namespace ContractorControl.Domain.Common;

public interface IAuditable
{
    public DateTime CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public DateTime? DeleteAt { get; set; }
}
