using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;
using System.Threading;

namespace ConsoleApp4
{
    public abstract class Message 
    {
        private static object myLock = new object();

        public static void SendRcvMsg(String identity, String text, String adr1, String adr2)
        {
            ZContext context = Context.GetContext();

            using (var client = new ZSocket(context, ZSocketType.DEALER))
            {
                client.Identity = Encoding.UTF8.GetBytes(identity);
                client.Connect(adr2);

                ZError error;
                //ZMessage incoming;
                //var poll = ZPollItem.CreateReceiver();

                using (var outgoing = new ZMessage())
                {
                    outgoing.Add(new ZFrame(client.Identity));
                    outgoing.Add(new ZFrame(adr1));
                    outgoing.Add(new ZFrame(text));

                    if (!client.Send(outgoing, out error))
                    {
                        Console.WriteLine("error");
                        if (error == ZError.ETERM)
                            return;    // Interrupted
                        throw new ZException(error);
                    }

                    Console.WriteLine("{0} send: {1}", identity, text);
                }

                /*while (true)
                {
                    if (!client.PollIn(poll, out incoming, out error, TimeSpan.FromMilliseconds(10)))
                    {
                        Console.WriteLine("error: reader/writer while receiving");

                        if (error == ZError.EAGAIN)
                        {
                            Thread.Sleep(1);
                            continue;
                        }
                        if (error == ZError.ETERM)
                            return;    // Interrupted
                        throw new ZException(error);
                    }

                    using (incoming)
                    {
                        string messageText = incoming[1].ReadString();
                        Console.WriteLine("{0} received: {1}", identity, messageText); 
                    }
                }*/
            }
        }

        public static void SendSignal(String adr)
        {
            ZContext context = Context.GetContext();

            using (var client = new ZSocket(context, ZSocketType.DEALER))
            {
                client.Connect(adr);
                ZError error;

                using (var outgoing = new ZMessage())
                {
                    outgoing.Add(new ZFrame(adr));
                    outgoing.Add(new ZFrame("wakeup"));

                    if (!client.Send(outgoing, out error))
                    {
                        Console.WriteLine("error");
                        if (error == ZError.ETERM)
                            return;    // Interrupted
                        throw new ZException(error);
                    }    
                    Console.WriteLine("send wakeup to {0}", adr);
                }
            }
        }

        public static void SendSignalAll(List <String> writers, List <String> readers)
        {
            ZContext context = Context.GetContext();

            using (var client = new ZSocket(context, ZSocketType.DEALER))
            {
                foreach (String adr in writers)
                {
                    client.Connect(adr);
                    ZError error;

                    using (var outgoing = new ZMessage())
                    {
                        outgoing.Add(new ZFrame(adr));
                        outgoing.Add(new ZFrame("wakeup"));

                        if (!client.Send(outgoing, out error))
                        {
                            Console.WriteLine("error");
                            if (error == ZError.ETERM)
                                return;    // Interrupted
                            throw new ZException(error);
                        }
                        Console.WriteLine("send wakeup to {0}", adr);
                    }
                }
                foreach (String adr in readers)
                {
                    client.Connect(adr);
                    ZError error;

                    using (var outgoing = new ZMessage())
                    {
                        outgoing.Add(new ZFrame(adr));
                        outgoing.Add(new ZFrame("wakeup"));

                        if (!client.Send(outgoing, out error))
                        {
                            Console.WriteLine("error");
                            if (error == ZError.ETERM)
                                return;    // Interrupted
                            throw new ZException(error);
                        }
                        Console.WriteLine("send wakeup to {0}", adr);
                    }
                }
            }
        }
    }
}