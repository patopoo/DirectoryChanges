#ifndef MWST247FileWatcherService_H
#define MWST247FileWatcherService_H

#include <windows.h> 
#include <string.h>
#include <stdio.h>
#include <sys\timeb.h>
#include <time.h>

#define MAX_LENGTH_REG 1024
char m_TipoServicio[MAX_LENGTH_REG]; // por defecto "MWST247"
char strLlave[MAX_LENGTH_REG];

#define TITULO_APLICACION "Servidor de Servicios"
#define NOM_ARCHIVO_LOG "MWST247FileWatcherService.err"
char m_NOM_ARCHIVO_LOG[MAX_LENGTH_REG];

char m_KEY_SERVIDOR_ST247[MAX_LENGTH_REG];
#define VAL_PATH_MENSAJES "PathMensajes"
#define VAL_IMPRIME_LOG "ImprimeLog"
#define VAL_COMANDO "Comando"

#define MAX_LARGO_TRAMA 1024
#define MIN_LARGO_TRAMA 1
#define OBLIGATORIO 1					  // Flag que indica que el log es obligatorio

char pathmensajes[255];
char imprimeLog[2];
char comando[255];

// PARA LA CREACION DEL PROCESO
STARTUPINFO si;
PROCESS_INFORMATION pi;

char mensajeA[5001];
char szFilePath[_MAX_PATH];

BOOL LevantaServicio();
void WINAPI ServiceMain(DWORD dwArgc, LPTSTR* lpszArgv);
BOOL ProcesaParametros(int argc, char* argv[]);
void PrintFile(unsigned int obligatorio, char * parchivo, char * pmensaje, char *direccionIP);
BOOL CtrlHandler(DWORD fdwCtrlType);
void WINAPI Handler(DWORD dwOpcode);
//int getVarName(char *lpVarName, char *sReturnValue, char *sDefaultValue);

// ++ servicio.h ++
#define MAX_LENGTH_REG 1024
//#define NOMBRE_SERVICIO      "MWST247Server"
//#define DESCRIPCION_SERVICIO "Servidor Transaccional " // ST247"
char m_NOMBRE_SERVICIO[MAX_LENGTH_REG]; //      "MWST247Server"
char m_DESCRIPCION_SERVICIO[MAX_LENGTH_REG]; // "Servidor Transaccional " // ST247"

BOOL EstaInstaladoServicio();
BOOL InstalarServicio();
BOOL DesinstalaServicio();
SC_HANDLE hSCM;
SC_HANDLE hService;
BOOL GetStatusDependServices();
// -- servicio.h --

SERVICE_TABLE_ENTRY st[] = {
	{m_NOMBRE_SERVICIO, ServiceMain},
	{NULL, NULL}
};

void EnviarTramaFin();
void LeeTipoServicio();
//void DefineAmbiente();

void LeeTipoServicio()
{
	strcpy(m_NOM_ARCHIVO_LOG, NOM_ARCHIVO_LOG);
	strcpy(m_KEY_SERVIDOR_ST247, "SOFTWARE\\NEXUS\\FileWatcherService");
	strcpy(m_NOMBRE_SERVICIO, "FileWatcherService");
	strcpy(m_DESCRIPCION_SERVICIO, "Servicio File Watcher");
}

/*
int getVarName(char *lpVarName, char *sReturnValue, char *sDefaultValue)
{
DWORD Status;
char		  InfoRegisterIni[MAX_LENGTH_REG];
unsigned long InfoRegisterIniLen=MAX_LENGTH_REG;
memset(InfoRegisterIni,0,InfoRegisterIniLen);
InfoRegisterIniLen=MAX_LENGTH_REG;

  Status = GetEnvironmentVariable(lpVarName,InfoRegisterIni,InfoRegisterIniLen);
  if (Status == 0)
  {
		strcpy(InfoRegisterIni, sDefaultValue);
		
		  }
		  strncpy(sReturnValue, InfoRegisterIni, Status);
		  
			return Status;
			}
*/
#endif //MWST247FileWatcherService_H