#define BASE_PATH "C:/Users/Ethan/Desktop"
#define DEFAULT_RESLENGTH 1024 * 1024

void writeHeader(char code[], char msg[], char dest[]) {
    
    combineStrings(dest, "HTTP/1.1 ");
    combineStrings(dest, code);
    combineStrings(dest, " ");
    combineStrings(dest, msg);
    combineStrings(dest, "\r\n");
    
    combineStrings(dest, "Content-Type: text/html; charset=UTF-8\r\n\r\n");
    
    
}

int writeData(char requestPath[], SOCKET msg_sock) {
    char path[500] = "";
    int i, j=0;
    int length = 0;
    combineStrings(path, BASE_PATH);
    
    char *q = strtok(requestPath, "%20");
    while(q != NULL) {
        combineStrings(path, " ");
        combineStrings(path, q);
        q = strtok(NULL, "%20");
    }
    printf("%s\n", path);
    
    //first, get the length of the file
    FILE *file;
    file = fopen(path, "r");
    if (file == NULL) {
        char response[] = "HTTP/1.1 404 Not Found\r\nContent-Type: text/html; charset=UTF-8\r\n\r\n404 - File not found\r\n\r\n";
        return send(msg_sock, response, sizeof(response)-1, 0);
    }
    
    
    char res[DEFAULT_RESLENGTH] = "";
    writeHeader("200", "OK", res);
    char data[50];
    while(fgets(data, 50, file) != NULL) {
        combineStrings(res, data);
    }
    fclose(file);
    
    
    combineStrings(res, "\r\n\r\n");
    
    //now we need to cut out any extra data
    for (i=0; i<strlen(res); i++) {
        if (res[i] != '\0') {
            length++;
        }
    }
    char response[length];
    for (i=0; i<strlen(res); i++) {
        if (res[i] != '\0') {
            response[j] = res[i];
            j++;
        }
    }
    return send(msg_sock, response, sizeof(response)-1, 0);
}

