using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MultiMasterChangeFeed
{
    public static class NonBlockingConsole
    {
        private static readonly BlockingCollection<string> m_Queue = new BlockingCollection<string>();

        static NonBlockingConsole()
        {
            new Thread(() => { do Console.WriteLine(m_Queue.Take()); while (true); })
            {
                IsBackground = true
            }
            .Start();
        }

        public static void WriteLine(string value)
        {
            m_Queue.Add(value);
        }
    }
}
