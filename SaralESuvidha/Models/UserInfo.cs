using System;

namespace SaralESuvidha.Models
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string Message { get; set; }
        public bool IsFailed { get; set; }
        public string Token { get; set; }
        public DateTime Expiry { get; set; }
    }
}
