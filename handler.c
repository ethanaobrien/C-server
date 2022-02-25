#include "utils.c"
#include "file.c"
#define DEFAULT_BUFLEN 1024

int onRequest(SOCKET msg_sock) {
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
    if (method == "GET") {
        printf("safdggdhhf");
    }
    
    msg_len = writeData(path, msg_sock);
    
    if (msg_len == 0) {
      printf("Client closed connection\n");
      closesocket(msg_sock);
      return -1;
    }

    if (msg_len == SOCKET_ERROR) {
      fprintf(stderr, "recv() failed with error %d\n", WSAGetLastError());
      WSACleanup();
      return -1;
    }

    if (msg_len == 0) {
      printf("Client closed connection\n");
      closesocket(msg_sock);
      return -1;
    }
    
    return 1;
}