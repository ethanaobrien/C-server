
void *serverLoop(void *arguments) {
    
    struct arg_struct *args = arguments;
    struct set Settings = args->Settings;
    int port = args->port;
    
    int addr_len;
    struct sockaddr_in local, client_addr;

    SOCKET sock, msg_sock;
    WSADATA wsaData;

    if (WSAStartup(0x202, &wsaData) == SOCKET_ERROR) {
        fprintf(stderr, "WSAStartup failed with error %d\n", WSAGetLastError());
        WSACleanup();
        return (void *)-1;
    }
    // Fill in the address structure
    local.sin_family = AF_INET;
    local.sin_addr.s_addr = INADDR_ANY;
    local.sin_port = htons(port);

    sock = socket(AF_INET, SOCK_STREAM, 0); // tcp socket

    if (sock == INVALID_SOCKET) {
        fprintf(stderr, "socket() failed with error %d\n", WSAGetLastError());
        WSACleanup();
        return (void *)-1;
    }

    if (bind(sock, (struct sockaddr * ) & local, sizeof(local)) == SOCKET_ERROR) {
        fprintf(stderr, "bind() failed with error %d\n", WSAGetLastError());
        WSACleanup();
        return (void *)-1;
    }

    if (listen(sock, 5) == SOCKET_ERROR) {
        fprintf(stderr, "listen() failed with error %d\n", WSAGetLastError());
        WSACleanup();
        return (void *)-1;
    }

    printf("Listening on 127.0.0.1:%i\n", port);

    while (1) {
        addr_len = sizeof(client_addr);
        msg_sock = accept(sock, (struct sockaddr * ) &client_addr, &addr_len);
        if (msg_sock == INVALID_SOCKET) {
            fprintf(stderr, "accept() failed with error %d\n", WSAGetLastError());
            WSACleanup();
            return (void *)-1;
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
    free(arguments);
}

void makeServer(int port, struct set Settings) {
    struct arg_struct *args = malloc(sizeof(struct arg_struct));
    args->Settings = Settings;
    args->port = port;
    pthread_t thread_id;
    pthread_create(&thread_id, NULL, serverLoop, (void*)args);
}
