using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Comunes
{
    public class Logeador
    {
        LogApp oLog;
        Queue<struct_Mensaje> cola;
        Object o4Lock;
        public int IntervaloLog { get; set; }

        // Volatile is used as hint to the compiler that this data 
        // member will be accessed by multiple threads. 
        private volatile bool _shouldStop;

        public Logeador(/*String sLogPathXML,*/ Queue<struct_Mensaje> cola, Object oo)
        {
            try
            {
                this.cola = cola;
                this.o4Lock = oo;
                //oLog = new LogApp(sLogPathXML);
                oLog = new LogApp();
                this.IntervaloLog = 2;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        public void Escribe(Comunes.TipoMsgLog tipoMsg, String sMsg)
        {
            struct_Mensaje sm = new struct_Mensaje();

            sm.tipoMsg = tipoMsg;
            sm.sMsg = sMsg;
            sm.sStackTrace = "";
            cola.Enqueue(sm);
        }

        public void Escribe(Comunes.TipoMsgLog tipoMsg, String sMsg, String sStackTrace)
        {
            struct_Mensaje sm = new struct_Mensaje();

            sm.tipoMsg = tipoMsg;
            sm.sMsg = sMsg;
            sm.sStackTrace = sStackTrace;
            cola.Enqueue(sm);
        }

        public void Escribidor()
        {
            struct_Mensaje item;
            string sSource;
            string sLog;
            string sEvent;
            EventLogEntryType tipoEvento = new EventLogEntryType();
            EventWaitHandle ewhLogger = new EventWaitHandle(false, EventResetMode.AutoReset, "ewhLogger");

            sSource = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            sLog = "Application";

            Console.WriteLine("ciclo de trabajo Escribidor");
            while (!_shouldStop)
            {
                lock (o4Lock)
                {
                    if (cola.Count > 0)
                    {
                        item = cola.Dequeue();
                        if (item.tipoMsg == TipoMsgLog.Error)
                        {
                            try
                            {
                                if (!EventLog.SourceExists(sSource))
                                    EventLog.CreateEventSource(sSource, sLog);

                                if (item.tipoMsg == TipoMsgLog.Error)
                                    tipoEvento = EventLogEntryType.Error;
                                else if (item.tipoMsg == TipoMsgLog.Warning)
                                    tipoEvento = EventLogEntryType.Warning;
                                else if (item.tipoMsg == TipoMsgLog.Info)
                                    tipoEvento = EventLogEntryType.Information;
                                else if (item.tipoMsg == TipoMsgLog.Debug)
                                    tipoEvento = EventLogEntryType.Information;

                                sEvent = item.sMsg;
                                EventLog.WriteEntry(sSource, sEvent, tipoEvento);
                                if (item.sStackTrace != "")
                                    EventLog.WriteEntry(sSource, item.sStackTrace, tipoEvento);
                            }
                            catch (Exception ex)
                            {
                                EventLog.WriteEntry(sSource, "Error en Escribidor : " + ex.ToString(), EventLogEntryType.Error);
                            }
                        }
                        oLog.Escribe(item.tipoMsg, item.sMsg);
                    }
                }
                ewhLogger.WaitOne(TimeSpan.FromMilliseconds(this.IntervaloLog), true);
            }
        }
    }
}
