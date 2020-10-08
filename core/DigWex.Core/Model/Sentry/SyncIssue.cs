using SharpRaven.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigWex.Model.Sentry
{
    public class SyncIssue : SentryEvent
    {
        public SyncIssue(Exception exception) : base(exception)
        {
        }

        public SyncIssue(SentryMessage message) : base(message)
        {
        }
    }
}
