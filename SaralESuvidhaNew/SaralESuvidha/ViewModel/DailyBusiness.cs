using System;

namespace SaralESuvidha.ViewModel;

public class DailyBusiness
{
    public DateTime FirstDay { get; set; }
    public DateTime Today { get; set; }
    public decimal MonthTillYesterday { get; set; }
    public decimal TodaySales { get; set; }
}