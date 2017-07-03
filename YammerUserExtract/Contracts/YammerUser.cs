using System.Collections.Generic;

namespace YammerUserExtract.Contracts
{
    public class YammerUser
    {
        public int id { get; set; }
        public string full_name { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string job_title { get; set; }
        public string location { get; set; }
        public string network_name { get; set; }
        
    }
}
