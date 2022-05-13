#include <winsock2.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <dirent.h>
#include <sys/stat.h>
#include <pthread.h>
#include <sys/types.h>
#include <stdbool.h>

boolean isRunning = TRUE;

struct set {
    boolean directoryListing;
    boolean index;
    boolean cors;
    boolean logRequests;
    char directory[300];
    char directoryListingTemplate[20000];
    unsigned long directoryListingTemplateSize;
};

struct arg_struct {
    SOCKET msg_sock;
    struct set Settings;
    int port;
};

#define DEFAULT_PORT 8887

SOCKET sock;

#include "handler.c"
#include "server.c"

pthread_t main_server;
struct set Settings;

#include "window.c"

int main(int argc, char *argv[]) {
    Settings.directoryListing = TRUE;
    Settings.index = FALSE;
    Settings.cors = FALSE;
    Settings.logRequests = FALSE;
    
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
    main_server = makeServer(DEFAULT_PORT, Settings);
    if (argc != 1) {
        pthread_join(main_server, NULL);
    } else {
        makeWindow();
    }
}

