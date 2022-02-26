
int writeData(char requestPath[], SOCKET msg_sock, struct set Settings) {
    char path[500] = "";
    char reqpth[100] = "";
    strcpy(reqpth, requestPath);
    int i=0, j=0;
    int length = 0;
    combineStrings(path, Settings.directory);
    
    char *q = strtok(reqpth, "%20");
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
    DIR *folder;
    boolean isDirectory = FALSE;
    file = fopen(path, "rb");
    if (file == NULL) {
        folder = opendir(path);
        if (folder == NULL) {
            combineStrings(path, "/index.html");
            file = fopen(path, "rb");
            if (file == NULL) {
                char response[] = "HTTP/1.1 404 Not Found\r\nAccept-Ranges: bytes\r\nContent-Type: text/html; charset=UTF-8\r\n\r\n404 - File not found";
                return send(msg_sock, response, sizeof(response)-1, 0);
            }
        } else {
            isDirectory = TRUE;
        }
    }
    if (isDirectory) {
        FILE *template;
        template = fopen("./directory-listing-template.html", "rb");
        if (template == NULL) {
            closedir(folder);
            char response[] = "HTTP/1.1 404 Not Found\r\nAccept-Ranges: bytes\r\nContent-Type: text/html; charset=UTF-8\r\n\r\n404 - File not found";
            return send(msg_sock, response, sizeof(response)-1, 0);
        }
        
        char response[] = "HTTP/1.1 200 OK\r\nAccept-Ranges: bytes\r\nContent-Type: text/html; charset=UTF-8\r\ntransfer-encoding: chunked\r\n\r\n";
        int msg_len;
        if (! writeToSocket(msg_sock, response, file)) {
            return 1;
        }
        
        fseek(template, 0, SEEK_END);
        unsigned long len = (unsigned long)ftell(template)+1;
        fseek(template, 0, SEEK_SET);
        
        unsigned char res[len];
        fread(res, len, 1, template);
        fclose(template);
        if (! writeToSocket(msg_sock, res, file)) {
            return 1;
        }
        
        struct dirent *entry;
        if (! compareStrings(requestPath, "/")) {
            if (! writeToSocket(msg_sock, "<script>onHasParentDirectory();</script>", file)) {
                return 1;
            }
        }
        char addSter[26+strlen(requestPath)];
        sprintf(addSter, "%s%s%s", "<script>start('", requestPath, "')</script>");
        if (! writeToSocket(msg_sock, res, file)) {
            return 1;
        }
        combineStrings(res, addSter);
        while((entry = readdir(folder))) {
            struct _stat filestat;
            char paaath[200] = "";
            combineStrings(paaath, Settings.directory);
            combineStrings(paaath, "/");
            combineStrings(paaath, entry->d_name);
            _stat(paaath, &filestat);
            char addStr[1000] = "";
            char isDir[6] = "";
            if (S_ISDIR(filestat.st_mode)) {
                strcpy(isDir, "true ");
            } else {
                strcpy(isDir, "false");
            }
            sprintf(addStr, "%s%s%s%s%s%s%s", "<script>addRow('", entry->d_name, "', '", entry->d_name, "', ", isDir, ", '', '', '', '');</script>");
            combineStrings(res, addStr);
        }
        closedir(folder);
        length = 0;
        for (i=0; i<strlen(res); i++) {
            if (res[i] != '\0') {
                length++;
            }
        }
        char responsee[length];
        for (i=0; i<strlen(res); i++) {
            if (res[i] != '\0') {
                responsee[j] = res[i];
                j++;
            }
        }
        return send(msg_sock, responsee, sizeof(responsee)-1, 0);
    }
    fseek(file, 0, SEEK_END);
    unsigned long len = (unsigned long)ftell(file)+1;
    fseek(file, 0, SEEK_SET);
    
    char ext[100];
    char *p = strtok(path, ".");
    while(p != NULL) {
        strcpy(ext, p);
        p = strtok(NULL, ".");
    }
    while(ext[i]) {
        tolower(ext[i]);
        i++;
    }
    char contentType[100] = "";
    if (compareStrings(ext, "html")) {
        strcpy(contentType, "text/html; charset=utf-8");
    } else if (compareStrings(ext, "json")) {
        strcpy(contentType, "application/json;");
    } else if (compareStrings(ext, "js")) {
        strcpy(contentType, "application/javascript; charset=utf-8");
    } else {
        strcpy(contentType, "text/plain; charset=utf-8");
    }
    int cl = getIntTextLen(len);
    int h1 = 76+strLength(contentType)+cl;
    char header[h1];
    sprintf(header, "%s%s%s%i%s", "HTTP/1.1 200 OK\r\nAccept-Ranges: bytes\r\nContent-Type: ", contentType, "\r\nContent-Length: ", len-1, "\r\n\r\n");
    int msg_len;
    msg_len = send(msg_sock, header, sizeof(header)-1, 0);
    if (msg_len == 0) {
        printf("Client closed connection\n");
        closesocket(msg_sock);
        fclose(file);
        return 0;
    }
    unsigned char res[len];
    fread(res, len, 1, file);
    fclose(file);
    
    return send(msg_sock, res, sizeof(res)-1, 0);
}

