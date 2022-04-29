#include <winsock2.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <dirent.h>
#include <sys/stat.h>
#include <pthread.h>
#include <sys/types.h>


struct set {
    boolean directoryListing;
    boolean index;
    char directory[300];
    char directoryListingTemplate[18679];
    unsigned long directoryListingTemplateSize;
};

struct arg_struct {
    SOCKET msg_sock;
    struct set Settings;
};


#include "handler.c"

#define DEFAULT_PORT 8887

int main(int argc, char *argv[]) {
    
    struct set Settings;
    
    Settings.directoryListing = TRUE;
    Settings.index = FALSE;
    
    FILE *template;
    template = fopen("./directory-listing-template.html", "rb");
    if (template == NULL) {
        printf("Failed to read directory listing template");
        exit(1);
    }
    
    Settings.directoryListingTemplateSize = 18679;
    fread(Settings.directoryListingTemplate, Settings.directoryListingTemplateSize, 1, template);
    fclose(template);
    
    
    if (argc > 1) {
        strcpy(Settings.directory, argv[1]);
    } else {
        printf("path to serve not inputted");
        exit(1);
        return -1;
    }
    
    
    int addr_len;
    struct sockaddr_in local, client_addr;

    SOCKET sock, msg_sock;
    WSADATA wsaData;

    if (WSAStartup(0x202, &wsaData) == SOCKET_ERROR) {
        fprintf(stderr, "WSAStartup failed with error %d\n", WSAGetLastError());
        WSACleanup();
        return -1;
    }
    // Fill in the address structure
    local.sin_family = AF_INET;
    local.sin_addr.s_addr = INADDR_ANY;
    local.sin_port = htons(DEFAULT_PORT);

    sock = socket(AF_INET, SOCK_STREAM, 0); // tcp socket

    if (sock == INVALID_SOCKET) {
        fprintf(stderr, "socket() failed with error %d\n", WSAGetLastError());
        WSACleanup();
        return -1;
    }

    if (bind(sock, (struct sockaddr * ) & local, sizeof(local)) == SOCKET_ERROR) {
        fprintf(stderr, "bind() failed with error %d\n", WSAGetLastError());
        WSACleanup();
        return -1;
    }

    if (listen(sock, 5) == SOCKET_ERROR) {
        fprintf(stderr, "listen() failed with error %d\n", WSAGetLastError());
        WSACleanup();
        return -1;
    }

    printf("Listening on 127.0.0.1:%i\n", DEFAULT_PORT);

    while (1) {
        addr_len = sizeof(client_addr);
        msg_sock = accept(sock, (struct sockaddr * ) &client_addr, &addr_len);
        if (msg_sock == INVALID_SOCKET) {
            fprintf(stderr, "accept() failed with error %d\n", WSAGetLastError());
            WSACleanup();
            return -1;
        }

        if (msg_sock == -1) {
            continue;
        }

        //printf("Connection from %s:%d\n", inet_ntoa(client_addr.sin_addr), htons(client_addr.sin_port));
        
        struct arg_struct *args = malloc(sizeof(struct arg_struct));
        args->msg_sock = msg_sock;
        args->Settings = Settings;
        
        pthread_t thread_id;
        pthread_create(&thread_id, NULL, onRequest, (void*)args);
        
    }
    WSACleanup();
}

