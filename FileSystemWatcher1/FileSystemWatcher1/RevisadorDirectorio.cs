using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

using System.IO;
using System.Security.Permissions;
using System.Configuration;
using System.Net;
using System.Threading;
using Comunes;

namespace FileSystemWatcher1
{
    public class RevisadorDirectorio
    {
        public String Backup {get; set;}
        public int DiasBackup { get; set; }
        public int MilisecsBorrado { get; set; }
        public bool EliminaRO { get; set; }
        public Logeador oLog { get; set; }
        Object o4Lock = new Object();

        public RevisadorDirectorio()
        {
            _shouldStop = false;
            this.MilisecsBorrado = -1;
        }

        // This method will be called when the thread is started. 
        public void Revisador_de_Directorios()
        {
            StringBuilder sbMsg = new StringBuilder();

            if (this.MilisecsBorrado == -1)
            {
                sbMsg.AppendLine("**Revisador_de_Directorios (para eliminar)** : <Deshabilitado>");
                oLog.Escribe(TipoMsgLog.Info, sbMsg.ToString());
            }
            else
            {
                try
                {
                    sbMsg.AppendLine("**Revisador_de_Directorios (para eliminar)**");
                    sbMsg.AppendLine(string.Format("Carpeta         : {0}", this.Backup));
                    sbMsg.AppendLine(string.Format("DiasBackup      : {0}", this.DiasBackup));
                    sbMsg.AppendLine(string.Format("EliminaRO       : {0}", this.EliminaRO));
                    sbMsg.AppendLine(string.Format("MilisecsBorrado : {0}", this.MilisecsBorrado));
                    oLog.Escribe(TipoMsgLog.Info, sbMsg.ToString());

                    //EventWaitHandle ewhRevisaDir = new EventWaitHandle(false, EventResetMode.AutoReset, "ewhRevisaDir"); 

                    Console.WriteLine("ciclo de trabajo RevisadorDirectorio");
                    while (!_shouldStop)
                    {
                        lock (o4Lock)
                        {
                            revisaDirBackup();
                            if (this.MilisecsBorrado > 0)
                            {
                                //ewhRevisaDir.WaitOne(TimeSpan.FromMilliseconds(this.MilisecsBorrado), true);
                                Thread.Sleep(this.MilisecsBorrado);
                            }
                        }
                    }
                    oLog.Escribe(TipoMsgLog.Info, "Fin ciclo de trabajo RevisadorDirectorio");
                }
                catch (ThreadAbortException taex)
                {
                    oLog.Escribe(TipoMsgLog.Error, "Abortando Revisador_de_Directorios", taex.StackTrace);
                    sbMsg = null;
                }
            }
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        // Volatile is used as hint to the compiler that this data 
        // member will be accessed by multiple threads. 
        private volatile bool _shouldStop;

        private void revisaDirBackup()
        {
            String lMsg;
            DateTime ahora = System.DateTime.Now;
            //oLog.Escribe(TipoMsgLog.Info, string.Format("revisaDirBackup: {0}", this.Backup));

            try
            {
                DirectoryInfo di = new DirectoryInfo(this.Backup);
                FileInfo[] rgFiles = di.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                TimeSpan ts;

                foreach (FileInfo files in rgFiles) // foreach (FileInfo fi in rgFiles)
                {
                    try
                    {
                        ts = ahora.Subtract(files.LastWriteTime);
                        lMsg = "";
                        //Console.WriteLine(files.FullName + " / " + ts.Days + " / " + ts.TotalDays + "/" + files.LastWriteTime);
                        // si corresponde se borran los archivos mas antiguos

                        if (this.DiasBackup==0 || ts.TotalDays > this.DiasBackup)
                        {
                            // si se indica que tambien se eliminan los archivos R/O se quita el atributo antes
                            if (this.EliminaRO)
                            {
                                if (files.IsReadOnly)
                                {
                                    files.IsReadOnly = false;
                                    lMsg = " (se removio R/O)";
                                }
                            }
                            oLog.Escribe(TipoMsgLog.Info, string.Format("Se elimina backup: {0} / {1}", files.Name, files.LastWriteTime + lMsg));
                            files.Delete();
                        }
                    }
                    catch (UnauthorizedAccessException uae)
                    {
                        oLog.Escribe(TipoMsgLog.Error, string.Format("revisaDirBackup UnauthorizedAccessException Error: {0}", uae.ToString()), uae.StackTrace);
                    }
                    catch (IOException ioe)
                    {
                        oLog.Escribe(TipoMsgLog.Error, string.Format("revisaDirBackup IOException Error: {0}", ioe.ToString()), ioe.StackTrace);
                    }
                    catch (Exception e)
                    {
                        oLog.Escribe(TipoMsgLog.Error, string.Format("revisaDirBackup IOException Error: {0}", e.ToString()), e.StackTrace);
                    }
                }
            }
            catch(DirectoryNotFoundException dnfex)
            {
                oLog.Escribe(TipoMsgLog.Error, string.Format("revisaDirBackup DirectoryNotFoundException Error: {0}", dnfex.ToString()), dnfex.StackTrace);
            }
            //oLog.Escribe(TipoMsgLog.Info, string.Format("fin revisaDirBackup: {0}", this.Backup));
        }
    }
}
