#include <winsock2.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <dirent.h>
#include <sys/stat.h>
#include <pthread.h>
#include <sys/types.h>
#include <stdbool.h>

struct set {
    boolean directoryListing;
    boolean index;
    char directory[300];
    char directoryListingTemplate[20000];
    unsigned long directoryListingTemplateSize;
};

struct arg_struct {
    SOCKET msg_sock;
    struct set Settings;
    int port;
};

#include "handler.c"
#include "server.c"

#define DEFAULT_PORT 8887

int main(int argc, char *argv[]) {
    
    struct set Settings;
    
    Settings.directoryListing = TRUE;
    Settings.index = FALSE;
    
    FILE *template;
    template = fopen("./directory-listing-template.html", "rb");
    if (template == NULL) {
        printf("Failed to read directory listing template");
        exit(1);
    }
    
    memset(Settings.directoryListingTemplate, '\0', sizeof(Settings.directoryListingTemplate));
    fread(Settings.directoryListingTemplate, 20000, 1, template);
    fclose(template);
    Settings.directoryListingTemplateSize = strlen(Settings.directoryListingTemplate);
    
    
    if (argc > 1) {
        strcpy(Settings.directory, argv[1]);
    } else {
        strcpy(Settings.directory, "C:");
        printf("Defaulting to C:/\n");
    }
    makeServer(DEFAULT_PORT, Settings);
    int ch;
    while(1) {
        ch = getchar();
        if (ch < 0) {
            clearerr(stdin);
        } else {
            //printf("%i", ch);
        }
    }
    
}

