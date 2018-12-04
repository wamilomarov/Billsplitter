using System.Runtime.Serialization;

namespace Billsplitter.Models
{
    [DataContract]
    public class GoogleUser
    {
        [DataMember(Name = "sub")]
        public string Sub { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "email")]
        public string Email { get; set; }
        [DataMember(Name = "picture")]
        public string Picture { get; set; }
    }
}