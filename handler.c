#include "utils.c"
#include "file.c"
#define DEFAULT_BUFLEN 1024 * 12

void *onRequest(void *arguments) {
    
    struct arg_struct *args = arguments;
    SOCKET msg_sock = args->msg_sock;
    struct set Settings = args->Settings;
    
    char *p;
    char *q;
    char path[300] = "";
    char method[10] = "";
    char szBuff[DEFAULT_BUFLEN];
    int msg_len;
    while(1) {
        msg_len = recv(msg_sock, szBuff, sizeof(szBuff), 0);
        if (msg_len == 0) {
            break;
        }
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
        msg_len = writeData(path, msg_sock, Settings);
        if (msg_len == 0) {
            printf("Client closed connection\n");
            break;
        }
        if (msg_len == SOCKET_ERROR) {
            fprintf(stderr, "recv() failed with error %d\n", WSAGetLastError());
            WSACleanup();
            break;
        }
    }
    closesocket(msg_sock);
    free(arguments);
}
