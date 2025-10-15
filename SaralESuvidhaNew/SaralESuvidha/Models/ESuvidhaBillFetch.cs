using System;

namespace SaralESuvidha.Models;

public class ESuvidhaBillFetch
{
    public long Id { get; set; }
    public string ConsumerId { get; set; }
    public string ESBillResponse { get; set; }
    public DateTime CreateDate { get; set; }
    
}