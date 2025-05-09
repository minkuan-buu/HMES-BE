namespace HMES.Data.DTO.RequestModel;

public class GHNReqModel
{
    public int CODAmount { get; set; }
    public DateTime? CODTransferDate { get; set; }
    public string ClientOrderCode { get; set; }
    public int ConvertedWeight { get; set; }
    public string Description { get; set; }
    public GHNFee Fee { get; set; }
    public int Height { get; set; }
    public bool IsPartialReturn { get; set; }
    public int Length { get; set; }
    public string OrderCode { get; set; }
    public string PartialReturnCode { get; set; }
    public int PaymentType { get; set; }
    public string Reason { get; set; }
    public string ReasonCode { get; set; }
    public long ShopID { get; set; }
    public string Status { get; set; }
    public DateTime Time { get; set; }
    public int TotalFee { get; set; }
    public string Type { get; set; }
    public string Warehouse { get; set; }
    public int Weight { get; set; }
    public int Width { get; set; }
}

public class GHNFee
{
    public int CODFailedFee { get; set; }
    public int CODFee { get; set; }
    public int Coupon { get; set; }
    public int DeliverRemoteAreasFee { get; set; }
    public int DocumentReturn { get; set; }
    public int DoubleCheck { get; set; }
    public int Insurance { get; set; }
    public int MainService { get; set; }
    public int PickRemoteAreasFee { get; set; }
    public int R2S { get; set; }
    public int Return { get; set; }
    public int StationDO { get; set; }
    public int StationPU { get; set; }
    public int Total { get; set; }
}