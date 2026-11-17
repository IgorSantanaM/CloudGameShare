using System;
using System.Collections.Generic;
using System.Text;

namespace CloudGameShare.GamingSession.Infra.Services.Exceptions
{
    public class ResourceNotConfiguredException : Exception
    {
        public ResourceNotConfiguredException(string message) : base(message) {}
    }
}
