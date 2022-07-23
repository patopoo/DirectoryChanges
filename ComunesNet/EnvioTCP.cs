using System;
using System.Collections.Generic;
using System.Text;
using Comunes;
using System.Net.Sockets;

namespace FileSystemWatcher1
{
    class EnvioTCP : Logeador
    {
        public String IPDest { get; set; }
        public int PortDest { get; set; }
        public String PathXML { get; set; }
        TcpClient client;
        NetworkStream stream;

        public EnvioTCP(String strIP, int iPort, String sPathXML)
            : base(sPathXML)
        {
            this.IPDest = strIP;
            this.PortDest = iPort;
            this.PathXML = sPathXML;
            base.Escribe(Comunes.TipoMsgLog.Info, "Instancia EnvioTCP");
        }

        public int Conecta()
        {
            base.Escribe(Comunes.TipoMsgLog.Info, string.Format("Conectando EnvioTCP IP:{0} Port:{1}", this.IPDest, this.PortDest));
            client = new TcpClient(this.IPDest, this.PortDest);
            base.Escribe(Comunes.TipoMsgLog.Info, "**Conectado");
            stream = client.GetStream();
            return 0;
        }

        public int Cierra()
        {
            stream.Close();
            client.Close();
            base.Escribe(TipoMsgLog.Info, "**Cerrado");
            return 0;
        }

        public int Comunica(string sInput, ref string sOutput)
        {
            int iRetu=0;

            Byte[] data = System.Text.Encoding.ASCII.GetBytes(sInput);
            stream.Write(data, 0, data.Length);
            base.Escribe(TipoMsgLog.Info, string.Format("Enviado : {0}", sInput.Substring(0,199)));
            return iRetu;
        }
    }
}
