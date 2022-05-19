#include <winsock2.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <dirent.h>
#include <sys/stat.h>
#include <pthread.h>
#include <sys/types.h>
#include <stdbool.h>
#define MAX_PATH_LEN 2048

#include "directory-listing-template.c"

struct set {
    boolean directoryListing;
    boolean index;
    boolean cors;
    boolean logRequests;
    boolean error;
    char directory[MAX_PATH_LEN];
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
#include "settings.c"
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
    loadSettings();
    Settings.directoryListingTemplateSize = strlen(directoryListingTemplate);
    main_server = makeServer();
    makeWindow();
}

