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
    boolean cors;
    boolean logRequests;
    boolean error;
    char directory[300];
    char directoryListingTemplate[20000];
    unsigned long directoryListingTemplateSize;
    int port;
    boolean isRunning;
};

struct arg_struct {
    SOCKET msg_sock;
};
struct set Settings;

SOCKET sock;

#include "handler.c"
#include "server.c"

pthread_t main_server;

#include "window.c"

int main(int argc, char *argv[]) {
    Settings.port = 8887;
    Settings.directoryListing = TRUE;
    Settings.index = FALSE;
    Settings.cors = FALSE;
    Settings.logRequests = FALSE;
    Settings.error = FALSE;
    Settings.isRunning = TRUE;
    strcpy(Settings.directory, "C:");
    
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
    main_server = makeServer();
    makeWindow();
}

