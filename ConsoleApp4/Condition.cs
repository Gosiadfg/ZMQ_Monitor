using System.Collections.Generic;
using System.Threading;
using System;

namespace ConsoleApp4
{
    public class Condition : Message
    {
        public static int writers = 0;
        public static int readers = 0;

        public int allowWrite = 0;
        public int allowRead = 0;

        public static List<Thread> writersList = new List<Thread>();
        public static List<Thread> readersList = new List<Thread>();

        public static List<String> writersAddress = new List<String>();
        public static List<String> readersAddress = new List<String>();

        public void signal(int value)
        {
            SendSignal(writersAddress[0]);

            if (value == 0) {
                if (writersList.Count != 0)
                {
                    writersList.RemoveAt(0);
                    writersAddress.RemoveAt(0);
                }
            }
            allowWrite = 1;
        }

        public void signalAll()
        {
            SendSignalAll(writersAddress, readersAddress); 

            if (writersList.Count != 0)
            {
                signal(0);
                allowWrite = 1;
            }
            else
            {
                readersList.Clear();
                allowRead = 1;
            }
        }

        public void wait(int value, String adr)
        {
            if (value == 0)
            {
                allowWrite = 0;
                writersList.Add(Thread.CurrentThread);
                writersAddress.Add(adr);
                while (true)
                {
                    Thread.Sleep(1000);
                    if (allowWrite==1 && !(writersList.Contains(Thread.CurrentThread)))
                        break;
                }
            }
            else
            {
                allowRead = 0;
                readersList.Add(Thread.CurrentThread);
                readersAddress.Add(adr);
                while (true)
                {
                    Thread.Sleep(1000);
                    if (allowRead==1 && !(readersList.Contains(Thread.CurrentThread)))
                        break;
                }
            }
        }
    }
}