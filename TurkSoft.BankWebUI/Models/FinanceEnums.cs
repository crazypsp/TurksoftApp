namespace TurkSoft.BankWebUI.Models
{
    public enum TransferRecordStatus { Draft = 0, Ready = 1, Queued = 2, Exported = 3 }
    public enum QueueStatus { Pending = 0, Processing = 1, Success = 2, Failed = 3 }
    public enum BankPullStatus { Ok = 0, Warning = 1, Error = 2 }
    public enum ConnectionType { Api = 0, HostToHost = 1, File = 2 }
}
