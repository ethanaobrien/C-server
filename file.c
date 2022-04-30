
int render404(SOCKET msg_sock, char method[]) {
    char header[] = "HTTP/1.1 404 Not Found\r\nAccept-Ranges: bytes\r\nContent-Length: 20\r\n\r\n";
    char response[] = "404 - File not found";
    if (! writeToSocket(msg_sock, header, NULL)) {
        return 0;
    }
    if (startsWith(method, "HEAD")) {
        return 1;
    }
    return send(msg_sock, response, sizeof(response)-1, 0);
}


int writeData(char requestPath[], SOCKET msg_sock, boolean hasRange, char rangeHeader[], struct set Settings, char method[]) {
    char path[300] = "";
    char decodedPath[300] = "";
    int i=0, j=0;
    int length = 0;
    combineStrings(path, Settings.directory);
    urldecode(decodedPath, requestPath);
    combineStrings(path, decodedPath);
    //printf("%s\n", path);
    
    //first, get the length of the file
    FILE *file;
    DIR *folder;
    boolean isDirectory = TRUE;
    boolean isIndex = FALSE;
    file = fopen(path, "rb");
    if (file == NULL) {
        if (Settings.index) {
            char indexPath[300] = "";
            combineStrings(indexPath, Settings.directory);
            combineStrings(indexPath, requestPath);
            combineStrings(indexPath, "index.html");
            file = fopen(indexPath, "rb");
            if (! endsWith(path, '/')) {
                char header[strlen(requestPath)+89];
                sprintf(header, "%s%s%s", "HTTP/1.1 301 Moved Permanently\r\nAccept-Ranges: bytes\r\nContent-Length: 0\r\nLocation: ", requestPath, "/\r\n\r\n");
                return send(msg_sock, header, sizeof(header)-1, 0);
            }
            if (file != NULL) {
                isIndex = TRUE;
                isDirectory = FALSE;
            }
        }
        if (isDirectory != FALSE && Settings.directoryListing) {
            folder = opendir(path);
            if (folder != NULL) {
                isDirectory = TRUE;
            } else {
                return render404(msg_sock, method);
            }
        } else if (isDirectory != FALSE && isIndex != TRUE) {
            return render404(msg_sock, method);
        }
    } else {
        isDirectory = FALSE;
    }
    if (isDirectory) {
        if (! endsWith(path, '/')) {
            char header[strlen(requestPath)+89];
            sprintf(header, "%s%s%s", "HTTP/1.1 301 Moved Permanently\r\nAccept-Ranges: bytes\r\nContent-Length: 0\r\nLocation: ", requestPath, "/\r\n\r\n");
            return send(msg_sock, header, sizeof(header)-1, 0);
        }
        //render directory
        unsigned long len = Settings.directoryListingTemplateSize;
        
        if (! compareStrings(requestPath, "/")) {
            len+=24;
        }
        len+=52;
        struct dirent *entry;
        while((entry = readdir(folder))) {
            len+=strlen(entry->d_name);
            len+=strlen(entry->d_name);
            len+=39;
        }
        unsigned char res[len];
        memset(res, '\0', sizeof(res));
        combineStrings2(res, Settings.directoryListingTemplate, Settings.directoryListingTemplateSize-2);
        combineStrings(res, "\n<script>");
        combineStrings(res, "\nstart(window.location.pathname);");
        if (! compareStrings(requestPath, "/")) {
            combineStrings(res, "\nonHasParentDirectory();");
        }
        rewinddir(folder);
        while((entry = readdir(folder))) {
            char addStr[40+strlen(entry->d_name)+strlen(entry->d_name)];
            char isDir[6] = "";
            if (isItDirectory(Settings, entry->d_name, requestPath)) {
                strcpy(isDir, "true ");
            } else {
                strcpy(isDir, "false");
            }
            sprintf(addStr, "%s%s%s%s%s%s%s", "\naddRow(\"", entry->d_name, "\", \"", entry->d_name, "\", ", isDir, ", '', '', '', '');");
            combineStrings(res, addStr);
        }
        combineStrings(res, "\n</script>");
        closedir(folder);
        int h1 = 101+getIntTextLen(len);
        char header[h1];
        sprintf(header, "%s%i%s", "HTTP/1.1 200 OK\r\nAccept-Ranges: bytes\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: ", len, "\r\n\r\n\r\n");
        if (! writeToSocket(msg_sock, header, NULL)) {
            return 0;
        }
        if (startsWith(method, "HEAD")) {
            return 1;
        }
        return send(msg_sock, res, sizeof(res)-1, 0);
    }
    
    //render file
    fseek(file, 0, SEEK_END);
    unsigned long len = (unsigned long)ftell(file)+1;
    fseek(file, 0, SEEK_SET);
    
    char ext[100] = "";
    char *p = strtok(path, ".");
    while(p != NULL) {
        strcpy(ext, p);
        p = strtok(NULL, ".");
    }
    i=0;
    while(ext[i]) {
        tolower(ext[i]);
        i++;
    }
    char contentType[100];
    char der[3000];
    memset(der, '\0', sizeof(der));
    getMime(ext, contentType);
    boolean hasContentType = (strlen(contentType) != 0);
    if (hasContentType) {
        sprintf(der, "%s%s%s", "Content-Type: ", contentType, "\r\n");
    }
    char *r;
    int fileOffset=0, fileEndOffset=len-1, code=200;
    char hea[3000];
    memset(hea, '\0', sizeof(hea));
    int cl = len-1;
    if (hasRange && strchr(rangeHeader, '=') != NULL) {
        r = strtok(rangeHeader, "=");
        r = strtok(NULL, "=");
        if (endsWith(r, '-') || strchr(r, '-') == NULL) {
            if (strchr(r, '-') != NULL) {
                r = strtok(r, "-");
            }
            fileOffset = atoi(r);
            cl = len-fileOffset-1;
            sprintf(hea, "%s%i%c%i%c%i%s", "content-range: bytes ", fileOffset, '-', len-2, '/', len-1, "\r\n");
            code = (fileOffset == 0) ? 200 : 206;
        } else {
            r = strtok(r, "-");
            fileOffset = atoi(r);
            printf("%s\n", r);
            r = strtok(NULL, "-");
            fileEndOffset = atoi(r);
            printf("%s\n", r);
            cl = fileEndOffset-fileOffset+1;
            sprintf(hea, "%s%i%c%i%c%i%s", "content-range: bytes: ", fileOffset, '-', fileEndOffset, '/', len-1, "\r\n");
            code = 206;
        }
        fseek(file, fileOffset, SEEK_SET);
    }
    int h1 = 81+getIntTextLen(cl)+getIntTextLen(code)+strLength(hea)+strLength(der);
    char header[h1];
    memset(header, '\0', sizeof(header));
    sprintf(header, "%s%i%s%i%s", "HTTP/1.1 ", code, " OK\r\nAccept-Ranges: bytes\r\nContent-Length: ", cl, "\r\nConnection: keep-alive\r\n");
    if (hasContentType) {
        strcat(header, der);
    }
    if (hasRange) {
        strcat(header, hea);
    }
    strcat(header, "\r\n");
    int msg_len;
    msg_len = send(msg_sock, header, sizeof(header)-1, 0);
    if (msg_len == 0) {
        //printf("Client closed connection\n");
        closesocket(msg_sock);
        fclose(file);
        return 0;
    }
    if (startsWith(method, "HEAD")) {
        return 1;
    }
    int readChunkSize = 1024;
    if (cl > readChunkSize || hasRange) {
        unsigned long readLen = 0;
        while (readLen < cl) {
            int a = readChunkSize;
            if (cl-readLen < readChunkSize) {
                a = cl-readLen;
            }
            if (readLen == cl) {
                break;
            }
            readLen+=a;
            unsigned char res[a+1];
            fread(res, a, 1, file);
            msg_len = send(msg_sock, res, sizeof(res)-1, 0);
            if (msg_len == 0) {
                closesocket(msg_sock);
                fclose(file);
                return 0;
            }
        }
        fclose(file);
        return 1;
    } else {
        unsigned char res[len];
        fread(res, len, 1, file);
        fclose(file);
        return send(msg_sock, res, sizeof(res)-1, 0);
    }
}

