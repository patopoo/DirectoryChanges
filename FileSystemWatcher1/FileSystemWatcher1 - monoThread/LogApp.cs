using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;
using log4net;
using log4net.Config;

namespace Comunes
{
    public enum TipoMsgLog
    {
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4
    }

    public struct struct_Mensaje
    {
        public TipoMsgLog tipoMsg;
        public string sMsg;
        public string sStackTrace;
    }

    class LogApp
    {
        public ILog _log_ = log4net.LogManager.GetLogger("LogFileAppender");
        //public ILog _log_ = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LogApp(/*String sPathXML*/)
        {
            log4net.Config.BasicConfigurator.Configure();
            //XmlConfigurator.Configure(new FileInfo(sPathXML));
        }

        public void Escribe(TipoMsgLog tipoMsg, String sMsg)
        {
            //Console.WriteLine(sMsg);
            ErrorLog(tipoMsg, sMsg);
        }

        public void ErrorLog(TipoMsgLog tipoMsg, string cadena)
        {
            switch (tipoMsg)
            {
                case TipoMsgLog.Error:
                    if (_log_.IsErrorEnabled)
                        _log_.Error(cadena);
                    break;
                case TipoMsgLog.Info:
                    if (_log_.IsInfoEnabled)
                        _log_.Info(cadena);
                    break;
                case TipoMsgLog.Warning:
                    if (_log_.IsWarnEnabled)
                        _log_.Warn(cadena);
                    break;
                case TipoMsgLog.Debug:
                    if (_log_.IsDebugEnabled)
                        _log_.Debug(cadena);
                    break;
                default:
                    if (_log_.IsErrorEnabled)
                        _log_.Error(cadena);
                    break;
            }
        }
    }
}
