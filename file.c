boolean writeHeaders(SOCKET msg_sock, int code, char msg[], struct set Settings, char range[], char content_type[], int content_length, char extra[]) {
    char header[getIntTextLen(code)+strlen(msg)+strlen(range)+strlen(content_type)+strlen(extra)+getIntTextLen(content_length)+79];
    memset(header, '\0', sizeof(header));
    sprintf(header, "%s%i%s%s%s%i%s%s%s%s%s", "HTTP/1.1 ", code, " ", msg, "\r\nConnection: keep-alive\r\nAccept-Ranges: bytes\r\nContent-Length: ", content_length, "\r\n", content_type, range, extra, "\r\n");
    int msg_len = send(msg_sock, header, sizeof(header)-1, 0);
    if (msg_len == 0) {
        printf("Client closed connection\n");
        closesocket(msg_sock);
        return FALSE;
    }
    return TRUE;
}

int render404(SOCKET msg_sock, char method[], struct set Settings) {
    if (!writeHeaders(msg_sock, 404, "Not Found", Settings, "", "", 20, "")) {
        return 0;
    }
    char response[] = "404 - File not found";
    if (startsWith(method, "HEAD")) {
        return 1;
    }
    return send(msg_sock, response, sizeof(response)-1, 0);
}

int putData(char requestPath[], SOCKET msg_sock, struct set Settings, unsigned char data[], int cl) {
    char path[300] = "";
    char decodedPath[300] = "";
    combineStrings(path, Settings.directory);
    urldecode(decodedPath, requestPath);
    combineStrings(path, decodedPath);
    boolean dataWritten = FALSE;
    FILE *file = fopen(path,"wb");
    int written = 0;
    int writeChunkSize = 1024;
    while (written < cl) {
        if (!dataWritten) {
            int a = strlen(data);
            if (strlen(data) >= cl) {
                a = cl;
            }
            fwrite(data, a, 1, file);
            written += a;
            dataWritten = TRUE;
        } else {
            int a = writeChunkSize;
            if (cl-written < writeChunkSize) {
                a = cl-written;
            }
            written += a;
            unsigned char szBuff[a];
            recv(msg_sock, szBuff, a, 0);
            fwrite(szBuff, a, 1, file);
        }
    }
    fclose(file);
    return writeHeaders(msg_sock, 201, "Created", Settings, "", "", 0, "") ? 1 : 0;
}

int deleteData(char requestPath[], SOCKET msg_sock, struct set Settings) {
    char path[300] = "";
    char decodedPath[300] = "";
    combineStrings(path, Settings.directory);
    urldecode(decodedPath, requestPath);
    combineStrings(path, decodedPath);
    remove(path);
    return writeHeaders(msg_sock, 200, "OK", Settings, "", "", 0, "") ? 1 : 0;
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
                char header[strlen(requestPath)+13];
                sprintf(header, "%s%s%s", "Location: ", requestPath, "/\r\n");
                return writeHeaders(msg_sock, 301, "Moved Permanently", Settings, "", "", 0, header) ? 1 : 0;
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
                return render404(msg_sock, method, Settings);
            }
        } else if (isDirectory != FALSE && isIndex != TRUE) {
            return render404(msg_sock, method, Settings);
        }
    } else {
        isDirectory = FALSE;
    }
    if (isDirectory) {
        if (! endsWith(path, '/')) {
            char header[strlen(requestPath)+89];
            return writeHeaders(msg_sock, 301, "Moved Permanently", Settings, "", "", 0, header) ? 1 : 0;
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
        if (!writeHeaders(msg_sock, 200, "OK", Settings, "", "Content-type: text/html; charset=utf-8\r\n", len-1, "")) {
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
    
    char ext[1000] = "";
    char *p = strtok(path, ".");
    while(p != NULL) {
        strcpy(ext, p);
        p = strtok(NULL, ".");
    }
    i=0;
    while(ext[i]) {
        ext[i] = tolower(ext[i]);
        i++;
    }
    char contentType[1000];
    getMime(ext, contentType);
    boolean hasContentType = (strlen(contentType) != 0);
    char der[strlen(contentType)+16];
    memset(der, '\0', sizeof(der));
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
            if (!endsWith(r, '-')) {
                r = strtok(r, "-");
            }
            fileOffset = atoi(r);
            cl = len-fileOffset-1;
            sprintf(hea, "%s%i%c%i%c%i%s", "Content-Range: bytes ", fileOffset, '-', len-2, '/', len-1, "\r\n");
            code = (fileOffset == 0) ? 200 : 206;
        } else {
            r = strtok(r, "-");
            fileOffset = atoi(r);
            r = strtok(NULL, "-");
            fileEndOffset = atoi(r);
            cl = fileEndOffset-fileOffset+1;
            sprintf(hea, "%s%i%c%i%c%i%s", "Content-Range: bytes: ", fileOffset, '-', fileEndOffset, '/', len-1, "\r\n");
            code = 206;
        }
        fseek(file, fileOffset, SEEK_SET);
    }
    if (!writeHeaders(msg_sock, code, (code == 200)?"OK":"Partial Content", Settings, hea, der, cl, "")) {
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
            int msg_len = send(msg_sock, res, sizeof(res)-1, 0);
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

