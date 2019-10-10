using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp4;
using System.Threading;

namespace ConsoleApp4
{
    public class ReadWrite : Condition
    {
        Condition condition = new Condition();
        private object myLock = new object();

        public void startreading(String id, String adr1, String adr2)
        {
            if (writers > 0)
            {
                Console.WriteLine("{0} sleeps", id);
                condition.wait(1, adr2);
            }  
            lock (myLock)
            {
                readers++;
            }
            Console.WriteLine("{0} enters", id);
            Thread.Sleep(3 * 1000);
            SendRcvMsg(id, "endreading", adr2, adr1);
        }

        public void startwriting(String id, String adr1, String adr2)
        {
            if (readers + writers > 0)
            {
                Console.WriteLine("{0} sleeps", id);
                condition.wait(0, adr2);
            }
            lock (myLock)
            {
                writers = 1;
            }
            Console.WriteLine("{0} enters", id);
            Thread.Sleep(3 * 1000);
            SendRcvMsg(id, "endwriting", adr2, adr1);
        }

        public void endreading(String id)
        {
            lock (myLock)
            {
                readers--;
            }
            if (readers == 0)
            {
                Console.WriteLine("{0} leaves and send wakeup", id);
                condition.signal(0);
            }
            else Console.WriteLine("{0} leaves", id);

        }

        public void endwriting(String id)
        {
            Console.WriteLine("{0} leaves and send wakeup", id);
            lock (myLock)
            {
                writers = 0;
            }
            condition.signalAll();
        }
    }
}