using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ZeroMQ;

namespace ConsoleApp4
{ 
    public static class Context
    {
        public static ZContext ContextCopy;
        public static ZContext GetContext()
        {
            return ContextCopy;
        }
    }

    public partial class Program:Message
    {
        private static object myLock = new object();

        static void Reader(ZContext context, int i, string adr1, string adr2, ReadWrite readwrite)
        {
            string ServerAddress = "tcp://127.0.0.1:5569";

            SendRcvMsg("Reader" + i, "startreading", adr1, ServerAddress);

            //new Thread(() => SendRcvMsg("Reader" + i, "", adr1, ServerAddress)).Start();
        }
        static void Writer(ZContext context, int i, string adr1, string adr2, ReadWrite readwrite)
        {
            string ServerAddress = "tcp://127.0.0.1:5569";

            SendRcvMsg("Writer" + i, "startwriting", adr1, ServerAddress);

            //new Thread(() => SendRcvMsg("Writer" + i, "", adr1, ServerAddress)).Start();
        }
            
        static void Server(ZContext context, ReadWrite readwrite)
        {
            using (var frontend = new ZSocket(context, ZSocketType.ROUTER))
            using (var backend = new ZSocket(context, ZSocketType.DEALER))
            {
                frontend.Bind("tcp://*:5569");
                backend.Bind("inproc://backend");

                for (int i = 0; i < 5; ++i)
                {
                    new Thread(() => ProgramStart(context, readwrite)).Start();
                }

                ZError error;
                if (!ZContext.Proxy(frontend, backend, out error))
                {
                    if (error == ZError.ETERM)
                        return;    // Interrupted
                    throw new ZException(error);
                }
            }
        }

        static void ProgramStart(ZContext context, ReadWrite readwrite)
        {
            using (var worker = new ZSocket(context, ZSocketType.DEALER))
            {
                worker.Connect("inproc://backend");

                ZError error;
                ZMessage request;
 
                while (true)
                {
                    if (null == (request = worker.ReceiveMessage(out error)))
                    {
                        if (error == ZError.ETERM)
                        {
                            Console.WriteLine("error: context terminated");
                            return;    // Interrupted
                        }
                        throw new ZException(error);
                    }
                    using (request)
                    {
                        string identity = request[1].ReadString();
                        string address = request[2].ReadString();
                        string content = request[3].ReadString();
                        string from = "tcp://127.0.0.1:5569";
                      
                        Console.WriteLine("Monitor received: {0} from {1}", content, identity);

                        Thread.Sleep(1000);

                        if (content == "startreading")
                        {
                            readwrite.startreading(identity, from, address);
                        }
                        if (content == "startwriting")
                        {
                            readwrite.startwriting(identity, from, address);
                        }
                        if (content == "endreading")
                        {
                            readwrite.endreading(identity);
                        }
                        if (content == "endwriting")
                        {
                            readwrite.endwriting(identity);
                        }
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            while (true)
            {
                using (var context = new ZContext())
                {
                    Context.ContextCopy = context;

                    ReadWrite readWrite = new ReadWrite();
                    new Thread(() => Reader(context, 0, "tcp://127.0.0.1:5570", "tcp://*:5570", readWrite)).Start();
                    new Thread(() => Reader(context, 1, "tcp://127.0.0.1:5571", "tcp://*:5571", readWrite)).Start();
                    new Thread(() => Writer(context, 2, "tcp://127.0.0.1:5572", "tcp://*:5572", readWrite)).Start();
                    new Thread(() => Reader(context, 3, "tcp://127.0.0.1:5573", "tcp://*:5573", readWrite)).Start();
                    new Thread(() => Writer(context, 4, "tcp://127.0.0.1:5574", "tcp://*:5574", readWrite)).Start();
                    new Thread(() => Server(context, readWrite)).Start();

                    Thread.Sleep(15 * 1000);
                }
                Console.WriteLine("\nPress q to quit or any other key to repeat.");
                if (Console.ReadKey(true).Key == ConsoleKey.Q) break;
            }  
        }
    }
}