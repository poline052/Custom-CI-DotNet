using System;

namespace Com.CI.Infrastructure
{
    public interface ICILogger
    {
        void WriteError(string error, Exception e);
        void WriteInfo(string info);
        void WriteWarning(string warning, Exception e);
    }
}
