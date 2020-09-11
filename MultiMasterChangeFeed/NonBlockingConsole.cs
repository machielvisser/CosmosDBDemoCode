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
            new Thread(
              () =>
              {
                  while (true) Console.WriteLine(m_Queue.Take());
              })
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
