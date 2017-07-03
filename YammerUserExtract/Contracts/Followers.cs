using System.Collections.Generic;

namespace YammerUserExtract.Contracts
{
    public class Followers
    {
        public List<Follower> users { get; set; }
        public bool more_available { get; set; }
        
    }
    public class Follower
    {
        public int id { get; set; }
        public string name { get; set; }
        public string full_name { get; set; }
    }
}
