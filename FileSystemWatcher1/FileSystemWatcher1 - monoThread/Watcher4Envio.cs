using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Security.Permissions;
using System.Configuration;
using System.Net;
using System.Threading;
using Comunes;
using Microsoft.Win32;

namespace FileSystemWatcher1
{
    public class Watcher4Envio
    {
        public Logeador oLog { get; set; }
        String sPathInput, sFiltro, sBackup;
        String sIPDest, sFTPUser, sFTPPssw, sTipoOperacion;
        String sLogFullPath, sPathEnviosErroneos;
        int iPortDest, iMilisecsToWait;
        String sAccionBackup;
        const string C_FTP = "FTP";
        const string C_TCP = "TCP";
        Object o4Lock = new Object();

        public Watcher4Envio()
        {
            _shouldStop = false;
        }

        // Volatile is used as hint to the compiler that this data 
        // member will be accessed by multiple threads. 
        private volatile bool _shouldStop;

        public void RequestStop()
        {
            _shouldStop = true;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void mdlWatcher()
        {
            try
            {
                StringBuilder sbMsg = new StringBuilder();
                // Create a new FileSystemWatcher and set its properties.
                //FileSystemWatcher vfileWatcher = new FileSystemWatcher();

                sbMsg.AppendLine(string.Format("**Iniciando {0}", AppDomain.CurrentDomain.FriendlyName));
                sbMsg.AppendLine(string.Format("Directorio      : {0}", this.PathInput));
                sbMsg.AppendLine(string.Format("Filtro          : {0}", this.Filtro));
                sbMsg.AppendLine(string.Format("Backup          : {0}", this.Backup));
                sbMsg.AppendLine(string.Format("Tipo Operacion  : {0}", this.TipoOperacion));
                sbMsg.AppendLine(string.Format("IPDest          : {0}", this.IPDest));
                sbMsg.AppendLine(string.Format("PortDest        : {0}", this.PortDest));
                sbMsg.AppendLine(string.Format("FTPUser         : {0}", this.FTPUser));
                sbMsg.AppendLine(string.Format("FTPPssw         : {0}", this.FTPPssw));
                sbMsg.AppendLine(string.Format("LogFullPath     : {0}", this.LogFullPath));
                sbMsg.AppendLine(string.Format("MilisecsToWait  : {0}", this.MilisecsToWait));
                oLog.Escribe(TipoMsgLog.Info, sbMsg.ToString());

                //throw new Exception("Test Exception");

                /* Watch for changes in LastAccess and LastWrite times, and
                   the renaming of files or directories. */
                /* [20/12/2013] PPV. Se cambia por un RevisadorDirectorio
                vfileWatcher.Path = this.PathInput;
                vfileWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Only watch text files.
                vfileWatcher.Filter = this.Filtro;
                vfileWatcher.InternalBufferSize = vfileWatcher.InternalBufferSize * 2;
                vfileWatcher.IncludeSubdirectories = false;

                // Add event handlers.
                //vfileWatcher.Changed += new FileSystemEventHandler(OnChanged);
                vfileWatcher.Created += new FileSystemEventHandler(OnCreated);
                vfileWatcher.Deleted += new FileSystemEventHandler(OnDeleted);
                vfileWatcher.Renamed += new RenamedEventHandler(OnRenamed);

                // Begin watching. // desabilitado
                vfileWatcher.EnableRaisingEvents = false;
                */

                    EventWaitHandle ewhRevisaDir = new EventWaitHandle(false, EventResetMode.AutoReset, "ewhRevisaDir");

                    // Wait for the user to quit the program.
                    Console.WriteLine("ciclo de trabajo Watcher4FTP");
                    while (!_shouldStop)
                    {
                        lock (o4Lock)
                        {
                            revisaDirEntrada();
                        if (iMilisecsToWait != 0)
                        {
                            ewhRevisaDir.WaitOne(TimeSpan.FromMilliseconds(iMilisecsToWait), true);
                        }
                        }
                    }
                    oLog.Escribe(TipoMsgLog.Info, "Fin ciclo de trabajo Watcher4FTP");
                

                //vfileWatcher.Dispose();
            }
            catch (Exception ex)
            {
                oLog.Escribe(TipoMsgLog.Error, string.Format("mdlWatcher Exception Error: {0}", ex.ToString(), ex.StackTrace));
            }
        }

        private void revisaDirEntrada()
        {
            //oLog.Escribe(TipoMsgLog.Info, string.Format("revisaDirEntrada: {0}", this.PathInput));
            try
            {
                DirectoryInfo di = new DirectoryInfo(this.PathInput);
                FileInfo[] rgFiles = di.GetFiles(this.Filtro, SearchOption.TopDirectoryOnly);

                foreach (FileInfo files in rgFiles)
                {
                    try
                    {
                        if (File.Exists(files.FullName))
                        {
                            switch(this.TipoOperacion)
                            {
                                case C_FTP:
                                    HaceEnvioFTP(files.FullName, files.Name);
                                    break;
                                case C_TCP:
                                    HaceEnvioTCP(files.FullName, files.Name);
                                    break;
                            }
                        }
                    }
                    catch (UnauthorizedAccessException uae)
                    {
                        oLog.Escribe(TipoMsgLog.Error, string.Format("revisaDirEntrada UnauthorizedAccessException Error: {0}", uae.ToString()), uae.StackTrace);
                    }
                    catch (IOException ioe)
                    {
                        oLog.Escribe(TipoMsgLog.Error, string.Format("revisaDirEntrada IOException Error: {0}", ioe.ToString()), ioe.StackTrace);
                    }
                    catch (Exception e)
                    {
                        oLog.Escribe(TipoMsgLog.Error, string.Format("revisaDirEntrada IOException Error: {0}", e.ToString()), e.StackTrace);
                    }
                }
            }
            catch (DirectoryNotFoundException dnfex)
            {
                oLog.Escribe(TipoMsgLog.Error, string.Format("revisaDirEntrada DirectoryNotFoundException Error: {0}", dnfex.ToString()), dnfex.StackTrace);
            }
            //oLog.Escribe(TipoMsgLog.Info, string.Format("fin revisaDirEntrada: {0}", this.PathInput));
        }

        /* [20/12/2013] PPV. Se cambia por un RevisadorDirectorio
        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine(string.Format("File: {0} {1}", e.FullPath, e.ChangeType));
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            bool fCopiado = false;
            oLog.Escribe(TipoMsgLog.Info, string.Format("File: {0} {1}", e.FullPath, e.ChangeType));

            if (File.Exists(e.FullPath))
            {
                try
                {
                    FileStream fs = File.OpenWrite(e.FullPath);
                    fCopiado = true;
                    fs.Close();
                }
                catch
                {
                    fCopiado = false;
                }
                finally
                {
                    if(fCopiado)
                    {
                        if (this.TipoOperacion.Equals(C_FTP))
                            HaceEnvioFTP_(e);
                        else
                            HaceEnvioTCP_(e);
                    }
                }
            }
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            oLog.Escribe(TipoMsgLog.Info, string.Format("File: {0} {1}", e.FullPath, e.ChangeType));
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            String sMsg;
            sMsg = String.Format("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
            oLog.Escribe(TipoMsgLog.Info, sMsg);
        }
        */

        #region envioTCP
        // mascara para el antiguo modelo
        private void HaceEnvioTCP_(FileSystemEventArgs e)
        {
            HaceEnvioTCP(e.FullPath, e.Name);
        }

        private void HaceEnvioTCP(String sFullPath, String sName)
        {
            string sLine = "", sFileTemp, sMsg = "";
            string sLineOut = "";
            uint iRetu;
            Utiles uu = new Utiles();

            try
            {
                sFileTemp = System.IO.Path.Combine(this.EnviosErroneos, sName);
                // se mueve a la carpeta temporal para trabajar en el
                if (MueveArch(sFullPath, sFileTemp, ref sMsg))
                {
                    oLog.Escribe(TipoMsgLog.Info, string.Format("MOVE: Archivo {0} movido a EnviosErroneos: {1}", sName, this.EnviosErroneos));
                    EnvioTCP oEnvioTCP = new EnvioTCP(this.IPDest, this.PortDest, oLog);

                    oEnvioTCP.Conecta();

                    System.IO.StreamReader sr = new System.IO.StreamReader(sFileTemp, System.Text.Encoding.Default, true);
                    while ((sLine = sr.ReadLine()) != null)
                    {
                        iRetu = (uint)oEnvioTCP.Comunica(sLine, ref sLineOut);
                        if (iRetu != 0)
                        {
                            throw new Exception(string.Format("Error {0} al enviar TCP", iRetu));
                        }
                    }
                    sr.Close();
                    oEnvioTCP.Cierra();

                    switch (this.AccionBackup)
                    {
                        case "R": // Respaldar
                            // al finalizar se borra de la carpeta backup
                            if (MueveArch(sFileTemp, System.IO.Path.Combine(this.Backup, sName), ref sMsg))
                            {
                                oLog.Escribe(TipoMsgLog.Info, string.Format("MOVE: Archivo {0} movido a Backup: {1}", sFileTemp, System.IO.Path.Combine(this.Backup, sName)));
                            }
                            else
                            {
                                oLog.Escribe(TipoMsgLog.Error, string.Format("Error {0} al mover a Backup: {1} {2}", sMsg, sFullPath, System.IO.Path.Combine(this.EnviosErroneos, sName)));
                            }
                            break;
                        case "B": // Borrar
                            // al finalizar se mueve a la carpeta backup
                            if (BorraArch(sFileTemp, ref sMsg))
                            {
                                oLog.Escribe(TipoMsgLog.Info, string.Format("Borra: Archivo {0} Borrado", sFileTemp));
                            }
                            else
                            {
                                oLog.Escribe(TipoMsgLog.Error, string.Format("Error {0} al borrar archivo {1}", sMsg, sFullPath));
                            }
                            break;
                    }
                }
                else
                {
                    oLog.Escribe(TipoMsgLog.Error, string.Format("Error {0} al mover a EnviosErroneos: {1} {2}", sMsg, sFullPath, System.IO.Path.Combine(this.EnviosErroneos, sName)));
                }
            }
            catch (Exception ex)
            {
                oLog.Escribe(TipoMsgLog.Error, string.Format("Error en HaceEnvioTCP: Arch:{0} Err:{1}", sFullPath, ex.ToString()), ex.StackTrace);
            }
        }
        #endregion

        #region envioFTP
        // mascara para el antiguo modelo
        private void HaceEnvioFTP_(FileSystemEventArgs e)
        {
            HaceEnvioFTP(e.FullPath, e.Name);
        }

        private void HaceEnvioFTP(String sFullPath, String sName)
        {
            String sMsg = "";
            EnvioFTP oEnvioFTP = new EnvioFTP();
            oEnvioFTP.oLog = oLog;

            if (oEnvioFTP.EnviaFTP(this.IPDest, this.PortDest, sFullPath, sName, this.FTPUser, this.FTPPssw, ref sMsg) == true)
            {
                oLog.Escribe(TipoMsgLog.Info, string.Format("FTP: Archivo {0} enviado a: {1}", sName, this.IPDest));
                switch (this.AccionBackup)
                {
                    case "R": // Respaldar
                        if (MueveArch(sFullPath, System.IO.Path.Combine(this.Backup, sName), ref sMsg))
                        {
                            oLog.Escribe(TipoMsgLog.Info, string.Format("MOVE: Archivo {0} movido a: {1}", sName, this.Backup));
                        }
                        else
                        {
                            oLog.Escribe(TipoMsgLog.Error, string.Format("Error {0} al mover: {1} {2}", sMsg, sFullPath, System.IO.Path.Combine(this.Backup, sName)));
                        }
                        break;
                    case "B": // Borrar
                        if (BorraArch(sFullPath, ref sMsg))
                        {
                            oLog.Escribe(TipoMsgLog.Info, string.Format("Borrar: Archivo {0} Borraado", sName));
                        }
                        else
                        {
                            oLog.Escribe(TipoMsgLog.Error, string.Format("Error {0} al Borrar: {1}", sMsg, sFullPath));
                        }
                        break;
                }
            }
            else
            {
                oLog.Escribe(TipoMsgLog.Error, string.Format("Error el Enviar FTP archivo: {0} / {1}", sName, sMsg));
                if (MueveArch(sFullPath, System.IO.Path.Combine(this.EnviosErroneos, sName), ref sMsg))
                {
                    oLog.Escribe(TipoMsgLog.Info, string.Format("MOVE: Archivo {0} movido a EnviosErroneos: {1}", sName, this.EnviosErroneos));
                }
                else
                {
                    oLog.Escribe(TipoMsgLog.Error, string.Format("Error {0}  al mover a EnviosErroneos: {1} {2}" + sMsg, sFullPath, System.IO.Path.Combine(this.EnviosErroneos, sName)));
                }
            }
        }
        #endregion

        private bool MueveArch(String sfileOri, String sfileDest, ref String sMsg)
        {
            bool retu = true;
            sMsg = "";

            try
            {
                if (File.Exists(sfileDest))
                {
                    sMsg = "Error al mover archivo, destino ya existe/";
                }
                else
                {
                    if (File.Exists(sfileOri))
                    {
                        File.Move(sfileOri, sfileDest);
                    }
                }
            }
            catch (Exception e)
            {
                sMsg = e.ToString();
                retu = true;
            }
            return retu;
        }

        private bool BorraArch(String sfileOri, ref String sMsg)
        {
            bool retu = true;
            sMsg = "";

            try
            {
                if (File.Exists(sfileOri))
                {
                    File.Delete(sfileOri);
                }
            }
            catch (Exception e)
            {
                sMsg = e.ToString();
                retu = false;
            }
            return retu;
        }

        #region PropiedadesPublicas
        public string PathInput
        {
            get
            {
                return this.sPathInput;
            }
            set
            {
                this.sPathInput = value;
            }
        }

        public string Filtro
        {
            get
            {
                return this.sFiltro;
            }
            set
            {
                this.sFiltro = value;
            }
        }

        public string Backup
        {
            get
            {
                return this.sBackup;
            }
            set
            {
                this.sBackup = value;
            }
        }

        public string IPDest
        {
            get
            {
                return this.sIPDest;
            }
            set
            {
                this.sIPDest = value;
            }
        }

        public int PortDest
        {
            get
            {
                return this.iPortDest;
            }
            set
            {
                this.iPortDest = value;
            }
        }

        public int MilisecsToWait
        {
            get
            {
                return this.iMilisecsToWait;
            }
            set
            {
                this.iMilisecsToWait = value;
            }
        }

        public string FTPUser
        {
            get
            {
                return this.sFTPUser;
            }
            set
            {
                this.sFTPUser = value;
            }
        }

        public string FTPPssw
        {
            get
            {
                return this.sFTPPssw;
            }
            set
            {
                this.sFTPPssw = value;
            }
        }

        public string LogFullPath
        {
            get
            {
                return this.sLogFullPath;
            }
            set
            {
                this.sLogFullPath = value;
            }
        }

        public string EnviosErroneos
        {
            get
            {
                return this.sPathEnviosErroneos;
            }
            set
            {
                this.sPathEnviosErroneos = value;
            }
        }

        public string TipoOperacion
        {
            get
            {
                return this.sTipoOperacion;
            }
            set
            {
                this.sTipoOperacion = value;
            }
        }

        public string AccionBackup
        {
            get
            {
                return this.sAccionBackup;
            }
            set
            {
                this.sAccionBackup = value;
            }
        }
        #endregion
    }
}
