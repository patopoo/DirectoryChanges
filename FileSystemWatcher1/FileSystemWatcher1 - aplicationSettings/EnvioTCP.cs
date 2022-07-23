using System;
using System.Collections.Generic;
using System.Text;
using Comunes;
using System.Net.Sockets;

namespace FileSystemWatcher1
{
    class EnvioTCP
    {
        public String IPDest { get; set; }
        public int PortDest { get; set; }
        public String PathXML { get; set; }
        public int Timeout { get; set; }
        TcpClient client;
        NetworkStream stream;
        public Logeador oLog { get; set; }

        public EnvioTCP(String strIP, int iPort, String sPathXML, Logeador oLog)
        {
            this.IPDest = strIP;
            this.PortDest = iPort;
            this.PathXML = sPathXML;
            this.Timeout = 4000;

            this.oLog = oLog;
            oLog.Escribe(TipoMsgLog.Info, "Instancia EnvioTCP");
        }

        public int Conecta()
        {
            oLog.Escribe(TipoMsgLog.Info, string.Format("Conectando EnvioTCP IP:{0} Port:{1}", this.IPDest, this.PortDest));
            client = new TcpClient(this.IPDest, this.PortDest);
            oLog.Escribe(TipoMsgLog.Info, "**Conectado");
            stream = client.GetStream();
            return 0;
        }

        public int Cierra()
        {
            stream.Close();
            client.Close();
            oLog.Escribe(TipoMsgLog.Info, "**Cerrado");
            return 0;
        }

        public uint Comunica(string sInput, ref string sOutput)
        {
            int iLeidos = 0, iTamanoMsg, iFaltantes;
            StringBuilder sbResp = new StringBuilder();
            Byte[] bResp = new Byte[client.ReceiveBufferSize];
            Utiles uu = new Utiles();

            try
            {
                Byte[] data = CalculaTamaño(sInput);
                stream.WriteTimeout = this.Timeout;
                stream.Write(data, 0, data.Length);
                oLog.Escribe(TipoMsgLog.Info, string.Format("Enviado: {0}", uu.Left(sInput, 200)));
                stream.ReadTimeout = this.Timeout;
                if (stream.CanRead)
                {
                    oLog.Escribe(TipoMsgLog.Info, string.Format("CanRead: {0} buffer:{1}", stream.CanRead, bResp.Length));
                    do
                    {
                        iLeidos = stream.Read(bResp, 0, 4);
                        iTamanoMsg = MakeDWord(bResp);
                        iFaltantes = iTamanoMsg;
                        oLog.Escribe(TipoMsgLog.Info, string.Format("iLeidos: {0} {1}", iLeidos, iTamanoMsg));
                        while (iFaltantes > 0)
                        {
                            iLeidos = stream.Read(bResp, 0, bResp.Length);
                            sbResp.Append(Encoding.ASCII.GetString(bResp, 0, iTamanoMsg));
                            iFaltantes -= iLeidos;
                        }                        
                        //oLog.Escribe(TipoMsgLog.Info, string.Format("Data Recibida: {0}", uu.Left(Encoding.ASCII.GetString(bResp, 4, iTamanoMsg),200)));
                    } while (stream.DataAvailable);
                    sOutput = sbResp.ToString();
                }
                else
                {
                    oLog.Escribe(TipoMsgLog.Error, "No se puede leer desde Network stream");
                }
                oLog.Escribe(TipoMsgLog.Info, string.Format("Recibido: {0}", uu.Left(sOutput, 200)));
            }
            catch (ObjectDisposedException odex)
            {
                oLog.Escribe(TipoMsgLog.Error, string.Format("Error ObjectDisposedException {0}", odex.ToString()), odex.StackTrace);
            }
            catch (SocketException soex)
            {
                oLog.Escribe(TipoMsgLog.Error, string.Format("Error ObjectDisposedException {0}", soex.ToString()), soex.StackTrace);
            }
            catch (Exception ex)
            {
                oLog.Escribe(TipoMsgLog.Error, string.Format("Error ObjectDisposedException {0}", ex.ToString()), ex.StackTrace);
            }
            return 0;
        }

        private byte[] CalculaTamaño(string sCade)
        {
            byte[] bbytes = new byte[4];
            GetDWord(sCade.Length, ref bbytes);
            List<byte> aa = new List<byte>();
            aa.Add(bbytes[0]);
            aa.Add(bbytes[1]);
            aa.Add(bbytes[2]);
            aa.Add(bbytes[3]);
            aa.AddRange(System.Text.Encoding.ASCII.GetBytes(sCade));
            return aa.ToArray();
        }

        private void GetDWord(int iSize, ref byte[] bbytes)
        {
            long dwLen = iSize + 4;

            bbytes[0] = 0x0;
            bbytes[1] = 0x0;
            bbytes[2] = 0x0;
            bbytes[3] = 0x0;

            bbytes[0] = (byte)(dwLen & 255);
            dwLen = dwLen / 256;
            bbytes[1] = (byte)(dwLen & 255);
            dwLen = dwLen / 256;
            bbytes[2] = (byte)(dwLen & 255);
            dwLen = dwLen / 256;
            bbytes[3] = (byte)(dwLen & 255);
        }

        private int MakeDWord(byte[] bbytes)
        {
            int dwLen = 0;
            int ival = 0, k;

            for (k = 0; k < 4; k++)
            {
                ival = (byte)(bbytes[k] & 255);
                dwLen += (ival * (int)Math.Pow(256,k));
            }
            return dwLen;
        }
    }
}
