#include "utils.c"
#include "mime.c"
#include "file.c"

void *onRequest(void *arguments) {
    
    struct arg_struct *args = arguments;
    SOCKET msg_sock = args->msg_sock;
    struct set Settings = args->Settings;
    
    char *p;
    char *q;
    char w[300] = "";
    char path[1000] = "";
    char method[100] = "";
    char range[1000] = "";
    int cl;
    boolean hasRange = FALSE;
    boolean hasBody = FALSE;
    unsigned char szBuff[1024*12];
    unsigned char data[1024*12];
    int msg_len;
    int i=0;
    while(1) {
        cl = 0;
        memset(szBuff, '\0', sizeof(szBuff));
        memset(data, '\0', sizeof(data));
        msg_len = recv(msg_sock, szBuff, sizeof(szBuff), 0);
        if (msg_len == 0 || msg_len == -1) {
            break;
        }
        pop(szBuff, "\r\n\r\n", data);
        p = strtok(szBuff, "\r\n");
        int j = 0;
        while(p != NULL) {
            if (j == 0) {
                strcpy(w, p);
            }
            i=0;
            while(p[i]) {
                if (p[i] == ':') break;
                p[i] = tolower(p[i]);
                i++;
            }
            if (startsWith("range", p)) {
                strcpy(range, p);
                hasRange = TRUE;
            }
            if (startsWith("content-length", p)) {
                char opsa[200] = "";
                hasBody = TRUE;
                strcpy(opsa, p);
                char *y;
                y = strtok(opsa, ":");
                y = strtok(NULL, ":");
                strcpy(opsa, y);
                while (opsa[0] == ' ') {
                    int op = 0;
                    while(opsa[op]) {
                        opsa[op] = opsa[op+1];
                        op++;
                    }
                }
                cl = atoi(opsa);
            }
            j++;
            p = strtok(NULL, "\r\n");
        }
        q = strtok(w, " ");
        i=0;
        while(q != NULL) {
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
        if (startsWith(method, "HEAD") || startsWith(method, "GET")) {
            msg_len = writeData(path, msg_sock, hasRange, range, Settings, method);
        } else if (startsWith(method, "PUT")) {
            msg_len = putData(path, msg_sock, Settings, data, cl);
        } else if (startsWith(method, "DELETE")) {
            msg_len = deleteData(path, msg_sock, Settings);
        } else {
            char header[] = "HTTP/1.1 405 Method Not Allowed\r\nAllow: GET, HEAD, PUT, DELETE\r\nAccept-Ranges: bytes\r\nContent-Length: 0\r\n\r\n";
            if (! writeToSocket(msg_sock, header, NULL)) {
                break;
            }
            //char response[] = "405 - Method not allowed";
            //msg_len = send(msg_sock, response, sizeof(response)-1, 0);
        }
        if (msg_len == 0 || msg_len == -1) {
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
