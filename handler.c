#include "utils.c"
#include "file.c"
#define DEFAULT_BUFLEN 1024 * 12

void *onRequest(void *arguments) {
    
    struct arg_struct *args = arguments;
    SOCKET msg_sock = args->msg_sock;
    struct set Settings = args->Settings;
    
    char *p;
    char *q;
    char w[300] = "";
    char path[300] = "";
    char method[10] = "";
    char range[300] = "";
    boolean hasRange = FALSE;
    char szBuff[DEFAULT_BUFLEN];
    int msg_len;
    while(1) {
        memset(szBuff, '\0', sizeof(szBuff));
        msg_len = recv(msg_sock, szBuff, sizeof(szBuff), 0);
        if (msg_len == 0) {
            break;
        }
        p = strtok(szBuff, "\r\n");
        int j = 0;
        while(p != NULL) {
            if (j == 0) {
                strcpy(w, p);
            }
            if (startsWith("Range", p)) {
                strcpy(range, p);
                hasRange = TRUE;
            }
            j++;
            p = strtok(NULL, "\r\n");
        }
        q = strtok(w, " ");
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
        
        msg_len = writeData(path, msg_sock, hasRange, range, Settings);
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
