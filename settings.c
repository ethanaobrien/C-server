#include <stdlib.h>

void loadSettings() {
    char path[strlen(getenv("APPDATA"))+26];
    memset(path, '\0', sizeof(path));
    strcat(path, getenv("APPDATA"));
    int i=0;
    while (path[i] != '\0') {
        if (path[i] == '\\') {
            path[i] = '/';
        }
        i++;
    }
    strcat(path, "/Simple C Server/");
    create_file_path_dirs(path);
    strcat(path, "config.0");
    FILE *file = fopen(path, "rb");
    if (file != NULL) {
        fseek(file, 0, SEEK_END);
        unsigned long len = (unsigned long)ftell(file)+1;
        fseek(file, 0, SEEK_SET);
        unsigned char data[len+1];
        memset(data, '\0', sizeof(data));
        fread(data, len, 1, file);
        fclose(file);
        char *q = strtok(data, "\r\n");
        int i=0;
        while(q != NULL) {
            int j=0;
            char a[sizeof(q)+2];
            memset(a, '\0', sizeof(a));
            strcpy(a, q);
            while (a[j]) {
                a[j] = a[j+1];
                j++;
            }
            if (i == 0) {
                memset(Settings.directory, '\0', sizeof(Settings.directory));
                strcpy(Settings.directory, a);
            } else if (i == 1) {
                Settings.port = atoi(a);
            } else if (i == 2) {
                Settings.cors = (a[0]=='1');
            } else if (i == 3) {
                Settings.index = (a[0]=='1');
            }
            i++;
            q = strtok(NULL, "\r\n");
        }
    }
}

void saveSettings() {
    char data[14+strlen(Settings.directory)+getIntTextLen(Settings.port)];
    sprintf(data, "1%s\r\n1%i\r\n1%i\r\n1%i\r\n", Settings.directory, Settings.port, Settings.cors, Settings.index);
    char path[strlen(getenv("APPDATA"))+26];
    memset(path, '\0', sizeof(path));
    strcat(path, getenv("APPDATA"));
    int i=0;
    while (path[i] != '\0') {
        if (path[i] == '\\') {
            path[i] = '/';
        }
        i++;
    }
    strcat(path, "/Simple C Server/config.0");
    create_file_path_dirs(path);
    remove(path);
    FILE *file = fopen(path, "wb");
    fwrite(data, sizeof(data), 1, file);
    fclose(file);
}
