
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
