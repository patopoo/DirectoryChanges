// MWST247FileWatcherService.cpp : Defines the entry point for the console application.
//
#include "stdafx.h"

#include <stdlib.h>
#include <stdio.h> 

#include <string.h>
#include <wtypes.h>  
#include <tchar.h>  
#include <windows.h>
#include "MWST247FileWatcherService.h"
#include "MWST247Event.h"

void ErrorDescription(DWORD p_dwError);

BOOL EsServicio = TRUE;
int BajarServicio = 0;
SERVICE_STATUS_HANDLE hServiceStatus=0;
SERVICE_STATUS Status;
#define KEY_WOW64_64KEY 0x0100
#define KEY_WOW64_32KEY 0x0200
void ErrorExit(LPTSTR lpszFunction, DWORD dwErr) ;
unsigned short NumeroPuerto;

int main(int argc, char* argv[])
{                
	char szFilePath[_MAX_PATH];
	
	/* verificar que no exista otra instancia del programa */
	if (FindWindow("ConsoleWindowClass",TITULO_APLICACION)!=NULL)
		return 1;
	
	LeeTipoServicio();
	SetConsoleTitle(TITULO_APLICACION);
	
	// si no hay parametros
	if (argc <= 1) 
	{
		// inicializamos servicio
		LevantaServicio();
		return 2;
	}
	
	if (_stricmp(argv[1], "-c") == 0) 
	{
		// levantamos como consola
		EsServicio = FALSE;
		ServiceMain(0, NULL);
		return 3;
	} 
	else if (_stricmp(argv[1], "-i") == 0) 
	{
		if (EstaInstaladoServicio()) 
		{
			printf("Servicio %s ya esta instalado\n", m_NOMBRE_SERVICIO);
		} 
		else 
		{
			// instalamos el servicio
			if (InstalarServicio()) 
			{
				printf("Se instalo servicio %s\n", m_NOMBRE_SERVICIO);
			} 
			else 
			{
				printf("No se pudo instalar el servicio %s . Error %d\n", m_NOMBRE_SERVICIO, GetLastError());
			}
		}
		return 4;
	} 
	else if (_stricmp(argv[1], "-u") == 0) 
	{
		// desinstalamos el servicio
		if (!EstaInstaladoServicio()) 
		{
			printf("El servicio %s no esta instalado\n", m_NOMBRE_SERVICIO);
		} 
		else 
		{
			if (DesinstalaServicio()) 
			{
				// Obtiene el path del executable
				GetModuleFileName(NULL, szFilePath, sizeof(szFilePath));
				printf("Se desinstalo el servicio %s. (Usted debe eliminar el archivo %s.)\n",
					m_NOMBRE_SERVICIO, szFilePath);
			} 
			else 
			{
				printf("No se puede desinstalar el servicio %s. Error %d\n", m_NOMBRE_SERVICIO, GetLastError());
			}
		}
		return 5;
	}
	
	LevantaServicio();
	return 0;
}

BOOL LevantaServicio()
{
	BOOL b;
	
	// coloca los valores iniciales de estado del servicio
	Status.dwServiceType =SERVICE_WIN32_OWN_PROCESS;
	Status.dwCurrentState = SERVICE_STOPPED;
	Status.dwControlsAccepted = SERVICE_ACCEPT_STOP;
	Status.dwWin32ExitCode = 0;
	Status.dwServiceSpecificExitCode = 0;
	Status.dwCheckPoint = 0;
	Status.dwWaitHint = 0;
	
	b = StartServiceCtrlDispatcher(&st[0]);
	return b;
}

void WINAPI ServiceMain(DWORD dwArgc,     // numero de argumentos
                        LPTSTR* lpszArgv) // argumentos que pasa el proceso que levanta el servicio
{                
	int i;
	HKEY pkeyreg=0;
	DWORD bufferSize;
	DWORD dwType;
	DWORD dwError =0;
	
	memset(comando, 0x0, sizeof(comando));
	memset(pathmensajes, 0x0, sizeof(pathmensajes));
	GetModuleFileName(NULL, szFilePath, sizeof(szFilePath));
	for (i=strlen(szFilePath)-1; i>0; i--)
	{
		if (szFilePath[i]=='\\')
		{
			strncpy(pathmensajes,szFilePath,i+1);
			break;
		}
	}
	
	if (EsServicio)
	{
		Status.dwCurrentState = SERVICE_START_PENDING;
		SetServiceStatus(hServiceStatus,&Status);
		hServiceStatus = RegisterServiceCtrlHandler(m_NOMBRE_SERVICIO,Handler);
		if (hServiceStatus == 0) 
		{
			PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, "error en funcion RegisterServiceCtrlHandler", NULL);
			return;
		}
	}
	
	
#ifdef _MWST247_X64_
    dwError=RegOpenKeyEx(HKEY_LOCAL_MACHINE, m_KEY_SERVIDOR_ST247, 0, KEY_WOW64_32KEY | KEY_READ, &pkeyreg) ;
#else
    dwError=RegOpenKeyEx(HKEY_LOCAL_MACHINE, m_KEY_SERVIDOR_ST247, 0, KEY_READ, &pkeyreg) ;
#endif
	
	if ( dwError != ERROR_SUCCESS )
	{
		ErrorExit("FUNCION",dwError);
		memset(mensajeA, 0x0, sizeof(mensajeA));
		sprintf(mensajeA,"Error al abrir la clave [HKLM\\%s] del registry [%d]", m_KEY_SERVIDOR_ST247, dwError);
		PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
		return ;
	}
	
	// Lee del registry el path de archivos de mensajeA
	bufferSize = sizeof(pathmensajes);
	if (RegQueryValueEx(pkeyreg,VAL_PATH_MENSAJES, NULL, &dwType, (LPBYTE)pathmensajes, &bufferSize) != ERROR_SUCCESS )
	{
		memset(mensajeA, 0x0, sizeof(mensajeA));
		sprintf(mensajeA,"Error al leer valor [%s] de la clave [HKLM\\%s] del registry", VAL_PATH_MENSAJES, m_KEY_SERVIDOR_ST247);
		PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
		//RegCloseKey(pkeyreg);
		//return ;
	}
	if (pathmensajes[strlen(pathmensajes)-1] != '\\')
		strcat(pathmensajes,"\\");

	// Lee del registry el flag que indica si debe o no imprimir los logs que no son obligatorios
	bufferSize = sizeof(imprimeLog);
	if (RegQueryValueEx(pkeyreg,VAL_IMPRIME_LOG, NULL, &dwType, (LPBYTE)imprimeLog,&bufferSize) != ERROR_SUCCESS )
	{
		strcpy(imprimeLog,"N");
	}    
	
	// Lee del registry el comando a ejecutar
	bufferSize = sizeof(comando);
	if (RegQueryValueEx(pkeyreg, VAL_COMANDO, NULL, &dwType, (LPBYTE)comando,&bufferSize) != ERROR_SUCCESS )
	{
		
		memset(mensajeA, 0x0, sizeof(mensajeA));
		sprintf(mensajeA,"Error al leer valor [%s] de la clave [HKLM\\%s] del registry",
			VAL_COMANDO, m_KEY_SERVIDOR_ST247);
		PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
		//RegCloseKey(pkeyreg);
		//return ;
	}
	
	RegCloseKey(pkeyreg);
	pkeyreg=0;
	
	memset(mensajeA, 0x0, sizeof(mensajeA));
	sprintf(mensajeA,"***** Inicio del programa *****\ncomando a EXEC [%s]\n",comando);
	PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
	
	
	if (EsServicio)
	{
		Status.dwCurrentState=SERVICE_RUNNING;
		SetServiceStatus(hServiceStatus,&Status);
	}
	
	/* +++++++++++++++++++++++++++++++++++++++++++++++++++++
	Código a ejecutarse repetitivamente
	+++++++++++++++++++++++++++++++++++++++++++++++++++++ */
	//PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG,"ANTES DE GetStatusDependServices", NULL);
	//	GetStatusDependServices();
	//PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG,"DESPUES DE GetStatusDependServices", NULL);
	
	
	for (; ; ) 
	{
		memset(mensajeA, 0x0, sizeof(mensajeA));
		sprintf(mensajeA,"***** Inicio del programa *****\ncomando a EXEC [%s]\n",comando);
		PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
		
		ZeroMemory( &si, sizeof(si) );
		
		si.cb = sizeof(si);
		ZeroMemory( &pi, sizeof(pi) );
		
		// Start the child process. 
		if( !CreateProcess( NULL, // No module name (use command line). 
			comando,           // Command line. 
			NULL,             // Process handle not inheritable. 
			NULL,             // Thread handle not inheritable. 
			TRUE,            // Set handle inheritance to FALSE. 
			CREATE_NEW_CONSOLE,                // No creation flags. 
			NULL,             // Use parent's environment block. 
			NULL,             // Use parent's starting directory. 
			&si,              // Pointer to STARTUPINFO structure.
			&pi )             // Pointer to PROCESS_INFORMATION structure.
			) 
		{
			printf( "CreateProcess failed." );
			return;
		}
		
		// Wait until child process exits.
		WaitForSingleObject( pi.hProcess, INFINITE );
		
		// Close process and thread handles. 
		CloseHandle( pi.hProcess );
		CloseHandle( pi.hThread );
		
		// aqui va codigo que verifica si se debe bajar el servidor
		if (BajarServicio==1)
			break;
	}
	
	// Close process and thread handles. 
	CloseHandle( pi.hProcess );
	CloseHandle( pi.hThread );
	if (EsServicio)
	{
		// Le comunicamos al "service manager" que paramos
		Status.dwCurrentState = SERVICE_STOPPED;
		SetServiceStatus(hServiceStatus,&Status);
	}
	return ;
	if (pkeyreg!=0)
		RegCloseKey(pkeyreg);
	PrintFile (OBLIGATORIO,m_NOM_ARCHIVO_LOG,"Proceso finalizó con problemas", NULL);
	if (EsServicio)
	{
		// Le comunicamos al "service manager" que paramos
		Status.dwCurrentState = SERVICE_STOPPED;
		SetServiceStatus(hServiceStatus,&Status);
	}
	return ;
} // Fin de la rutina main

// Esta funcion recibe mensajes del "service control manager"
void WINAPI Handler(DWORD dwOpcode)
{
	switch (dwOpcode) 
	{
    case SERVICE_CONTROL_STOP: // 1
		Status.dwCurrentState = SERVICE_STOP_PENDING;
		SetServiceStatus(hServiceStatus,&Status);
		EnviarTramaFin();
		break;
    case SERVICE_CONTROL_PAUSE: // 2
		break;
    case SERVICE_CONTROL_CONTINUE: // 3
		break;
    case SERVICE_CONTROL_INTERROGATE: // 4
		break;
    case SERVICE_CONTROL_SHUTDOWN: // 5
		Status.dwCurrentState = SERVICE_STOP_PENDING;
		SetServiceStatus(hServiceStatus,&Status);
		EnviarTramaFin();
		break;
    default:
		break;
	}
}


/* -----------------------------------------------------------------------------------
PrintFile: RUTINA PARA IMPRIMIR EN UN ARCHIVO
----------------------------------------------------------------------------------- */
void PrintFile(unsigned int obligatorio, char *pArchivoA, char *pMensajeA,char *pDireccionIPA)
{
	FILE *stream;
	char archivoA[256];
	struct _timeb timebuffer;
	char *timeline=NULL;
	char fechaHora[40];
	
	// capturar fecha y hora
	_ftime( &timebuffer );
	timeline = ctime( & ( timebuffer.time ) );
	sprintf(fechaHora,"[%.4s %.19s.%03hu]",&timeline[20],timeline,timebuffer.millitm);
	if (obligatorio || ( strcmp(imprimeLog, "S") == 0 ) ) 
	{
		sprintf(archivoA,"%s%s", pathmensajes, pArchivoA);
		stream = fopen( archivoA, "a" );
		if (stream==NULL)
		{
			printf("No se pudo abrir archivo %s\n",archivoA);
			return;
		}
		if (pDireccionIPA!=NULL)
			fprintf( stream, "%s [%s] %s\n", fechaHora, pDireccionIPA, pMensajeA);
		else
			fprintf( stream, "%s%s\n", fechaHora, pMensajeA );
		fclose( stream );
		if (pDireccionIPA!=NULL)
			printf("%s [%s] %s\n", fechaHora, pDireccionIPA, pMensajeA );
		else
			printf("%s%s\n", fechaHora, pMensajeA );
	}
	return;
}


void EnviarTramaFin()
{
	
	BajarServicio=1;
	
	if( !TerminateProcess(pi.hProcess,0 ))
		printf( "TerminateProcess failed." );
	
	else
	{
		CloseHandle( pi.hProcess );
		CloseHandle( pi.hThread );
		
	}
	
	return;
}

void ErrorExit(LPTSTR lpszFunction, DWORD dwErr) 
{ 
    LPVOID lpMsgBuf;
	
    FormatMessage(
        FORMAT_MESSAGE_ALLOCATE_BUFFER | 
        FORMAT_MESSAGE_FROM_SYSTEM,
        NULL,
        dwErr,
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPTSTR) &lpMsgBuf,
        0, NULL );
	
	PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG,(char*)lpMsgBuf, NULL);
	
    LocalFree(lpMsgBuf);
}

BOOL GetStatusDependServices()
{
	
    ENUM_SERVICE_STATUS service;
    
    DWORD dwBytesNeeded = 0;
    DWORD dwServicesReturned = 0;
    DWORD dwResumedHandle = 0;
    DWORD dwServiceType = SERVICE_WIN32 | SERVICE_DRIVER;
	BOOL retVal ;
	unsigned iIndex ;
	
    SC_HANDLE hHandle = OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
	
    if (NULL == hHandle) {
        ErrorDescription(GetLastError());
        return -1;
    }
    else {
		memset(mensajeA, 0x0, sizeof(mensajeA));
		sprintf(mensajeA,"***** Open SCM sucessfully *****");
		PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
    }
	
	
    // Query services
    retVal = EnumServicesStatus(hHandle, dwServiceType, SERVICE_STATE_ALL, 
        &service, sizeof(ENUM_SERVICE_STATUS), &dwBytesNeeded, &dwServicesReturned,
        &dwResumedHandle);
	
    if (!retVal) {
        // Need big buffer
        if (ERROR_MORE_DATA == GetLastError()) {
            // Set the buffer
            DWORD dwBytes = sizeof(ENUM_SERVICE_STATUS) + dwBytesNeeded;
            //ENUM_SERVICE_STATUS* pServices = NULL;
            //pServices = new ENUM_SERVICE_STATUS [dwBytes];
			
			LPENUM_SERVICE_STATUS   pServices = NULL;
			pServices = (LPENUM_SERVICE_STATUS) malloc(dwBytes);
			
            // Now query again for services
            EnumServicesStatus(hHandle, SERVICE_WIN32 , SERVICE_STATE_ALL, 
                pServices, dwBytes, &dwBytesNeeded, &dwServicesReturned, &dwResumedHandle);
			
            // now traverse each service to get information
            for (iIndex = 0; iIndex < dwServicesReturned; iIndex++) {
                
				memset(mensajeA, 0x0, sizeof(mensajeA));
				
				switch((pServices + iIndex)->ServiceStatus.dwCurrentState)
				{
				case SERVICE_STOPPED:
					{
						sprintf(mensajeA,"**Display Name >%s<,Service Name >%s<, Status >SERVICE_STOPPED< ",
							(pServices + iIndex)->lpDisplayName ,
							(pServices + iIndex)->lpServiceName);
						PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
						break;
					}
				case SERVICE_START_PENDING:
					{
						sprintf(mensajeA,"**Display Name >%s<,Service Name >%s<, Status >SERVICE_START_PENDING< ",
							(pServices + iIndex)->lpDisplayName ,
							(pServices + iIndex)->lpServiceName);
						PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
						break;
					}
				case SERVICE_STOP_PENDING:
					{
						sprintf(mensajeA,"**Display Name >%s<,Service Name >%s<, Status >SERVICE_STOP_PENDING< ",
							(pServices + iIndex)->lpDisplayName ,
							(pServices + iIndex)->lpServiceName);
						PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
						break;
					}
				case SERVICE_RUNNING:
					{
						sprintf(mensajeA,"**Display Name >%s<,Service Name >%s<, Status >SERVICE_RUNNING< ",
							(pServices + iIndex)->lpDisplayName ,
							(pServices + iIndex)->lpServiceName);
						PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
						break;
					}
				case SERVICE_CONTINUE_PENDING:
					{
						sprintf(mensajeA,"**Display Name >%s<,Service Name >%s<, Status >SERVICE_CONTINUE_PENDING< ",
							(pServices + iIndex)->lpDisplayName ,
							(pServices + iIndex)->lpServiceName);
						PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
						break;
					}
				case SERVICE_PAUSE_PENDING:
					{
						sprintf(mensajeA,"**Display Name >%s<,Service Name >%s<, Status >SERVICE_PAUSE_PENDING< ",
							(pServices + iIndex)->lpDisplayName ,
							(pServices + iIndex)->lpServiceName);
						PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
						break;
					}
				case SERVICE_PAUSED:
					{
						sprintf(mensajeA,"**Display Name >%s<,Service Name >%s<, Status >SERVICE_PAUSED< ",
							(pServices + iIndex)->lpDisplayName ,
							(pServices + iIndex)->lpServiceName);
						PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
						break;
					}
				}
				
				
				
            }
			
            //delete [] pServices;
            pServices = NULL;
			
        }
        // there is any other reason
        else {
            ErrorDescription(GetLastError());
        }
    }
	
    if (!CloseServiceHandle(hHandle))
	{
        ErrorDescription(GetLastError());
    }
    else
	{
		memset(mensajeA, 0x0, sizeof(mensajeA));
		sprintf(mensajeA,"***** Close SCM sucessfully *****");
		PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
		
    }
    return 0;
}

void ErrorDescription(DWORD p_dwError)
{    
    HLOCAL hLocal = NULL;
    
	
	FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ALLOCATE_BUFFER,
		NULL, p_dwError, MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US),(LPTSTR)&hLocal, 
		0, NULL);
	
	memset(mensajeA, 0x0, sizeof(mensajeA));
	sprintf(mensajeA,"Error --->> %s",(LPCTSTR)LocalLock(hLocal));
	PrintFile (OBLIGATORIO, m_NOM_ARCHIVO_LOG, mensajeA, NULL);
    
}

BOOL EstaInstaladoServicio()
{
	BOOL bResult = FALSE;
	
	// abrimos el "Service Control Manager"
	hSCM = OpenSCManager(NULL, // maquina local
		NULL, // base de datos activa
		SC_MANAGER_ALL_ACCESS); // acceso total
	if (hSCM) 
	{
		// tratamos de abrir el servicio
		hService = OpenService(hSCM,
			m_NOMBRE_SERVICIO,
			SERVICE_QUERY_CONFIG);
		if (hService) 
		{
			bResult = TRUE;
			CloseServiceHandle(hService);
		}
		CloseServiceHandle(hSCM);
	}
    
	return bResult;
}

BOOL InstalarServicio()
{
	SC_HANDLE hSCM;
	SC_HANDLE hService;
	char szFilePath[255];
	char szKey[256];
	HKEY hKey;
	DWORD dwData;
	SERVICE_DESCRIPTION sdBuf;
	
	// abrimos el "Service Control Manager"
	hSCM = OpenSCManager(NULL, // maquina local
		NULL, // base de datos activa
		SC_MANAGER_ALL_ACCESS); // acceso total
	if (!hSCM) return FALSE;
	
	// obtenemos el nombre del ejecutable
	GetModuleFileName(NULL, szFilePath, sizeof(szFilePath));
	
	// creamos el servicio
	hService = CreateService(hSCM,
		m_NOMBRE_SERVICIO,
		m_DESCRIPCION_SERVICIO,
		SERVICE_ALL_ACCESS,
		SERVICE_WIN32_OWN_PROCESS | SERVICE_INTERACTIVE_PROCESS,
		SERVICE_AUTO_START,        // inicio manual
		SERVICE_ERROR_NORMAL,
		szFilePath,
		NULL,
		NULL,
		NULL,
		NULL,
		NULL);
	if (!hService) 
	{
		CloseServiceHandle(hSCM);
		return FALSE;
	}
	
	
	sdBuf.lpDescription = (char*)malloc(501);
	memset(sdBuf.lpDescription,0x0,501);
	
	strcpy(sdBuf.lpDescription, "Ejecuta como servicio la apliacion FileSystemWatcher");
	
	if( !ChangeServiceConfig2(
        hService,                 // handle to service
        SERVICE_CONFIG_DESCRIPTION, // change: description
        &sdBuf) )                   // value: new description
        printf("No se puede cambiar la descripción del servicio MWST247Service");
	
	free(sdBuf.lpDescription);
	
	
	// coloca entradas en el registry para soportar acceso al log de eventos
	hKey = NULL;
	strcpy(szKey, "SYSTEM\\CurrentControlSet\\Services\\EventLog\\Application\\");
	strcat(szKey, m_NOMBRE_SERVICIO);
	if (RegCreateKey(HKEY_LOCAL_MACHINE, szKey, &hKey) != ERROR_SUCCESS) 
	{
		CloseServiceHandle(hService);
		CloseServiceHandle(hSCM);
		return FALSE;
	}
	
	RegSetValueEx(hKey,
		"EventMessageFile",
		0,
		REG_EXPAND_SZ, 
		(CONST BYTE*)szFilePath,
		strlen(szFilePath) + 1);     
	
	dwData = EVENTLOG_ERROR_TYPE | EVENTLOG_WARNING_TYPE | EVENTLOG_INFORMATION_TYPE;
	RegSetValueEx(hKey,
		"TypesSupported",
		0,
		REG_DWORD,
		(CONST BYTE*)&dwData,
		sizeof(DWORD));
	RegCloseKey(hKey);
	
	// cerramos los handles
	CloseServiceHandle(hService);
	CloseServiceHandle(hSCM);
	return TRUE;
}

BOOL DesinstalaServicio()
{
	SC_HANDLE hSCM;
	BOOL bResult = FALSE;
	SC_HANDLE hService;
	
	// abrimos el "Service Control Manager"
	hSCM = OpenSCManager(NULL, // maquina local
		NULL, // base de datos activa
		SC_MANAGER_ALL_ACCESS); // acceso total
	if (!hSCM) return FALSE;
	
	hService = OpenService(hSCM,
		m_NOMBRE_SERVICIO,
		DELETE);
	if (hService) 
	{
		if (DeleteService(hService)) 
		{
			bResult = TRUE;
		} 
		CloseServiceHandle(hService);
	}
    
	CloseServiceHandle(hSCM);
	return bResult;
}
