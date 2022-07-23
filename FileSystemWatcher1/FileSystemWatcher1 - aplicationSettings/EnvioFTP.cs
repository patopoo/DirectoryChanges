using System;
using System.Collections.Generic;
using System.Text;
using Comunes;
using System.Net.Sockets;
using System.IO;

namespace FileSystemWatcher1
{
    class EnvioFTP
    {
        public Logeador oLog { get; set; }

        public bool EnviaFTP(String IPdest, int iPort, String sFullPath, String sNombreArch, String sFTPUser, String sFTPPssw, ref String sMsgErr)
        {
            bool retu = true;
            try
            {
                if (File.Exists(sFullPath))
                {
                    ftp pp = new ftp("ftp://" + IPdest, sFTPUser, sFTPPssw);
                    pp.Binary = false;
                    retu = pp.upload(sFullPath, sNombreArch, ref sMsgErr);

                    #region FTPManual
                    /*
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + IPdest + "/" + sNombreArch);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential(sFTPUser, sFTPPssw);

                StreamReader sourceStream = new StreamReader(sFileName);
                byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                sourceStream.Close();
                request.ContentLength = fileContents.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

                response.Close();
                 */
                    #endregion
                }
            }
            catch(Exception ex)
            {
                oLog.Escribe(TipoMsgLog.Error, string.Format("Error EnviaFTP {0}", ex.ToString()), ex.StackTrace);
            }

            return retu;
        }
    }
}
