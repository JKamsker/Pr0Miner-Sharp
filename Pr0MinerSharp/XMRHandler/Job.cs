using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0MinerSharp.XMRHandler
{
    public class Job
    {
        private string blob;
        private string job_id;
        private string target;

        public Job(string blob, string job_id, string target)
        {
            this.blob = blob;
            this.job_id = job_id;
            this.target = target;
        }

        public string getBlob()
        {
            return blob;
        }

        public string getJob_id()
        {
            return job_id;
        }

        public string getTarget()
        {
            return target;
        }
    }
}