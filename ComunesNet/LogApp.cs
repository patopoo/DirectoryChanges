using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using log4net.Config;

namespace Comunes
{
    class LogApp
    {
        public ILog _log_ = log4net.LogManager.GetLogger("Log");

        public LogApp(String sPathXML)
        {
            //log4net.Config.BasicConfigurator.Configure();
            XmlConfigurator.Configure(new FileInfo(sPathXML));
        }

        public void Escribe(String sMsg)
        {
            Console.WriteLine(sMsg);
            ErrorLog(sMsg);
        }

        public void ErrorLog(string cadena)
        {
            if (_log_.IsErrorEnabled)
            {
                _log_.Error(cadena);
            }

        }
    }
}
