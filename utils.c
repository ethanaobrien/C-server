boolean compareStrings(char a[], char b[]) {
    int y = strcmp(a, b);
    if (y == 0) {
        return TRUE;
    }
    return FALSE;
}

void combineStrings(char s1[], char s2[]) {
    strcat(s1, s2);
}

//from https://stackoverflow.com/a/70358229/15318223
void create_file_path_dirs(char *file_path) {
    char *dir_path = (char *) malloc(strlen(file_path) + 1);
    char *next_sep = strchr(file_path, '/');
    while (next_sep != NULL) {
        int dir_path_len = next_sep - file_path;
        memcpy(dir_path, file_path, dir_path_len);
        dir_path[dir_path_len] = '\0';
        mkdir(dir_path);
        next_sep = strchr(next_sep + 1, '/');
    }
    free(dir_path);
}

int strLength(char str[]) {
    int i=0;
    for (i=0;i<strlen(str);i++) {
        if (str[i] == '\0') {
            return i;
        }
        i++;
    }
}

void combineStrings2(char s1[], char s2[], unsigned long len) {
    int length, j, x=0;
    length = 0;
    for (j = 0; s2[j] != '\0'; ++j, ++length) {
        if (len < x) {
            break;
        }
        x++;
        s1[length] = s2[j];
    }
    s1[length] = '\0';
}

int getIntTextLen(int a) {
    int i = 10;
    int j = 1;
    while (1) {
        if (a < i) {
            return j;
        }
        i*=10;
        j++;
    }
}

boolean writeToSocket(SOCKET msg_sock, char res[], FILE *file) {
    int msg_len;
    msg_len = send(msg_sock, res, strlen(res)-1, 0);
    if (msg_len == 0 || msg_len == -1) {
        printf("Client closed connection\n");
        closesocket(msg_sock);
        if (file != NULL) {
            fclose(file);
        }
        return FALSE;
    }
    return TRUE;
}

boolean isItDirectory(char entryName[], char requestPath[]) {
    struct _stat filestat;
    char path[MAX_PATH_LEN] = "";
    combineStrings(path, Settings.directory);
    combineStrings(path, requestPath);
    combineStrings(path, "/");
    combineStrings(path, entryName);
    _stat(path, &filestat);
    return S_ISDIR(filestat.st_mode);
}

boolean endsWith(char string1[], char string2) {
    return string1[strlen(string1)-1] == string2;
}

boolean startsWith(const char *pre, const char *str)
{
    return strncmp(pre, str, strlen(pre)) == 0;
}

void urldecode(char *dst, const char *src)
{
    char a, b;
    while (*src) {
        if ((*src == '%') &&
            ((a = src[1]) && (b = src[2])) &&
            (isxdigit(a) && isxdigit(b))) {
            if (a >= 'a')
                a -= 'a'-'A';
            if (a >= 'A')
                a -= ('A' - 10);
            else
                a -= '0';
            if (b >= 'a')
                b -= 'a'-'A';
            if (b >= 'A')
                b -= ('A' - 10);
            else
                b -= '0';
            *dst++ = 16*a+b;
            src+=3;
        } else {
            *dst++ = *src++;
        }
    }
    *dst++ = '\0';
}

void pop(char in[], char delim[], char dest[]) {
    char *ret = strstr(in, delim);
    if (ret == NULL) return;
    char a[sizeof(&dest)+strlen(delim)];
    memset(a,'\0',strlen(a));
    strcpy(a, ret);
    memset(a,' ',strlen(delim));
    for (int i=0; i<sizeof(&dest);i++) {
        dest[i] = a[i+strlen(delim)];
    }
}
