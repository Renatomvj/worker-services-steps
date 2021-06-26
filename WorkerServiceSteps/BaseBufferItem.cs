using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServiceSteps
{
    public abstract class BaseBufferItem
    {
        public int RetryCount { get; set; }
    }

    public class BufferItem : BaseBufferItem
    {
        public int ItemId { get; set; }
        public string Text { get; set; }
        public bool ContainsBadWords { get; set; }
    }
}
