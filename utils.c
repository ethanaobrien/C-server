
char *getMime(char path[]) {
    char *rv[100];
    
    char *p = strtok(path, ".");
    while(p != NULL) {
        p = strtok(path, ".");
    }
    printf("%s", rv);
    return rv;
}

boolean comparePath(char a[], char b[]) {
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

void combineStringss(unsigned char s1[], unsigned char s2[]) {
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
