
int render404(SOCKET msg_sock) {
    char response[] = "HTTP/1.1 404 Not Found\r\nAccept-Ranges: bytes\r\nContent-Type: text/html; charset=UTF-8\r\nContent-Length: 20\r\n\r\n404 - File not found";
    return send(msg_sock, response, sizeof(response)-1, 0);
}


int writeData(char requestPath[], SOCKET msg_sock, struct set Settings) {
    char path[300] = "";
    char decodedPath[300] = "";
    int i=0, j=0;
    int length = 0;
    combineStrings(path, Settings.directory);
    urldecode(decodedPath, requestPath);
    combineStrings(path, decodedPath);
    printf("%s\n", path);
    
    //first, get the length of the file
    FILE *file;
    DIR *folder;
    boolean isDirectory = TRUE;
    boolean isIndex = FALSE;
    file = fopen(path, "rb");
    if (file == NULL) {
        if (! endsWith(path, '/')) {
            char header[strlen(requestPath)+87];
            sprintf(header, "%s%s%s", "HTTP/1.1 301 Moved Permanently\r\nAccept-Ranges: bytes\r\nContent-Length: 0\r\nLocation: ", requestPath, "/\r\n\r\n");
            return send(msg_sock, header, sizeof(header)-1, 0);
        }
        if (Settings.index) {
            char indexPath[300] = "";
            combineStrings(indexPath, Settings.directory);
            combineStrings(indexPath, requestPath);
            combineStrings(indexPath, "index.html");
            file = fopen(indexPath, "rb");
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
                return render404(msg_sock);
            }
        } else if (isDirectory != FALSE) {
            return render404(msg_sock);
        }
    } else {
        isDirectory = FALSE;
    }
    if (isDirectory) {
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
            sprintf(addStr, "%s%s%s%s%s%s%s", "\naddRow('", entry->d_name, "', '", entry->d_name, "', ", isDir, ", '', '', '', '');");
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
        return send(msg_sock, res, sizeof(res)-1, 0);
    }
    
    //render file
    fseek(file, 0, SEEK_END);
    unsigned long len = (unsigned long)ftell(file)+1;
    fseek(file, 0, SEEK_SET);
    
    char ext[100];
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
    char contentType[100] = "";
    if (compareStrings(ext, "html") || isIndex) {
        strcpy(contentType, "text/html; charset=utf-8");
    } else if (compareStrings(ext, "json")) {
        strcpy(contentType, "application/json;");
    } else if (compareStrings(ext, "mp4")) {
        strcpy(contentType, "video/mp4;");
    } else if (compareStrings(ext, "js")) {
        strcpy(contentType, "application/javascript; charset=utf-8");
    } else {
        strcpy(contentType, "text/plain; charset=utf-8");
    }
    int cl = getIntTextLen(len);
    int h1 = 76+24+strLength(contentType)+cl;
    char header[h1];
    sprintf(header, "%s%s%s%i%s", "HTTP/1.1 200 OK\r\nAccept-Ranges: bytes\r\nContent-Type: ", contentType, "\r\nContent-Length: ", len-1, "\r\nConnection: keep-alive\r\n\r\n");
    int msg_len;
    msg_len = send(msg_sock, header, sizeof(header)-1, 0);
    if (msg_len == 0) {
        //printf("Client closed connection\n");
        closesocket(msg_sock);
        fclose(file);
        return 0;
    }
    unsigned char res[len];
    fread(res, len, 1, file);
    fclose(file);
    return send(msg_sock, res, sizeof(res)-1, 0);
}

