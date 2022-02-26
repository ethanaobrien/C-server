
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
    if (a < 10) {
        return 1;
    } else if (a < 100) {
        return 2;
    } else if (a < 1000) {
        return 3;
    } else if (a < 10000) {
        return 4;
    } else if (a < 100000) {
        return 5;
    } else if (a < 1000000) {
        return 6;
    } else if (a < 10000000) {
        return 7;
    } else if (a < 100000000) {
        return 8;
    } else if (a < 1000000000) {
        return 9;
    } else if (a < 10000000000) {
        return 10;
    }
    return 11;
}

boolean writeToSocket(SOCKET msg_sock, char res[], FILE *file) {
    int msg_len;
    msg_len = send(msg_sock, res, strlen(res)-1, 0);
    if (msg_len == 0) {
        printf("Client closed connection\n");
        closesocket(msg_sock);
        fclose(file);
        return FALSE;
    }
    return TRUE;
}

