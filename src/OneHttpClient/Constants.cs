using System;

namespace OneHttpClient
{
    public static class Constants
    {
        public static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(100);
        public static readonly TimeSpan DefaultConnectionLeaseTimeout = TimeSpan.FromMinutes(10);
        public static readonly int DefaultConnectionLimit = 10;
    }
}
