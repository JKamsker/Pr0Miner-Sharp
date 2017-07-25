using System.Collections.Generic;

namespace Pr0MinerSharp.DataTypes
{
    public class PoolStats
    {
        public double hashes { get; set; }
        public List<Toplist> toplist { get; set; }
    }
}