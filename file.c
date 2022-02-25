#define BASE_PATH "C:/Users/697510/emulatorjs"

int writeData(char requestPath[], SOCKET msg_sock) {
    char path[500] = "";
    int i, j=0;
    int length = 0;
    combineStrings(path, BASE_PATH);
    
    char *q = strtok(requestPath, "%20");
    combineStrings(path, q);
    while(q != NULL) {
        q = strtok(NULL, "%20");
        if (q != NULL) {
            combineStrings(path, " ");
            combineStrings(path, q);
        }
    }
    printf("%s\n", path);
    
    //first, get the length of the file
    FILE *file;
    file = fopen(path, "rb");
    if (file == NULL) {
        char response[] = "HTTP/1.1 404 Not Found\r\nAccept-Ranges: bytes\r\nContent-Type: text/html; charset=UTF-8\r\n\r\n404 - File not found\r\n\r\n";
        return send(msg_sock, response, sizeof(response)-1, 0);
    }
    fseek(file, 0, SEEK_END);
    unsigned long len = (unsigned long)ftell(file);
    fseek(file, 0, SEEK_SET);
    
    //todo: set content length header
    char *mime = getMime(path);
    char header[] = "HTTP/1.1 200 OK\r\nAccept-Ranges: bytes\r\nContent-Type: text/html; charset=UTF-8\r\n\r\n";
    int msg_len;
    msg_len = send(msg_sock, header, sizeof(header)-1, 0);
    if (msg_len == 0) {
        printf("Client closed connection\n");
        closesocket(msg_sock);
        return 0;
    }
    unsigned char res[len];
    fread(res, len, 1, file);
    fclose(file);
    
    return send(msg_sock, res, sizeof(res)-1, 0);
}

