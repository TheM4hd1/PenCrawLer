using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeNCrawLer_0._1._0.Utilities
{
    static class RequestCalculator
    {
        static object lockCrawler = new object();
        public static int crawlerRequests = 0;
        static int currentCrawlerRequests = 0;

        static object lockDirbuster = new object();
        public static int dirbusterRequests = 0;
        static int currentDirbusterRequests = 0;

        public static int CalculateCrawlerSpeed()
        {
            lock(lockCrawler)
            {
                int requestsPerSecond = crawlerRequests - currentCrawlerRequests;
                currentCrawlerRequests = crawlerRequests;

                return requestsPerSecond;
            }
        }

        public static int CalculateDirbusterSpeed()
        {
            lock(lockDirbuster)
            {
                int requestsPerSecond = dirbusterRequests - currentDirbusterRequests;
                currentDirbusterRequests = dirbusterRequests;

                return requestsPerSecond;
            }
        }

        public static void IncreaseCrawler()
        {
            Interlocked.Increment(ref crawlerRequests);
        }

        public static void IncreaseDirbuster()
        {
            Interlocked.Increment(ref dirbusterRequests);
        }

        public static void ResetData()
        {
            crawlerRequests = 0;
            currentCrawlerRequests = 0;

            dirbusterRequests = 0;
            currentDirbusterRequests = 0;
        }

    }
}
