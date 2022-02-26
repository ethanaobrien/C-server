#include "utils.c"
#include "file.c"
#define DEFAULT_BUFLEN 1024

void *onRequest(void *arguments) {
    
    struct arg_struct *args = arguments;
    SOCKET msg_sock = args->msg_sock;
    struct set Settings = args->Settings;
    
    
    char *p;
    char *q;
    char path[100];
    char method[10];
    char szBuff[DEFAULT_BUFLEN];
    int msg_len = recv(msg_sock, szBuff, sizeof(szBuff), 0);
    
    p = strtok(szBuff, "\n");
    q = strtok(p, " ");
    int i = 0;
    while(q != NULL || i < 2) {
        if (i == 0) {
            strcpy(method, q);
        } else if (i == 1) {
            strcpy(path, q);
        }
        i++;
        q = strtok(NULL, " ");
    }
    q = strtok(path, "?");
    strcpy(path, q);
    int length = 0, j=0;
    for (int i=0; i<strlen(path); i++) {
        if (path[i] != '\0') {
            length++;
        }
    }
    char paaaath[length];
    for (i=0; i<strlen(path); i++) {
        if (path[i] != '\0') {
            paaaath[j] = path[i];
            j++;
        }
    }
    
    
    msg_len = writeData(paaaath, msg_sock, Settings);
    
    if (msg_len == 0) {
        printf("Client closed connection\n");
    }

    if (msg_len == SOCKET_ERROR) {
      fprintf(stderr, "recv() failed with error %d\n", WSAGetLastError());
      WSACleanup();
    }
    closesocket(msg_sock);
    free(arguments);
}