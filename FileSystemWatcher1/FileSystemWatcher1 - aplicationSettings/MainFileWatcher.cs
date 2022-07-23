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
            Console.Title = Properties.Settings.Default.NombreApp;
            Queue<struct_Mensaje> cola = new Queue<struct_Mensaje>();
            Object oo = new object();
            Logeador oLog;
            Utiles uu = new Utiles();
            int iMilisecKeyboard;

            if (uu.revisaExistDir(Properties.Settings.Default.Directorio) &&
                uu.revisaExistDir(Path.GetDirectoryName(Properties.Settings.Default.LogFullPath)))
            {
                oLog = new Logeador(Properties.Settings.Default.LogPathXML, cola, oo);

                iMilisecKeyboard = Properties.Settings.Default.MilisecsKeyboard;
                Watcher4Envio oWa = new Watcher4Envio(Properties.Settings.Default.LogPathXML);

                oWa.oLog = oLog;
                oWa.TipoOperacion = Properties.Settings.Default.TipoOeracion;
                oWa.PathInput = Properties.Settings.Default.Directorio;
                oWa.EnviosErroneos = Properties.Settings.Default.EnviosErroneos;
                oWa.Filtro = Properties.Settings.Default.Filtro;
                oWa.Backup = Properties.Settings.Default.Backup;
                oWa.IPDest = Properties.Settings.Default.IPDest;
                oWa.MilisecsToWait = Properties.Settings.Default.MilisecsToWait;
                oWa.AccionBackup = Properties.Settings.Default.AccionBackup;

                oWa.PortDest = Properties.Settings.Default.PortDest; //Convert.ToInt32(
                oWa.FTPUser = Properties.Settings.Default.FTPUser;
                oWa.FTPPssw = Properties.Settings.Default.FTPPssw;

                oWa.LogFullPath = Properties.Settings.Default.LogFullPath;
                oWa.LogPathXML = Properties.Settings.Default.LogPathXML;

                RevisadorDirectorio oReviDir = new RevisadorDirectorio(Properties.Settings.Default.LogPathXML);
                oReviDir.oLog = oLog;
                oReviDir.Backup = Properties.Settings.Default.Backup;
                oReviDir.DiasBackup = Properties.Settings.Default.DiasBackup;
                oReviDir.EliminaRO = Properties.Settings.Default.EliminaRO;
                oReviDir.MilisecsBorrado = Properties.Settings.Default.MilisecsBorrado;

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
                oWa.oLog.Escribe(TipoMsgLog.Info, string.Format("MilisecKeyboard  : {0}", iMilisecKeyboard));

                EventWaitHandle ewhEsperaKey = new EventWaitHandle(false, EventResetMode.AutoReset, "ewhEsperaKey"); 

                do
                {
                    while (Console.KeyAvailable == false)
                    {
                        ewhEsperaKey.WaitOne(TimeSpan.FromMilliseconds(iMilisecKeyboard), true);
                    }
                    cki = Console.ReadKey(true);
                } while (cki.Key.ToString() != "End");

                oWa.oLog.Escribe(TipoMsgLog.Info, "'End' --> Solicitando Fin de sub procesos");

                oWa.RequestStop();
                oReviDir.RequestStop();
                oLog.RequestStop();
                tReviDir.Abort();

                tWa.Join();
                tReviDir.Join();
                tLogueador.Join();

                oWa.oLog.Escribe(TipoMsgLog.Info, "'End' --> finalizar");
            }
        }
    }
}
