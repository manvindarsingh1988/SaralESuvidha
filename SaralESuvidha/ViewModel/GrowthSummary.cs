namespace SaralESuvidha.ViewModel;

public class GrowthSummary
{
    
    public string RetailUserId { get; set; }
    public string RetailerDetail { get; set; }
    public string Period { get; set; }
    public int? TotalCount { get; set; }
    public decimal? TotalSalesBetweenPeriod { get; set; }
    public int? OneMonthBackCount { get; set; }
    public decimal? OneMonthBackTotal { get; set; }
    public int? TwoMonthBackCount { get; set; }
    public decimal? TwoMonthBackTotal { get; set; }
    public int? ThreeMonthBackCount { get; set; }
    public decimal? ThreeMonthBackTotal { get; set; }
    public int? FirstDiffTotal { get; set; }
    public int? FirstDiffCount { get; set; }
    public int? SecondDiffTotal { get; set; }
    public int? SecondDiffCount { get; set; }
    public int? ThirdDiffTotal { get; set; }
    public int? ThirdDiffCount { get; set; }
    public int? COSTAvgTotalP { get; set; }
    public int? COSTAvgCountP { get; set; }
    public int? FSDiffTotal { get; set; }
    public int? FSDiffCount { get; set; }
    public int? STDiffTotal { get; set; }
    
}