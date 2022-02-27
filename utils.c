
boolean compareStrings(char a[], char b[]) {
    int y = strcmp(a, b);
    if (y == 0) {
        return TRUE;
    }
    return FALSE;
}

void combineStrings(char s1[], char s2[]) {
    int length, j;
    length = 0;
    while (s1[length] != '\0') {
        ++length;
    }
    for (j = 0; s2[j] != '\0'; ++j, ++length) {
        s1[length] = s2[j];
    }
    s1[length] = '\0';
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
    if (msg_len == 0) {
        printf("Client closed connection\n");
        closesocket(msg_sock);
        if (file != NULL) {
            fclose(file);
        }
        return FALSE;
    }
    return TRUE;
}

