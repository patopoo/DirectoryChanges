using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Security.Permissions;
using System.Configuration;
using System.Net;
using System.Threading;
using Comunes;

namespace FileSystemWatcher1
{
    class MainFileWatcher
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo cki;
            //Console.Title =  System.Configuration.ConfigurationManager.AppSettings["NombreApp;
            Console.Title =  System.Configuration.ConfigurationManager.AppSettings["NombreApp"];
            Queue<struct_Mensaje> cola = new Queue<struct_Mensaje>();
            Object o4lock = new object();
            Logeador oLog;
            Utiles uu = new Utiles();
            int iMilisecKeyboard;

            if (uu.revisaExistDir( System.Configuration.ConfigurationManager.AppSettings["Directorio"]) &&
                uu.revisaExistDir(Path.GetDirectoryName( System.Configuration.ConfigurationManager.AppSettings["LogFullPath"])))
            {
                //oLog = new Logeador( System.Configuration.ConfigurationManager.AppSettings["LogPathXML, cola, oo);
                oLog = new Logeador(cola, o4lock);

                iMilisecKeyboard = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MilisecsKeyboard"]);
                //Watcher4Envio oWa = new Watcher4Envio( System.Configuration.ConfigurationManager.AppSettings["LogPathXML);
                Watcher4Envio oWa = new Watcher4Envio();
                //RevisadorDirectorio oReviDir = new RevisadorDirectorio( System.Configuration.ConfigurationManager.AppSettings["LogPathXML);
                RevisadorDirectorio oReviDir = new RevisadorDirectorio();

                oWa.oLog = oLog;
                LeeParametrosFW(ref oWa, ref oReviDir);

                oReviDir.oLog = oLog;

                Thread tLogueador = new Thread(new ThreadStart(oLog.Escribidor));
                Thread tWa = new Thread(new ThreadStart(oWa.mdlWatcher));
                Thread tReviDir = new Thread(new ThreadStart(oReviDir.Revisador_de_Directorios));

                tLogueador.Name = "Logueador"; tLogueador.IsBackground = true; tLogueador.Priority = ThreadPriority.BelowNormal;
                tWa.Name = "Watcher"; tWa.IsBackground = true; tWa.Priority = ThreadPriority.Normal;
                tReviDir.Name = "Eliminador"; tReviDir.IsBackground = true; tReviDir.Priority = ThreadPriority.BelowNormal;
                tLogueador.Start();
                tWa.Start();
                tReviDir.Start();

                // Loop until worker thread activates.
                while (!tLogueador.IsAlive) ;
                while (!tReviDir.IsAlive) ;
                while (!tWa.IsAlive) ;
                
                // Wait for the user to quit the program.
                Console.WriteLine("Presione 'End' para finalizar.");
                oLog.Escribe(TipoMsgLog.Info, string.Format("MilisecKeyboard  : {0}", iMilisecKeyboard));

                EventWaitHandle ewhEsperaKey = new EventWaitHandle(false, EventResetMode.AutoReset, "ewhEsperaKey"); 

                do
                {
                    while (Console.KeyAvailable == false)
                    {
                        ewhEsperaKey.WaitOne(TimeSpan.FromMilliseconds(iMilisecKeyboard), true);
                    }
                    cki = Console.ReadKey(true);
                } while (cki.Key.ToString() != "End");

                oLog.Escribe(TipoMsgLog.Info, "'End' --> Solicitando Fin de sub procesos");

                oWa.RequestStop();
                oReviDir.RequestStop();
                oLog.RequestStop();
                tReviDir.Abort();

                tWa.Join();
                tReviDir.Join();
                tLogueador.Join();

                oLog.Escribe(TipoMsgLog.Info, "'End' --> finalizar");
            }
        }

        private static void LeeParametrosFW(ref Watcher4Envio oWa, ref RevisadorDirectorio oReviDir)
        {
            oWa.TipoOperacion = System.Configuration.ConfigurationManager.AppSettings["TipoOeracion"];
            oWa.PathInput = System.Configuration.ConfigurationManager.AppSettings["Directorio"];
            oWa.EnviosErroneos = System.Configuration.ConfigurationManager.AppSettings["EnviosErroneos"];
            oWa.Filtro = System.Configuration.ConfigurationManager.AppSettings["Filtro"];
            oWa.Backup = System.Configuration.ConfigurationManager.AppSettings["Backup"];
            oWa.IPDest = System.Configuration.ConfigurationManager.AppSettings["IPDest"];
            oWa.MilisecsToWait = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MilisecsToWait"]);
            oWa.AccionBackup = System.Configuration.ConfigurationManager.AppSettings["AccionBackup"];

            oWa.PortDest = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["PortDest"]); //Convert.ToInt32(
            oWa.FTPUser = System.Configuration.ConfigurationManager.AppSettings["FTPUser"];
            oWa.FTPPssw = System.Configuration.ConfigurationManager.AppSettings["FTPPssw"];

            oWa.LogFullPath = System.Configuration.ConfigurationManager.AppSettings["LogFullPath"];
            //oWa.LogPathXML =  System.Configuration.ConfigurationManager.AppSettings["LogPathXML;

            oReviDir.Backup = System.Configuration.ConfigurationManager.AppSettings["Backup"];
            oReviDir.DiasBackup = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["DiasBackup"]);
            oReviDir.EliminaRO = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["EliminaRO"]);
            oReviDir.MilisecsBorrado = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MilisecsBorrado"]);
        }
    }
}
